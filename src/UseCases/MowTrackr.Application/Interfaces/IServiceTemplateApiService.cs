namespace MowTrackr.Application.Interfaces;

/// <summary>
/// Client for the service template endpoints.
/// </summary>
internal interface IServiceTemplateApiService
{
    /// <summary>Lists templates.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The templates.</returns>
    Task<ApiResult<List<ServiceTemplateResponse>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Creates a template.</summary>
    /// <param name="request">The details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saved template (or the blocking findings on a 409).</returns>
    Task<ApiResult<ServiceTemplateSaveResponse>> CreateAsync(ServiceTemplateRequest request, CancellationToken cancellationToken);

    /// <summary>Updates a template.</summary>
    /// <param name="id">The template key.</param>
    /// <param name="request">The new details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saved template (or the blocking findings on a 409).</returns>
    Task<ApiResult<ServiceTemplateSaveResponse>> UpdateAsync(Guid id, ServiceTemplateRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a template.</summary>
    /// <param name="id">The template key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result.</returns>
    Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Checks a candidate interval against other templates.</summary>
    /// <param name="request">The candidate interval.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The findings.</returns>
    Task<ApiResult<List<IntervalIssue>>> CheckIntervalsAsync(IntervalCheckRequest request, CancellationToken cancellationToken);
}