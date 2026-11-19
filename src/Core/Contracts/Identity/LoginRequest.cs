using MowTrackr.Core.Constants;

namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// Payload for a local sign-in.
/// </summary>
public sealed class LoginRequest
{
    /// <summary>Gets or sets the email address.</summary>
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the password.</summary>
    [Required]
    [StringLength(PasswordPolicy.MaxLength)]
    public string Password { get; set; } = string.Empty;
}