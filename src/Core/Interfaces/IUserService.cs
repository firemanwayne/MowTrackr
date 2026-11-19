using MowTrackr.Core.Contracts.Identity;

namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Profile and role management.
/// </summary>
public interface IUserService
{
    /// <summary>Gets a user's profile.</summary>
    /// <param name="id">The user key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The profile.</returns>
    Task<Result<UserResponse>> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Lists all users.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The users.</returns>
    Task<Result<IReadOnlyList<UserResponse>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Edits a user's profile.</summary>
    /// <param name="id">The user key.</param>
    /// <param name="request">The new profile values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated profile.</returns>
    Task<Result<UserResponse>> UpdateProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken cancellationToken);

    /// <summary>Texts a verification code to a phone number the user wants to use.</summary>
    /// <param name="id">The user key.</param>
    /// <param name="request">The phone number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Whether the text was sent.</returns>
    Task<Result<StartPhoneVerificationResponse>> StartPhoneVerificationAsync(Guid id, StartPhoneVerificationRequest request, CancellationToken cancellationToken);

    /// <summary>Finishes phone verification: saves the number as the user's verified phone if the code is right.</summary>
    /// <param name="id">The user key.</param>
    /// <param name="request">The number and the texted code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated profile.</returns>
    Task<Result<UserResponse>> VerifyPhoneAsync(Guid id, VerifyPhoneRequest request, CancellationToken cancellationToken);

    /// <summary>Replaces a user's roles.</summary>
    /// <param name="id">The user key.</param>
    /// <param name="request">The new role set.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated profile.</returns>
    Task<Result<UserResponse>> SetRolesAsync(Guid id, SetRolesRequest request, CancellationToken cancellationToken);
}