using MowTrackr.Assets.Models;

namespace MowTrackr.Maintenance.Models;

/// <summary>
/// A reusable service definition (e.g. "Annual Inspection"): a checklist, a due interval, the asset types it applies to, and
/// the pre-check and post-check inspections assigned to it.
/// </summary>
public sealed class ServiceTemplate : EntityBase
{
    /// <summary>Gets or sets the template name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the asset types this template applies to.</summary>
    public List<AssetType> AssetTypes { get; set; } = [];

    /// <summary>Gets or sets the inspection templates (pre-check and post-check) assigned to this service.</summary>
    public List<InspectionTemplate> Inspections { get; set; } = [];

    /// <summary>Gets or sets an optional description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the engine-hours interval.</summary>
    public decimal? IntervalHours { get; set; }

    /// <summary>Gets or sets the calendar interval in days.</summary>
    public int? IntervalDays { get; set; }

    /// <summary>Gets or sets a value indicating whether the template participates in due checks.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the checklist.</summary>
    public List<ServiceTask> Tasks { get; set; } = [];
}
