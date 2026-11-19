namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// A service template as returned by the API.
/// </summary>
/// <param name="Id">The template key.</param>
/// <param name="Name">The template name.</param>
/// <param name="Description">The description.</param>
/// <param name="IntervalHours">The engine-hours interval.</param>
/// <param name="IntervalDays">The calendar interval in days.</param>
/// <param name="IsActive">Whether the template is used for due checks.</param>
/// <param name="Tasks">The ordered checklist.</param>
public sealed record ServiceTemplateResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal? IntervalHours,
    int? IntervalDays,
    bool IsActive,
    IReadOnlyList<ServiceTaskResponse> Tasks);