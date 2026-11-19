using MowTrackr.Api.Extensions;
using MowTrackr.Core.Contracts.Identity;

namespace MowTrackr.Api.Controllers;

/// <summary>
/// Invitation administration: send, list, resend and revoke. Admins only.
/// </summary>
/// <param name="invitations">The invitation service.</param>
[ApiController]
[Authorize(Roles = AppRoles.Admin)]
[Route(ApiRoutes.Invitations)]
public sealed class InvitationsController(IInvitationService invitations) : ControllerBase
{
    /// <summary>Lists every invitation, newest first.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invitations.</returns>
    [HttpGet]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        var result = await invitations.ListAsync(cancellationToken).ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Invites someone and emails them the link.</summary>
    /// <param name="request">The invitee and role.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invitation and whether the email was sent.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateInvitationRequest request, CancellationToken cancellationToken)
    {
        var result = await invitations.CreateAsync(request, User.GetUserId(), cancellationToken).ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Sends a fresh link for an invitation that is still open.</summary>
    /// <param name="id">The invitation key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invitation and whether the email was sent.</returns>
    [HttpPost("{id:guid}/resend")]
    public async Task<IActionResult> ResendAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await invitations.ResendAsync(id, User.GetUserId(), cancellationToken).ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Revokes an invitation so its link stops working.</summary>
    /// <param name="id">The invitation key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The revoked invitation.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RevokeAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await invitations.RevokeAsync(id, cancellationToken).ConfigureAwait(false);

        return result.ToActionResult();
    }
}