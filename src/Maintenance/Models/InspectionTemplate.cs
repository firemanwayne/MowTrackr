using MowTrackr.Assets.Models;

namespace MowTrackr.Maintenance.Models;

/// <summary>
/// Extra things for a technician to check on a work order once a piece of equipment is old enough or has enough
/// hours on it (for example a "Chain Saw 500 Hour Inspection"). Applies to one asset type.
/// </summary>
public sealed class InspectionTemplate : EntityBase
{
    /// <summary>Gets or sets the template name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the asset type key this inspection applies to.</summary>
    public Guid AssetTypeId { get; set; }

    /// <summary>Gets or sets the asset type this inspection applies to.</summary>
    public AssetType? AssetType { get; set; }

    /// <summary>Gets or sets an optional description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the engine hours at or above which the inspection applies.</summary>
    public decimal? MinEngineHours { get; set; }

    /// <summary>Gets or sets the age in months (since purchase) at or above which the inspection applies.</summary>
    public int? MinAgeMonths { get; set; }

    /// <summary>Gets or sets a value indicating whether the inspection is added to work orders.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the checklist.</summary>
    public List<InspectionTask> Tasks { get; set; } = [];
}
