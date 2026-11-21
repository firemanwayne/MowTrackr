namespace MowTrackr.Application.Services;

/// <summary>
/// Default <see cref="IMaintenanceApiService"/>.
/// </summary>
/// <param name="http">The HTTP client.</param>
/// <param name="localizer">The string localizer for error messages.</param>
internal sealed class MaintenanceApiService(HttpClient http, IStringLocalizer<SharedResource> localizer) : ApiServiceBase(http, localizer), IMaintenanceApiService
{
    /// <inheritdoc />
    public Task<ApiResult<List<MaintenanceDueResponse>>> GetDueAsync(CancellationToken cancellationToken)
    {
        return GetAsync<List<MaintenanceDueResponse>>($"{ApiRoutes.Maintenance}/due", cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<WorkOrderResponse>> CreateWorkOrderAsync(WorkOrderCreateRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<WorkOrderCreateRequest, WorkOrderResponse>(HttpMethod.Post, $"{ApiRoutes.Maintenance}/work-orders", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<List<WorkOrderResponse>>> ListWorkOrdersAsync(CancellationToken cancellationToken)
    {
        return GetAsync<List<WorkOrderResponse>>($"{ApiRoutes.Maintenance}/work-orders", cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<WorkOrderResponse>> GetWorkOrderAsync(Guid id, CancellationToken cancellationToken)
    {
        return GetAsync<WorkOrderResponse>($"{ApiRoutes.Maintenance}/work-orders/{id}", cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<WorkOrderResponse>> SetTaskAsync(Guid workOrderId, Guid taskId, bool isComplete, CancellationToken cancellationToken)
    {
        var body = new WorkOrderTaskUpdateRequest { IsComplete = isComplete };

        return SendAsync<WorkOrderTaskUpdateRequest, WorkOrderResponse>(HttpMethod.Put, $"{ApiRoutes.Maintenance}/work-orders/{workOrderId}/tasks/{taskId}", body, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<WorkOrderResponse>> CompleteAsync(Guid id, CompleteWorkOrderRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<CompleteWorkOrderRequest, WorkOrderResponse>(HttpMethod.Post, $"{ApiRoutes.Maintenance}/work-orders/{id}/complete", request, cancellationToken);
    }
}