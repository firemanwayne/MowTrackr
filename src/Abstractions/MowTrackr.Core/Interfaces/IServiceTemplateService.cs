using MowTrackr.Core.Contracts.Maintenance;

namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Manages service templates (named checklists with a maintenance interval).
/// </summary>
public interface IServiceTemplateService
{
    /// <summary>Lists all templates.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The templates.</returns>
    Task<Result<IReadOnlyList<ServiceTemplateResponse>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Gets one template.</summary>
    /// <param name="id">The template key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The template.</returns>
    Task<Result<ServiceTemplateResponse>> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Creates a template. Blocked (conflict) when its interval duplicates another template.</summary>
    /// <param name="request">The template details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saved template and any validation findings.</returns>
    Task<Result<ServiceTemplateSaveResponse>> CreateAsync(ServiceTemplateRequest request, CancellationToken cancellationToken);

    /// <summary>Updates a template. Blocked (conflict) when its interval duplicates another template.</summary>
    /// <param name="id">The template key.</param>
    /// <param name="request">The new details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saved template and any validation findings.</returns>
    Task<Result<ServiceTemplateSaveResponse>> UpdateAsync(Guid id, ServiceTemplateRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a template.</summary>
    /// <param name="id">The template key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Checks a candidate interval without saving (for live feedback in the editor).</summary>
    /// <param name="request">The candidate interval.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Errors and warnings.</returns>
    Task<Result<IReadOnlyList<IntervalIssue>>> CheckIntervalsAsync(IntervalCheckRequest request, CancellationToken cancellationToken);
}