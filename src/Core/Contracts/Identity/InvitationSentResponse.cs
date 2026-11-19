namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// Outcome of sending (or re-sending) an invitation. The invitation exists even when the email could not be sent,
/// so the administrator can send it again.
/// </summary>
/// <param name="Invitation">The invitation.</param>
/// <param name="EmailSent">Whether the email was handed to the mail service.</param>
public sealed record InvitationSentResponse(InvitationResponse Invitation, bool EmailSent);