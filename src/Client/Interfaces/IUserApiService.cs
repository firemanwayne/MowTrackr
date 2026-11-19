namespace MowTrackr.Client.Interfaces;

/// <summary>
/// Client for the profile and user administration endpoints.
/// </summary>
internal interface IUserApiService
{
    /// <summary>Gets the signed-in user's profile.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The profile.</returns>
    Task<ApiResult<UserResponse>> GetMeAsync(CancellationToken cancellationToken);

    /// <summary>Edits the signed-in user's profile.</summary>
    /// <param name="request">The new values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated profile.</returns>
    Task<ApiResult<UserResponse>> UpdateMeAsync(UpdateProfileRequest request, CancellationToken cancellationToken);

    /// <summary>Lists all users (manager/admin).</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The users.</returns>
    Task<ApiResult<List<UserResponse>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Replaces a user's roles (admin).</summary>
    /// <param name="id">The user key.</param>
    /// <param name="request">The new role set.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated user.</returns>
    Task<ApiResult<UserResponse>> SetRolesAsync(Guid id, SetRolesRequest request, CancellationToken cancellationToken);
}