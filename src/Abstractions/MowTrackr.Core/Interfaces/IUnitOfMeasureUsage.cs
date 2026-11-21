namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Reports where a unit of measure is still used, so it cannot be deleted while it is. Each domain that uses units registers one.
/// </summary>
public interface IUnitOfMeasureUsage
{
    /// <summary>Describes how a unit is used.</summary>
    /// <param name="unitId">The unit key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A short description such as "3 mowers", or <see langword="null"/> when it is not used.</returns>
    Task<string?> DescribeUseAsync(Guid unitId, CancellationToken cancellationToken);
}
