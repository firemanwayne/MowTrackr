using MowTrackr.Application.Interfaces;

namespace MowTrackr.Application.UnitTests.Fakes;

/// <summary>
/// An <see cref="ITokenStore"/> that keeps the token in memory.
/// </summary>
internal sealed class InMemoryTokenStore : ITokenStore
{
    private string? _token;

    public InMemoryTokenStore(string? token = null)
    {
        _token = token;
    }

    public string? Current => _token;

    public ValueTask<string?> GetAsync()
    {
        return ValueTask.FromResult(_token);
    }

    public ValueTask SetAsync(string token)
    {
        _token = token;

        return ValueTask.CompletedTask;
    }

    public ValueTask ClearAsync()
    {
        _token = null;

        return ValueTask.CompletedTask;
    }
}