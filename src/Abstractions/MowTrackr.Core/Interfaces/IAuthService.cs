using MowTrackr.Core.Contracts.Identity;

namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Registers and signs in users, issuing JWTs.
/// </summary>
public interface IAuthService
{
    /// <summary>Reports whether open self-registration is available (only until the first account exists).</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The registration status.</returns>
    Task<Result<RegistrationStatusResponse>> GetRegistrationStatusAsync(CancellationToken cancellationToken);

    /// <summary>Creates the first account, which becomes an Admin, and emails a confirmation link. After that registration is by invitation only.</summary>
    /// <param name="request">The registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Where the confirmation link was sent.</returns>
    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    /// <summary>Confirms an email address from the emailed link.</summary>
    /// <param name="request">The user and token from the link.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An empty success, or why the link is not valid.</returns>
    Task<Result<MessageResponse>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken);

    /// <summary>Sends a new confirmation email if the address belongs to an unconfirmed account. Always succeeds, so it cannot be used to find accounts.</summary>
    /// <param name="request">The address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task.</returns>
    Task<Result<MessageResponse>> ResendConfirmationAsync(ResendConfirmationRequest request, CancellationToken cancellationToken);

    /// <summary>Signs in with email and password.</summary>
    /// <param name="request">The credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A signed-in session.</returns>
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    /// <summary>Signs in (provisioning if needed) a user authenticated by an external provider.</summary>
    /// <param name="command">The provider identity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A signed-in session.</returns>
    Task<Result<AuthResponse>> ExternalLoginAsync(ExternalLoginCommand command, CancellationToken cancellationToken);
}