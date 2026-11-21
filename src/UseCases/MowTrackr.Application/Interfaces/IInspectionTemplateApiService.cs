namespace MowTrackr.Application.Interfaces;

/// <summary>
/// Client for the inspection template endpoints.
/// </summary>
internal interface IInspectionTemplateApiService
{
    /// <summary>Lists all inspection templates.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The templates.</returns>
    Task<ApiResult<List<InspectionTemplateResponse>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Creates an inspection template.</summary>
    /// <param name="request">The template details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created template.</returns>
    Task<ApiResult<InspectionTemplateResponse>> CreateAsync(InspectionTemplateRequest request, CancellationToken cancellationToken);

    /// <summary>Updates an inspection template.</summary>
    /// <param name="id">The template key.</param>
    /// <param name="request">The new details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated template.</returns>
    Task<ApiResult<InspectionTemplateResponse>> UpdateAsync(Guid id, InspectionTemplateRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes an inspection template.</summary>
    /// <param name="id">The template key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
