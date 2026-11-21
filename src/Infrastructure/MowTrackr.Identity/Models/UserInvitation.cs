namespace MowTrackr.Identity.Models;

/// <summary>
/// An administrator's invitation for someone to join MowTrackr. Only a hash of the link token is stored, so a
/// database leak cannot be used to accept invitations.
/// </summary>
public sealed class UserInvitation : EntityBase
{
    /// <summary>Gets or sets the invited email address as typed by the administrator.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the upper-cased email address used for lookups.</summary>
    public string NormalizedEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the role granted when the invitation is accepted.</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Gets or sets the language code of the invitation email and the new profile.</summary>
    public string Language { get; set; } = "en";

    /// <summary>Gets or sets the SHA-256 hash (hex) of the secret link token.</summary>
    [NotAudited]
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>Gets or sets the key of the administrator who sent the latest invitation email.</summary>
    public Guid InvitedByUserId { get; set; }

    /// <summary>Gets or sets the name of the administrator who sent the latest invitation email.</summary>
    public string InvitedByName { get; set; } = string.Empty;

    /// <summary>Gets or sets when the latest invitation email was sent.</summary>
    public DateTimeOffset InvitedAt { get; set; }

    /// <summary>Gets or sets when the invitation stops working.</summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>Gets or sets when the invitation was accepted.</summary>
    public DateTimeOffset? AcceptedAt { get; set; }

    /// <summary>Gets or sets when the invitation was revoked.</summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>Works out where the invitation is in its life at a point in time.</summary>
    /// <param name="now">The time to evaluate against.</param>
    /// <returns>The status.</returns>
    public InvitationStatus GetStatus(DateTimeOffset now)
    {
        if (AcceptedAt is not null)
        {
            return InvitationStatus.Accepted;
        }

        if (RevokedAt is not null)
        {
            return InvitationStatus.Revoked;
        }

        return now >= ExpiresAt ? InvitationStatus.Expired : InvitationStatus.Pending;
    }
}