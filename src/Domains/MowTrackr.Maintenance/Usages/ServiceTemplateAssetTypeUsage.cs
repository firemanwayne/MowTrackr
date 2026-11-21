using MowTrackr.Maintenance.Models;

namespace MowTrackr.Maintenance.Usages;

/// <summary>
/// Reports service templates that apply to an asset type, so the type cannot be deleted while they do.
/// </summary>
/// <param name="templates">The service template repository.</param>
internal sealed class ServiceTemplateAssetTypeUsage(IRepository<ServiceTemplate> templates) : IAssetTypeUsage
{
    /// <inheritdoc />
    public async Task<string?> DescribeUseAsync(Guid assetTypeId, CancellationToken cancellationToken)
    {
        var items = await templates
            .ListAsync(new QuerySpec<ServiceTemplate> { Filter = t => t.AssetTypes.Any(a => a.Id == assetTypeId) }, cancellationToken)
            .ConfigureAwait(false);

        return items.Count == 0 ? null : $"{items.Count} service template(s)";
    }
}
