namespace MowTrackr.Application.Services;

/// <summary>
/// Default <see cref="IOilTypeApiService"/>.
/// </summary>
/// <param name="http">The HTTP client.</param>
/// <param name="localizer">The string localizer for error messages.</param>
internal sealed class OilTypeApiService(HttpClient http, IStringLocalizer<SharedResource> localizer) : ApiServiceBase(http, localizer), IOilTypeApiService
{
    /// <inheritdoc />
    public Task<ApiResult<List<OilTypeResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        return GetAsync<List<OilTypeResponse>>(ApiRoutes.OilTypes, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<OilTypeResponse>> CreateAsync(OilTypeRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<OilTypeRequest, OilTypeResponse>(HttpMethod.Post, ApiRoutes.OilTypes, request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<OilTypeResponse>> UpdateAsync(Guid id, OilTypeRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<OilTypeRequest, OilTypeResponse>(HttpMethod.Put, $"{ApiRoutes.OilTypes}/{id}", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return SendAsync<bool>(HttpMethod.Delete, $"{ApiRoutes.OilTypes}/{id}", cancellationToken);
    }
}
