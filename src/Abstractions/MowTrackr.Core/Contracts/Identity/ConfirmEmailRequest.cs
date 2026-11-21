namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// Payload for confirming an email address from the emailed link.
/// </summary>
public sealed class ConfirmEmailRequest
{
    /// <summary>Gets or sets the user key from the link.</summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>Gets or sets the secret token from the link.</summary>
    [Required]
    [StringLength(2000)]
    public string Token { get; set; } = string.Empty;
}