namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// Payload for editing the signed-in user's profile.
/// </summary>
public sealed class UpdateProfileRequest
{
    /// <summary>Gets or sets the display name.</summary>
    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the phone number.</summary>
    [Phone]
    [StringLength(30)]
    public string? PhoneNumber { get; set; }
}