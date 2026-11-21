using MowTrackr.Core.Contracts.Assets;

namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Manages the list of oil types (SAE 10W-30, synthetic 5W-30, 2-cycle mix, ...). Anyone signed in can list; changing it is an Admin task.
/// </summary>
public interface IOilTypeService
{
    /// <summary>Lists the oil types by name.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The oil types.</returns>
    Task<Result<IReadOnlyList<OilTypeResponse>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Adds an oil type.</summary>
    /// <param name="request">The name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created oil type.</returns>
    Task<Result<OilTypeResponse>> CreateAsync(OilTypeRequest request, CancellationToken cancellationToken);

    /// <summary>Renames an oil type.</summary>
    /// <param name="id">The oil type key.</param>
    /// <param name="request">The new name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated oil type.</returns>
    Task<Result<OilTypeResponse>> UpdateAsync(Guid id, OilTypeRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes an oil type that nothing uses.</summary>
    /// <param name="id">The oil type key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when deleted.</returns>
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
