using MowTrackr.Assets.Models;

namespace MowTrackr.Assets.Services;

/// <summary>
/// Default <see cref="IAssetTypeService"/> backed by the generic repository.
/// </summary>
/// <param name="assetTypes">The asset type repository.</param>
/// <param name="usages">What checks whether a type is still in use, one per kind of record that points at types.</param>
/// <param name="logger">The logger.</param>
internal sealed class AssetTypeService(IRepository<AssetType> assetTypes, IEnumerable<IAssetTypeUsage> usages, ILogger<AssetTypeService> logger) : IAssetTypeService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<AssetTypeResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var items = await assetTypes.ListAsync(new QuerySpec<AssetType> { OrderBy = t => t.Name }, cancellationToken).ConfigureAwait(false);

        return Result<IReadOnlyList<AssetTypeResponse>>.Ok(items.Select(t => new AssetTypeResponse(t.Id, t.Name, t.HasTires)).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<AssetTypeResponse>> CreateAsync(AssetTypeRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        var existing = await assetTypes.ListAsync(new QuerySpec<AssetType> { Filter = t => t.Name == name }, cancellationToken).ConfigureAwait(false);

        if (existing.Count > 0)
        {
            return Result<AssetTypeResponse>.Conflict("An asset type with that name already exists.");
        }

        var assetType = new AssetType { Name = name, HasTires = request.HasTires };

        await assetTypes.AddAsync(assetType, cancellationToken).ConfigureAwait(false);

        await assetTypes.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Created asset type {AssetTypeId} ({AssetTypeName})", assetType.Id, assetType.Name);

        return Result<AssetTypeResponse>.Ok(new AssetTypeResponse(assetType.Id, assetType.Name, assetType.HasTires));
    }

    /// <inheritdoc />
    public async Task<Result<AssetTypeResponse>> UpdateAsync(Guid id, AssetTypeRequest request, CancellationToken cancellationToken)
    {
        var assetType = await assetTypes.GetByIdAsync(id, new QuerySpec<AssetType> { Track = true }, cancellationToken).ConfigureAwait(false);

        if (assetType is null)
        {
            return Result<AssetTypeResponse>.NotFound("Asset type not found.");
        }

        var name = request.Name.Trim();

        var existing = await assetTypes.ListAsync(new QuerySpec<AssetType> { Filter = t => t.Name == name && t.Id != id }, cancellationToken).ConfigureAwait(false);

        if (existing.Count > 0)
        {
            return Result<AssetTypeResponse>.Conflict("An asset type with that name already exists.");
        }

        assetType.Name = name;

        assetType.HasTires = request.HasTires;

        await assetTypes.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Updated asset type {AssetTypeId}", id);

        return Result<AssetTypeResponse>.Ok(new AssetTypeResponse(assetType.Id, assetType.Name, assetType.HasTires));
    }

    /// <inheritdoc />
    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var assetType = await assetTypes.GetByIdAsync(id, new QuerySpec<AssetType> { Track = true }, cancellationToken).ConfigureAwait(false);

        if (assetType is null)
        {
            return Result<bool>.NotFound("Asset type not found.");
        }

        var inUse = new List<string>();

        foreach (var usage in usages)
        {
            var description = await usage.DescribeUseAsync(id, cancellationToken).ConfigureAwait(false);

            if (description is not null)
            {
                inUse.Add(description);
            }
        }

        if (inUse.Count > 0)
        {
            return Result<bool>.Conflict($"This type is still used by {string.Join(" and ", inUse)}. Move or delete those first.");
        }

        assetTypes.Remove(assetType);

        await assetTypes.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Deleted asset type {AssetTypeId}", id);

        return Result<bool>.Ok(true);
    }
}
