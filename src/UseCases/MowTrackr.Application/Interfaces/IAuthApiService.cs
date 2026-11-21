namespace MowTrackr.Application.Interfaces;

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

    /// <summary>Reports whether open registration is available (only until the first account exists).</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The registration status.</returns>
    Task<ApiResult<RegistrationStatusResponse>> GetRegistrationStatusAsync(CancellationToken cancellationToken);

    /// <summary>Describes the invitation behind a link token.</summary>
    /// <param name="token">The secret token from the invitation link.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invitation preview.</returns>
    Task<ApiResult<InvitationPreviewResponse>> GetInvitationAsync(string token, CancellationToken cancellationToken);

    /// <summary>Accepts an invitation and creates the account.</summary>
    /// <param name="request">The token and the account details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The session.</returns>
    Task<ApiResult<AuthResponse>> AcceptInvitationAsync(AcceptInvitationRequest request, CancellationToken cancellationToken);

    /// <summary>Creates the first account, which becomes an Admin, and emails a confirmation link.</summary>
    /// <param name="request">The registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Where the confirmation link was sent.</returns>
    Task<ApiResult<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    /// <summary>Confirms an email address from the emailed link.</summary>
    /// <param name="request">The user and token from the link.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A message.</returns>
    Task<ApiResult<MessageResponse>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken);

    /// <summary>Asks for a new confirmation email.</summary>
    /// <param name="email">The account's email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A message.</returns>
    Task<ApiResult<MessageResponse>> ResendConfirmationAsync(string email, CancellationToken cancellationToken);

    /// <summary>Signs in with email and password.</summary>
    /// <param name="request">The credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The session.</returns>
    Task<ApiResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}