namespace MowTrackr.Localization.Services;

/// <summary>
/// Default <see cref="ICultureService"/>.
/// </summary>
internal sealed class CultureService : ICultureService
{
    /// <inheritdoc />
    public string Resolve(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return AppLanguages.Default;
        }

        var twoLetter = language.Trim().Split('-', '_')[0].ToLowerInvariant();

        return AppLanguages.IsSupported(twoLetter) ? twoLetter : AppLanguages.Default;
    }

    /// <inheritdoc />
    public string GetCurrent()
    {
        return Resolve(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
    }

    /// <inheritdoc />
    public string Apply(string? language)
    {
        var resolved = Resolve(language);

        var culture = CultureInfo.GetCultureInfo(resolved);

        CultureInfo.DefaultThreadCurrentCulture = culture;

        CultureInfo.DefaultThreadCurrentUICulture = culture;

        CultureInfo.CurrentCulture = culture;

        CultureInfo.CurrentUICulture = culture;

        return resolved;
    }
}