namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// Payload for scheduling a work order from a template.
/// </summary>
public sealed class WorkOrderCreateRequest
{
    /// <summary>Gets or sets the mower to service.</summary>
    [Required]
    public Guid MowerId { get; set; }

    /// <summary>Gets or sets the template whose checklist is copied onto the work order.</summary>
    [Required]
    public Guid ServiceTemplateId { get; set; }

    /// <summary>Gets or sets the planned date.</summary>
    public DateOnly? ScheduledFor { get; set; }

    /// <summary>Gets or sets the technician assigned to the work.</summary>
    public Guid? AssignedToUserId { get; set; }

    /// <summary>Gets or sets optional notes.</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}