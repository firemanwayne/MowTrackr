namespace MowTrackr.Client.Services;

/// <summary>
/// Default <see cref="IAssetTypeApiService"/>.
/// </summary>
/// <param name="http">The HTTP client.</param>
/// <param name="localizer">The string localizer for error messages.</param>
internal sealed class AssetTypeApiService(HttpClient http, IStringLocalizer<SharedResource> localizer) : ApiServiceBase(http, localizer), IAssetTypeApiService
{
    /// <inheritdoc />
    public Task<ApiResult<List<AssetTypeResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        return GetAsync<List<AssetTypeResponse>>(ApiRoutes.AssetTypes, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<AssetTypeResponse>> CreateAsync(AssetTypeRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<AssetTypeRequest, AssetTypeResponse>(HttpMethod.Post, ApiRoutes.AssetTypes, request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<AssetTypeResponse>> UpdateAsync(Guid id, AssetTypeRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<AssetTypeRequest, AssetTypeResponse>(HttpMethod.Put, $"{ApiRoutes.AssetTypes}/{id}", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return SendAsync<bool>(HttpMethod.Delete, $"{ApiRoutes.AssetTypes}/{id}", cancellationToken);
    }
}
