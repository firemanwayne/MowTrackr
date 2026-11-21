using Microsoft.JSInterop;

namespace MowTrackr.Application.Services;

/// <summary>
/// Default <see cref="IPreferencesService"/> backed by the browser's <c>localStorage</c> via <c>prefs.js</c>.
/// </summary>
/// <param name="js">The JS runtime.</param>
/// <param name="cultures">The culture service.</param>
internal sealed class PreferencesService(IJSRuntime js, ICultureService cultures) : IPreferencesService
{
    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        var stored = await js.InvokeAsync<string?>("mowtrackr.getLanguage");

        cultures.Apply(stored);
    }

    /// <inheritdoc />
    public async Task<string> GetThemeAsync()
    {
        return await js.InvokeAsync<string>("mowtrackr.getTheme");
    }

    /// <inheritdoc />
    public async Task<bool> SetLanguageAsync(string language)
    {
        var resolved = cultures.Resolve(language);

        await js.InvokeVoidAsync("mowtrackr.setLanguage", resolved);

        return !string.Equals(resolved, cultures.GetCurrent(), StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async Task SetThemeAsync(string theme)
    {
        var resolved = AppThemes.IsSupported(theme) ? theme : AppThemes.Default;

        await js.InvokeVoidAsync("mowtrackr.applyTheme", resolved);
    }

    /// <inheritdoc />
    public async Task<bool> ApplyProfileAsync(UserResponse user)
    {
        await SetThemeAsync(user.Theme);

        return await SetLanguageAsync(user.Language);
    }
}