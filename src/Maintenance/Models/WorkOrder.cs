namespace MowTrackr.Maintenance.Models;

/// <summary>
/// A scheduled piece of maintenance on one mower, worked through a checklist.
/// </summary>
public sealed class WorkOrder : EntityBase
{
    /// <summary>Gets or sets the mower key.</summary>
    public Guid MowerId { get; set; }

    /// <summary>Gets or sets the mower name at scheduling time.</summary>
    public string MowerName { get; set; } = string.Empty;

    /// <summary>Gets or sets the source template key.</summary>
    public Guid ServiceTemplateId { get; set; }

    /// <summary>Gets or sets the template name at scheduling time.</summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>Gets or sets the lifecycle state.</summary>
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Scheduled;

    /// <summary>Gets or sets the planned date.</summary>
    public DateOnly? ScheduledFor { get; set; }

    /// <summary>Gets or sets the assigned technician (a user key; no foreign key across domains).</summary>
    public Guid? AssignedToUserId { get; set; }

    /// <summary>Gets or sets when the order was closed.</summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>Gets or sets the mower's engine hours at completion.</summary>
    public decimal? EngineHoursAtService { get; set; }

    /// <summary>Gets or sets free-form notes.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets the checklist.</summary>
    public List<WorkOrderTask> Tasks { get; set; } = [];
}