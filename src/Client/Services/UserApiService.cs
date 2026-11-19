namespace MowTrackr.Client.Services;

/// <summary>
/// Default <see cref="IUserApiService"/>.
/// </summary>
/// <param name="http">The HTTP client.</param>
internal sealed class UserApiService(HttpClient http) : ApiServiceBase(http), IUserApiService
{
    /// <inheritdoc />
    public Task<ApiResult<UserResponse>> GetMeAsync(CancellationToken cancellationToken)
    {
        return GetAsync<UserResponse>($"{ApiRoutes.Users}/me", cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<UserResponse>> UpdateMeAsync(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<UpdateProfileRequest, UserResponse>(HttpMethod.Put, $"{ApiRoutes.Users}/me", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<List<UserResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        return GetAsync<List<UserResponse>>(ApiRoutes.Users, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<UserResponse>> SetRolesAsync(Guid id, SetRolesRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<SetRolesRequest, UserResponse>(HttpMethod.Put, $"{ApiRoutes.Users}/{id}/roles", request, cancellationToken);
    }
}