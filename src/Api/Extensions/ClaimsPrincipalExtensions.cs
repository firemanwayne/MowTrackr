using System.Security.Claims;

namespace MowTrackr.Api.Extensions;

/// <summary>
/// Helpers for reading the signed-in user from the JWT claims.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>Gets the signed-in user's key.</summary>
    /// <param name="user">The principal.</param>
    /// <returns>The user key, or <see cref="Guid.Empty"/> if absent.</returns>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(AppClaims.UserId);

        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }

    /// <summary>Gets a value indicating whether the user is an Admin or Manager.</summary>
    /// <param name="user">The principal.</param>
    /// <returns><see langword="true"/> if the user has manager-level access.</returns>
    public static bool IsManagerOrAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole(AppRoles.Admin) || user.IsInRole(AppRoles.Manager);
    }
}