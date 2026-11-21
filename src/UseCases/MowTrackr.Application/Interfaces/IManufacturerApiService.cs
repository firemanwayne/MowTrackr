namespace MowTrackr.Application.Interfaces;

/// <summary>
/// Client for the manufacturer and model lookup endpoints.
/// </summary>
internal interface IManufacturerApiService
{
    /// <summary>Lists manufacturers with their models.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The manufacturers.</returns>
    Task<ApiResult<List<ManufacturerResponse>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Adds a manufacturer.</summary>
    /// <param name="request">The name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created manufacturer.</returns>
    Task<ApiResult<ManufacturerResponse>> CreateAsync(ManufacturerRequest request, CancellationToken cancellationToken);

    /// <summary>Adds a model to a manufacturer.</summary>
    /// <param name="manufacturerId">The manufacturer key.</param>
    /// <param name="request">The name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created model.</returns>
    Task<ApiResult<ManufacturerModelResponse>> CreateModelAsync(Guid manufacturerId, ManufacturerRequest request, CancellationToken cancellationToken);
}
