using MowTrackr.Core.Contracts.Assets;

namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// A service template as returned by the API.
/// </summary>
/// <param name="Id">The template key.</param>
/// <param name="Name">The template name.</param>
/// <param name="AssetTypes">The asset types it applies to.</param>
/// <param name="AssetTypeNames">The asset type names, joined for display.</param>
/// <param name="Inspections">The assigned pre-check and post-check inspections.</param>
/// <param name="Description">The description.</param>
/// <param name="IntervalHours">The engine-hours interval.</param>
/// <param name="IntervalDays">The calendar interval in days.</param>
/// <param name="IsActive">Whether the template participates in due checks.</param>
/// <param name="Tasks">The checklist.</param>
public sealed record ServiceTemplateResponse(
    Guid Id,
    string Name,
    IReadOnlyList<AssetTypeResponse> AssetTypes,
    string AssetTypeNames,
    IReadOnlyList<InspectionSummaryResponse> Inspections,
    string? Description,
    decimal? IntervalHours,
    int? IntervalDays,
    bool IsActive,
    IReadOnlyList<ServiceTaskResponse> Tasks);
