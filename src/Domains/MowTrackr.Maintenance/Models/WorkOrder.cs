namespace MowTrackr.Maintenance.Models;

/// <summary>
/// A scheduled piece of maintenance or a repair on one mower, worked through a checklist.
/// </summary>
public sealed class WorkOrder : EntityBase
{
    /// <summary>Gets or sets the mower key.</summary>
    public Guid MowerId { get; set; }

    /// <summary>Gets or sets the mower name at scheduling time.</summary>
    public string MowerName { get; set; } = string.Empty;

    /// <summary>Gets or sets what the work order is for.</summary>
    public WorkOrderKind Kind { get; set; } = WorkOrderKind.Service;

    /// <summary>Gets or sets the source template key, or <see langword="null"/> for a repair.</summary>
    public Guid? ServiceTemplateId { get; set; }

    /// <summary>Gets or sets the title at scheduling time: the service name, or what is being repaired.</summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>Gets or sets, for a repair raised from a check, the work order it was found on.</summary>
    public Guid? ParentWorkOrderId { get; set; }

    /// <summary>Gets or sets the lifecycle state.</summary>
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Scheduled;

    /// <summary>Gets or sets the planned date.</summary>
    public DateOnly? ScheduledFor { get; set; }

    /// <summary>Gets or sets the assigned technician (a user key; no foreign key across domains).</summary>
    public Guid? AssignedToUserId { get; set; }

    /// <summary>Gets or sets when the order was closed.</summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>Gets or sets the mower's engine hours when the work was scheduled (refreshed when work starts); read-only to users.</summary>
    public decimal? EngineHoursAtStart { get; set; }

    /// <summary>Gets or sets the mower's engine hours at completion.</summary>
    public decimal? EngineHoursAtService { get; set; }

    /// <summary>Gets or sets when the pre-check was submitted; service steps unlock once it has been.</summary>
    public DateTimeOffset? PreCheckSubmittedAt { get; set; }

    /// <summary>Gets or sets when the post-check was submitted; the order cannot be completed without it.</summary>
    public DateTimeOffset? PostCheckSubmittedAt { get; set; }

    /// <summary>Gets or sets free-form notes.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets the service steps and check items.</summary>
    public List<WorkOrderTask> Tasks { get; set; } = [];
}
