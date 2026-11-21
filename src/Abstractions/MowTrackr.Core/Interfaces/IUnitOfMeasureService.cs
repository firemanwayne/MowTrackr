using MowTrackr.Core.Contracts.Assets;

namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Manages the list of units of measure (quart, ounce, gallon, each, ...). Anyone signed in can list; changing it is an Admin task.
/// </summary>
public interface IUnitOfMeasureService
{
    /// <summary>Lists the units by name.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The units.</returns>
    Task<Result<IReadOnlyList<UnitOfMeasureResponse>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Adds a unit.</summary>
    /// <param name="request">The name and label.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created unit.</returns>
    Task<Result<UnitOfMeasureResponse>> CreateAsync(UnitOfMeasureRequest request, CancellationToken cancellationToken);

    /// <summary>Renames a unit.</summary>
    /// <param name="id">The unit key.</param>
    /// <param name="request">The new name and label.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated unit.</returns>
    Task<Result<UnitOfMeasureResponse>> UpdateAsync(Guid id, UnitOfMeasureRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a unit that nothing uses.</summary>
    /// <param name="id">The unit key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when deleted.</returns>
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
