namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// One checklist item on a work order.
/// </summary>
/// <param name="Id">The task key.</param>
/// <param name="Description">The task description.</param>
/// <param name="SortOrder">The display order.</param>
/// <param name="IsRequired">Whether it must be checked off to complete the work order.</param>
/// <param name="IsComplete">Whether it has been checked off.</param>
/// <param name="CompletedAt">When it was checked off.</param>
/// <param name="CompletedByUserId">Who checked it off.</param>
public sealed record WorkOrderTaskResponse(
    Guid Id,
    string Description,
    int SortOrder,
    bool IsRequired,
    bool IsComplete,
    DateTimeOffset? CompletedAt,
    Guid? CompletedByUserId);