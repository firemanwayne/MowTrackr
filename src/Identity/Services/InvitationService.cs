using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using MowTrackr.Core.Constants;
using MowTrackr.Core.Options;
using MowTrackr.Identity.Extensions;
using MowTrackr.Identity.Interfaces;
using MowTrackr.Identity.Models;
using MowTrackr.Identity.Options;

namespace MowTrackr.Identity.Services;

/// <summary>
/// Default <see cref="IInvitationService"/>.
/// </summary>
/// <param name="invitations">The invitation store.</param>
/// <param name="userManager">The user manager.</param>
/// <param name="tokens">The JWT issuer.</param>
/// <param name="email">The email sender.</param>
/// <param name="client">Where the client lives (for the accept link).</param>
/// <param name="options">The invitation settings.</param>
/// <param name="time">The clock.</param>
/// <param name="logger">The logger.</param>
internal sealed class InvitationService(
    IRepository<UserInvitation> invitations,
    UserManager<ApplicationUser> userManager,
    ITokenService tokens,
    IEmailService email,
    IOptions<ClientOptions> client,
    IOptions<InvitationOptions> options,
    TimeProvider time,
    ILogger<InvitationService> logger) : IInvitationService
{
    private const string InvalidInvitationMessage = "This invitation is no longer valid. Ask an administrator to send you a new one.";

    private const string UnknownInviterName = "An administrator";

    private const int TokenBytes = 32;

    /// <inheritdoc />
    public async Task<Result<InvitationSentResponse>> CreateAsync(CreateInvitationRequest request, Guid inviterId, CancellationToken cancellationToken)
    {
        var role = AppRoles.All.FirstOrDefault(r => string.Equals(r, request.Role, StringComparison.OrdinalIgnoreCase));

        if (role is null)
        {
            return Result<InvitationSentResponse>.Invalid($"Unknown role '{request.Role}'.");
        }

        var address = request.Email.Trim();

        var normalized = userManager.NormalizeEmail(address) ?? address.ToUpperInvariant();

        if (await userManager.FindByEmailAsync(address).ConfigureAwait(false) is not null)
        {
            return Result<InvitationSentResponse>.Conflict("A user with that email already exists.");
        }

        var now = time.GetUtcNow();

        var open = await invitations.ListAsync(
            new QuerySpec<UserInvitation> { Filter = i => i.NormalizedEmail == normalized && i.AcceptedAt == null && i.RevokedAt == null && i.ExpiresAt > now },
            cancellationToken).ConfigureAwait(false);

        if (open.Count > 0)
        {
            return Result<InvitationSentResponse>.Conflict("There is already a pending invitation for that email. Resend or revoke it instead.");
        }

        var token = NewToken();

        var invitation = new UserInvitation
        {
            Email = address,
            NormalizedEmail = normalized,
            Role = role,
            Language = AppLanguages.IsSupported(request.Language) ? request.Language!.ToLowerInvariant() : AppLanguages.Default,
        };

        await StampAsync(invitation, token, inviterId).ConfigureAwait(false);

        await invitations.AddAsync(invitation, cancellationToken).ConfigureAwait(false);

        await invitations.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Invitation {InvitationId} created for role {Role}", invitation.Id, invitation.Role);

        var sent = await SendAsync(invitation, token, cancellationToken).ConfigureAwait(false);

        return Result<InvitationSentResponse>.Ok(new InvitationSentResponse(ToResponse(invitation), sent));
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<InvitationResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var all = await invitations.ListAsync(null, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<InvitationResponse> responses = all.OrderByDescending(i => i.InvitedAt).Select(ToResponse).ToList();

        return Result<IReadOnlyList<InvitationResponse>>.Ok(responses);
    }

    /// <inheritdoc />
    public async Task<Result<InvitationSentResponse>> ResendAsync(Guid id, Guid inviterId, CancellationToken cancellationToken)
    {
        var invitation = await invitations.GetByIdAsync(id, new QuerySpec<UserInvitation> { Track = true }, cancellationToken).ConfigureAwait(false);

        if (invitation is null)
        {
            return Result<InvitationSentResponse>.NotFound("Invitation not found.");
        }

        var status = invitation.GetStatus(time.GetUtcNow());

        if (status == InvitationStatus.Accepted)
        {
            return Result<InvitationSentResponse>.Conflict("That invitation was already accepted.");
        }

        if (status == InvitationStatus.Revoked)
        {
            return Result<InvitationSentResponse>.Conflict("That invitation was revoked. Create a new one instead.");
        }

        var token = NewToken();

        await StampAsync(invitation, token, inviterId).ConfigureAwait(false);

        await invitations.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Invitation {InvitationId} re-sent", invitation.Id);

        var sent = await SendAsync(invitation, token, cancellationToken).ConfigureAwait(false);

        return Result<InvitationSentResponse>.Ok(new InvitationSentResponse(ToResponse(invitation), sent));
    }

    /// <inheritdoc />
    public async Task<Result<InvitationResponse>> RevokeAsync(Guid id, CancellationToken cancellationToken)
    {
        var invitation = await invitations.GetByIdAsync(id, new QuerySpec<UserInvitation> { Track = true }, cancellationToken).ConfigureAwait(false);

        if (invitation is null)
        {
            return Result<InvitationResponse>.NotFound("Invitation not found.");
        }

        if (invitation.AcceptedAt is not null)
        {
            return Result<InvitationResponse>.Conflict("That invitation was already accepted. Manage the user instead.");
        }

        if (invitation.RevokedAt is null)
        {
            var now = time.GetUtcNow();

            invitation.RevokedAt = now;

            invitation.UpdatedAt = now;

            await invitations.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            logger.LogInformation("Invitation {InvitationId} revoked", invitation.Id);
        }

        return Result<InvitationResponse>.Ok(ToResponse(invitation));
    }

    /// <inheritdoc />
    public async Task<Result<InvitationPreviewResponse>> PreviewAsync(string token, CancellationToken cancellationToken)
    {
        var invitation = await FindPendingAsync(token, track: false, cancellationToken).ConfigureAwait(false);

        if (invitation is null)
        {
            return Result<InvitationPreviewResponse>.NotFound(InvalidInvitationMessage);
        }

        return Result<InvitationPreviewResponse>.Ok(new InvitationPreviewResponse(invitation.Email, invitation.InvitedByName, invitation.Role, invitation.Language, invitation.ExpiresAt));
    }

    /// <inheritdoc />
    public async Task<Result<AuthResponse>> AcceptAsync(AcceptInvitationRequest request, CancellationToken cancellationToken)
    {
        var invitation = await FindPendingAsync(request.Token, track: true, cancellationToken).ConfigureAwait(false);

        if (invitation is null)
        {
            return Result<AuthResponse>.NotFound(InvalidInvitationMessage);
        }

        if (await userManager.FindByEmailAsync(invitation.Email).ConfigureAwait(false) is not null)
        {
            return Result<AuthResponse>.Conflict("An account with that email already exists. Sign in instead.");
        }

        var user = new ApplicationUser
        {
            UserName = invitation.Email,
            Email = invitation.Email,
            EmailConfirmed = true,
            DisplayName = request.DisplayName.Trim(),
            Language = AppLanguages.IsSupported(request.Language) ? request.Language!.ToLowerInvariant() : invitation.Language,
            Theme = AppThemes.IsSupported(request.Theme) ? AppThemes.All.First(t => string.Equals(t, request.Theme, StringComparison.OrdinalIgnoreCase)) : AppThemes.Default,
        };

        var created = await userManager.CreateAsync(user, request.Password).ConfigureAwait(false);

        if (!created.Succeeded)
        {
            return Result<AuthResponse>.Invalid(string.Join(" ", created.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(user, invitation.Role).ConfigureAwait(false);

        var now = time.GetUtcNow();

        invitation.AcceptedAt = now;

        invitation.UpdatedAt = now;

        await invitations.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Invitation {InvitationId} accepted by user {UserId}", invitation.Id, user.Id);

        var roles = (await userManager.GetRolesAsync(user).ConfigureAwait(false)).ToList();

        var (token, expiresAt) = tokens.CreateToken(user, roles);

        return Result<AuthResponse>.Ok(new AuthResponse(token, expiresAt, user.ToResponse(roles)));
    }

    /// <summary>Hashes a link token for storage and lookup.</summary>
    /// <param name="token">The secret token.</param>
    /// <returns>The SHA-256 hash as upper-case hex.</returns>
    internal static string HashToken(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    /// <summary>Creates a new random, URL-safe link token.</summary>
    /// <returns>The token.</returns>
    private static string NewToken()
    {
        return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(TokenBytes));
    }

    /// <summary>Maps an invitation to its response.</summary>
    /// <param name="invitation">The invitation.</param>
    /// <returns>The response.</returns>
    private InvitationResponse ToResponse(UserInvitation invitation)
    {
        return new InvitationResponse(
            invitation.Id,
            invitation.Email,
            invitation.Role,
            invitation.Language,
            invitation.InvitedByName,
            invitation.InvitedAt,
            invitation.ExpiresAt,
            invitation.AcceptedAt,
            invitation.RevokedAt,
            invitation.GetStatus(time.GetUtcNow()));
    }

    /// <summary>Records a fresh token, sender and expiry on an invitation.</summary>
    /// <param name="invitation">The invitation to update.</param>
    /// <param name="token">The new secret token.</param>
    /// <param name="inviterId">The administrator sending it.</param>
    /// <returns>A task.</returns>
    private async Task StampAsync(UserInvitation invitation, string token, Guid inviterId)
    {
        var inviter = await userManager.FindByIdAsync(inviterId.ToString()).ConfigureAwait(false);

        var now = time.GetUtcNow();

        invitation.TokenHash = HashToken(token);

        invitation.InvitedByUserId = inviterId;

        invitation.InvitedByName = string.IsNullOrWhiteSpace(inviter?.DisplayName) ? inviter?.Email ?? UnknownInviterName : inviter.DisplayName;

        invitation.InvitedAt = now;

        invitation.ExpiresAt = now.AddDays(options.Value.ExpiryDays);

        invitation.UpdatedAt = now;
    }

    /// <summary>Finds the invitation for a link token, if it can still be accepted.</summary>
    /// <param name="token">The secret token from the link.</param>
    /// <param name="track">Whether the entity will be modified.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The pending invitation, or <see langword="null"/>.</returns>
    private async Task<UserInvitation?> FindPendingAsync(string token, bool track, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var hash = HashToken(token.Trim());

        var matches = await invitations.ListAsync(new QuerySpec<UserInvitation> { Filter = i => i.TokenHash == hash, Track = track }, cancellationToken).ConfigureAwait(false);

        var invitation = matches.FirstOrDefault();

        return invitation is not null && invitation.GetStatus(time.GetUtcNow()) == InvitationStatus.Pending ? invitation : null;
    }

    /// <summary>Emails an invitation link; a failure is logged and reported, never thrown, so the invitation is kept.</summary>
    /// <param name="invitation">The invitation.</param>
    /// <param name="token">The secret token for the link.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> if the mail service accepted the message.</returns>
    private async Task<bool> SendAsync(UserInvitation invitation, string token, CancellationToken cancellationToken)
    {
        var url = $"{client.Value.BaseUrl.TrimEnd('/')}/accept-invitation?token={Uri.EscapeDataString(token)}";

        var message = new InvitationEmail(invitation.Email, invitation.InvitedByName, url, invitation.ExpiresAt, invitation.Language);

        try
        {
            return await email.SendInvitationAsync(message, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to send the email for invitation {InvitationId}", invitation.Id);

            return false;
        }
    }
}