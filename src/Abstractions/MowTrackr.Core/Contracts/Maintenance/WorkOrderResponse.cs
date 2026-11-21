namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// A work order as returned by the API.
/// </summary>
/// <param name="Id">The work order key.</param>
/// <param name="MowerId">The mower key.</param>
/// <param name="MowerName">The mower name.</param>
/// <param name="ServiceTemplateId">The service template key, or <see langword="null"/> for a repair.</param>
/// <param name="TemplateName">The title: the service name, or what is being repaired.</param>
/// <param name="Status">The lifecycle state.</param>
/// <param name="ScheduledFor">The planned date.</param>
/// <param name="AssignedToUserId">The assigned technician.</param>
/// <param name="CompletedAt">When it was closed.</param>
/// <param name="EngineHoursAtStart">The hours when work started.</param>
/// <param name="EngineHoursAtService">The hours at completion.</param>
/// <param name="Notes">Free-form notes.</param>
/// <param name="Tasks">The service steps and check items.</param>
/// <param name="Kind">Service or repair.</param>
/// <param name="ParentWorkOrderId">For a repair raised from a check, the work order it was found on.</param>
/// <param name="PreCheckSubmittedAt">When the pre-check was submitted, if it has been.</param>
/// <param name="PostCheckSubmittedAt">When the post-check was submitted, if it has been.</param>
public sealed record WorkOrderResponse(
    Guid Id,
    Guid MowerId,
    string MowerName,
    Guid? ServiceTemplateId,
    string TemplateName,
    WorkOrderStatus Status,
    DateOnly? ScheduledFor,
    Guid? AssignedToUserId,
    DateTimeOffset? CompletedAt,
    decimal? EngineHoursAtStart,
    decimal? EngineHoursAtService,
    string? Notes,
    IReadOnlyList<WorkOrderTaskResponse> Tasks,
    WorkOrderKind Kind = WorkOrderKind.Service,
    Guid? ParentWorkOrderId = null,
    DateTimeOffset? PreCheckSubmittedAt = null,
    DateTimeOffset? PostCheckSubmittedAt = null);
