using MowTrackr.Api.Extensions;
using MowTrackr.Core.Contracts.Assets;

namespace MowTrackr.Api.Controllers;

/// <summary>
/// Asset types. Anyone signed in can read the list; only admins can add, rename or delete.
/// </summary>
/// <param name="assetTypes">The asset type service.</param>
[ApiController]
[Authorize]
[Route(ApiRoutes.AssetTypes)]
public sealed class AssetTypesController(IAssetTypeService assetTypes) : ControllerBase
{
    /// <summary>Lists the asset types.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The asset types.</returns>
    [HttpGet]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        var result = await assetTypes
            .ListAsync(cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Adds an asset type (admins only).</summary>
    /// <param name="request">The type name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created type.</returns>
    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> CreateAsync([FromBody] AssetTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await assetTypes
            .CreateAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Renames an asset type (admins only).</summary>
    /// <param name="id">The type key.</param>
    /// <param name="request">The new name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated type.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] AssetTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await assetTypes
            .UpdateAsync(id, request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Deletes an asset type that nothing uses (admins only).</summary>
    /// <param name="id">The type key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await assetTypes
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return result.ToNoContentResult();
    }
}
