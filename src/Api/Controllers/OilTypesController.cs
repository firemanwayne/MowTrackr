using MowTrackr.Api.Extensions;
using MowTrackr.Core.Contracts.Assets;

namespace MowTrackr.Api.Controllers;

/// <summary>
/// Oil types (SAE 10W-30, synthetic 5W-30, 2-cycle mix, ...). Anyone signed in can read the list; only admins can add, rename or delete.
/// </summary>
/// <param name="oilTypes">The oil type service.</param>
[ApiController]
[Authorize]
[Route(ApiRoutes.OilTypes)]
public sealed class OilTypesController(IOilTypeService oilTypes) : ControllerBase
{
    /// <summary>Lists the oil types.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The oil types.</returns>
    [HttpGet]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        var result = await oilTypes
            .ListAsync(cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Adds an oil type (admins only).</summary>
    /// <param name="request">The name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created oil type.</returns>
    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> CreateAsync([FromBody] OilTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await oilTypes
            .CreateAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Renames an oil type (admins only).</summary>
    /// <param name="id">The oil type key.</param>
    /// <param name="request">The new name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated oil type.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] OilTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await oilTypes
            .UpdateAsync(id, request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Deletes an oil type that nothing uses (admins only).</summary>
    /// <param name="id">The oil type key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content when deleted.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await oilTypes
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return result.ToNoContentResult();
    }
}
