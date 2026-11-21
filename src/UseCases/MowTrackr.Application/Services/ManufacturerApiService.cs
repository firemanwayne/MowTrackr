namespace MowTrackr.Application.Services;

/// <summary>
/// Default <see cref="IManufacturerApiService"/>.
/// </summary>
/// <param name="http">The HTTP client.</param>
/// <param name="localizer">The string localizer for error messages.</param>
internal sealed class ManufacturerApiService(HttpClient http, IStringLocalizer<SharedResource> localizer) : ApiServiceBase(http, localizer), IManufacturerApiService
{
    /// <inheritdoc />
    public Task<ApiResult<List<ManufacturerResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        return GetAsync<List<ManufacturerResponse>>(ApiRoutes.Manufacturers, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<ManufacturerResponse>> CreateAsync(ManufacturerRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<ManufacturerRequest, ManufacturerResponse>(HttpMethod.Post, ApiRoutes.Manufacturers, request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<ManufacturerModelResponse>> CreateModelAsync(Guid manufacturerId, ManufacturerRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<ManufacturerRequest, ManufacturerModelResponse>(HttpMethod.Post, $"{ApiRoutes.Manufacturers}/{manufacturerId}/models", request, cancellationToken);
    }
}
