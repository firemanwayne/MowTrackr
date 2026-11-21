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
    public Task<ApiResult<WorkOrderResponse>> SetTaskAsync(Guid workOrderId, Guid taskId, WorkOrderTaskUpdateRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<WorkOrderTaskUpdateRequest, WorkOrderResponse>(HttpMethod.Put, $"{ApiRoutes.Maintenance}/work-orders/{workOrderId}/tasks/{taskId}", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<WorkOrderResponse>> CompleteAsync(Guid id, CompleteWorkOrderRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<CompleteWorkOrderRequest, WorkOrderResponse>(HttpMethod.Post, $"{ApiRoutes.Maintenance}/work-orders/{id}/complete", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<WorkOrderResponse>> SubmitCheckAsync(Guid workOrderId, InspectionTiming timing, SubmitCheckRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<SubmitCheckRequest, WorkOrderResponse>(HttpMethod.Post, $"{ApiRoutes.Maintenance}/work-orders/{workOrderId}/checks/{timing}", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<WorkOrderResponse>> CreateRepairAsync(Guid workOrderId, Guid taskId, CancellationToken cancellationToken)
    {
        return SendAsync<WorkOrderResponse>(HttpMethod.Post, $"{ApiRoutes.Maintenance}/work-orders/{workOrderId}/tasks/{taskId}/repair", cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<bool>> DeleteWorkOrderAsync(Guid id, CancellationToken cancellationToken)
    {
        return SendAsync<bool>(HttpMethod.Delete, $"{ApiRoutes.Maintenance}/work-orders/{id}", cancellationToken);
    }
}
