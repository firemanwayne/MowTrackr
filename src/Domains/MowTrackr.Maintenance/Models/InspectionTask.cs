namespace MowTrackr.Maintenance.Models;

/// <summary>
/// One item belonging to an <see cref="InspectionTemplate"/>.
/// </summary>
public sealed class InspectionTask : EntityBase
{
    /// <summary>Gets or sets the owning template key.</summary>
    public Guid InspectionTemplateId { get; set; }

    /// <summary>Gets or sets the item text.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the display order.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets a value indicating whether the item must be answered.</summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>Gets or sets the kind of answer.</summary>
    public CheckItemType ItemType { get; set; } = CheckItemType.YesNo;

    /// <summary>Gets or sets the choices for a choice item, separated by commas.</summary>
    public string? Choices { get; set; }

    /// <summary>Gets or sets the answers that mean something needs repair, separated by commas.</summary>
    public string? RepairOn { get; set; }
}
