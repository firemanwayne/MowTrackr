namespace MowTrackr.Client.Interfaces;

/// <summary>
/// Starts and ends the signed-in session (token storage plus auth-state notification).
/// </summary>
internal interface ISessionService
{
    /// <summary>Begins a session from a JWT.</summary>
    /// <param name="token">The token.</param>
    /// <returns>A task.</returns>
    Task SignInAsync(string token);

    /// <summary>Ends the session.</summary>
    /// <returns>A task.</returns>
    Task SignOutAsync();
}