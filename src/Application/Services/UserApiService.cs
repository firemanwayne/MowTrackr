namespace MowTrackr.Application.Services;

/// <summary>
/// Default <see cref="IUserApiService"/>.
/// </summary>
/// <param name="http">The HTTP client.</param>
/// <param name="localizer">The string localizer for error messages.</param>
internal sealed class UserApiService(HttpClient http, IStringLocalizer<SharedResource> localizer) : ApiServiceBase(http, localizer), IUserApiService
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
    public Task<ApiResult<StartPhoneVerificationResponse>> StartPhoneVerificationAsync(StartPhoneVerificationRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<StartPhoneVerificationRequest, StartPhoneVerificationResponse>(HttpMethod.Post, $"{ApiRoutes.Users}/me/phone/send-code", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<UserResponse>> VerifyPhoneAsync(VerifyPhoneRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<VerifyPhoneRequest, UserResponse>(HttpMethod.Post, $"{ApiRoutes.Users}/me/phone/verify", request, cancellationToken);
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