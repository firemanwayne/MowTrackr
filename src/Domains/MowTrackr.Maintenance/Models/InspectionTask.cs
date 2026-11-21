namespace MowTrackr.Maintenance.Models;

/// <summary>
/// One checklist item belonging to an <see cref="InspectionTemplate"/>.
/// </summary>
public sealed class InspectionTask : EntityBase
{
    /// <summary>Gets or sets the owning template key.</summary>
    public Guid InspectionTemplateId { get; set; }

    /// <summary>Gets or sets the task description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the display order.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets a value indicating whether the task must be checked off to complete a work order.</summary>
    public bool IsRequired { get; set; } = true;
}
