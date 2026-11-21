using MowTrackr.Assets.Models;

namespace MowTrackr.Assets.Services;

/// <summary>
/// Default <see cref="IAssetTypeService"/> backed by the generic repository.
/// </summary>
/// <param name="assetTypes">The asset type repository.</param>
/// <param name="logger">The logger.</param>
internal sealed class AssetTypeService(IRepository<AssetType> assetTypes, ILogger<AssetTypeService> logger) : IAssetTypeService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<AssetTypeResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var items = await assetTypes.ListAsync(new QuerySpec<AssetType> { OrderBy = t => t.Name }, cancellationToken).ConfigureAwait(false);

        return Result<IReadOnlyList<AssetTypeResponse>>.Ok(items.Select(t => new AssetTypeResponse(t.Id, t.Name)).ToList());
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

        var assetType = new AssetType { Name = name };

        await assetTypes.AddAsync(assetType, cancellationToken).ConfigureAwait(false);

        await assetTypes.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Created asset type {AssetTypeId} ({AssetTypeName})", assetType.Id, assetType.Name);

        return Result<AssetTypeResponse>.Ok(new AssetTypeResponse(assetType.Id, assetType.Name));
    }
}
