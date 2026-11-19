namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// Options for the due-maintenance check. Omitted values fall back to the configured defaults.
/// </summary>
public sealed class MaintenanceDueQuery
{
    /// <summary>Gets or sets how many engine hours ahead to look for upcoming service.</summary>
    public decimal? LookaheadHours { get; set; }

    /// <summary>Gets or sets how many days ahead to look for upcoming service.</summary>
    public int? LookaheadDays { get; set; }
}