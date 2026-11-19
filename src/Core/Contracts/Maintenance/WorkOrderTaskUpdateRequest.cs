namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// Payload for checking or unchecking a work order task.
/// </summary>
public sealed class WorkOrderTaskUpdateRequest
{
    /// <summary>Gets or sets a value indicating whether the task is complete.</summary>
    public bool IsComplete { get; set; }
}