namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// A candidate interval to validate against existing templates.
/// </summary>
public sealed class IntervalCheckRequest
{
    /// <summary>Gets or sets the candidate engine-hours interval.</summary>
    public decimal? IntervalHours { get; set; }

    /// <summary>Gets or sets the candidate calendar interval in days.</summary>
    public int? IntervalDays { get; set; }

    /// <summary>Gets or sets a template to ignore (the one being edited).</summary>
    public Guid? ExcludeTemplateId { get; set; }
}