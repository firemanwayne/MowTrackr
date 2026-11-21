namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// What the invitee sees before accepting: enough to confirm it is theirs, nothing more.
/// </summary>
/// <param name="Email">The invited email address (becomes the account's email).</param>
/// <param name="InvitedByName">Who sent the invitation.</param>
/// <param name="Role">The role they will receive.</param>
/// <param name="Language">The invitation language code.</param>
/// <param name="ExpiresAt">When the invitation stops working.</param>
public sealed record InvitationPreviewResponse(
    string Email,
    string InvitedByName,
    string Role,
    string Language,
    DateTimeOffset ExpiresAt);