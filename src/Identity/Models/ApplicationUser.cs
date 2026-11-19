namespace MowTrackr.Identity.Models;

/// <summary>
/// An application user (local or external-provider account).
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>Gets or sets the name shown in the UI.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets when the account was created.</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}