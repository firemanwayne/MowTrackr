namespace MowTrackr.Maintenance.Models;

/// <summary>
/// One checklist item belonging to a <see cref="ServiceTemplate"/>.
/// </summary>
public sealed class ServiceTask : EntityBase
{
    /// <summary>Gets or sets the owning template key.</summary>
    public Guid ServiceTemplateId { get; set; }

    /// <summary>Gets or sets the task description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the display order.</summary>
    public int SortOrder { get; set; }
}