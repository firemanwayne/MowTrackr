namespace MowTrackr.Client.Services;

/// <summary>
/// Default <see cref="IAuthApiService"/>.
/// </summary>
/// <param name="http">The HTTP client.</param>
/// <param name="localizer">The string localizer for error messages.</param>
internal sealed class AuthApiService(HttpClient http, IStringLocalizer<SharedResource> localizer) : ApiServiceBase(http, localizer), IAuthApiService
{
    private readonly HttpClient _http = http;

    /// <inheritdoc />
    public string GetExternalLoginUrl(string provider)
    {
        return new Uri(_http.BaseAddress!, $"{ApiRoutes.Auth}/external/{Uri.EscapeDataString(provider)}").ToString();
    }

    /// <inheritdoc />
    public Task<ApiResult<List<string>>> GetProvidersAsync(CancellationToken cancellationToken)
    {
        return GetAsync<List<string>>($"{ApiRoutes.Auth}/providers", cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<RegisterRequest, AuthResponse>(HttpMethod.Post, $"{ApiRoutes.Auth}/register", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<LoginRequest, AuthResponse>(HttpMethod.Post, $"{ApiRoutes.Auth}/login", request, cancellationToken);
    }
}