namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// Filters for listing work orders.
/// </summary>
public sealed class WorkOrderListQuery
{
    /// <summary>Gets or sets a technician to filter by.</summary>
    public Guid? AssignedToUserId { get; set; }

    /// <summary>Gets or sets a status to filter by.</summary>
    public WorkOrderStatus? Status { get; set; }
}