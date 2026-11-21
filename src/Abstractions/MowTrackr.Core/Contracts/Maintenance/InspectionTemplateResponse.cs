namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// An inspection (check) template as returned by the API.
/// </summary>
/// <param name="Id">The template key.</param>
/// <param name="Name">The template name.</param>
/// <param name="AssetTypeId">The asset type it is limited to, or <see langword="null"/> for any type.</param>
/// <param name="AssetType">The asset type name, empty for any type.</param>
/// <param name="Timing">Pre-check (before) or post-check (after).</param>
/// <param name="DisplayName">The name shown to people.</param>
/// <param name="Description">The description.</param>
/// <param name="MinEngineHours">The engine hours threshold for automatic inclusion.</param>
/// <param name="MinAgeMonths">The age threshold for automatic inclusion.</param>
/// <param name="IsActive">Whether it is added to work orders.</param>
/// <param name="Tasks">The items.</param>
public sealed record InspectionTemplateResponse(
    Guid Id,
    string Name,
    Guid? AssetTypeId,
    string AssetType,
    InspectionTiming Timing,
    string DisplayName,
    string? Description,
    decimal? MinEngineHours,
    int? MinAgeMonths,
    bool IsActive,
    IReadOnlyList<InspectionTaskResponse> Tasks);
