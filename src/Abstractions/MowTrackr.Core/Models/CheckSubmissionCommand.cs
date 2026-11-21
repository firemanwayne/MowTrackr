using MowTrackr.Core.Contracts.Maintenance;

namespace MowTrackr.Core.Models;

/// <summary>
/// Submits the pre-check or post-check of a work order.
/// </summary>
/// <param name="WorkOrderId">The work order.</param>
/// <param name="Timing">Which check is being submitted.</param>
/// <param name="Answers">The answers.</param>
/// <param name="UserId">The signed-in user.</param>
/// <param name="CanActOnAny">Whether the user may act on work orders assigned to others (managers and admins).</param>
public sealed record CheckSubmissionCommand(Guid WorkOrderId, InspectionTiming Timing, IReadOnlyList<CheckAnswerRequest> Answers, Guid UserId, bool CanActOnAny);
