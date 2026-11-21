namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// One item of an inspection (check) template, as sent by the client.
/// </summary>
public sealed class InspectionTaskRequest
{
    /// <summary>Gets or sets the item text, for example "Oil level".</summary>
    [Required]
    [StringLength(300)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the item must be answered.</summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>Gets or sets the kind of answer.</summary>
    public CheckItemType ItemType { get; set; } = CheckItemType.YesNo;

    /// <summary>Gets or sets the choices for a <see cref="CheckItemType.Choice"/> item, separated by commas.</summary>
    [StringLength(300)]
    public string? Choices { get; set; }

    /// <summary>Gets or sets the answers that mean something needs repair, separated by commas ("No", or "None").</summary>
    [StringLength(300)]
    public string? RepairOn { get; set; }
}
