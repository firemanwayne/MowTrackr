using MowTrackr.Core.Contracts.Assets;

namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Manages the manufacturer and model lookup lists that feed the mower dropdowns.
/// </summary>
public interface IManufacturerService
{
    /// <summary>Lists manufacturers with their models.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The manufacturers.</returns>
    Task<Result<IReadOnlyList<ManufacturerResponse>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Adds a manufacturer.</summary>
    /// <param name="request">The manufacturer name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created manufacturer.</returns>
    Task<Result<ManufacturerResponse>> CreateAsync(ManufacturerRequest request, CancellationToken cancellationToken);

    /// <summary>Adds a model to a manufacturer.</summary>
    /// <param name="manufacturerId">The manufacturer key.</param>
    /// <param name="request">The model name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created model.</returns>
    Task<Result<ManufacturerModelResponse>> CreateModelAsync(Guid manufacturerId, ManufacturerRequest request, CancellationToken cancellationToken);
}
