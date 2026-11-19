using MowTrackr.Core.Contracts.Identity;

namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Registers and signs in users, issuing JWTs.
/// </summary>
public interface IAuthService
{
    /// <summary>Creates a local account. The very first account becomes an Admin.</summary>
    /// <param name="request">The registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A signed-in session.</returns>
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

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