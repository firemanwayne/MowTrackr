namespace MowTrackr.Client.Interfaces;

/// <summary>
/// Holds the user's language and theme: cached in the browser so they apply before sign-in, and refreshed from the profile after it.
/// </summary>
internal interface IPreferencesService
{
    /// <summary>Applies the cached (or browser-default) language as the app culture. Call before the host starts.</summary>
    /// <returns>A task.</returns>
    Task InitializeAsync();

    /// <summary>Gets the theme currently applied ("Light" or "Dark").</summary>
    /// <returns>The theme.</returns>
    Task<string> GetThemeAsync();

    /// <summary>Caches a language choice. The new language takes effect after a page reload.</summary>
    /// <param name="language">The language code.</param>
    /// <returns><see langword="true"/> if the language differs from the one in effect (so a reload is needed).</returns>
    Task<bool> SetLanguageAsync(string language);

    /// <summary>Applies a theme immediately and caches it.</summary>
    /// <param name="theme">The theme name.</param>
    /// <returns>A task.</returns>
    Task SetThemeAsync(string theme);

    /// <summary>Adopts the preferences stored on the user's profile.</summary>
    /// <param name="user">The profile.</param>
    /// <returns><see langword="true"/> if the language changed and the page must be reloaded to apply it.</returns>
    Task<bool> ApplyProfileAsync(UserResponse user);
}