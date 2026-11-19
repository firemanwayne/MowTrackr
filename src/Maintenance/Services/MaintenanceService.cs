using MowTrackr.Assets.Models;
using MowTrackr.Maintenance.Extensions;
using MowTrackr.Maintenance.Models;
using MowTrackr.Maintenance.Options;

namespace MowTrackr.Maintenance.Services;

/// <summary>
/// Default <see cref="IMaintenanceService"/>: due checks and the work order lifecycle.
/// </summary>
/// <param name="mowers">The mower repository.</param>
/// <param name="templates">The template repository.</param>
/// <param name="workOrders">The work order repository.</param>
/// <param name="options">The due-check defaults.</param>
/// <param name="logger">The logger.</param>
internal sealed class MaintenanceService(
    IRepository<Mower> mowers,
    IRepository<ServiceTemplate> templates,
    IRepository<WorkOrder> workOrders,
    IOptions<MaintenanceOptions> options,
    ILogger<MaintenanceService> logger) : IMaintenanceService
{
    private static readonly string[] TaskIncludes = [nameof(WorkOrder.Tasks)];

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<MaintenanceDueResponse>>> GetDueAsync(MaintenanceDueQuery query, CancellationToken cancellationToken)
    {
        var lookaheadHours = query.LookaheadHours ?? options.Value.LookaheadHours;

        var lookaheadDays = query.LookaheadDays ?? options.Value.LookaheadDays;

        var activeMowers = await mowers.ListAsync(new QuerySpec<Mower> { Filter = m => m.IsActive }, cancellationToken).ConfigureAwait(false);

        var activeTemplates = await templates.ListAsync(new QuerySpec<ServiceTemplate> { Filter = t => t.IsActive }, cancellationToken).ConfigureAwait(false);

        var orders = await workOrders.ListAsync(new QuerySpec<WorkOrder> { Filter = w => w.Status != WorkOrderStatus.Cancelled }, cancellationToken).ConfigureAwait(false);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var due = new List<MaintenanceDueResponse>();

        foreach (var mower in activeMowers)
        {
            foreach (var template in activeTemplates.Where(t => t.IntervalHours is not null || t.IntervalDays is not null))
            {
                var pairOrders = orders.Where(o => o.MowerId == mower.Id && o.ServiceTemplateId == template.Id).ToList();

                var item = Evaluate(mower, template, pairOrders, today);

                var isUpcoming = item.HoursUntilDue <= lookaheadHours || item.DaysUntilDue <= lookaheadDays;

                if (item.IsOverdue || isUpcoming)
                {
                    due.Add(item);
                }
            }
        }

        var sorted = due
            .OrderByDescending(d => d.IsOverdue)
            .ThenBy(d => d.DaysUntilDue ?? int.MaxValue)
            .ThenBy(d => d.HoursUntilDue ?? decimal.MaxValue)
            .ToList();

        return Result<IReadOnlyList<MaintenanceDueResponse>>.Ok(sorted);
    }

    /// <inheritdoc />
    public async Task<Result<WorkOrderResponse>> CreateWorkOrderAsync(WorkOrderCreateRequest request, CancellationToken cancellationToken)
    {
        var mower = await mowers.GetByIdAsync(request.MowerId, null, cancellationToken).ConfigureAwait(false);

        if (mower is null)
        {
            return Result<WorkOrderResponse>.NotFound("Mower not found.");
        }

        var templateSpec = new QuerySpec<ServiceTemplate> { Includes = [nameof(ServiceTemplate.Tasks)] };

        var template = await templates.GetByIdAsync(request.ServiceTemplateId, templateSpec, cancellationToken).ConfigureAwait(false);

        if (template is null)
        {
            return Result<WorkOrderResponse>.NotFound("Service template not found.");
        }

        var existing = await workOrders.ListAsync(new QuerySpec<WorkOrder> { Filter = w => w.MowerId == mower.Id && w.ServiceTemplateId == template.Id }, cancellationToken).ConfigureAwait(false);

        if (existing.Any(w => w.IsOpen()))
        {
            return Result<WorkOrderResponse>.Conflict("This mower already has an open work order for that service.");
        }

        var order = new WorkOrder
        {
            MowerId = mower.Id,
            MowerName = mower.Name,
            ServiceTemplateId = template.Id,
            TemplateName = template.Name,
            ScheduledFor = request.ScheduledFor,
            AssignedToUserId = request.AssignedToUserId,
            EngineHoursAtStart = mower.EngineHours,
            Notes = request.Notes?.Trim(),
        };

        foreach (var task in template.Tasks.OrderBy(t => t.SortOrder))
        {
            order.Tasks.Add(new WorkOrderTask { WorkOrderId = order.Id, Description = task.Description, SortOrder = task.SortOrder, IsRequired = task.IsRequired });
        }

        await workOrders.AddAsync(order, cancellationToken).ConfigureAwait(false);

        await workOrders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Scheduled work order {WorkOrderId} for mower {MowerId} using template {TemplateId}", order.Id, mower.Id, template.Id);

        return Result<WorkOrderResponse>.Ok(order.ToResponse());
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<WorkOrderResponse>>> ListWorkOrdersAsync(WorkOrderListQuery query, CancellationToken cancellationToken)
    {
        var assignee = query.AssignedToUserId;

        var status = query.Status;

        var spec = new QuerySpec<WorkOrder>
        {
            Includes = TaskIncludes,
            Filter = w => (assignee == null || w.AssignedToUserId == assignee) && (status == null || w.Status == status),
            OrderBy = w => w.CreatedAt,
        };

        var items = await workOrders.ListAsync(spec, cancellationToken).ConfigureAwait(false);

        return Result<IReadOnlyList<WorkOrderResponse>>.Ok(items.Select(w => w.ToResponse()).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<WorkOrderResponse>> GetWorkOrderAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await workOrders.GetByIdAsync(id, new QuerySpec<WorkOrder> { Includes = TaskIncludes }, cancellationToken).ConfigureAwait(false);

        if (order is null)
        {
            return Result<WorkOrderResponse>.NotFound("Work order not found.");
        }

        return Result<WorkOrderResponse>.Ok(order.ToResponse());
    }

    /// <inheritdoc />
    public async Task<Result<WorkOrderResponse>> SetTaskCompletionAsync(TaskCompletionCommand command, CancellationToken cancellationToken)
    {
        var spec = new QuerySpec<WorkOrder> { Includes = TaskIncludes, Track = true };

        var order = await workOrders.GetByIdAsync(command.WorkOrderId, spec, cancellationToken).ConfigureAwait(false);

        if (order is null)
        {
            return Result<WorkOrderResponse>.NotFound("Work order not found.");
        }

        if (!command.CanActOnAny && order.AssignedToUserId != command.UserId)
        {
            return Result<WorkOrderResponse>.Forbidden("This work order is assigned to someone else.");
        }

        if (!order.IsOpen())
        {
            return Result<WorkOrderResponse>.Conflict("This work order is closed.");
        }

        var task = order.Tasks.FirstOrDefault(t => t.Id == command.TaskId);

        if (task is null)
        {
            return Result<WorkOrderResponse>.NotFound("Task not found on this work order.");
        }

        task.IsComplete = command.IsComplete;

        task.CompletedAt = command.IsComplete ? DateTimeOffset.UtcNow : null;

        task.CompletedByUserId = command.IsComplete ? command.UserId : null;

        var wasScheduled = order.Status == WorkOrderStatus.Scheduled;

        order.Status = order.Tasks.Any(t => t.IsComplete) ? WorkOrderStatus.InProgress : WorkOrderStatus.Scheduled;

        if (wasScheduled && order.Status == WorkOrderStatus.InProgress)
        {
            // Work is starting now: take the mower's current hours as the starting reading.
            var mower = await mowers.GetByIdAsync(order.MowerId, null, cancellationToken).ConfigureAwait(false);

            order.EngineHoursAtStart = mower?.EngineHours ?? order.EngineHoursAtStart;
        }

        order.UpdatedAt = DateTimeOffset.UtcNow;

        await workOrders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<WorkOrderResponse>.Ok(order.ToResponse());
    }

    /// <inheritdoc />
    public async Task<Result<WorkOrderResponse>> CompleteWorkOrderAsync(Guid id, CompleteWorkOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await workOrders.GetByIdAsync(id, new QuerySpec<WorkOrder> { Includes = TaskIncludes, Track = true }, cancellationToken).ConfigureAwait(false);

        if (order is null)
        {
            return Result<WorkOrderResponse>.NotFound("Work order not found.");
        }

        if (!order.IsOpen())
        {
            return Result<WorkOrderResponse>.Conflict("This work order is already closed.");
        }

        if (order.Tasks.Any(t => t.IsRequired && !t.IsComplete))
        {
            return Result<WorkOrderResponse>.Invalid("Every required task must be checked off before the work order can be completed.");
        }

        var mower = await mowers.GetByIdAsync(order.MowerId, new QuerySpec<Mower> { Track = true }, cancellationToken).ConfigureAwait(false);

        var minimumHours = Math.Max(order.EngineHoursAtStart ?? 0m, mower?.EngineHours ?? 0m);

        if (request.EngineHoursAtService < minimumHours)
        {
            return Result<WorkOrderResponse>.Invalid($"Engine hours when done cannot be less than the current hours ({minimumHours:0.##}).");
        }

        if (mower is not null && request.EngineHoursAtService > mower.EngineHours)
        {
            mower.EngineHours = request.EngineHoursAtService;

            mower.UpdatedAt = DateTimeOffset.UtcNow;
        }

        order.Status = WorkOrderStatus.Completed;

        order.CompletedAt = DateTimeOffset.UtcNow;

        order.EngineHoursAtService = request.EngineHoursAtService;

        order.Notes = string.IsNullOrWhiteSpace(request.Notes) ? order.Notes : request.Notes.Trim();

        order.UpdatedAt = DateTimeOffset.UtcNow;

        await workOrders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Completed work order {WorkOrderId} at {EngineHours} hours", order.Id, request.EngineHoursAtService);

        return Result<WorkOrderResponse>.Ok(order.ToResponse());
    }

    /// <summary>Computes how close a mower is to being due for a template, from its last completed service.</summary>
    /// <param name="mower">The mower.</param>
    /// <param name="template">The template.</param>
    /// <param name="pairOrders">The non-cancelled work orders for this mower/template pair.</param>
    /// <param name="today">Today's date (UTC).</param>
    /// <returns>The due summary.</returns>
    private static MaintenanceDueResponse Evaluate(Mower mower, ServiceTemplate template, IReadOnlyList<WorkOrder> pairOrders, DateOnly today)
    {
        var lastCompleted = pairOrders
            .Where(o => o.Status == WorkOrderStatus.Completed && o.CompletedAt is not null)
            .OrderByDescending(o => o.CompletedAt)
            .FirstOrDefault();

        var baselineHours = lastCompleted?.EngineHoursAtService ?? 0m;

        var baselineDate = lastCompleted?.CompletedAt is { } completedAt
            ? DateOnly.FromDateTime(completedAt.UtcDateTime)
            : mower.PurchaseDate;

        decimal? hoursUntil = template.IntervalHours is { } hours
            ? baselineHours + hours - mower.EngineHours
            : null;

        int? daysUntil = template.IntervalDays is { } days
            ? baselineDate.AddDays(days).DayNumber - today.DayNumber
            : null;

        var isOverdue = hoursUntil <= 0 || daysUntil <= 0;

        var hasOpen = pairOrders.Any(o => o.IsOpen());

        return new MaintenanceDueResponse(mower.Id, mower.Name, template.Id, template.Name, hoursUntil, daysUntil, isOverdue, hasOpen);
    }
}
