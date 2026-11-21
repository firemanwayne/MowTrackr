using MowTrackr.Core.Contracts.Assets;
using MowTrackr.Assets.Models;
using MowTrackr.Maintenance.Models;

namespace MowTrackr.Maintenance.Extensions;

/// <summary>
/// Mapping helpers for <see cref="ServiceTemplate"/>.
/// </summary>
internal static class ServiceTemplateExtensions
{
    /// <summary>Maps a template to its API response.</summary>
    /// <param name="template">The entity (tasks, asset types and inspections must be loaded).</param>
    /// <returns>The response.</returns>
    public static ServiceTemplateResponse ToResponse(this ServiceTemplate template)
    {
        var tasks = template.Tasks
            .OrderBy(t => t.SortOrder)
            .Select(t => new ServiceTaskResponse(t.Id, t.Description, t.SortOrder, t.IsRequired, t.Condition))
            .ToList();

        var types = template.AssetTypes
            .OrderBy(t => t.Name)
            .Select(t => new AssetTypeResponse(t.Id, t.Name, t.HasTires))
            .ToList();

        var inspections = template.Inspections
            .OrderBy(i => i.Timing)
            .ThenBy(i => i.Name)
            .Select(i => new InspectionSummaryResponse(i.Id, i.Name, i.Timing))
            .ToList();

        return new ServiceTemplateResponse(
            template.Id,
            template.Name,
            types,
            string.Join(", ", types.Select(t => t.Name)),
            inspections,
            template.Description,
            template.IntervalHours,
            template.IntervalDays,
            template.IsActive,
            tasks);
    }

    /// <summary>Gets the name shown for a unit of one asset type: the type followed by the template name ("Push Mower Annual Inspection").</summary>
    /// <param name="template">The entity.</param>
    /// <param name="assetTypeName">The unit's asset type name, if known.</param>
    /// <returns>The title.</returns>
    public static string TitleFor(this ServiceTemplate template, string? assetTypeName)
    {
        return string.IsNullOrWhiteSpace(assetTypeName) ? template.Name : $"{assetTypeName} {template.Name}";
    }

    /// <summary>Checks whether a service step applies to an asset type.</summary>
    /// <param name="task">The step.</param>
    /// <param name="assetType">The unit's asset type.</param>
    /// <returns><see langword="true"/> when the step applies.</returns>
    public static bool AppliesTo(this ServiceTask task, AssetType assetType)
    {
        return task.Condition switch
        {
            ServiceTaskCondition.AssetHasTires => assetType.HasTires,
            _ => true,
        };
    }

    /// <summary>Maps a template to the shape the interval validator compares against.</summary>
    /// <param name="template">The entity.</param>
    /// <returns>The interval definition.</returns>
    public static IntervalDefinition ToIntervalDefinition(this ServiceTemplate template)
    {
        return new IntervalDefinition(template.Id, template.Name, template.IntervalHours, template.IntervalDays);
    }

    /// <summary>Copies request values onto an entity, replacing its checklist. Asset types and inspections are assigned by the service.</summary>
    /// <param name="template">The entity to modify.</param>
    /// <param name="request">The new values.</param>
    public static void Apply(this ServiceTemplate template, ServiceTemplateRequest request)
    {
        template.Name = request.Name.Trim();
        template.Description = request.Description?.Trim();
        template.IntervalHours = request.IntervalHours;
        template.IntervalDays = request.IntervalDays;
        template.IsActive = request.IsActive;
        template.UpdatedAt = DateTimeOffset.UtcNow;

        template.Tasks.Clear();

        var order = 0;

        foreach (var task in request.Tasks.Where(t => !string.IsNullOrWhiteSpace(t.Description)))
        {
            template.Tasks.Add(new ServiceTask
            {
                ServiceTemplateId = template.Id,
                Description = task.Description.Trim(),
                SortOrder = order++,
                IsRequired = task.IsRequired,
                Condition = task.Condition,
            });
        }
    }
}
