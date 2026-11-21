namespace MowTrackr.Maintenance.Models;

/// <summary>
/// A checklist item on a work order, copied from a template so later template edits do not alter history.
/// </summary>
public sealed class WorkOrderTask : EntityBase
{
    /// <summary>Gets or sets the owning work order key.</summary>
    public Guid WorkOrderId { get; set; }

    /// <summary>Gets or sets where the task came from: the inspection's name, or <see langword="null"/> for the service's own checklist.</summary>
    public string? Source { get; set; }

    /// <summary>Gets or sets the task description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the display order.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets a value indicating whether the task must be checked off to complete the work order.</summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the task has been checked off.</summary>
    public bool IsComplete { get; set; }

    /// <summary>Gets or sets when the task was checked off.</summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>Gets or sets who checked the task off.</summary>
    public Guid? CompletedByUserId { get; set; }
}