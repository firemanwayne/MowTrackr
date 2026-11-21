namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// An inspection template as returned by the API.
/// </summary>
/// <param name="Id">The template key.</param>
/// <param name="Name">The template name (without the asset type).</param>
/// <param name="AssetTypeId">The asset type key.</param>
/// <param name="AssetType">The asset type name.</param>
/// <param name="DisplayName">The asset type followed by the name, e.g. "Chain Saw 500 Hour Inspection".</param>
/// <param name="Description">The description.</param>
/// <param name="MinEngineHours">The engine hours at or above which it applies.</param>
/// <param name="MinAgeMonths">The age in months at or above which it applies.</param>
/// <param name="IsActive">Whether it is added to work orders.</param>
/// <param name="Tasks">The ordered checklist.</param>
public sealed record InspectionTemplateResponse(
    Guid Id,
    string Name,
    Guid AssetTypeId,
    string AssetType,
    string DisplayName,
    string? Description,
    decimal? MinEngineHours,
    int? MinAgeMonths,
    bool IsActive,
    IReadOnlyList<ServiceTaskResponse> Tasks);
