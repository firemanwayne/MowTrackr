using MowTrackr.Maintenance.Models;

namespace MowTrackr.Maintenance.Lookups;

/// <summary>
/// Default <see cref="IOpenRepairLookup"/>: equipment with a repair work order that is scheduled or in progress.
/// </summary>
/// <param name="workOrders">The work order repository.</param>
internal sealed class OpenRepairLookup(IRepository<WorkOrder> workOrders) : IOpenRepairLookup
{
    /// <inheritdoc />
    public async Task<IReadOnlySet<Guid>> GetMowerIdsNeedingRepairAsync(CancellationToken cancellationToken)
    {
        var spec = new QuerySpec<WorkOrder>
        {
            Filter = w => w.Kind == WorkOrderKind.Repair && (w.Status == WorkOrderStatus.Scheduled || w.Status == WorkOrderStatus.InProgress),
        };

        var open = await workOrders
            .ListAsync(spec, cancellationToken)
            .ConfigureAwait(false);

        return open.Select(w => w.MowerId).ToHashSet();
    }
}
