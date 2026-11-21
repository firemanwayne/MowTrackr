namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// Request to check a candidate interval against the existing templates for the same asset types.
/// </summary>
public sealed class IntervalCheckRequest
{
    /// <summary>Gets or sets the engine-hours interval.</summary>
    public decimal? IntervalHours { get; set; }

    /// <summary>Gets or sets the calendar interval in days.</summary>
    public int? IntervalDays { get; set; }

    /// <summary>Gets or sets the asset types the candidate applies to; only templates sharing one of them are compared.</summary>
    public List<Guid> AssetTypeIds { get; set; } = [];

    /// <summary>Gets or sets a template to leave out of the comparison (the one being edited).</summary>
    public Guid? ExcludeTemplateId { get; set; }
}
