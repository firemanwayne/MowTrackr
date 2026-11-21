using MowTrackr.Core.Contracts.Maintenance;

namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Finds mowers needing service and runs work orders.
/// </summary>
public interface IMaintenanceService
{
    /// <summary>Lists mower/template pairs that are due or coming due.</summary>
    /// <param name="query">Lookahead options; omitted values use the configured defaults.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The due items, most urgent first.</returns>
    Task<Result<IReadOnlyList<MaintenanceDueResponse>>> GetDueAsync(MaintenanceDueQuery query, CancellationToken cancellationToken);

    /// <summary>Schedules a work order, copying the template checklist.</summary>
    /// <param name="request">The work order details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created work order.</returns>
    Task<Result<WorkOrderResponse>> CreateWorkOrderAsync(WorkOrderCreateRequest request, CancellationToken cancellationToken);

    /// <summary>Lists work orders.</summary>
    /// <param name="query">Filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The work orders.</returns>
    Task<Result<IReadOnlyList<WorkOrderResponse>>> ListWorkOrdersAsync(WorkOrderListQuery query, CancellationToken cancellationToken);

    /// <summary>Gets one work order.</summary>
    /// <param name="id">The work order key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The work order.</returns>
    Task<Result<WorkOrderResponse>> GetWorkOrderAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Checks or unchecks a task.</summary>
    /// <param name="command">The task update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated work order.</returns>
    Task<Result<WorkOrderResponse>> SetTaskCompletionAsync(TaskCompletionCommand command, CancellationToken cancellationToken);

    /// <summary>Closes a work order (all tasks must be checked) and records the mower's engine hours.</summary>
    /// <param name="id">The work order key.</param>
    /// <param name="request">Completion details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The completed work order.</returns>
    Task<Result<WorkOrderResponse>> CompleteWorkOrderAsync(Guid id, CompleteWorkOrderRequest request, CancellationToken cancellationToken);
}