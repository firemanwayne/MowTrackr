using MowTrackr.Assets.Models;

namespace MowTrackr.Maintenance.Models;

/// <summary>
/// A reusable service definition (e.g. "Quarterly Maintenance"): a checklist plus a due interval.
/// </summary>
public sealed class ServiceTemplate : EntityBase
{
    /// <summary>Gets or sets the template name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the asset type key this template applies to.</summary>
    public Guid AssetTypeId { get; set; }

    /// <summary>Gets or sets the asset type this template applies to.</summary>
    public AssetType? AssetType { get; set; }

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