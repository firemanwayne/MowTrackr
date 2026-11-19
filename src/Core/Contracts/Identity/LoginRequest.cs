namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// Payload for a local sign-in.
/// </summary>
public sealed class LoginRequest
{
    /// <summary>Gets or sets the email address.</summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the password.</summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}