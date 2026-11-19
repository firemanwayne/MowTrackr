using MowTrackr.Core.Constants;

namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// Payload for accepting an invitation and creating the account.
/// </summary>
public sealed class AcceptInvitationRequest
{
    /// <summary>Gets or sets the secret token from the invitation link.</summary>
    [Required]
    [StringLength(200)]
    public string Token { get; set; } = string.Empty;

    /// <summary>Gets or sets the name shown in the UI.</summary>
    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the password.</summary>
    [Required]
    [StringLength(PasswordPolicy.MaxLength, MinimumLength = PasswordPolicy.MinLength)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Gets or sets the initial UI language code; defaults to the invitation's language when omitted or unsupported.</summary>
    [StringLength(10)]
    public string? Language { get; set; }

    /// <summary>Gets or sets the initial colour theme; defaults to Light when omitted or unsupported.</summary>
    [StringLength(10)]
    public string? Theme { get; set; }
}