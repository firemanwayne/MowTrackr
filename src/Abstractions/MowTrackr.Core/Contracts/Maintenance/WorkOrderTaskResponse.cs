namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// A service step or check item on a work order.
/// </summary>
/// <param name="Id">The task key.</param>
/// <param name="Source">Where the task came from: the inspection's name, or <see langword="null"/> for the service's own steps.</param>
/// <param name="Description">The task text.</param>
/// <param name="SortOrder">The display order.</param>
/// <param name="IsRequired">Whether the task must be done to complete the work order.</param>
/// <param name="IsComplete">Whether the step is checked off, or the check item answered.</param>
/// <param name="CompletedAt">When it was done.</param>
/// <param name="CompletedByUserId">Who did it.</param>
/// <param name="Phase">Service step, pre-check item or post-check item.</param>
/// <param name="ItemType">The kind of answer, for a check item.</param>
/// <param name="Choices">The choices, separated by commas, for a choice item.</param>
/// <param name="Answer">The answer given, for a check item.</param>
/// <param name="IsFinding">Whether the answer means something needs repair.</param>
/// <param name="RepairWorkOrderId">The repair work order raised for this finding, if any.</param>
public sealed record WorkOrderTaskResponse(
    Guid Id,
    string? Source,
    string Description,
    int SortOrder,
    bool IsRequired,
    bool IsComplete,
    DateTimeOffset? CompletedAt,
    Guid? CompletedByUserId,
    TaskPhase Phase = TaskPhase.Service,
    CheckItemType ItemType = CheckItemType.YesNo,
    string? Choices = null,
    string? Answer = null,
    bool IsFinding = false,
    Guid? RepairWorkOrderId = null);
