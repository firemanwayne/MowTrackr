namespace MowTrackr.Core.Constants;

/// <summary>
/// UI languages a user can pick on their profile (ISO 639-1 codes, usable as culture names).
/// </summary>
public static class AppLanguages
{
    /// <summary>English.</summary>
    public const string English = "en";

    /// <summary>Spanish.</summary>
    public const string Spanish = "es";

    /// <summary>The language used when none is chosen.</summary>
    public const string Default = English;

    /// <summary>Every supported language code.</summary>
    public static readonly IReadOnlyList<string> All = [English, Spanish];

    /// <summary>Checks whether a language code is supported.</summary>
    /// <param name="language">The code to check.</param>
    /// <returns><see langword="true"/> if supported.</returns>
    public static bool IsSupported(string? language)
    {
        return language is not null && All.Contains(language, StringComparer.OrdinalIgnoreCase);
    }
}