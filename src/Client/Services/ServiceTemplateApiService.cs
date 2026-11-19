namespace MowTrackr.Client.Services;

/// <summary>
/// Default <see cref="IServiceTemplateApiService"/>.
/// </summary>
/// <param name="http">The HTTP client.</param>
/// <param name="localizer">The string localizer for error messages.</param>
internal sealed class ServiceTemplateApiService(HttpClient http, IStringLocalizer<SharedResource> localizer) : ApiServiceBase(http, localizer), IServiceTemplateApiService
{
    /// <inheritdoc />
    public Task<ApiResult<List<ServiceTemplateResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        return GetAsync<List<ServiceTemplateResponse>>(ApiRoutes.ServiceTemplates, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<ServiceTemplateSaveResponse>> CreateAsync(ServiceTemplateRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<ServiceTemplateRequest, ServiceTemplateSaveResponse>(HttpMethod.Post, ApiRoutes.ServiceTemplates, request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<ServiceTemplateSaveResponse>> UpdateAsync(Guid id, ServiceTemplateRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<ServiceTemplateRequest, ServiceTemplateSaveResponse>(HttpMethod.Put, $"{ApiRoutes.ServiceTemplates}/{id}", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return SendAsync<bool>(HttpMethod.Delete, $"{ApiRoutes.ServiceTemplates}/{id}", cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<List<IntervalIssue>>> CheckIntervalsAsync(IntervalCheckRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<IntervalCheckRequest, List<IntervalIssue>>(HttpMethod.Post, $"{ApiRoutes.ServiceTemplates}/check-intervals", request, cancellationToken);
    }
}