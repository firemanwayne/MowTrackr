using Microsoft.AspNetCore.WebUtilities;

using MowTrackr.Core.Constants;
using MowTrackr.Core.Options;
using MowTrackr.Identity.Extensions;
using MowTrackr.Identity.Interfaces;
using MowTrackr.Identity.Models;

using System.Text;

namespace MowTrackr.Identity.Services;

/// <summary>
/// Default <see cref="IAuthService"/> built on ASP.NET Core Identity's <see cref="UserManager{TUser}"/>.
/// </summary>
/// <param name="userManager">The user manager.</param>
/// <param name="tokens">The JWT issuer.</param>
/// <param name="invitations">The invitation store (new external sign-ins need an invitation).</param>
/// <param name="email">The email sender (confirmation links).</param>
/// <param name="client">Where the client lives (for the confirmation link).</param>
/// <param name="time">The clock.</param>
/// <param name="logger">The logger.</param>
internal sealed partial class AuthService(
    UserManager<ApplicationUser> userManager,
    ITokenService tokens,
    IRepository<UserInvitation> invitations,
    IEmailService email,
    IOptions<ClientOptions> client,
    TimeProvider time,
    ILogger<AuthService> logger) : IAuthService
{
    private const string InvitationRequiredMessage = "MowTrackr is invitation only. Ask an administrator to invite you.";

    private const string BadCredentialsMessage = "Invalid email or password, or this account is temporarily locked. Try again in a few minutes.";

    private const string EmailNotConfirmedMessage = "Confirm your email address before signing in. Check your inbox for the link, or request a new one.";

    private const string InvalidConfirmationMessage = "This confirmation link is not valid or has expired. Request a new one from the sign-in page.";

    /// <inheritdoc />
    public async Task<Result<RegistrationStatusResponse>> GetRegistrationStatusAsync(CancellationToken cancellationToken)
    {
        var anyUsers = await userManager
            .Users
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<RegistrationStatusResponse>.Ok(new RegistrationStatusResponse(!anyUsers));
    }

    /// <inheritdoc />
    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (await userManager.Users.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return Result<RegisterResponse>.Forbidden(InvitationRequiredMessage);
        }

        var existing = await userManager
            .FindByEmailAsync(request.Email)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            return Result<RegisterResponse>.Conflict("An account with that email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName.Trim(),
            Language = AppLanguages.IsSupported(request.Language) ? request.Language!.ToLowerInvariant() : AppLanguages.Default,
            Theme = AppThemes.IsSupported(request.Theme) ? AppThemes.All.First(t => string.Equals(t, request.Theme, StringComparison.OrdinalIgnoreCase)) : AppThemes.Default,
        };

        var created = await userManager.CreateAsync(user, request.Password).ConfigureAwait(false);

        if (!created.Succeeded)
        {
            return Result<RegisterResponse>.Invalid(string.Join(" ", created.Errors.Select(e => e.Description)));
        }

        await AssignInitialRoleAsync(user)
            .ConfigureAwait(false);

        logger.LogInformation("Registered user {UserId}; confirmation email pending", user.Id);

        var sent = await SendConfirmationAsync(user, cancellationToken).ConfigureAwait(false);

        return Result<RegisterResponse>.Ok(new RegisterResponse(request.Email, sent));
    }

    /// <inheritdoc />
    public async Task<Result<MessageResponse>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager
            .FindByIdAsync(request.UserId.ToString())
            .ConfigureAwait(false);

        if (user is null)
        {
            return Result<MessageResponse>.Invalid(InvalidConfirmationMessage);
        }

        if (user.EmailConfirmed)
        {
            return Result<MessageResponse>.Ok(new MessageResponse("Your email address is already confirmed."));
        }

        string token;

        try
        {
            token = Encoding
                .UTF8
                .GetString(WebEncoders.Base64UrlDecode(request.Token));
        }
        catch (FormatException)
        {
            return Result<MessageResponse>.Invalid(InvalidConfirmationMessage);
        }

        var confirmed = await userManager
            .ConfirmEmailAsync(user, token)
            .ConfigureAwait(false);

        if (!confirmed.Succeeded)
        {
            return Result<MessageResponse>.Invalid(InvalidConfirmationMessage);
        }

        LogEmailConfirmationInformation(user.Id);

        return Result<MessageResponse>.Ok(new MessageResponse("Your email address is confirmed. You can sign in now."));
    }



    /// <inheritdoc />
    public async Task<Result<MessageResponse>> ResendConfirmationAsync(ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager
            .FindByEmailAsync(request.Email)
            .ConfigureAwait(false);

        if (user is not null && !user.EmailConfirmed)
        {
            await SendConfirmationAsync(user, cancellationToken)
                .ConfigureAwait(false);
        }

        // The same answer whether or not the account exists, so this cannot be used to discover accounts.
        return Result<MessageResponse>.Ok(new MessageResponse("If that account still needs confirming, we sent a new link."));
    }

    /// <inheritdoc />
    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager
            .FindByEmailAsync(request.Email)
            .ConfigureAwait(false);

        // One message for unknown account, wrong password and locked account, so none of them can be told apart.
        if (user is null || await userManager.IsLockedOutAsync(user).ConfigureAwait(false))
        {
            return Result<AuthResponse>.Unauthorized(BadCredentialsMessage);
        }

        var valid = await userManager
            .CheckPasswordAsync(user, request.Password)
            .ConfigureAwait(false);

        if (!valid)
        {
            await userManager
                .AccessFailedAsync(user)
                .ConfigureAwait(false);

            return Result<AuthResponse>.Unauthorized(BadCredentialsMessage);
        }

        await userManager
            .ResetAccessFailedCountAsync(user)
            .ConfigureAwait(false);

        // Only someone who knows the password learns the account is unconfirmed.
        return !user.EmailConfirmed
            ? Result<AuthResponse>.Forbidden(EmailNotConfirmedMessage)
            : await CreateSessionAsync(user).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<AuthResponse>> ExternalLoginAsync(ExternalLoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager
            .FindByLoginAsync(command.Provider, command.ProviderKey)
            .ConfigureAwait(false);

        if (user is null)
        {
            var linked = await LinkOrCreateAsync(command, cancellationToken)
                .ConfigureAwait(false);

            if (!linked.IsSuccess || linked.Value is null)
            {
                return Result<AuthResponse>.Invalid(linked.Error ?? "Unable to sign in with that provider.");
            }

            user = linked.Value;
        }

        return await CreateSessionAsync(user)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Links the provider login to an existing same-email user, or creates a new user. A new user needs a pending
    /// invitation for that email (which also sets their role); only the very first account needs none.
    /// </summary>
    /// <param name="command">The provider identity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user, or an error.</returns>
    private async Task<Result<ApplicationUser>> LinkOrCreateAsync(ExternalLoginCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return Result<ApplicationUser>.Invalid($"{command.Provider} did not share an email address, which MowTrackr requires.");
        }

        var user = await userManager
            .FindByEmailAsync(command.Email)
            .ConfigureAwait(false);

        // Never attach a provider identity to an account whose email nobody has verified: whoever controls that
        // address at the provider (or merely claims it) would take the account over.
        if (user is not null && !user.EmailConfirmed)
        {
            return Result<ApplicationUser>.Forbidden("That account's email address is not confirmed yet. Sign in with your password and confirm it first.");
        }

        if (user is null)
        {
            var firstAccount = !await userManager
                .Users
                .AnyAsync(cancellationToken)
                .ConfigureAwait(false);

            var invitation = firstAccount ? null : await FindPendingInvitationAsync(command.Email, cancellationToken).ConfigureAwait(false);

            if (!firstAccount && invitation is null)
            {
                return Result<ApplicationUser>.Forbidden(InvitationRequiredMessage);
            }

            user = new ApplicationUser
            {
                UserName = command.Email,
                Email = command.Email,
                EmailConfirmed = true,
                DisplayName = string.IsNullOrWhiteSpace(command.DisplayName) ? command.Email : command.DisplayName.Trim(),
                Language = invitation?.Language ?? AppLanguages.Default,
            };

            var created = await userManager
                .CreateAsync(user)
                .ConfigureAwait(false);

            if (!created.Succeeded)
            {
                return Result<ApplicationUser>.Invalid(string.Join(" ", created.Errors.Select(e => e.Description)));
            }

            if (invitation is null)
            {
                await AssignInitialRoleAsync(user).ConfigureAwait(false);
            }
            else
            {
                await userManager
                    .AddToRoleAsync(user, invitation.Role)
                    .ConfigureAwait(false);

                invitation.AcceptedAt = time.GetUtcNow();

                invitation.UpdatedAt = invitation.AcceptedAt.Value;

                await invitations
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        var login = new UserLoginInfo(command.Provider, command.ProviderKey, command.Provider);

        var added = await userManager
            .AddLoginAsync(user, login)
            .ConfigureAwait(false);

        if (!added.Succeeded)
        {
            return Result<ApplicationUser>.Invalid(string.Join(" ", added.Errors.Select(e => e.Description)));
        }

        LogProviderInformation(command.Provider, user.Id);

        return Result<ApplicationUser>.Ok(user);
    }

    /// <summary>Emails a confirmation link; a failure is logged and reported, never thrown, so registration is kept.</summary>
    /// <param name="user">The user to confirm.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> if the mail service accepted the message.</returns>
    private async Task<bool> SendConfirmationAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var token = await userManager
            .GenerateEmailConfirmationTokenAsync(user)
            .ConfigureAwait(false);

        var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var url = $"{client.Value.BaseUrl.TrimEnd('/')}/confirm-email?userId={user.Id}&token={encoded}";

        var message = new EmailConfirmationEmail(user.Email ?? string.Empty, user.DisplayName, url, user.Language);

        try
        {
            return await email
                .SendEmailConfirmationAsync(message, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to send the confirmation email for user {UserId}", user.Id);

            return false;
        }
    }

    /// <summary>Finds the open invitation for an email address, tracked so it can be marked accepted.</summary>
    /// <param name="emailAddress">The email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The pending invitation, or <see langword="null"/>.</returns>
    private async Task<UserInvitation?> FindPendingInvitationAsync(string emailAddress, CancellationToken cancellationToken)
    {
        var normalized = userManager.NormalizeEmail(emailAddress.Trim()) ?? emailAddress.Trim().ToUpperInvariant();

        var now = time.GetUtcNow();

        var open = await invitations.ListAsync(
            new QuerySpec<UserInvitation> { Filter = i => i.NormalizedEmail == normalized && i.AcceptedAt == null && i.RevokedAt == null && i.ExpiresAt > now, Track = true },
            cancellationToken)
            .ConfigureAwait(false);

        return open.OrderByDescending(i => i.InvitedAt).FirstOrDefault();
    }

    /// <summary>Makes the first account an Admin; every later account starts as a Technician.</summary>
    /// <param name="user">The new user.</param>
    /// <returns>A task.</returns>
    private async Task AssignInitialRoleAsync(ApplicationUser user)
    {
        var admins = await userManager
            .GetUsersInRoleAsync(AppRoles.Admin)
            .ConfigureAwait(false);

        var role = admins.Count == 0 ? AppRoles.Admin : AppRoles.Technician;

        await userManager
            .AddToRoleAsync(user, role)
            .ConfigureAwait(false);
    }

    /// <summary>Builds the signed-in session (token plus profile) for a user.</summary>
    /// <param name="user">The user.</param>
    /// <returns>The session.</returns>
    private async Task<Result<AuthResponse>> CreateSessionAsync(ApplicationUser user)
    {
        var roles = (await userManager.GetRolesAsync(user).ConfigureAwait(false)).ToList();

        var (token, expiresAt) = tokens.CreateToken(user, roles);

        return Result<AuthResponse>.Ok(new AuthResponse(token, expiresAt, user.ToResponse(roles)));
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Linked {provider} login to user {id}")]
    private partial void LogProviderInformation(string provider, Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Email confirmed for user {userId}")]
    private partial void LogEmailConfirmationInformation(Guid userId);
}