namespace MowTrackr.Maintenance.Options;

/// <summary>
/// Defaults for the due-maintenance check, bound from the "Maintenance" configuration section.
/// </summary>
public sealed class MaintenanceOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Maintenance";

    /// <summary>Gets or sets how many engine hours ahead counts as "coming due".</summary>
    public decimal LookaheadHours { get; set; } = 10;

    /// <summary>Gets or sets how many days ahead counts as "coming due".</summary>
    public int LookaheadDays { get; set; } = 14;
}