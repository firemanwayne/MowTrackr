using MowTrackr.Assets.Models;
using MowTrackr.Maintenance.Extensions;
using MowTrackr.Maintenance.Models;
using MowTrackr.Maintenance.Validators;

namespace MowTrackr.Maintenance.Services;

/// <summary>
/// Default <see cref="IInspectionTemplateService"/>: CRUD for inspection (check) templates.
/// </summary>
/// <param name="templates">The inspection template repository.</param>
/// <param name="assetTypes">The asset type repository.</param>
/// <param name="logger">The logger.</param>
internal sealed class InspectionTemplateService(
    IRepository<InspectionTemplate> templates,
    IRepository<AssetType> assetTypes,
    ILogger<InspectionTemplateService> logger) : IInspectionTemplateService
{
    private static readonly string[] _includes = [nameof(InspectionTemplate.Tasks), nameof(InspectionTemplate.AssetType)];

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<InspectionTemplateResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var items = await templates
            .ListAsync(new QuerySpec<InspectionTemplate> { Includes = _includes, OrderBy = t => t.Name }, cancellationToken)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<InspectionTemplateResponse>>.Ok(items.Select(t => t.ToResponse()).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<InspectionTemplateResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var template = await templates
            .GetByIdAsync(id, new QuerySpec<InspectionTemplate> { Includes = _includes }, cancellationToken)
            .ConfigureAwait(false);

        if (template is null)
        {
            return Result<InspectionTemplateResponse>.NotFound("Inspection template not found.");
        }

        return Result<InspectionTemplateResponse>.Ok(template.ToResponse());
    }

    /// <inheritdoc />
    public async Task<Result<InspectionTemplateResponse>> CreateAsync(InspectionTemplateRequest request, CancellationToken cancellationToken)
    {
        var error = await ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (error is not null)
        {
            return Result<InspectionTemplateResponse>.Invalid(error);
        }

        var template = new InspectionTemplate();

        template.Apply(request);

        await templates.AddAsync(template, cancellationToken).ConfigureAwait(false);

        await templates.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Created inspection template {TemplateId} ({TemplateName})", template.Id, template.Name);

        return await GetAsync(template.Id, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<InspectionTemplateResponse>> UpdateAsync(Guid id, InspectionTemplateRequest request, CancellationToken cancellationToken)
    {
        var template = await templates
            .GetByIdAsync(id, new QuerySpec<InspectionTemplate> { Includes = [nameof(InspectionTemplate.Tasks)], Track = true }, cancellationToken)
            .ConfigureAwait(false);

        if (template is null)
        {
            return Result<InspectionTemplateResponse>.NotFound("Inspection template not found.");
        }

        var error = await ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (error is not null)
        {
            return Result<InspectionTemplateResponse>.Invalid(error);
        }

        template.Apply(request);

        await templates.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Updated inspection template {TemplateId}", template.Id);

        return await GetAsync(template.Id, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var template = await templates
            .GetByIdAsync(id, new QuerySpec<InspectionTemplate> { Includes = [nameof(InspectionTemplate.ServiceTemplates)], Track = true }, cancellationToken)
            .ConfigureAwait(false);

        if (template is null)
        {
            return Result<bool>.NotFound("Inspection template not found.");
        }

        if (template.ServiceTemplates.Count > 0)
        {
            return Result<bool>.Conflict($"This inspection is assigned to {template.ServiceTemplates.Count} service template(s). Remove it from them first, or mark it inactive.");
        }

        templates.Remove(template);

        await templates.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Deleted inspection template {TemplateId}", id);

        return Result<bool>.Ok(true);
    }

    /// <summary>Checks a request: it has items, choice items have choices, and the asset type (if any) exists.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An error message, or <see langword="null"/> when the request is valid.</returns>
    private async Task<string?> ValidateAsync(InspectionTemplateRequest request, CancellationToken cancellationToken)
    {
        var items = request.Tasks.Where(t => !string.IsNullOrWhiteSpace(t.Description)).ToList();

        if (items.Count == 0)
        {
            return "Add at least one item.";
        }

        foreach (var item in items.Where(i => i.ItemType == CheckItemType.Choice))
        {
            var choices = CheckAnswerValidator.SplitList(item.Choices);

            if (choices.Count < 2)
            {
                return $"\"{item.Description}\": list at least two choices, separated by commas.";
            }

            var unknown = CheckAnswerValidator.SplitList(item.RepairOn).FirstOrDefault(r => !choices.Contains(r, StringComparer.OrdinalIgnoreCase));

            if (unknown is not null)
            {
                return $"\"{item.Description}\": '{unknown}' is not one of the choices.";
            }
        }

        if (request.AssetTypeId is not { } typeId)
        {
            return null;
        }

        var types = await assetTypes
            .ListAsync(new QuerySpec<AssetType> { Filter = t => t.Id == typeId }, cancellationToken)
            .ConfigureAwait(false);

        return types.Count == 0 ? "Choose an asset type, or leave it empty for any type." : null;
    }
}
