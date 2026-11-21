namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Tells the equipment list which units have a repair that is not finished, so Assets does not need to know about
/// Maintenance. Implemented by Maintenance.
/// </summary>
public interface IOpenRepairLookup
{
    /// <summary>Gets the keys of the equipment that has at least one open repair work order.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The equipment keys.</returns>
    Task<IReadOnlySet<Guid>> GetMowerIdsNeedingRepairAsync(CancellationToken cancellationToken);
}
