using MowTrackr.Maintenance.Extensions;
using MowTrackr.Maintenance.Models;

namespace MowTrackr.Maintenance.Services;

/// <summary>
/// Default <see cref="IServiceTemplateService"/>.
/// </summary>
/// <param name="templates">The template repository.</param>
/// <param name="workOrders">The work order repository.</param>
/// <param name="validator">The interval validator.</param>
/// <param name="logger">The logger.</param>
internal sealed class ServiceTemplateService(
    IRepository<ServiceTemplate> templates,
    IRepository<WorkOrder> workOrders,
    IIntervalValidator validator,
    ILogger<ServiceTemplateService> logger) : IServiceTemplateService
{
    private static readonly string[] TaskIncludes = [nameof(ServiceTemplate.Tasks)];

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ServiceTemplateResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var spec = new QuerySpec<ServiceTemplate> { Includes = TaskIncludes, OrderBy = t => t.Name };

        var items = await templates.ListAsync(spec, cancellationToken).ConfigureAwait(false);

        return Result<IReadOnlyList<ServiceTemplateResponse>>.Ok(items.Select(t => t.ToResponse()).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<ServiceTemplateResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var template = await templates.GetByIdAsync(id, new QuerySpec<ServiceTemplate> { Includes = TaskIncludes }, cancellationToken).ConfigureAwait(false);

        if (template is null)
        {
            return Result<ServiceTemplateResponse>.NotFound("Service template not found.");
        }

        return Result<ServiceTemplateResponse>.Ok(template.ToResponse());
    }

    /// <inheritdoc />
    public async Task<Result<ServiceTemplateSaveResponse>> CreateAsync(ServiceTemplateRequest request, CancellationToken cancellationToken)
    {
        var issues = await ValidateAsync(request, null, cancellationToken).ConfigureAwait(false);

        if (issues.Any(i => i.Severity == IssueSeverity.Error))
        {
            return Result<ServiceTemplateSaveResponse>.Conflict("The service template has conflicts.", new ServiceTemplateSaveResponse(null, issues));
        }

        var template = new ServiceTemplate();

        template.Apply(request);

        await templates.AddAsync(template, cancellationToken).ConfigureAwait(false);

        await templates.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Created service template {TemplateId} ({TemplateName})", template.Id, template.Name);

        return Result<ServiceTemplateSaveResponse>.Ok(new ServiceTemplateSaveResponse(template.ToResponse(), issues));
    }

    /// <inheritdoc />
    public async Task<Result<ServiceTemplateSaveResponse>> UpdateAsync(Guid id, ServiceTemplateRequest request, CancellationToken cancellationToken)
    {
        var spec = new QuerySpec<ServiceTemplate> { Includes = TaskIncludes, Track = true };

        var template = await templates.GetByIdAsync(id, spec, cancellationToken).ConfigureAwait(false);

        if (template is null)
        {
            return Result<ServiceTemplateSaveResponse>.NotFound("Service template not found.");
        }

        var issues = await ValidateAsync(request, id, cancellationToken).ConfigureAwait(false);

        if (issues.Any(i => i.Severity == IssueSeverity.Error))
        {
            return Result<ServiceTemplateSaveResponse>.Conflict("The service template has conflicts.", new ServiceTemplateSaveResponse(null, issues));
        }

        template.Apply(request);

        await templates.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Updated service template {TemplateId}", template.Id);

        return Result<ServiceTemplateSaveResponse>.Ok(new ServiceTemplateSaveResponse(template.ToResponse(), issues));
    }

    /// <inheritdoc />
    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var template = await templates.GetByIdAsync(id, new QuerySpec<ServiceTemplate> { Track = true }, cancellationToken).ConfigureAwait(false);

        if (template is null)
        {
            return Result<bool>.NotFound("Service template not found.");
        }

        var used = await workOrders.ListAsync(new QuerySpec<WorkOrder> { Filter = w => w.ServiceTemplateId == id }, cancellationToken).ConfigureAwait(false);

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
        var existing = await LoadActiveIntervalsAsync(cancellationToken).ConfigureAwait(false);

        return Result<IReadOnlyList<IntervalIssue>>.Ok(validator.Validate(request, existing));
    }

    /// <summary>Validates a template request against the other active templates.</summary>
    /// <param name="request">The template request.</param>
    /// <param name="excludeId">The template being edited, if any.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The findings.</returns>
    private async Task<IReadOnlyList<IntervalIssue>> ValidateAsync(ServiceTemplateRequest request, Guid? excludeId, CancellationToken cancellationToken)
    {
        var existing = await LoadActiveIntervalsAsync(cancellationToken).ConfigureAwait(false);

        var candidate = new IntervalCheckRequest
        {
            IntervalHours = request.IntervalHours,
            IntervalDays = request.IntervalDays,
            ExcludeTemplateId = excludeId,
        };

        var issues = validator.Validate(candidate, existing).ToList();

        if (request.Tasks.All(t => string.IsNullOrWhiteSpace(t.Description)))
        {
            issues.Add(new IntervalIssue(IssueSeverity.Error, "Add at least one checklist task."));
        }

        return issues;
    }

    /// <summary>Loads the interval settings of every active template.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The interval definitions.</returns>
    private async Task<IReadOnlyCollection<IntervalDefinition>> LoadActiveIntervalsAsync(CancellationToken cancellationToken)
    {
        var active = await templates.ListAsync(new QuerySpec<ServiceTemplate> { Filter = t => t.IsActive }, cancellationToken).ConfigureAwait(false);

        return active.Select(t => t.ToIntervalDefinition()).ToList();
    }
}