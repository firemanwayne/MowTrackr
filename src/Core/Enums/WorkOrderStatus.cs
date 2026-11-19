namespace MowTrackr.Core.Enums;

/// <summary>
/// Lifecycle state of a maintenance work order.
/// </summary>
public enum WorkOrderStatus
{
    /// <summary>Scheduled but not started.</summary>
    Scheduled,

    /// <summary>At least one task has been worked.</summary>
    InProgress,

    /// <summary>All tasks done and the order closed.</summary>
    Completed,

    /// <summary>Cancelled without completing.</summary>
    Cancelled,
}