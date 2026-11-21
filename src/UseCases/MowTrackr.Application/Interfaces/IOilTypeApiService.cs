namespace MowTrackr.Application.Interfaces;

/// <summary>
/// Client for the oil type endpoints.
/// </summary>
internal interface IOilTypeApiService
{
    /// <summary>Lists the oil types.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The oil types.</returns>
    Task<ApiResult<List<OilTypeResponse>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Adds an oil type (admins only).</summary>
    /// <param name="request">The name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created oil type.</returns>
    Task<ApiResult<OilTypeResponse>> CreateAsync(OilTypeRequest request, CancellationToken cancellationToken);

    /// <summary>Renames an oil type (admins only).</summary>
    /// <param name="id">The oil type key.</param>
    /// <param name="request">The new name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated oil type.</returns>
    Task<ApiResult<OilTypeResponse>> UpdateAsync(Guid id, OilTypeRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes an oil type that nothing uses (admins only).</summary>
    /// <param name="id">The oil type key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The outcome.</returns>
    Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
