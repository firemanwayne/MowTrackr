using MowTrackr.Core.Constants;
using MowTrackr.Identity.Extensions;
using MowTrackr.Identity.Interfaces;
using MowTrackr.Identity.Models;

namespace MowTrackr.Identity.Services;

/// <summary>
/// Default <see cref="IAuthService"/> built on ASP.NET Core Identity's <see cref="UserManager{TUser}"/>.
/// </summary>
/// <param name="userManager">The user manager.</param>
/// <param name="tokens">The JWT issuer.</param>
/// <param name="logger">The logger.</param>
internal sealed class AuthService(UserManager<ApplicationUser> userManager, ITokenService tokens, ILogger<AuthService> logger) : IAuthService
{
    /// <inheritdoc />
    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var existing = await userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);

        if (existing is not null)
        {
            return Result<AuthResponse>.Conflict("An account with that email already exists.");
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
            return Result<AuthResponse>.Invalid(string.Join(" ", created.Errors.Select(e => e.Description)));
        }

        await AssignInitialRoleAsync(user).ConfigureAwait(false);

        logger.LogInformation("Registered user {UserId}", user.Id);

        return await CreateSessionAsync(user).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);

        if (user is null)
        {
            return Result<AuthResponse>.Unauthorized("Invalid email or password.");
        }

        if (await userManager.IsLockedOutAsync(user).ConfigureAwait(false))
        {
            return Result<AuthResponse>.Unauthorized("This account is temporarily locked. Try again later.");
        }

        var valid = await userManager.CheckPasswordAsync(user, request.Password).ConfigureAwait(false);

        if (!valid)
        {
            await userManager.AccessFailedAsync(user).ConfigureAwait(false);

            return Result<AuthResponse>.Unauthorized("Invalid email or password.");
        }

        await userManager.ResetAccessFailedCountAsync(user).ConfigureAwait(false);

        return await CreateSessionAsync(user).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<AuthResponse>> ExternalLoginAsync(ExternalLoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByLoginAsync(command.Provider, command.ProviderKey).ConfigureAwait(false);

        if (user is null)
        {
            var linked = await LinkOrCreateAsync(command).ConfigureAwait(false);

            if (!linked.IsSuccess || linked.Value is null)
            {
                return Result<AuthResponse>.Invalid(linked.Error ?? "Unable to sign in with that provider.");
            }

            user = linked.Value;
        }

        return await CreateSessionAsync(user).ConfigureAwait(false);
    }

    /// <summary>Links the provider login to an existing same-email user, or creates a new user.</summary>
    /// <param name="command">The provider identity.</param>
    /// <returns>The user, or an error.</returns>
    private async Task<Result<ApplicationUser>> LinkOrCreateAsync(ExternalLoginCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return Result<ApplicationUser>.Invalid($"{command.Provider} did not share an email address, which MowTrackr requires.");
        }

        var user = await userManager.FindByEmailAsync(command.Email).ConfigureAwait(false);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = command.Email,
                Email = command.Email,
                EmailConfirmed = true,
                DisplayName = string.IsNullOrWhiteSpace(command.DisplayName) ? command.Email : command.DisplayName.Trim(),
            };

            var created = await userManager.CreateAsync(user).ConfigureAwait(false);

            if (!created.Succeeded)
            {
                return Result<ApplicationUser>.Invalid(string.Join(" ", created.Errors.Select(e => e.Description)));
            }

            await AssignInitialRoleAsync(user).ConfigureAwait(false);
        }

        var login = new UserLoginInfo(command.Provider, command.ProviderKey, command.Provider);

        var added = await userManager.AddLoginAsync(user, login).ConfigureAwait(false);

        if (!added.Succeeded)
        {
            return Result<ApplicationUser>.Invalid(string.Join(" ", added.Errors.Select(e => e.Description)));
        }

        logger.LogInformation("Linked {Provider} login to user {UserId}", command.Provider, user.Id);

        return Result<ApplicationUser>.Ok(user);
    }

    /// <summary>Makes the first account an Admin; every later account starts as a Technician.</summary>
    /// <param name="user">The new user.</param>
    /// <returns>A task.</returns>
    private async Task AssignInitialRoleAsync(ApplicationUser user)
    {
        var admins = await userManager.GetUsersInRoleAsync(AppRoles.Admin).ConfigureAwait(false);

        var role = admins.Count == 0 ? AppRoles.Admin : AppRoles.Technician;

        await userManager.AddToRoleAsync(user, role).ConfigureAwait(false);
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
}