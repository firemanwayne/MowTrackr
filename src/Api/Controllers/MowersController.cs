using MowTrackr.Api.Extensions;
using MowTrackr.Core.Contracts.Assets;

namespace MowTrackr.Api.Controllers;

/// <summary>
/// Mower asset register. Anyone signed in can read; managers and admins can change.
/// </summary>
/// <param name="mowers">The mower service.</param>
[ApiController]
[Authorize]
[Route(ApiRoutes.Mowers)]
public sealed class MowersController(IMowerService mowers) : ControllerBase
{
    /// <summary>Lists all mowers.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The mowers.</returns>
    [HttpGet]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        var result = await mowers
            .ListAsync(cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Gets one mower.</summary>
    /// <param name="id">The mower key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The mower.</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await mowers
            .GetAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Adds a mower.</summary>
    /// <param name="request">The mower details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created mower.</returns>
    [HttpPost]
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public async Task<IActionResult> CreateAsync([FromBody] MowerRequest request, CancellationToken cancellationToken)
    {
        var result = await mowers
            .CreateAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Updates a mower.</summary>
    /// <param name="id">The mower key.</param>
    /// <param name="request">The new details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated mower.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] MowerRequest request, CancellationToken cancellationToken)
    {
        var result = await mowers
            .UpdateAsync(id, request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Deletes a mower.</summary>
    /// <param name="id">The mower key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await mowers
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return result.ToNoContentResult();
    }
}