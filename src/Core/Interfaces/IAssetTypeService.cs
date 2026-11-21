using MowTrackr.Core.Contracts.Assets;

namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Manages the asset type lookup list (chain saw, push mower, ...).
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
}
