namespace MowTrackr.Core.Constants;

/// <summary>
/// Colour themes a user can pick on their profile.
/// </summary>
public static class AppThemes
{
    /// <summary>Light mode.</summary>
    public const string Light = "Light";

    /// <summary>Dark mode.</summary>
    public const string Dark = "Dark";

    /// <summary>The theme used when none is chosen.</summary>
    public const string Default = Light;

    /// <summary>Every supported theme.</summary>
    public static readonly IReadOnlyList<string> All = [Light, Dark];

    /// <summary>Checks whether a theme name is supported.</summary>
    /// <param name="theme">The name to check.</param>
    /// <returns><see langword="true"/> if supported.</returns>
    public static bool IsSupported(string? theme)
    {
        return theme is not null && All.Contains(theme, StringComparer.OrdinalIgnoreCase);
    }
}