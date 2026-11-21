using MowTrackr.Api.Extensions;
using MowTrackr.Core.Contracts.Maintenance;

namespace MowTrackr.Api.Controllers;

/// <summary>
/// The due-maintenance check and the work order lifecycle. Technicians only see and work their own assigned orders.
/// </summary>
/// <param name="maintenance">The maintenance service.</param>
[ApiController]
[Authorize]
[Route(ApiRoutes.Maintenance)]
public sealed class MaintenanceController(IMaintenanceService maintenance) : ControllerBase
{
    /// <summary>Lists mower/service pairs that are due or coming due.</summary>
    /// <param name="query">Optional lookahead overrides.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The due items, most urgent first.</returns>
    [HttpGet("due")]
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public async Task<IActionResult> GetDueAsync([FromQuery] MaintenanceDueQuery query, CancellationToken cancellationToken)
    {
        var result = await maintenance
            .GetDueAsync(query, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Schedules a work order from a template.</summary>
    /// <param name="request">The work order details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created work order.</returns>
    [HttpPost("work-orders")]
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public async Task<IActionResult> CreateWorkOrderAsync([FromBody] WorkOrderCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await maintenance
            .CreateWorkOrderAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Lists work orders. Technicians are limited to their own.</summary>
    /// <param name="query">Optional filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The work orders.</returns>
    [HttpGet("work-orders")]
    public async Task<IActionResult> ListWorkOrdersAsync([FromQuery] WorkOrderListQuery query, CancellationToken cancellationToken)
    {
        if (!User.IsManagerOrAdmin())
        {
            query.AssignedToUserId = User.GetUserId();
        }

        var result = await maintenance
            .ListWorkOrdersAsync(query, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Gets one work order.</summary>
    /// <param name="id">The work order key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The work order.</returns>
    [HttpGet("work-orders/{id:guid}")]
    public async Task<IActionResult> GetWorkOrderAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await maintenance
            .GetWorkOrderAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (result.IsSuccess && !CanAccess(result.Value!))
        {
            return Forbid();
        }

        return result.ToActionResult();
    }

    /// <summary>Checks or unchecks a task on a work order.</summary>
    /// <param name="id">The work order key.</param>
    /// <param name="taskId">The task key.</param>
    /// <param name="request">The desired state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated work order.</returns>
    [HttpPut("work-orders/{id:guid}/tasks/{taskId:guid}")]
    public async Task<IActionResult> SetTaskAsync(Guid id, Guid taskId, [FromBody] WorkOrderTaskUpdateRequest request, CancellationToken cancellationToken)
    {
        var command = new TaskCompletionCommand(id, taskId, request.IsComplete, User.GetUserId(), User.IsManagerOrAdmin(), request.Details);

        var result = await maintenance
            .SetTaskCompletionAsync(command, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Submits the pre-check or post-check of a work order. Any technician on the order may submit it.</summary>
    /// <param name="id">The work order key.</param>
    /// <param name="timing">Which check: <c>Before</c> (pre-check) or <c>After</c> (post-check).</param>
    /// <param name="request">The answers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated work order.</returns>
    [HttpPost("work-orders/{id:guid}/checks/{timing}")]
    public async Task<IActionResult> SubmitCheckAsync(Guid id, InspectionTiming timing, [FromBody] SubmitCheckRequest request, CancellationToken cancellationToken)
    {
        var command = new CheckSubmissionCommand(id, timing, request.Answers, User.GetUserId(), User.IsManagerOrAdmin());

        var result = await maintenance
            .SubmitCheckAsync(command, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Creates a repair work order for a problem found by a check item. The technician confirms each one.</summary>
    /// <param name="id">The work order the finding is on.</param>
    /// <param name="taskId">The check item that found the problem.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The new repair work order.</returns>
    [HttpPost("work-orders/{id:guid}/tasks/{taskId:guid}/repair")]
    public async Task<IActionResult> CreateRepairAsync(Guid id, Guid taskId, CancellationToken cancellationToken)
    {
        var command = new RepairCommand(id, taskId, User.GetUserId(), User.IsManagerOrAdmin());

        var result = await maintenance
            .CreateRepairAsync(command, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Permanently deletes a work order (admins only).</summary>
    /// <param name="id">The work order key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content when deleted.</returns>
    [HttpDelete("work-orders/{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> DeleteWorkOrderAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await maintenance
            .DeleteWorkOrderAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return result.ToNoContentResult();
    }

    /// <summary>Completes a work order once every task is checked off.</summary>
    /// <param name="id">The work order key.</param>
    /// <param name="request">Completion details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The completed work order.</returns>
    [HttpPost("work-orders/{id:guid}/complete")]
    public async Task<IActionResult> CompleteAsync(Guid id, [FromBody] CompleteWorkOrderRequest request, CancellationToken cancellationToken)
    {
        var existing = await maintenance
            .GetWorkOrderAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing.IsSuccess && !CanAccess(existing.Value!))
        {
            return Forbid();
        }

        var result = await maintenance
            .CompleteWorkOrderAsync(id, request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Checks whether the caller may see or act on a work order.</summary>
    /// <param name="order">The work order.</param>
    /// <returns><see langword="true"/> for managers/admins and the assigned technician.</returns>
    private bool CanAccess(WorkOrderResponse order)
    {
        return User.IsManagerOrAdmin() || order.AssignedToUserId == User.GetUserId();
    }
}