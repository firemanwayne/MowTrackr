using MowTrackr.Assets.Models;
using MowTrackr.Maintenance.Extensions;
using MowTrackr.Maintenance.Models;

namespace MowTrackr.Maintenance.Services;

/// <summary>
/// Default <see cref="IServiceTemplateService"/>: template CRUD with interval conflict detection.
/// </summary>
/// <param name="templates">The template repository.</param>
/// <param name="assetTypes">The asset type repository.</param>
/// <param name="inspections">The inspection template repository.</param>
/// <param name="workOrders">The work order repository.</param>
/// <param name="validator">The interval validator.</param>
/// <param name="logger">The logger.</param>
internal sealed class ServiceTemplateService(
    IRepository<ServiceTemplate> templates,
    IRepository<AssetType> assetTypes,
    IRepository<InspectionTemplate> inspections,
    IRepository<WorkOrder> workOrders,
    IIntervalValidator validator,
    ILogger<ServiceTemplateService> logger) : IServiceTemplateService
{
    private static readonly string[] TaskIncludes = [nameof(ServiceTemplate.Tasks), nameof(ServiceTemplate.AssetTypes), nameof(ServiceTemplate.Inspections)];

    /// <summary>The asset types and inspections a request refers to, once checked.</summary>
    /// <param name="Issues">Problems and warnings found.</param>
    /// <param name="Types">The asset types.</param>
    /// <param name="Inspections">The inspection templates.</param>
    private sealed record Resolved(List<IntervalIssue> Issues, List<AssetType> Types, List<InspectionTemplate> Inspections);

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ServiceTemplateResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var spec = new QuerySpec<ServiceTemplate> { Includes = TaskIncludes, OrderBy = t => t.Name };

        var items = await templates
            .ListAsync(spec, cancellationToken)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<ServiceTemplateResponse>>.Ok(items.Select(t => t.ToResponse()).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<ServiceTemplateResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var template = await templates
            .GetByIdAsync(id, new QuerySpec<ServiceTemplate> { Includes = TaskIncludes }, cancellationToken)
            .ConfigureAwait(false);

        if (template is null)
        {
            return Result<ServiceTemplateResponse>.NotFound("Service template not found.");
        }

        return Result<ServiceTemplateResponse>.Ok(template.ToResponse());
    }

    /// <inheritdoc />
    public async Task<Result<ServiceTemplateSaveResponse>> CreateAsync(ServiceTemplateRequest request, CancellationToken cancellationToken)
    {
        var resolved = await ResolveAsync(request, null, cancellationToken).ConfigureAwait(false);

        if (resolved.Issues.Any(i => i.Severity == IssueSeverity.Error))
        {
            return Result<ServiceTemplateSaveResponse>.Conflict("The service template has conflicts.", new ServiceTemplateSaveResponse(null, resolved.Issues));
        }

        var template = new ServiceTemplate();

        template.Apply(request);

        Assign(template, resolved);

        await templates.AddAsync(template, cancellationToken).ConfigureAwait(false);

        await templates.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Created service template {TemplateId} ({TemplateName})", template.Id, template.Name);

        var created = await GetAsync(template.Id, cancellationToken).ConfigureAwait(false);

        return Result<ServiceTemplateSaveResponse>.Ok(new ServiceTemplateSaveResponse(created.Value, resolved.Issues));
    }

    /// <inheritdoc />
    public async Task<Result<ServiceTemplateSaveResponse>> UpdateAsync(Guid id, ServiceTemplateRequest request, CancellationToken cancellationToken)
    {
        var spec = new QuerySpec<ServiceTemplate> { Includes = TaskIncludes, Track = true };

        var template = await templates
            .GetByIdAsync(id, spec, cancellationToken)
            .ConfigureAwait(false);

        if (template is null)
        {
            return Result<ServiceTemplateSaveResponse>.NotFound("Service template not found.");
        }

        var resolved = await ResolveAsync(request, id, cancellationToken).ConfigureAwait(false);

        if (resolved.Issues.Any(i => i.Severity == IssueSeverity.Error))
        {
            return Result<ServiceTemplateSaveResponse>.Conflict("The service template has conflicts.", new ServiceTemplateSaveResponse(null, resolved.Issues));
        }

        template.Apply(request);

        Assign(template, resolved);

        await templates.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Updated service template {TemplateId}", template.Id);

        var updated = await GetAsync(template.Id, cancellationToken).ConfigureAwait(false);

        return Result<ServiceTemplateSaveResponse>.Ok(new ServiceTemplateSaveResponse(updated.Value, resolved.Issues));
    }

    /// <inheritdoc />
    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var template = await templates
            .GetByIdAsync(id, new QuerySpec<ServiceTemplate> { Track = true }, cancellationToken)
            .ConfigureAwait(false);

        if (template is null)
        {
            return Result<bool>.NotFound("Service template not found.");
        }

        var used = await workOrders
            .ListAsync(new QuerySpec<WorkOrder> { Filter = w => w.ServiceTemplateId == id }, cancellationToken)
            .ConfigureAwait(false);

        if (used.Count > 0)
        {
            return Result<bool>.Conflict("This template has work orders. Mark it inactive instead of deleting it.");
        }

        templates.Remove(template);

        await templates.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Deleted service template {TemplateId}", id);

        return Result<bool>.Ok(true);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<IntervalIssue>>> CheckIntervalsAsync(IntervalCheckRequest request, CancellationToken cancellationToken)
    {
        var existing = await LoadActiveIntervalsAsync(request.AssetTypeIds, cancellationToken).ConfigureAwait(false);

        return Result<IReadOnlyList<IntervalIssue>>.Ok(validator.Validate(request, existing));
    }

    /// <summary>Puts the resolved asset types and inspections on the template.</summary>
    /// <param name="template">The tracked template.</param>
    /// <param name="resolved">The checked references.</param>
    private static void Assign(ServiceTemplate template, Resolved resolved)
    {
        template.AssetTypes.Clear();

        template.AssetTypes.AddRange(resolved.Types);

        template.Inspections.Clear();

        template.Inspections.AddRange(resolved.Inspections);
    }

    /// <summary>Checks a request: its asset types and inspections exist and fit together, it has steps, and its interval does not clash.</summary>
    /// <param name="request">The request.</param>
    /// <param name="excludeId">The template being edited, if any.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The issues found and the entities the request refers to.</returns>
    private async Task<Resolved> ResolveAsync(ServiceTemplateRequest request, Guid? excludeId, CancellationToken cancellationToken)
    {
        var typeIds = request.AssetTypeIds.Distinct().ToList();

        var existing = await LoadActiveIntervalsAsync(typeIds, cancellationToken).ConfigureAwait(false);

        var candidate = new IntervalCheckRequest
        {
            AssetTypeIds = typeIds,
            IntervalHours = request.IntervalHours,
            IntervalDays = request.IntervalDays,
            ExcludeTemplateId = excludeId,
        };

        var issues = validator.Validate(candidate, existing).ToList();

        var types = await assetTypes
            .ListAsync(new QuerySpec<AssetType> { Filter = t => typeIds.Contains(t.Id), Track = true }, cancellationToken)
            .ConfigureAwait(false);

        if (typeIds.Count == 0 || types.Count != typeIds.Count)
        {
            issues.Add(new IntervalIssue(IssueSeverity.Error, "Choose at least one asset type."));
        }

        var inspectionIds = request.InspectionTemplateIds.Distinct().ToList();

        var assigned = await inspections
            .ListAsync(new QuerySpec<InspectionTemplate> { Filter = i => inspectionIds.Contains(i.Id), Track = true }, cancellationToken)
            .ConfigureAwait(false);

        if (assigned.Count != inspectionIds.Count)
        {
            issues.Add(new IntervalIssue(IssueSeverity.Error, "One of the chosen inspections no longer exists."));
        }

        foreach (var inspection in assigned.Where(i => i.AssetTypeId is { } limit && !typeIds.Contains(limit)))
        {
            issues.Add(new IntervalIssue(IssueSeverity.Error, $"'{inspection.Name}' is for a different type of equipment than this service."));
        }

        if (request.Tasks.All(t => string.IsNullOrWhiteSpace(t.Description)))
        {
            issues.Add(new IntervalIssue(IssueSeverity.Error, "Add at least one checklist task."));
        }

        return new Resolved(issues, types.ToList(), assigned.ToList());
    }

    /// <summary>Loads the intervals of the active templates that share an asset type with the candidate.</summary>
    /// <param name="assetTypeIds">The candidate's asset types; empty means every active template.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The interval definitions.</returns>
    private async Task<IReadOnlyCollection<IntervalDefinition>> LoadActiveIntervalsAsync(IReadOnlyCollection<Guid> assetTypeIds, CancellationToken cancellationToken)
    {
        var ids = assetTypeIds.ToList();

        var active = await templates
            .ListAsync(new QuerySpec<ServiceTemplate> { Filter = t => t.IsActive && (ids.Count == 0 || t.AssetTypes.Any(a => ids.Contains(a.Id))) }, cancellationToken)
            .ConfigureAwait(false);

        return active.Select(t => t.ToIntervalDefinition()).ToList();
    }
}
