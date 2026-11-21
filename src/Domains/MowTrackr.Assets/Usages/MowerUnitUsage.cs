using MowTrackr.Assets.Models;

namespace MowTrackr.Assets.Usages;

/// <summary>
/// Reports mowers whose oil is measured in a unit, so the unit cannot be deleted while they are.
/// </summary>
/// <param name="mowers">The mower repository.</param>
internal sealed class MowerUnitUsage(IRepository<Mower> mowers) : IUnitOfMeasureUsage
{
    /// <inheritdoc />
    public async Task<string?> DescribeUseAsync(Guid unitId, CancellationToken cancellationToken)
    {
        var items = await mowers
            .ListAsync(new QuerySpec<Mower> { Filter = m => m.OilUnitId == unitId }, cancellationToken)
            .ConfigureAwait(false);

        return items.Count == 0 ? null : $"{items.Count} mower(s)";
    }
}
