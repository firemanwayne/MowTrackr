using Microsoft.JSInterop;

namespace MowTrackr.Application.Stores;

/// <summary>
/// <see cref="ITokenStore"/> backed by the browser's <c>localStorage</c>, with an in-memory cache.
/// </summary>
/// <param name="js">The JS runtime.</param>
internal sealed class LocalStorageTokenStore(IJSRuntime js) : ITokenStore
{
    private const string StorageKey = "mowtrackr.token";

    private string? _cached;

    private bool _loaded;

    /// <inheritdoc />
    public async ValueTask<string?> GetAsync()
    {
        if (_loaded)
        {
            return _cached;
        }

        try
        {
            _cached = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        }
        catch (JSException)
        {
            _cached = null;
        }

        _loaded = true;

        return _cached;
    }

    /// <inheritdoc />
    public async ValueTask SetAsync(string token)
    {
        _cached = token;

        _loaded = true;

        await js.InvokeVoidAsync("localStorage.setItem", StorageKey, token);
    }

    /// <inheritdoc />
    public async ValueTask ClearAsync()
    {
        _cached = null;

        _loaded = true;

        await js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
    }
}