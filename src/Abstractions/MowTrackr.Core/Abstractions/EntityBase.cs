namespace MowTrackr.Core.Abstractions;

/// <summary>
/// Base class for every persisted entity: a GUID key plus created/updated audit stamps. <see cref="UpdatedAt"/> and <see cref="UpdatedBy"/> are
/// filled in automatically every time the row is saved.
/// </summary>
public abstract class EntityBase
{
    /// <summary>Gets or sets the primary key.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets when the row was created.</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets or sets when the row was last modified.</summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the key of the user who last created or changed the row, or <see langword="null"/> when the change was
    /// not made by a signed-in user (for example a startup migration or an anonymous sign-up).
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>Records when the row was changed and by whom. Called automatically on every save.</summary>
    /// <param name="at">When the change was made.</param>
    /// <param name="by">The signed-in user, or <see langword="null"/> when there is none.</param>
    public void MarkUpdated(DateTimeOffset at, Guid? by)
    {
        UpdatedAt = at;

        UpdatedBy = by;
    }
}