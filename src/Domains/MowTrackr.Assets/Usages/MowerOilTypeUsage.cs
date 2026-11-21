using MowTrackr.Assets.Models;

namespace MowTrackr.Assets.Usages;

/// <summary>
/// Reports mowers that require an oil type, so the oil type cannot be deleted while they do.
/// </summary>
/// <param name="mowers">The mower repository.</param>
internal sealed class MowerOilTypeUsage(IRepository<Mower> mowers) : IOilTypeUsage
{
    /// <inheritdoc />
    public async Task<string?> DescribeUseAsync(Guid oilTypeId, CancellationToken cancellationToken)
    {
        var items = await mowers
            .ListAsync(new QuerySpec<Mower> { Filter = m => m.OilTypeId == oilTypeId }, cancellationToken)
            .ConfigureAwait(false);

        return items.Count == 0 ? null : $"{items.Count} mower(s)";
    }
}
