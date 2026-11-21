namespace MowTrackr.Data.Models;

/// <summary>
/// One row of the change history: a single create, update or delete of one entity, saved in the same transaction as the
/// change itself. It deliberately is not an <see cref="EntityBase"/>, so recording a change never records itself.
/// </summary>
internal sealed class ChangeRecord
{
    /// <summary>Gets or sets the primary key.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the CLR name of the entity that changed, such as <c>Mower</c>.</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Gets or sets the key of the entity that changed.</summary>
    public Guid EntityId { get; set; }

    /// <summary>Gets or sets what happened.</summary>
    public ChangeAction Action { get; set; }

    /// <summary>Gets or sets when the change was saved.</summary>
    public DateTimeOffset ChangedAt { get; set; }

    /// <summary>Gets or sets the signed-in user who made the change, or <see langword="null"/> when there was none.</summary>
    public Guid? ChangedBy { get; set; }

    /// <summary>
    /// Gets or sets the changed values as JSON, <c>{"Name":{"old":"A","new":"B"}}</c>, or <see langword="null"/> when none
    /// were recorded. Properties marked <see cref="NotAuditedAttribute"/> never appear.
    /// </summary>
    public string? Changes { get; set; }
}
