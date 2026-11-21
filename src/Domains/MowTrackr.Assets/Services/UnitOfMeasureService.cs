using MowTrackr.Assets.Models;

namespace MowTrackr.Assets.Services;

/// <summary>
/// Default <see cref="IUnitOfMeasureService"/>: the list of units, with admin changes and a guard against deleting one in use.
/// </summary>
/// <param name="units">The unit repository.</param>
/// <param name="usages">Everything that can report a unit still being used.</param>
/// <param name="logger">The logger.</param>
internal sealed class UnitOfMeasureService(IRepository<UnitOfMeasure> units, IEnumerable<IUnitOfMeasureUsage> usages, ILogger<UnitOfMeasureService> logger) : IUnitOfMeasureService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<UnitOfMeasureResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var items = await units
            .ListAsync(new QuerySpec<UnitOfMeasure> { OrderBy = u => u.Name }, cancellationToken)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<UnitOfMeasureResponse>>.Ok(items.Select(ToResponse).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<UnitOfMeasureResponse>> CreateAsync(UnitOfMeasureRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        var abbreviation = request.Abbreviation.Trim();

        if (await ExistsAsync(name, abbreviation, null, cancellationToken).ConfigureAwait(false))
        {
            return Result<UnitOfMeasureResponse>.Conflict("A unit with that name or label already exists.");
        }

        var unit = new UnitOfMeasure { Name = name, Abbreviation = abbreviation };

        await units.AddAsync(unit, cancellationToken).ConfigureAwait(false);

        await units.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Created unit {UnitId} ({UnitName})", unit.Id, unit.Name);

        return Result<UnitOfMeasureResponse>.Ok(ToResponse(unit));
    }

    /// <inheritdoc />
    public async Task<Result<UnitOfMeasureResponse>> UpdateAsync(Guid id, UnitOfMeasureRequest request, CancellationToken cancellationToken)
    {
        var unit = await units
            .GetByIdAsync(id, new QuerySpec<UnitOfMeasure> { Track = true }, cancellationToken)
            .ConfigureAwait(false);

        if (unit is null)
        {
            return Result<UnitOfMeasureResponse>.NotFound("Unit not found.");
        }

        var name = request.Name.Trim();

        var abbreviation = request.Abbreviation.Trim();

        if (await ExistsAsync(name, abbreviation, id, cancellationToken).ConfigureAwait(false))
        {
            return Result<UnitOfMeasureResponse>.Conflict("A unit with that name or label already exists.");
        }

        unit.Name = name;

        unit.Abbreviation = abbreviation;

        await units.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Updated unit {UnitId}", id);

        return Result<UnitOfMeasureResponse>.Ok(ToResponse(unit));
    }

    /// <inheritdoc />
    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var unit = await units
            .GetByIdAsync(id, new QuerySpec<UnitOfMeasure> { Track = true }, cancellationToken)
            .ConfigureAwait(false);

        if (unit is null)
        {
            return Result<bool>.NotFound("Unit not found.");
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
            return Result<bool>.Conflict($"This unit is still used by {string.Join(" and ", inUse)}. Change those first.");
        }

        units.Remove(unit);

        await units.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Deleted unit {UnitId}", id);

        return Result<bool>.Ok(true);
    }

    /// <summary>Checks whether another unit already has the name or label.</summary>
    /// <param name="name">The name.</param>
    /// <param name="abbreviation">The label.</param>
    /// <param name="excludeId">The unit being edited, if any.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when another unit has it.</returns>
    private async Task<bool> ExistsAsync(string name, string abbreviation, Guid? excludeId, CancellationToken cancellationToken)
    {
        var matches = await units
            .ListAsync(new QuerySpec<UnitOfMeasure> { Filter = u => (u.Name == name || u.Abbreviation == abbreviation) && u.Id != excludeId }, cancellationToken)
            .ConfigureAwait(false);

        return matches.Count > 0;
    }

    /// <summary>Maps a unit to its response.</summary>
    /// <param name="unit">The unit.</param>
    /// <returns>The response.</returns>
    private static UnitOfMeasureResponse ToResponse(UnitOfMeasure unit)
    {
        return new UnitOfMeasureResponse(unit.Id, unit.Name, unit.Abbreviation);
    }
}
