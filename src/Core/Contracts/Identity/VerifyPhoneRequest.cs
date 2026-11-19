namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// Payload for finishing phone verification with the code that was texted.
/// </summary>
public sealed class VerifyPhoneRequest
{
    /// <summary>Gets or sets the phone number the code was sent to.</summary>
    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the code from the text message.</summary>
    [Required]
    [StringLength(10, MinimumLength = 4)]
    public string Code { get; set; } = string.Empty;
}