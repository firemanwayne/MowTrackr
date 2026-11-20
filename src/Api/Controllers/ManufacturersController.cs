using MowTrackr.Api.Extensions;
using MowTrackr.Core.Contracts.Assets;

namespace MowTrackr.Api.Controllers;

/// <summary>
/// Manufacturer and model lookup lists. Anyone signed in can read; managers and admins can add.
/// </summary>
/// <param name="manufacturers">The manufacturer service.</param>
[ApiController]
[Authorize]
[Route(ApiRoutes.Manufacturers)]
public sealed class ManufacturersController(IManufacturerService manufacturers) : ControllerBase
{
    /// <summary>Lists manufacturers with their models.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The manufacturers.</returns>
    [HttpGet]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        var result = await manufacturers.ListAsync(cancellationToken).ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Adds a manufacturer.</summary>
    /// <param name="request">The manufacturer name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created manufacturer.</returns>
    [HttpPost]
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public async Task<IActionResult> CreateAsync([FromBody] ManufacturerRequest request, CancellationToken cancellationToken)
    {
        var result = await manufacturers.CreateAsync(request, cancellationToken).ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Adds a model to a manufacturer.</summary>
    /// <param name="id">The manufacturer key.</param>
    /// <param name="request">The model name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created model.</returns>
    [HttpPost("{id:guid}/models")]
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public async Task<IActionResult> CreateModelAsync(Guid id, [FromBody] ManufacturerRequest request, CancellationToken cancellationToken)
    {
        var result = await manufacturers.CreateModelAsync(id, request, cancellationToken).ConfigureAwait(false);

        return result.ToActionResult();
    }
}
