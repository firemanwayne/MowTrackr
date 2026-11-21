using MowTrackr.Core.Contracts.Maintenance;

namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Manages inspection templates: extra checklist steps added to work orders by an equipment's age or engine hours.
/// </summary>
public interface IInspectionTemplateService
{
    /// <summary>Lists all inspection templates.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The templates.</returns>
    Task<Result<IReadOnlyList<InspectionTemplateResponse>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Gets one inspection template.</summary>
    /// <param name="id">The template key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The template.</returns>
    Task<Result<InspectionTemplateResponse>> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Creates an inspection template.</summary>
    /// <param name="request">The template details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saved template.</returns>
    Task<Result<InspectionTemplateResponse>> CreateAsync(InspectionTemplateRequest request, CancellationToken cancellationToken);

    /// <summary>Updates an inspection template.</summary>
    /// <param name="id">The template key.</param>
    /// <param name="request">The new details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saved template.</returns>
    Task<Result<InspectionTemplateResponse>> UpdateAsync(Guid id, InspectionTemplateRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes an inspection template. Work orders already created keep their copied steps.</summary>
    /// <param name="id">The template key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
