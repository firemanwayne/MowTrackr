using MowTrackr.Client.Providers;

namespace MowTrackr.Client.Services;

/// <summary>
/// Default <see cref="ISessionService"/>.
/// </summary>
/// <param name="tokens">The token store.</param>
/// <param name="provider">The auth state provider to notify.</param>
internal sealed class SessionService(ITokenStore tokens, JwtAuthenticationStateProvider provider) : ISessionService
{
    /// <inheritdoc />
    public async Task SignInAsync(string token)
    {
        await tokens.SetAsync(token);

        provider.NotifyStateChanged();
    }

    /// <inheritdoc />
    public async Task SignOutAsync()
    {
        await tokens.ClearAsync();

        provider.NotifyStateChanged();
    }
}