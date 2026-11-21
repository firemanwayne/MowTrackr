namespace MowTrackr.Application.Interfaces;

/// <summary>
/// Client for the units of measure endpoints.
/// </summary>
internal interface IUnitApiService
{
    /// <summary>Lists the units.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The units.</returns>
    Task<ApiResult<List<UnitOfMeasureResponse>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Adds a unit (admins only).</summary>
    /// <param name="request">The name and label.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created unit.</returns>
    Task<ApiResult<UnitOfMeasureResponse>> CreateAsync(UnitOfMeasureRequest request, CancellationToken cancellationToken);

    /// <summary>Renames a unit (admins only).</summary>
    /// <param name="id">The unit key.</param>
    /// <param name="request">The new name and label.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated unit.</returns>
    Task<ApiResult<UnitOfMeasureResponse>> UpdateAsync(Guid id, UnitOfMeasureRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a unit that nothing uses (admins only).</summary>
    /// <param name="id">The unit key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The outcome.</returns>
    Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
