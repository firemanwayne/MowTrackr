namespace MowTrackr.Client.Interfaces;

/// <summary>
/// Client for the authentication endpoints.
/// </summary>
internal interface IAuthApiService
{
    /// <summary>Gets the absolute URL that starts an external sign-in.</summary>
    /// <param name="provider">The provider name.</param>
    /// <returns>The URL.</returns>
    string GetExternalLoginUrl(string provider);

    /// <summary>Lists the enabled external providers.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The provider names.</returns>
    Task<ApiResult<List<string>>> GetProvidersAsync(CancellationToken cancellationToken);

    /// <summary>Registers a local account.</summary>
    /// <param name="request">The registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The session.</returns>
    Task<ApiResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    /// <summary>Signs in with email and password.</summary>
    /// <param name="request">The credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The session.</returns>
    Task<ApiResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}