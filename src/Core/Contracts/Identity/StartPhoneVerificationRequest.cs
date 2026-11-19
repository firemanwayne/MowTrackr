namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// Payload for starting phone verification: the number to text a code to.
/// </summary>
public sealed class StartPhoneVerificationRequest
{
    /// <summary>Gets or sets the phone number in international (E.164) format, for example +15551234567.</summary>
    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
}