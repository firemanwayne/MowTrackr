using MowTrackr.Maintenance.Models;

namespace MowTrackr.Maintenance.Extensions;

/// <summary>
/// Mapping helpers for <see cref="ServiceTemplate"/>.
/// </summary>
internal static class ServiceTemplateExtensions
{
    /// <summary>Maps a template to its API response.</summary>
    /// <param name="template">The entity (tasks must be loaded).</param>
    /// <returns>The response.</returns>
    public static ServiceTemplateResponse ToResponse(this ServiceTemplate template)
    {
        var tasks = template.Tasks
            .OrderBy(t => t.SortOrder)
            .Select(t => new ServiceTaskResponse(t.Id, t.Description, t.SortOrder, t.IsRequired))
            .ToList();

        return new ServiceTemplateResponse(
            template.Id,
            template.Name,
            template.AssetTypeId,
            template.AssetType?.Name ?? string.Empty,
            template.DisplayName(),
            template.Description,
            template.IntervalHours,
            template.IntervalDays,
            template.IsActive,
            tasks);
    }

    /// <summary>Gets the name shown to people: the asset type followed by the template name ("Chain Saw Annual Maintenance").</summary>
    /// <param name="template">The entity (asset type loaded).</param>
    /// <returns>The display name.</returns>
    public static string DisplayName(this ServiceTemplate template)
    {
        return template.AssetType is null ? template.Name : $"{template.AssetType.Name} {template.Name}";
    }

    /// <summary>Maps a template to the shape the interval validator compares against.</summary>
    /// <param name="template">The entity.</param>
    /// <returns>The interval definition.</returns>
    public static IntervalDefinition ToIntervalDefinition(this ServiceTemplate template)
    {
        return new IntervalDefinition(template.Id, template.Name, template.IntervalHours, template.IntervalDays);
    }

    /// <summary>Copies request values onto an entity, replacing its checklist.</summary>
    /// <param name="template">The entity to modify.</param>
    /// <param name="request">The new values.</param>
    public static void Apply(this ServiceTemplate template, ServiceTemplateRequest request)
    {
        template.Name = request.Name.Trim();

        template.AssetTypeId = request.AssetTypeId ?? Guid.Empty;

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
            });
        }
    }
}