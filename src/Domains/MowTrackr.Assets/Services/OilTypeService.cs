using MowTrackr.Assets.Models;

namespace MowTrackr.Assets.Services;

/// <summary>
/// Default <see cref="IOilTypeService"/>: the list of oil types, with admin changes and a guard against deleting one in use.
/// </summary>
/// <param name="oilTypes">The oil type repository.</param>
/// <param name="usages">Everything that can report an oil type still being used.</param>
/// <param name="logger">The logger.</param>
internal sealed class OilTypeService(IRepository<OilType> oilTypes, IEnumerable<IOilTypeUsage> usages, ILogger<OilTypeService> logger) : IOilTypeService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<OilTypeResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var items = await oilTypes
            .ListAsync(new QuerySpec<OilType> { OrderBy = o => o.Name }, cancellationToken)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<OilTypeResponse>>.Ok(items.Select(ToResponse).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<OilTypeResponse>> CreateAsync(OilTypeRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        if (await ExistsAsync(name, null, cancellationToken).ConfigureAwait(false))
        {
            return Result<OilTypeResponse>.Conflict("An oil type with that name already exists.");
        }

        var oilType = new OilType { Name = name };

        await oilTypes.AddAsync(oilType, cancellationToken).ConfigureAwait(false);

        await oilTypes.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Created oil type {OilTypeId} ({OilTypeName})", oilType.Id, oilType.Name);

        return Result<OilTypeResponse>.Ok(ToResponse(oilType));
    }

    /// <inheritdoc />
    public async Task<Result<OilTypeResponse>> UpdateAsync(Guid id, OilTypeRequest request, CancellationToken cancellationToken)
    {
        var oilType = await oilTypes
            .GetByIdAsync(id, new QuerySpec<OilType> { Track = true }, cancellationToken)
            .ConfigureAwait(false);

        if (oilType is null)
        {
            return Result<OilTypeResponse>.NotFound("Oil type not found.");
        }

        var name = request.Name.Trim();

        if (await ExistsAsync(name, id, cancellationToken).ConfigureAwait(false))
        {
            return Result<OilTypeResponse>.Conflict("An oil type with that name already exists.");
        }

        oilType.Name = name;

        await oilTypes.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Updated oil type {OilTypeId}", id);

        return Result<OilTypeResponse>.Ok(ToResponse(oilType));
    }

    /// <inheritdoc />
    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var oilType = await oilTypes
            .GetByIdAsync(id, new QuerySpec<OilType> { Track = true }, cancellationToken)
            .ConfigureAwait(false);

        if (oilType is null)
        {
            return Result<bool>.NotFound("Oil type not found.");
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
            return Result<bool>.Conflict($"This oil type is still used by {string.Join(" and ", inUse)}. Change those first.");
        }

        oilTypes.Remove(oilType);

        await oilTypes.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Deleted oil type {OilTypeId}", id);

        return Result<bool>.Ok(true);
    }

    /// <summary>Checks whether another oil type already has the name.</summary>
    /// <param name="name">The name.</param>
    /// <param name="excludeId">The oil type being edited, if any.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when another oil type has it.</returns>
    private async Task<bool> ExistsAsync(string name, Guid? excludeId, CancellationToken cancellationToken)
    {
        var matches = await oilTypes
            .ListAsync(new QuerySpec<OilType> { Filter = o => o.Name == name && o.Id != excludeId }, cancellationToken)
            .ConfigureAwait(false);

        return matches.Count > 0;
    }

    /// <summary>Maps an oil type to its response.</summary>
    /// <param name="oilType">The oil type.</param>
    /// <returns>The response.</returns>
    private static OilTypeResponse ToResponse(OilType oilType)
    {
        return new OilTypeResponse(oilType.Id, oilType.Name);
    }
}
