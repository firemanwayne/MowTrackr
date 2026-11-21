namespace MowTrackr.Core.Models;

/// <summary>
/// Creates a repair work order for something found during a check.
/// </summary>
/// <param name="WorkOrderId">The work order the finding is on.</param>
/// <param name="TaskId">The check item that found the problem.</param>
/// <param name="UserId">The signed-in user.</param>
/// <param name="CanActOnAny">Whether the user may act on work orders assigned to others (managers and admins).</param>
public sealed record RepairCommand(Guid WorkOrderId, Guid TaskId, Guid UserId, bool CanActOnAny);
