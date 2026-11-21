namespace MowTrackr.Application.Interfaces;

/// <summary>
/// Client for the invitation administration endpoints (Admin only).
/// </summary>
internal interface IInvitationApiService
{
    /// <summary>Lists every invitation, newest first.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invitations.</returns>
    Task<ApiResult<List<InvitationResponse>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Invites someone and emails them the link.</summary>
    /// <param name="request">The invitee and role.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invitation and whether the email was sent.</returns>
    Task<ApiResult<InvitationSentResponse>> CreateAsync(CreateInvitationRequest request, CancellationToken cancellationToken);

    /// <summary>Sends a fresh link for an invitation that is still open.</summary>
    /// <param name="id">The invitation key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invitation and whether the email was sent.</returns>
    Task<ApiResult<InvitationSentResponse>> ResendAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Revokes an invitation so its link stops working.</summary>
    /// <param name="id">The invitation key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The revoked invitation.</returns>
    Task<ApiResult<InvitationResponse>> RevokeAsync(Guid id, CancellationToken cancellationToken);
}