namespace MowTrackr.Identity.Models;

/// <summary>
/// An application user (local or external-provider account).
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>Gets or sets the name shown in the UI.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the preferred UI language code ("en" or "es").</summary>
    public string Language { get; set; } = "en";

    /// <summary>Gets or sets the preferred colour theme ("Light" or "Dark").</summary>
    public string Theme { get; set; } = "Light";

    /// <summary>Gets or sets when the account was created.</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}