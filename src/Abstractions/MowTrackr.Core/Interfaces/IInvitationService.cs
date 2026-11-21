using MowTrackr.Core.Contracts.Identity;

namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Invitation-only onboarding: administrators invite, resend and revoke; invitees preview and accept.
/// </summary>
public interface IInvitationService
{
    /// <summary>Creates an invitation and emails the link.</summary>
    /// <param name="request">The invitee and role.</param>
    /// <param name="inviterId">The administrator sending it.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invitation and whether the email was sent.</returns>
    Task<Result<InvitationSentResponse>> CreateAsync(CreateInvitationRequest request, Guid inviterId, CancellationToken cancellationToken);

    /// <summary>Lists every invitation, newest first.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invitations.</returns>
    Task<Result<IReadOnlyList<InvitationResponse>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Issues a fresh link (and expiry) for an invitation that has not been accepted or revoked, and emails it.</summary>
    /// <param name="id">The invitation key.</param>
    /// <param name="inviterId">The administrator sending it.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invitation and whether the email was sent.</returns>
    Task<Result<InvitationSentResponse>> ResendAsync(Guid id, Guid inviterId, CancellationToken cancellationToken);

    /// <summary>Withdraws an invitation so its link stops working.</summary>
    /// <param name="id">The invitation key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The revoked invitation.</returns>
    Task<Result<InvitationResponse>> RevokeAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Checks a link token and describes the invitation (anonymous).</summary>
    /// <param name="token">The secret token from the link.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The preview, or not-found if the link is unknown, used, revoked or expired.</returns>
    Task<Result<InvitationPreviewResponse>> PreviewAsync(string token, CancellationToken cancellationToken);

    /// <summary>Accepts an invitation: creates the account with the invited role and signs it in (anonymous).</summary>
    /// <param name="request">The token and the new account's details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A signed-in session.</returns>
    Task<Result<AuthResponse>> AcceptAsync(AcceptInvitationRequest request, CancellationToken cancellationToken);
}