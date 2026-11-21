namespace MowTrackr.Application.Interfaces;

/// <summary>
/// Starts and ends the signed-in session (token storage, auth-state notification and profile preferences).
/// </summary>
internal interface ISessionService
{
    /// <summary>Begins a session from a JWT and adopts the profile's language and theme.</summary>
    /// <param name="token">The token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> if the profile's language differs from the one in effect, so the caller should reload the page.</returns>
    Task<bool> SignInAsync(string token, CancellationToken cancellationToken);

    /// <summary>Ends the session.</summary>
    /// <returns>A task.</returns>
    Task SignOutAsync();
}