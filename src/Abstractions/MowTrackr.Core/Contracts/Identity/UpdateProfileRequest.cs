using MowTrackr.Core.Constants;

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


    /// <summary>Gets or sets the preferred UI language code ("en" or "es").</summary>
    [Required]
    [StringLength(10)]
    public string Language { get; set; } = AppLanguages.Default;

    /// <summary>Gets or sets the preferred colour theme ("Light" or "Dark").</summary>
    [Required]
    [StringLength(10)]
    public string Theme { get; set; } = AppThemes.Default;
}