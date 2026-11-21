namespace MowTrackr.Application.Services;

/// <summary>
/// Default <see cref="IUnitApiService"/>.
/// </summary>
/// <param name="http">The HTTP client.</param>
/// <param name="localizer">The string localizer for error messages.</param>
internal sealed class UnitApiService(HttpClient http, IStringLocalizer<SharedResource> localizer) : ApiServiceBase(http, localizer), IUnitApiService
{
    /// <inheritdoc />
    public Task<ApiResult<List<UnitOfMeasureResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        return GetAsync<List<UnitOfMeasureResponse>>(ApiRoutes.Units, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<UnitOfMeasureResponse>> CreateAsync(UnitOfMeasureRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<UnitOfMeasureRequest, UnitOfMeasureResponse>(HttpMethod.Post, ApiRoutes.Units, request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<UnitOfMeasureResponse>> UpdateAsync(Guid id, UnitOfMeasureRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<UnitOfMeasureRequest, UnitOfMeasureResponse>(HttpMethod.Put, $"{ApiRoutes.Units}/{id}", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return SendAsync<bool>(HttpMethod.Delete, $"{ApiRoutes.Units}/{id}", cancellationToken);
    }
}
