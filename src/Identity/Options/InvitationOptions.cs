namespace MowTrackr.Identity.Options;

/// <summary>
/// Invitation settings, bound from the "Invitations" configuration section.
/// </summary>
public sealed class InvitationOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Invitations";

    /// <summary>Gets or sets how many days an invitation link works for.</summary>
    public int ExpiryDays { get; set; } = 7;
}