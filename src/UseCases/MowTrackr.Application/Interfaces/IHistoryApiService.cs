namespace MowTrackr.Application.Interfaces;

/// <summary>
/// Client for the change history endpoint (Admin only).
/// </summary>
internal interface IHistoryApiService
{
    /// <summary>Lists the changes made to one item, newest first.</summary>
    /// <param name="entityType">The entity name, such as <see cref="HistoryEntityTypes.Mower"/>.</param>
    /// <param name="entityId">The item's key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The history entries.</returns>
    Task<ApiResult<List<ChangeHistoryResponse>>> ListAsync(string entityType, Guid entityId, CancellationToken cancellationToken);
}
