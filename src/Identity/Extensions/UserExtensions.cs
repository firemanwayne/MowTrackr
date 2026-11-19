using MowTrackr.Identity.Models;

namespace MowTrackr.Identity.Extensions;

/// <summary>
/// Mapping helpers for <see cref="ApplicationUser"/>.
/// </summary>
internal static class UserExtensions
{
    /// <summary>Maps a user and its roles to the API response.</summary>
    /// <param name="user">The user.</param>
    /// <param name="roles">The user's roles.</param>
    /// <returns>The response.</returns>
    public static UserResponse ToResponse(this ApplicationUser user, IReadOnlyList<string> roles)
    {
        return new UserResponse(user.Id, user.Email ?? string.Empty, user.DisplayName, user.PhoneNumber, roles, user.Language, user.Theme);
    }
}