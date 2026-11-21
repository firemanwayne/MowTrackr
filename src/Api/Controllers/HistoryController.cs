using MowTrackr.Api.Extensions;

namespace MowTrackr.Api.Controllers;

/// <summary>
/// The change history of an item: who created, changed or deleted it, and what changed. Admins only.
/// </summary>
/// <param name="history">The change history service.</param>
[ApiController]
[Authorize(Roles = AppRoles.Admin)]
[Route(ApiRoutes.History)]
public sealed class HistoryController(IChangeHistoryService history) : ControllerBase
{
    /// <summary>Lists the changes made to one item, newest first.</summary>
    /// <param name="entityType">The entity name, such as <c>Mower</c>.</param>
    /// <param name="entityId">The item's key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The history entries.</returns>
    [HttpGet("{entityType}/{entityId:guid}")]
    public async Task<IActionResult> ListAsync(string entityType, Guid entityId, CancellationToken cancellationToken)
    {
        var result = await history
            .ListAsync(entityType, entityId, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }
}
