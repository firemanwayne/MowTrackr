using MowTrackr.Assets.Models;

namespace MowTrackr.Maintenance.Models;

/// <summary>
/// A check for a technician to complete on a work order: a pre-check (before the work starts) or a post-check (after it is
/// done). It reaches a work order by being assigned to a service template, or automatically once a piece of equipment is old
/// enough or has enough hours on it. It can be limited to one asset type, or apply to any.
/// </summary>
public sealed class InspectionTemplate : EntityBase
{
    /// <summary>Gets or sets the template name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the asset type key this inspection is limited to, or <see langword="null"/> for any type.</summary>
    public Guid? AssetTypeId { get; set; }

    /// <summary>Gets or sets the asset type this inspection is limited to.</summary>
    public AssetType? AssetType { get; set; }

    /// <summary>Gets or sets whether this is a pre-check (before the work) or a post-check (after it).</summary>
    public InspectionTiming Timing { get; set; } = InspectionTiming.After;

    /// <summary>Gets or sets an optional description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the engine hours at or above which the inspection is added automatically.</summary>
    public decimal? MinEngineHours { get; set; }

    /// <summary>Gets or sets the age in months (since purchase) at or above which the inspection is added automatically.</summary>
    public int? MinAgeMonths { get; set; }

    /// <summary>Gets or sets a value indicating whether the inspection is added to work orders.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the service templates this inspection is assigned to.</summary>
    public List<ServiceTemplate> ServiceTemplates { get; set; } = [];

    /// <summary>Gets or sets the items.</summary>
    public List<InspectionTask> Tasks { get; set; } = [];
}
