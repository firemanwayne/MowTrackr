namespace MowTrackr.Application.Interfaces;

/// <summary>
/// Client for the asset type lookup endpoints.
/// </summary>
internal interface IAssetTypeApiService
{
    /// <summary>Lists the asset types.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The asset types.</returns>
    Task<ApiResult<List<AssetTypeResponse>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Adds an asset type.</summary>
    /// <param name="request">The name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created type.</returns>
    Task<ApiResult<AssetTypeResponse>> CreateAsync(AssetTypeRequest request, CancellationToken cancellationToken);

    /// <summary>Renames an asset type.</summary>
    /// <param name="id">The type key.</param>
    /// <param name="request">The new name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated type.</returns>
    Task<ApiResult<AssetTypeResponse>> UpdateAsync(Guid id, AssetTypeRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes an asset type.</summary>
    /// <param name="id">The type key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
