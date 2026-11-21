namespace MowTrackr.Application.Services;

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
    public Task<ApiResult<RegistrationStatusResponse>> GetRegistrationStatusAsync(CancellationToken cancellationToken)
    {
        return GetAsync<RegistrationStatusResponse>($"{ApiRoutes.Auth}/registration", cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<InvitationPreviewResponse>> GetInvitationAsync(string token, CancellationToken cancellationToken)
    {
        return GetAsync<InvitationPreviewResponse>($"{ApiRoutes.Auth}/invitation?token={Uri.EscapeDataString(token)}", cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<AuthResponse>> AcceptInvitationAsync(AcceptInvitationRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<AcceptInvitationRequest, AuthResponse>(HttpMethod.Post, $"{ApiRoutes.Auth}/accept-invitation", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<RegisterRequest, RegisterResponse>(HttpMethod.Post, $"{ApiRoutes.Auth}/register", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<MessageResponse>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<ConfirmEmailRequest, MessageResponse>(HttpMethod.Post, $"{ApiRoutes.Auth}/confirm-email", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<MessageResponse>> ResendConfirmationAsync(string email, CancellationToken cancellationToken)
    {
        return SendAsync<ResendConfirmationRequest, MessageResponse>(HttpMethod.Post, $"{ApiRoutes.Auth}/resend-confirmation", new ResendConfirmationRequest { Email = email }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<LoginRequest, AuthResponse>(HttpMethod.Post, $"{ApiRoutes.Auth}/login", request, cancellationToken);
    }
}