namespace MowTrackr.Application.Interfaces;

/// <summary>
/// Persists the JWT between page loads.
/// </summary>
internal interface ITokenStore
{
    /// <summary>Gets the stored token, if any.</summary>
    /// <returns>The token, or <see langword="null"/>.</returns>
    ValueTask<string?> GetAsync();

    /// <summary>Stores a token.</summary>
    /// <param name="token">The token.</param>
    /// <returns>A task.</returns>
    ValueTask SetAsync(string token);

    /// <summary>Removes the stored token.</summary>
    /// <returns>A task.</returns>
    ValueTask ClearAsync();
}