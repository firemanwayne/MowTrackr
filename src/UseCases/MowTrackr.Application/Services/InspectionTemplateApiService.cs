namespace MowTrackr.Application.Services;

/// <summary>
/// Default <see cref="IInspectionTemplateApiService"/>.
/// </summary>
/// <param name="http">The HTTP client.</param>
/// <param name="localizer">The string localizer for error messages.</param>
internal sealed class InspectionTemplateApiService(HttpClient http, IStringLocalizer<SharedResource> localizer) : ApiServiceBase(http, localizer), IInspectionTemplateApiService
{
    /// <inheritdoc />
    public Task<ApiResult<List<InspectionTemplateResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        return GetAsync<List<InspectionTemplateResponse>>(ApiRoutes.InspectionTemplates, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<InspectionTemplateResponse>> CreateAsync(InspectionTemplateRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<InspectionTemplateRequest, InspectionTemplateResponse>(HttpMethod.Post, ApiRoutes.InspectionTemplates, request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<InspectionTemplateResponse>> UpdateAsync(Guid id, InspectionTemplateRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<InspectionTemplateRequest, InspectionTemplateResponse>(HttpMethod.Put, $"{ApiRoutes.InspectionTemplates}/{id}", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return SendAsync<bool>(HttpMethod.Delete, $"{ApiRoutes.InspectionTemplates}/{id}", cancellationToken);
    }
}
