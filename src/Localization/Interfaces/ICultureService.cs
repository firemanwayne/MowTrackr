namespace MowTrackr.Localization.Interfaces;

/// <summary>
/// Maps the app's language codes to cultures and applies the chosen one to the current app domain.
/// </summary>
public interface ICultureService
{
    /// <summary>Resolves any language code or culture name (for example "es-MX" or a browser locale) to a supported language code.</summary>
    /// <param name="language">The candidate, or <see langword="null"/>.</param>
    /// <returns>A supported language code; English when nothing matches.</returns>
    string Resolve(string? language);

    /// <summary>Gets the supported language code of the culture currently in effect.</summary>
    /// <returns>The language code.</returns>
    string GetCurrent();

    /// <summary>Applies a language as the default culture for all threads (formats and string resources).</summary>
    /// <param name="language">The language code; unsupported values fall back to English.</param>
    /// <returns>The language code that was applied.</returns>
    string Apply(string? language);
}