using MowTrackr.Application.Providers;

namespace MowTrackr.Application.Services;

/// <summary>
/// Default <see cref="ISessionService"/>.
/// </summary>
/// <param name="tokens">The token store.</param>
/// <param name="provider">The auth state provider to notify.</param>
/// <param name="users">The user API (to read the profile preferences).</param>
/// <param name="preferences">The preferences service.</param>
internal sealed class SessionService(ITokenStore tokens, JwtAuthenticationStateProvider provider, IUserApiService users, IPreferencesService preferences) : ISessionService
{
    /// <inheritdoc />
    public async Task<bool> SignInAsync(string token, CancellationToken cancellationToken)
    {
        await tokens.SetAsync(token);

        provider.NotifyStateChanged();

        var profile = await users.GetMeAsync(cancellationToken);

        if (!profile.IsSuccess || profile.Value is null)
        {
            return false;
        }

        return await preferences.ApplyProfileAsync(profile.Value);
    }

    /// <inheritdoc />
    public async Task SignOutAsync()
    {
        await tokens.ClearAsync();

        provider.NotifyStateChanged();
    }
}