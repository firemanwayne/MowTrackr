namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Reports where an oil type is still used, so it cannot be deleted while it is.
/// </summary>
public interface IOilTypeUsage
{
    /// <summary>Describes how an oil type is used.</summary>
    /// <param name="oilTypeId">The oil type key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A short description such as "3 mowers", or <see langword="null"/> when it is not used.</returns>
    Task<string?> DescribeUseAsync(Guid oilTypeId, CancellationToken cancellationToken);
}
