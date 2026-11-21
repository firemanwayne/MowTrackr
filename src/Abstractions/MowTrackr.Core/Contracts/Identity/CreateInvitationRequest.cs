namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// Payload for inviting someone to MowTrackr.
/// </summary>
public sealed class CreateInvitationRequest
{
    /// <summary>Gets or sets the invitee's email address.</summary>
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the role the invitee receives on accepting.</summary>
    [Required]
    public string Role { get; set; } = Constants.AppRoles.Technician;

    /// <summary>Gets or sets the language of the invitation email and the new profile; defaults to English when omitted or unsupported.</summary>
    [StringLength(10)]
    public string? Language { get; set; }
}