namespace MowTrackr.Maintenance.Models;

/// <summary>
/// A service step or check item on a work order, copied from a template so later template edits do not alter history.
/// </summary>
public sealed class WorkOrderTask : EntityBase
{
    /// <summary>Gets or sets the owning work order key.</summary>
    public Guid WorkOrderId { get; set; }

    /// <summary>Gets or sets where the task came from: the inspection's name, or <see langword="null"/> for the service's own checklist.</summary>
    public string? Source { get; set; }

    /// <summary>Gets or sets which part of the work order the task belongs to.</summary>
    public TaskPhase Phase { get; set; } = TaskPhase.Service;

    /// <summary>Gets or sets the task description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the display order.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets a value indicating whether the task must be done to complete the work order.</summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>Gets or sets the kind of answer, for a check item.</summary>
    public CheckItemType ItemType { get; set; } = CheckItemType.YesNo;

    /// <summary>Gets or sets the choices for a choice item, separated by commas.</summary>
    public string? Choices { get; set; }

    /// <summary>Gets or sets the answers that mean something needs repair, separated by commas.</summary>
    public string? RepairOn { get; set; }

    /// <summary>Gets or sets the answer given to a check item.</summary>
    public string? Answer { get; set; }

    /// <summary>Gets or sets the repair work order raised for this finding, if any.</summary>
    public Guid? RepairWorkOrderId { get; set; }

    /// <summary>Gets or sets a value indicating whether the step has been checked off, or the check item answered.</summary>
    public bool IsComplete { get; set; }

    /// <summary>Gets or sets when the task was done.</summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>Gets or sets who did the task.</summary>
    public Guid? CompletedByUserId { get; set; }

    /// <summary>Gets or sets whether the step asks for a quantity.</summary>
    public DetailRequirement QuantityRequirement { get; set; } = DetailRequirement.None;

    /// <summary>Gets or sets whether the step asks for a part number.</summary>
    public DetailRequirement PartNumberRequirement { get; set; } = DetailRequirement.None;

    /// <summary>Gets or sets whether the step asks for notes.</summary>
    public DetailRequirement NotesRequirement { get; set; } = DetailRequirement.None;

    /// <summary>Gets or sets the quantity recorded. For an oil step it starts as the equipment's oil capacity.</summary>
    public decimal? Quantity { get; set; }

    /// <summary>Gets or sets the label of the quantity's unit, copied from the step or the equipment when the work order is made.</summary>
    public string? QuantityUnit { get; set; }

    /// <summary>Gets or sets the part number recorded.</summary>
    public string? PartNumber { get; set; }

    /// <summary>Gets or sets what to use for the step, copied when the work order is made (the oil type the equipment requires, for an oil step).</summary>
    public string? Product { get; set; }

    /// <summary>Gets or sets the notes recorded on the step.</summary>
    public string? StepNotes { get; set; }
}
