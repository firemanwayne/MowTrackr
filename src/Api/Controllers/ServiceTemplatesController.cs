using MowTrackr.Api.Extensions;
using MowTrackr.Core.Contracts.Maintenance;

namespace MowTrackr.Api.Controllers;

/// <summary>
/// Service templates (named checklists with a due interval). Anyone signed in can read; managers and admins can change.
/// </summary>
/// <param name="templates">The template service.</param>
[ApiController]
[Authorize]
[Route(ApiRoutes.ServiceTemplates)]
public sealed class ServiceTemplatesController(IServiceTemplateService templates) : ControllerBase
{
    /// <summary>Lists all templates.</summary>
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

    /// <summary>Gets one template.</summary>
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

    /// <summary>Creates a template. Returns 409 with the findings when an interval conflicts with another template.</summary>
    /// <param name="request">The template details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saved template and any warnings.</returns>
    [HttpPost]
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public async Task<IActionResult> CreateAsync([FromBody] ServiceTemplateRequest request, CancellationToken cancellationToken)
    {
        var result = await templates
            .CreateAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Updates a template. Returns 409 with the findings when an interval conflicts with another template.</summary>
    /// <param name="id">The template key.</param>
    /// <param name="request">The new details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saved template and any warnings.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] ServiceTemplateRequest request, CancellationToken cancellationToken)
    {
        var result = await templates
            .UpdateAsync(id, request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Deletes a template that has no work orders.</summary>
    /// <param name="id">The template key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await templates
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return result.ToNoContentResult();
    }

    /// <summary>Checks a candidate interval against the other templates without saving.</summary>
    /// <param name="request">The candidate interval.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Errors and warnings.</returns>
    [HttpPost("check-intervals")]
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public async Task<IActionResult> CheckIntervalsAsync([FromBody] IntervalCheckRequest request, CancellationToken cancellationToken)
    {
        var result = await templates
            .CheckIntervalsAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }
}