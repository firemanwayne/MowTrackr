namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// The details a technician records against a service step. The quantity's unit is not sent: it comes from the step or the equipment.
/// </summary>
public sealed class TaskDetailsRequest
{
    /// <summary>Gets or sets the quantity used.</summary>
    [Range(0, 100_000)]
    public decimal? Quantity { get; set; }

    /// <summary>Gets or sets the part number.</summary>
    [StringLength(100)]
    public string? PartNumber { get; set; }

    /// <summary>Gets or sets notes about the step.</summary>
    [StringLength(500)]
    public string? Notes { get; set; }
}
