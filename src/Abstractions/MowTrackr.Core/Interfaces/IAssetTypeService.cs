using MowTrackr.Core.Contracts.Assets;

namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Manages the asset type list (chain saw, push mower, ...). Anyone signed in can list; changing it is an Admin task.
/// </summary>
public interface IAssetTypeService
{
    /// <summary>Lists the asset types by name.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The asset types.</returns>
    Task<Result<IReadOnlyList<AssetTypeResponse>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Adds an asset type.</summary>
    /// <param name="request">The type name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created type.</returns>
    Task<Result<AssetTypeResponse>> CreateAsync(AssetTypeRequest request, CancellationToken cancellationToken);

    /// <summary>Renames an asset type.</summary>
    /// <param name="id">The type key.</param>
    /// <param name="request">The new name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated type.</returns>
    Task<Result<AssetTypeResponse>> UpdateAsync(Guid id, AssetTypeRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes an asset type. Refused (conflict) while any equipment, service template or inspection template uses it.</summary>
    /// <param name="id">The type key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
