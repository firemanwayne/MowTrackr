namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// A user profile as returned by the API.
/// </summary>
/// <param name="Id">The user key.</param>
/// <param name="Email">The email address.</param>
/// <param name="DisplayName">The display name.</param>
/// <param name="PhoneNumber">The phone number.</param>
/// <param name="Roles">The assigned roles.</param>
public sealed record UserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string? PhoneNumber,
    IReadOnlyList<string> Roles);