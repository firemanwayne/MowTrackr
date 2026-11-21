using MowTrackr.Api.Extensions;
using MowTrackr.Core.Contracts.Assets;

namespace MowTrackr.Api.Controllers;

/// <summary>
/// Units of measure (quart, ounce, gallon, each, ...). Anyone signed in can read the list; only admins can add, rename or delete.
/// </summary>
/// <param name="units">The unit service.</param>
[ApiController]
[Authorize]
[Route(ApiRoutes.Units)]
public sealed class UnitsController(IUnitOfMeasureService units) : ControllerBase
{
    /// <summary>Lists the units.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The units.</returns>
    [HttpGet]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        var result = await units
            .ListAsync(cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Adds a unit (admins only).</summary>
    /// <param name="request">The name and label.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created unit.</returns>
    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> CreateAsync([FromBody] UnitOfMeasureRequest request, CancellationToken cancellationToken)
    {
        var result = await units
            .CreateAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Renames a unit (admins only).</summary>
    /// <param name="id">The unit key.</param>
    /// <param name="request">The new name and label.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated unit.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UnitOfMeasureRequest request, CancellationToken cancellationToken)
    {
        var result = await units
            .UpdateAsync(id, request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Deletes a unit that nothing uses (admins only).</summary>
    /// <param name="id">The unit key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content when deleted.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await units
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return result.ToNoContentResult();
    }
}
