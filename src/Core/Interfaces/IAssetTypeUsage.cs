namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Tells whether (and how much) an asset type is referenced by one kind of record. Each domain that points at asset
/// types registers one, so deleting a type can be refused with a clear reason without the Assets domain knowing about
/// the others.
/// </summary>
public interface IAssetTypeUsage
{
    /// <summary>Describes how the asset type is used.</summary>
    /// <param name="assetTypeId">The asset type key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>For example "3 mowers", or <see langword="null"/> when nothing of this kind uses the type.</returns>
    Task<string?> DescribeUseAsync(Guid assetTypeId, CancellationToken cancellationToken);
}
