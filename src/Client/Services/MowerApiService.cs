namespace MowTrackr.Client.Services;

/// <summary>
/// Default <see cref="IMowerApiService"/>.
/// </summary>
/// <param name="http">The HTTP client.</param>
internal sealed class MowerApiService(HttpClient http) : ApiServiceBase(http), IMowerApiService
{
    /// <inheritdoc />
    public Task<ApiResult<List<MowerResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        return GetAsync<List<MowerResponse>>(ApiRoutes.Mowers, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<MowerResponse>> CreateAsync(MowerRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<MowerRequest, MowerResponse>(HttpMethod.Post, ApiRoutes.Mowers, request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<MowerResponse>> UpdateAsync(Guid id, MowerRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<MowerRequest, MowerResponse>(HttpMethod.Put, $"{ApiRoutes.Mowers}/{id}", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return SendAsync<bool>(HttpMethod.Delete, $"{ApiRoutes.Mowers}/{id}", cancellationToken);
    }
}