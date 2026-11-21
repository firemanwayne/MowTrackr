using MowTrackr.Assets.Models;

namespace MowTrackr.Assets.Usages;

/// <summary>
/// Counts the equipment records that use an asset type.
/// </summary>
/// <param name="mowers">The mower repository.</param>
internal sealed class MowerAssetTypeUsage(IRepository<Mower> mowers) : IAssetTypeUsage
{
    /// <inheritdoc />
    public async Task<string?> DescribeUseAsync(Guid assetTypeId, CancellationToken cancellationToken)
    {
        var items = await mowers.ListAsync(new QuerySpec<Mower> { Filter = m => m.AssetTypeId == assetTypeId }, cancellationToken).ConfigureAwait(false);

        return items.Count == 0 ? null : $"{items.Count} equipment record(s)";
    }
}
