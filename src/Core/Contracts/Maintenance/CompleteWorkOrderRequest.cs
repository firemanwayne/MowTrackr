namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// Payload for closing a work order.
/// </summary>
public sealed class CompleteWorkOrderRequest
{
    /// <summary>Gets or sets the mower's engine hours at the time of service.</summary>
    [Range(0, 1_000_000)]
    public decimal EngineHoursAtService { get; set; }

    /// <summary>Gets or sets closing notes.</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}