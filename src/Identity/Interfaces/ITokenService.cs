using MowTrackr.Identity.Models;

namespace MowTrackr.Identity.Interfaces;

/// <summary>
/// Issues JWTs for users.
/// </summary>
internal interface ITokenService
{
    /// <summary>Creates a signed token for a user.</summary>
    /// <param name="user">The user.</param>
    /// <param name="roles">The user's roles.</param>
    /// <returns>The token and its expiry.</returns>
    (string Token, DateTimeOffset ExpiresAt) CreateToken(ApplicationUser user, IReadOnlyList<string> roles);
}