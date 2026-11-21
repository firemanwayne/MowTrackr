using MowTrackr.Api.Extensions;
using MowTrackr.Core.Contracts.Assets;

namespace MowTrackr.Api.Controllers;

/// <summary>
/// Asset type lookup list. Anyone signed in can read; managers and admins can add.
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
        var result = await assetTypes.ListAsync(cancellationToken).ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Adds an asset type.</summary>
    /// <param name="request">The type name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created type.</returns>
    [HttpPost]
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public async Task<IActionResult> CreateAsync([FromBody] AssetTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await assetTypes.CreateAsync(request, cancellationToken).ConfigureAwait(false);

        return result.ToActionResult();
    }
}
