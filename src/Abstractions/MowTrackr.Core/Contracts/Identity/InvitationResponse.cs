namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// An invitation as listed for administrators. The secret link token is never returned.
/// </summary>
/// <param name="Id">The invitation key.</param>
/// <param name="Email">The invited email address.</param>
/// <param name="Role">The role granted on acceptance.</param>
/// <param name="Language">The invitation language code.</param>
/// <param name="InvitedByName">Who sent the latest invitation email.</param>
/// <param name="InvitedAt">When the latest invitation email was sent.</param>
/// <param name="ExpiresAt">When the invitation stops working.</param>
/// <param name="AcceptedAt">When it was accepted, if it was.</param>
/// <param name="RevokedAt">When it was revoked, if it was.</param>
/// <param name="Status">Where the invitation is in its life.</param>
public sealed record InvitationResponse(
    Guid Id,
    string Email,
    string Role,
    string Language,
    string InvitedByName,
    DateTimeOffset InvitedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? RevokedAt,
    InvitationStatus Status);