namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// Payload for asking for a new confirmation email.
/// </summary>
public sealed class ResendConfirmationRequest
{
    /// <summary>Gets or sets the account's email address.</summary>
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;
}