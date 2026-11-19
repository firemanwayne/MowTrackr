namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// A work order as returned by the API.
/// </summary>
/// <param name="Id">The work order key.</param>
/// <param name="MowerId">The mower key.</param>
/// <param name="MowerName">The mower name.</param>
/// <param name="ServiceTemplateId">The source template key.</param>
/// <param name="TemplateName">The template name at scheduling time.</param>
/// <param name="Status">The lifecycle state.</param>
/// <param name="ScheduledFor">The planned date.</param>
/// <param name="AssignedToUserId">The assigned technician.</param>
/// <param name="CompletedAt">When the order was closed.</param>
/// <param name="EngineHoursAtStart">The mower's engine hours when the work started (read-only).</param>
/// <param name="EngineHoursAtService">Engine hours recorded at completion.</param>
/// <param name="Notes">Free-form notes.</param>
/// <param name="Tasks">The checklist.</param>
public sealed record WorkOrderResponse(
    Guid Id,
    Guid MowerId,
    string MowerName,
    Guid ServiceTemplateId,
    string TemplateName,
    WorkOrderStatus Status,
    DateOnly? ScheduledFor,
    Guid? AssignedToUserId,
    DateTimeOffset? CompletedAt,
    decimal? EngineHoursAtStart,
    decimal? EngineHoursAtService,
    string? Notes,
    IReadOnlyList<WorkOrderTaskResponse> Tasks);