using MowTrackr.Core.Constants;

namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// Payload for creating a local account/profile.
/// </summary>
public sealed class RegisterRequest
{
    /// <summary>Gets or sets the email address (also the user name).</summary>
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the password.</summary>
    [Required]
    [StringLength(PasswordPolicy.MaxLength, MinimumLength = PasswordPolicy.MinLength)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Gets or sets the name shown in the UI.</summary>
    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the initial UI language code; defaults to English when omitted or unsupported.</summary>
    [StringLength(10)]
    public string? Language { get; set; }

    /// <summary>Gets or sets the initial colour theme; defaults to Light when omitted or unsupported.</summary>
    [StringLength(10)]
    public string? Theme { get; set; }
}