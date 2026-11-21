using MowTrackr.Assets.Models;
using MowTrackr.Maintenance.Models;

namespace MowTrackr.Maintenance.Extensions;

/// <summary>
/// Mapping and rule helpers for <see cref="InspectionTemplate"/>.
/// </summary>
internal static class InspectionTemplateExtensions
{
    /// <summary>Gets the name shown to people: the asset type followed by the template name ("Chain Saw 500 Hour Inspection").</summary>
    /// <param name="template">The entity (asset type loaded).</param>
    /// <returns>The display name.</returns>
    public static string DisplayName(this InspectionTemplate template)
    {
        return template.AssetType is null ? template.Name : $"{template.AssetType.Name} {template.Name}";
    }

    /// <summary>
    /// Decides whether an inspection belongs on a work order for a piece of equipment: it must be active, for the
    /// equipment's asset type, and the equipment must have reached the engine hours or the age (whichever is set;
    /// reaching either is enough).
    /// </summary>
    /// <param name="template">The inspection template.</param>
    /// <param name="mower">The equipment.</param>
    /// <param name="today">Today's date, used to work out the age.</param>
    /// <returns><see langword="true"/> if the inspection's steps should be added.</returns>
    public static bool AppliesTo(this InspectionTemplate template, Mower mower, DateOnly today)
    {
        if (!template.IsActive || template.AssetTypeId != mower.AssetTypeId)
        {
            return false;
        }

        var hoursReached = template.MinEngineHours is { } hours && mower.EngineHours >= hours;

        var ageReached = template.MinAgeMonths is { } months && AgeInMonths(mower.PurchaseDate, today) >= months;

        return hoursReached || ageReached;
    }

    /// <summary>Counts the whole months between two dates.</summary>
    /// <param name="from">The earlier date (the purchase date).</param>
    /// <param name="to">The later date.</param>
    /// <returns>Whole months elapsed; zero if <paramref name="to"/> is not after <paramref name="from"/>.</returns>
    public static int AgeInMonths(DateOnly from, DateOnly to)
    {
        var months = ((to.Year - from.Year) * 12) + to.Month - from.Month;

        if (to.Day < from.Day)
        {
            months--;
        }

        return Math.Max(0, months);
    }

    /// <summary>Maps a template to its API response.</summary>
    /// <param name="template">The entity (tasks and asset type loaded).</param>
    /// <returns>The response.</returns>
    public static InspectionTemplateResponse ToResponse(this InspectionTemplate template)
    {
        var tasks = template.Tasks
            .OrderBy(t => t.SortOrder)
            .Select(t => new ServiceTaskResponse(t.Id, t.Description, t.SortOrder, t.IsRequired))
            .ToList();

        return new InspectionTemplateResponse(
            template.Id,
            template.Name,
            template.AssetTypeId,
            template.AssetType?.Name ?? string.Empty,
            template.DisplayName(),
            template.Description,
            template.MinEngineHours,
            template.MinAgeMonths,
            template.IsActive,
            tasks);
    }

    /// <summary>Copies request values onto an entity, replacing its checklist.</summary>
    /// <param name="template">The entity to modify.</param>
    /// <param name="request">The new values.</param>
    public static void Apply(this InspectionTemplate template, InspectionTemplateRequest request)
    {
        template.Name = request.Name.Trim();

        template.AssetTypeId = request.AssetTypeId ?? Guid.Empty;

        template.Description = request.Description?.Trim();

        template.MinEngineHours = request.MinEngineHours;

        template.MinAgeMonths = request.MinAgeMonths;

        template.IsActive = request.IsActive;

        template.UpdatedAt = DateTimeOffset.UtcNow;

        template.Tasks.Clear();

        var order = 0;

        foreach (var task in request.Tasks.Where(t => !string.IsNullOrWhiteSpace(t.Description)))
        {
            template.Tasks.Add(new InspectionTask
            {
                InspectionTemplateId = template.Id,
                Description = task.Description.Trim(),
                SortOrder = order++,
                IsRequired = task.IsRequired,
            });
        }
    }
}
