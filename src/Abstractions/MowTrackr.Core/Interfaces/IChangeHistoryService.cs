using MowTrackr.Core.Constants;
using MowTrackr.Core.Contracts.History;

namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Reads the change history that is recorded automatically whenever an entity is created, changed or deleted.
/// </summary>
public interface IChangeHistoryService
{
    /// <summary>Lists the changes made to one item, newest first.</summary>
    /// <param name="entityType">The entity name, such as <see cref="HistoryEntityTypes.Mower"/>.</param>
    /// <param name="entityId">The item's key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The history entries, or an error when the entity type is blank.</returns>
    Task<Result<IReadOnlyList<ChangeHistoryResponse>>> ListAsync(string entityType, Guid entityId, CancellationToken cancellationToken);
}
