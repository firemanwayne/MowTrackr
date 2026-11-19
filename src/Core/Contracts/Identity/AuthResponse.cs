namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// A signed-in session: the JWT plus the user it was issued for.
/// </summary>
/// <param name="Token">The JWT bearer token.</param>
/// <param name="ExpiresAt">When the token expires.</param>
/// <param name="User">The signed-in user.</param>
public sealed record AuthResponse(string Token, DateTimeOffset ExpiresAt, UserResponse User);