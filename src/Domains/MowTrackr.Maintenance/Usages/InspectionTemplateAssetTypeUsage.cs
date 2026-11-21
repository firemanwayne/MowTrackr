using MowTrackr.Maintenance.Models;

namespace MowTrackr.Maintenance.Usages;

/// <summary>
/// Counts the inspection templates that use an asset type.
/// </summary>
/// <param name="templates">The inspection template repository.</param>
internal sealed class InspectionTemplateAssetTypeUsage(IRepository<InspectionTemplate> templates) : IAssetTypeUsage
{
    /// <inheritdoc />
    public async Task<string?> DescribeUseAsync(Guid assetTypeId, CancellationToken cancellationToken)
    {
        var items = await templates.ListAsync(new QuerySpec<InspectionTemplate> { Filter = t => t.AssetTypeId == assetTypeId }, cancellationToken).ConfigureAwait(false);

        return items.Count == 0 ? null : $"{items.Count} inspection template(s)";
    }
}
