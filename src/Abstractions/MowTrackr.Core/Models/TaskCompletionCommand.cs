using MowTrackr.Core.Contracts.Maintenance;

namespace MowTrackr.Core.Models;

/// <summary>
/// Request to check or uncheck a work order task on behalf of a user.
/// </summary>
/// <param name="WorkOrderId">The work order key.</param>
/// <param name="TaskId">The task key.</param>
/// <param name="IsComplete">The desired state.</param>
/// <param name="UserId">The acting user.</param>
/// <param name="Details">The details to save with the step, or <see langword="null"/> to leave them as they are.</param>
/// <param name="CanActOnAny">Whether the user may act on work orders not assigned to them (managers/admins).</param>
public sealed record TaskCompletionCommand(Guid WorkOrderId, Guid TaskId, bool IsComplete, Guid UserId, bool CanActOnAny, TaskDetailsRequest? Details = null);