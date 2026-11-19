namespace MowTrackr.Client.Interfaces;

/// <summary>
/// Tells the app that the signed-in session has ended without the user asking (the token expired, or the API refused
/// it). A singleton, so the HTTP pipeline (which Blazor builds in its own service scope) and the authentication state
/// provider share one signal.
/// </summary>
internal interface ISessionExpiry
{
    /// <summary>Raised when the session ended unexpectedly, so the UI should re-check who is signed in.</summary>
    event Action? Expired;

    /// <summary>Remembers that the sign-in page should say the session expired, without raising <see cref="Expired"/>.</summary>
    void FlagExpired();

    /// <summary>Remembers that the session expired and raises <see cref="Expired"/>.</summary>
    void SignalExpired();

    /// <summary>Reads and clears the "session expired" notice.</summary>
    /// <returns><see langword="true"/> if the sign-in page should explain that the session expired.</returns>
    bool ConsumeNotice();
}