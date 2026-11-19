using MowTrackr.Core.Constants;
using MowTrackr.Identity.Extensions;
using MowTrackr.Identity.Models;

namespace MowTrackr.Identity.Services;

/// <summary>
/// Default <see cref="IUserService"/>.
/// </summary>
/// <param name="userManager">The user manager.</param>
/// <param name="logger">The logger.</param>
internal sealed class UserService(UserManager<ApplicationUser> userManager, ILogger<UserService> logger) : IUserService
{
    /// <inheritdoc />
    public async Task<Result<UserResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id.ToString()).ConfigureAwait(false);

        if (user is null)
        {
            return Result<UserResponse>.NotFound("User not found.");
        }

        return Result<UserResponse>.Ok(await ToResponseAsync(user).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<UserResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var users = await userManager.Users.OrderBy(u => u.DisplayName).ToListAsync(cancellationToken).ConfigureAwait(false);

        var responses = new List<UserResponse>(users.Count);

        foreach (var user in users)
        {
            responses.Add(await ToResponseAsync(user).ConfigureAwait(false));
        }

        return Result<IReadOnlyList<UserResponse>>.Ok(responses);
    }

    /// <inheritdoc />
    public async Task<Result<UserResponse>> UpdateProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id.ToString()).ConfigureAwait(false);

        if (user is null)
        {
            return Result<UserResponse>.NotFound("User not found.");
        }

        user.DisplayName = request.DisplayName.Trim();

        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

        var updated = await userManager.UpdateAsync(user).ConfigureAwait(false);

        if (!updated.Succeeded)
        {
            return Result<UserResponse>.Invalid(string.Join(" ", updated.Errors.Select(e => e.Description)));
        }

        return Result<UserResponse>.Ok(await ToResponseAsync(user).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public async Task<Result<UserResponse>> SetRolesAsync(Guid id, SetRolesRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id.ToString()).ConfigureAwait(false);

        if (user is null)
        {
            return Result<UserResponse>.NotFound("User not found.");
        }

        var wanted = request.Roles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var unknown = wanted.Where(r => !AppRoles.All.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();

        if (unknown.Count > 0)
        {
            return Result<UserResponse>.Invalid($"Unknown role(s): {string.Join(", ", unknown)}.");
        }

        var current = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        var removingAdmin = current.Contains(AppRoles.Admin) && !wanted.Contains(AppRoles.Admin, StringComparer.OrdinalIgnoreCase);

        if (removingAdmin)
        {
            var admins = await userManager.GetUsersInRoleAsync(AppRoles.Admin).ConfigureAwait(false);

            if (admins.Count <= 1)
            {
                return Result<UserResponse>.Conflict("At least one Admin must remain.");
            }
        }

        await userManager.RemoveFromRolesAsync(user, current.Except(wanted, StringComparer.OrdinalIgnoreCase)).ConfigureAwait(false);

        await userManager.AddToRolesAsync(user, wanted.Except(current, StringComparer.OrdinalIgnoreCase)).ConfigureAwait(false);

        logger.LogInformation("Set roles for user {UserId} to {Roles}", user.Id, wanted);

        return Result<UserResponse>.Ok(await ToResponseAsync(user).ConfigureAwait(false));
    }

    /// <summary>Maps a user to its response, loading roles.</summary>
    /// <param name="user">The user.</param>
    /// <returns>The response.</returns>
    private async Task<UserResponse> ToResponseAsync(ApplicationUser user)
    {
        var roles = (await userManager.GetRolesAsync(user).ConfigureAwait(false)).ToList();

        return user.ToResponse(roles);
    }
}