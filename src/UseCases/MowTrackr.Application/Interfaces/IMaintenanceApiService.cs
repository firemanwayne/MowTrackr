namespace MowTrackr.Application.Interfaces;

/// <summary>
/// Client for the maintenance and work order endpoints.
/// </summary>
internal interface IMaintenanceApiService
{
    /// <summary>Lists due and coming-due service.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The due items.</returns>
    Task<ApiResult<List<MaintenanceDueResponse>>> GetDueAsync(CancellationToken cancellationToken);

    /// <summary>Schedules a work order.</summary>
    /// <param name="request">The details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created work order.</returns>
    Task<ApiResult<WorkOrderResponse>> CreateWorkOrderAsync(WorkOrderCreateRequest request, CancellationToken cancellationToken);

    /// <summary>Lists work orders visible to the signed-in user.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The work orders.</returns>
    Task<ApiResult<List<WorkOrderResponse>>> ListWorkOrdersAsync(CancellationToken cancellationToken);

    /// <summary>Gets one work order.</summary>
    /// <param name="id">The work order key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The work order.</returns>
    Task<ApiResult<WorkOrderResponse>> GetWorkOrderAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Checks or unchecks a task.</summary>
    /// <param name="workOrderId">The work order key.</param>
    /// <param name="taskId">The task key.</param>
    /// <param name="request">The desired state, and the details to save with the step.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated work order.</returns>
    Task<ApiResult<WorkOrderResponse>> SetTaskAsync(Guid workOrderId, Guid taskId, WorkOrderTaskUpdateRequest request, CancellationToken cancellationToken);

    /// <summary>Completes a work order.</summary>
    /// <param name="id">The work order key.</param>
    /// <param name="request">Completion details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The completed work order.</returns>
    Task<ApiResult<WorkOrderResponse>> CompleteAsync(Guid id, CompleteWorkOrderRequest request, CancellationToken cancellationToken);

    /// <summary>Submits the pre-check or post-check of a work order.</summary>
    /// <param name="workOrderId">The work order key.</param>
    /// <param name="timing">Before (pre-check) or After (post-check).</param>
    /// <param name="request">The answers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated work order.</returns>
    Task<ApiResult<WorkOrderResponse>> SubmitCheckAsync(Guid workOrderId, InspectionTiming timing, SubmitCheckRequest request, CancellationToken cancellationToken);

    /// <summary>Creates a repair work order for a finding on a check item.</summary>
    /// <param name="workOrderId">The work order the finding is on.</param>
    /// <param name="taskId">The check item.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The new repair work order.</returns>
    Task<ApiResult<WorkOrderResponse>> CreateRepairAsync(Guid workOrderId, Guid taskId, CancellationToken cancellationToken);

    /// <summary>Permanently deletes a work order (admins only).</summary>
    /// <param name="id">The work order key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The outcome.</returns>
    Task<ApiResult<bool>> DeleteWorkOrderAsync(Guid id, CancellationToken cancellationToken);
}
