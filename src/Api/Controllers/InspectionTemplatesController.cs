using MowTrackr.Api.Extensions;
using MowTrackr.Core.Contracts.Maintenance;

namespace MowTrackr.Api.Controllers;

/// <summary>
/// Inspection templates (extra checklist steps added to work orders by age or engine hours). Managers and admins only.
/// </summary>
/// <param name="templates">The inspection template service.</param>
[ApiController]
[Authorize(Roles = AppRoles.ManagerOrAdmin)]
[Route(ApiRoutes.InspectionTemplates)]
public sealed class InspectionTemplatesController(IInspectionTemplateService templates) : ControllerBase
{
    /// <summary>Lists all inspection templates.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The templates.</returns>
    [HttpGet]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        var result = await templates
            .ListAsync(cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Gets one inspection template.</summary>
    /// <param name="id">The template key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The template.</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await templates
            .GetAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Creates an inspection template.</summary>
    /// <param name="request">The template details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created template.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] InspectionTemplateRequest request, CancellationToken cancellationToken)
    {
        var result = await templates
            .CreateAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Updates an inspection template.</summary>
    /// <param name="id">The template key.</param>
    /// <param name="request">The new details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated template.</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] InspectionTemplateRequest request, CancellationToken cancellationToken)
    {
        var result = await templates
            .UpdateAsync(id, request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Deletes an inspection template.</summary>
    /// <param name="id">The template key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await templates
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }
}
