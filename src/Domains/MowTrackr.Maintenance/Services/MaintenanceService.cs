using MowTrackr.Assets.Models;
using MowTrackr.Maintenance.Extensions;
using MowTrackr.Maintenance.Models;
using MowTrackr.Maintenance.Options;
using MowTrackr.Maintenance.Validators;

namespace MowTrackr.Maintenance.Services;

/// <summary>
/// Default <see cref="IMaintenanceService"/>: due checks and the work order lifecycle, including pre-checks, post-checks and repairs.
/// </summary>
/// <param name="mowers">The mower repository.</param>
/// <param name="templates">The template repository.</param>
/// <param name="workOrders">The work order repository.</param>
/// <param name="inspections">The inspection template repository.</param>
/// <param name="options">The due-check defaults.</param>
/// <param name="logger">The logger.</param>
internal sealed class MaintenanceService(
    IRepository<Mower> mowers,
    IRepository<ServiceTemplate> templates,
    IRepository<WorkOrder> workOrders,
    IRepository<InspectionTemplate> inspections,
    IOptions<MaintenanceOptions> options,
    ILogger<MaintenanceService> logger) : IMaintenanceService
{
    private const int TitleLength = 100;

    private const int TaskLength = 300;

    private const int NotesLength = 1000;

    private static readonly string[] TaskIncludes = [nameof(WorkOrder.Tasks)];

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<MaintenanceDueResponse>>> GetDueAsync(MaintenanceDueQuery query, CancellationToken cancellationToken)
    {
        var lookaheadHours = query.LookaheadHours ?? options.Value.LookaheadHours;

        var lookaheadDays = query.LookaheadDays ?? options.Value.LookaheadDays;

        var activeMowers = await mowers
            .ListAsync(new QuerySpec<Mower> { Filter = m => m.IsActive, Includes = [nameof(Mower.AssetType)] }, cancellationToken)
            .ConfigureAwait(false);

        var activeTemplates = await templates
            .ListAsync(new QuerySpec<ServiceTemplate> { Filter = t => t.IsActive, Includes = [nameof(ServiceTemplate.AssetTypes)] }, cancellationToken)
            .ConfigureAwait(false);

        var orders = await workOrders
            .ListAsync(new QuerySpec<WorkOrder> { Filter = w => w.Status != WorkOrderStatus.Cancelled && w.Kind == WorkOrderKind.Service }, cancellationToken)
            .ConfigureAwait(false);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var due = new List<MaintenanceDueResponse>();

        foreach (var mower in activeMowers)
        {
            foreach (var template in activeTemplates.Where(t => t.AssetTypes.Any(a => a.Id == mower.AssetTypeId) && (t.IntervalHours is not null || t.IntervalDays is not null)))
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
        var mower = await mowers
            .GetByIdAsync(request.MowerId, new QuerySpec<Mower> { Includes = [nameof(Mower.AssetType)] }, cancellationToken)
            .ConfigureAwait(false);

        if (mower is null)
        {
            return Result<WorkOrderResponse>.NotFound("Mower not found.");
        }

        var templateSpec = new QuerySpec<ServiceTemplate>
        {
            Includes = [nameof(ServiceTemplate.Tasks), nameof(ServiceTemplate.AssetTypes), $"{nameof(ServiceTemplate.Inspections)}.{nameof(InspectionTemplate.Tasks)}"],
        };

        var template = await templates
            .GetByIdAsync(request.ServiceTemplateId, templateSpec, cancellationToken)
            .ConfigureAwait(false);

        if (template is null)
        {
            return Result<WorkOrderResponse>.NotFound("Service template not found.");
        }

        if (template.AssetTypes.All(a => a.Id != mower.AssetTypeId))
        {
            return Result<WorkOrderResponse>.Invalid("That service is for a different type of equipment.");
        }

        var existing = await workOrders
            .ListAsync(new QuerySpec<WorkOrder> { Filter = w => w.MowerId == mower.Id && w.ServiceTemplateId == template.Id }, cancellationToken)
            .ConfigureAwait(false);

        if (existing.Any(w => w.IsOpen()))
        {
            return Result<WorkOrderResponse>.Conflict("This mower already has an open work order for that service.");
        }

        var order = new WorkOrder
        {
            MowerId = mower.Id,
            MowerName = mower.Name,
            ServiceTemplateId = template.Id,
            TemplateName = Truncate(template.TitleFor(mower.AssetType?.Name), TitleLength),
            ScheduledFor = request.ScheduledFor,
            AssignedToUserId = request.AssignedToUserId,
            EngineHoursAtStart = mower.EngineHours,
            Notes = request.Notes?.Trim(),
        };

        var checks = await CollectInspectionsAsync(template, mower, cancellationToken).ConfigureAwait(false);

        var number = 0;

        AddCheckItems(order, checks.Where(i => i.Timing == InspectionTiming.Before), TaskPhase.Before, ref number);

        var type = mower.AssetType ?? new AssetType();

        foreach (var task in template.Tasks.Where(t => t.AppliesTo(type)).OrderBy(t => t.SortOrder))
        {
            order.Tasks.Add(new WorkOrderTask { WorkOrderId = order.Id, Description = task.Description, SortOrder = number++, IsRequired = task.IsRequired });
        }

        AddCheckItems(order, checks.Where(i => i.Timing == InspectionTiming.After), TaskPhase.After, ref number);

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

        var items = await workOrders
            .ListAsync(spec, cancellationToken)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<WorkOrderResponse>>.Ok(items.Select(w => w.ToResponse()).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<WorkOrderResponse>> GetWorkOrderAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await workOrders
            .GetByIdAsync(id, new QuerySpec<WorkOrder> { Includes = TaskIncludes }, cancellationToken)
            .ConfigureAwait(false);

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

        var order = await workOrders
            .GetByIdAsync(command.WorkOrderId, spec, cancellationToken)
            .ConfigureAwait(false);

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

        if (task.Phase != TaskPhase.Service)
        {
            return Result<WorkOrderResponse>.Invalid("Answer the pre-check and post-check items with the check form.");
        }

        if (command.IsComplete && order.Tasks.Any(t => t.Phase == TaskPhase.Before) && order.PreCheckSubmittedAt is null)
        {
            return Result<WorkOrderResponse>.Conflict("Submit the pre-check before starting the work.");
        }

        task.IsComplete = command.IsComplete;

        task.CompletedAt = command.IsComplete ? DateTimeOffset.UtcNow : null;

        task.CompletedByUserId = command.IsComplete ? command.UserId : null;

        await StartIfNeededAsync(order, cancellationToken).ConfigureAwait(false);

        order.UpdatedAt = DateTimeOffset.UtcNow;

        await workOrders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<WorkOrderResponse>.Ok(order.ToResponse());
    }

    /// <inheritdoc />
    public async Task<Result<WorkOrderResponse>> SubmitCheckAsync(CheckSubmissionCommand command, CancellationToken cancellationToken)
    {
        var order = await workOrders
            .GetByIdAsync(command.WorkOrderId, new QuerySpec<WorkOrder> { Includes = TaskIncludes, Track = true }, cancellationToken)
            .ConfigureAwait(false);

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

        var isBefore = command.Timing == InspectionTiming.Before;

        var phase = isBefore ? TaskPhase.Before : TaskPhase.After;

        var items = order.Tasks.Where(t => t.Phase == phase).ToList();

        if (items.Count == 0)
        {
            return Result<WorkOrderResponse>.Invalid(isBefore ? "This work order has no pre-check." : "This work order has no post-check.");
        }

        if (!isBefore && order.Tasks.Any(t => t.Phase == TaskPhase.Service && t.IsRequired && !t.IsComplete))
        {
            return Result<WorkOrderResponse>.Conflict("Finish the service steps before the post-check.");
        }

        var answers = command.Answers
            .GroupBy(a => a.TaskId)
            .ToDictionary(g => g.Key, g => g.Last().Answer?.Trim());

        if (answers.Keys.Any(id => items.All(i => i.Id != id)))
        {
            return Result<WorkOrderResponse>.Invalid("An answer does not belong to this check.");
        }

        var hours = new List<(WorkOrderTask Item, decimal Value)>();

        foreach (var item in items.Where(i => i.ItemType != CheckItemType.ServiceStepsComplete))
        {
            answers.TryGetValue(item.Id, out var answer);

            if (string.IsNullOrWhiteSpace(answer))
            {
                if (item.IsRequired)
                {
                    return Result<WorkOrderResponse>.Invalid($"Answer \"{item.Description}\".");
                }

                continue;
            }

            var error = CheckAnswerValidator.Validate(item, answer);

            if (error is not null)
            {
                return Result<WorkOrderResponse>.Invalid(error);
            }

            if (item.ItemType == CheckItemType.EngineHours)
            {
                CheckAnswerValidator.TryParseHours(answer, out var value);

                hours.Add((item, value));
            }
        }

        var mower = hours.Count == 0
            ? null
            : await mowers.GetByIdAsync(order.MowerId, new QuerySpec<Mower> { Track = true }, cancellationToken).ConfigureAwait(false);

        foreach (var (item, value) in hours)
        {
            if (mower is not null && value < mower.EngineHours)
            {
                return Result<WorkOrderResponse>.Invalid($"\"{item.Description}\": the hours cannot be lower than the recorded {mower.EngineHours:0.##}.");
            }
        }

        var now = DateTimeOffset.UtcNow;

        foreach (var item in items)
        {
            if (item.ItemType == CheckItemType.ServiceStepsComplete)
            {
                item.Answer = CheckAnswerValidator.Yes;

                MarkDone(item, now, command.UserId);

                continue;
            }

            answers.TryGetValue(item.Id, out var answer);

            if (string.IsNullOrWhiteSpace(answer))
            {
                item.Answer = null;

                item.IsComplete = false;

                item.CompletedAt = null;

                item.CompletedByUserId = null;

                continue;
            }

            item.Answer = CheckAnswerValidator.Canonical(item, answer);

            MarkDone(item, now, command.UserId);
        }

        if (mower is not null)
        {
            var latest = hours.Max(h => h.Value);

            if (latest > mower.EngineHours)
            {
                mower.EngineHours = latest;

                mower.UpdatedAt = now;
            }
        }

        if (isBefore)
        {
            order.PreCheckSubmittedAt = now;
        }
        else
        {
            order.PostCheckSubmittedAt = now;
        }

        await StartIfNeededAsync(order, cancellationToken).ConfigureAwait(false);

        order.UpdatedAt = now;

        await workOrders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Submitted the {Timing} check on work order {WorkOrderId}", command.Timing, order.Id);

        return Result<WorkOrderResponse>.Ok(order.ToResponse());
    }

    /// <inheritdoc />
    public async Task<Result<WorkOrderResponse>> CreateRepairAsync(RepairCommand command, CancellationToken cancellationToken)
    {
        var order = await workOrders
            .GetByIdAsync(command.WorkOrderId, new QuerySpec<WorkOrder> { Includes = TaskIncludes, Track = true }, cancellationToken)
            .ConfigureAwait(false);

        if (order is null)
        {
            return Result<WorkOrderResponse>.NotFound("Work order not found.");
        }

        if (!command.CanActOnAny && order.AssignedToUserId != command.UserId)
        {
            return Result<WorkOrderResponse>.Forbidden("This work order is assigned to someone else.");
        }

        var finding = order.Tasks.FirstOrDefault(t => t.Id == command.TaskId);

        if (finding is null)
        {
            return Result<WorkOrderResponse>.NotFound("Check item not found on this work order.");
        }

        if (!CheckAnswerValidator.IsFinding(finding))
        {
            return Result<WorkOrderResponse>.Invalid("That answer does not need a repair.");
        }

        if (finding.RepairWorkOrderId is not null)
        {
            return Result<WorkOrderResponse>.Conflict("A repair work order was already created for this finding.");
        }

        var mower = await mowers
            .GetByIdAsync(order.MowerId, null, cancellationToken)
            .ConfigureAwait(false);

        var repair = new WorkOrder
        {
            MowerId = order.MowerId,
            MowerName = order.MowerName,
            Kind = WorkOrderKind.Repair,
            TemplateName = Truncate($"Repair: {finding.Description}", TitleLength),
            ParentWorkOrderId = order.Id,
            AssignedToUserId = order.AssignedToUserId,
            EngineHoursAtStart = mower?.EngineHours,
            Notes = Truncate($"Found during {order.TemplateName}: {finding.Description} - {finding.Answer}.", NotesLength),
        };

        repair.Tasks.Add(new WorkOrderTask
        {
            WorkOrderId = repair.Id,
            Description = Truncate($"Repair: {finding.Description} ({finding.Answer})", TaskLength),
            SortOrder = 0,
        });

        finding.RepairWorkOrderId = repair.Id;

        order.UpdatedAt = DateTimeOffset.UtcNow;

        await workOrders.AddAsync(repair, cancellationToken).ConfigureAwait(false);

        await workOrders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Created repair work order {RepairId} for a finding on work order {WorkOrderId}", repair.Id, order.Id);

        return Result<WorkOrderResponse>.Ok(repair.ToResponse());
    }

    /// <inheritdoc />
    public async Task<Result<bool>> DeleteWorkOrderAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await workOrders
            .GetByIdAsync(id, new QuerySpec<WorkOrder> { Includes = TaskIncludes, Track = true }, cancellationToken)
            .ConfigureAwait(false);

        if (order is null)
        {
            return Result<bool>.NotFound("Work order not found.");
        }

        var now = DateTimeOffset.UtcNow;

        // Repairs raised from this work order stay, but no longer point at it.
        var repairs = await workOrders
            .ListAsync(new QuerySpec<WorkOrder> { Filter = w => w.ParentWorkOrderId == id, Track = true }, cancellationToken)
            .ConfigureAwait(false);

        foreach (var repair in repairs)
        {
            repair.ParentWorkOrderId = null;

            repair.UpdatedAt = now;
        }

        // If this was a repair, the finding that raised it can offer a new one.
        if (order.ParentWorkOrderId is { } parentId)
        {
            var parent = await workOrders
                .GetByIdAsync(parentId, new QuerySpec<WorkOrder> { Includes = TaskIncludes, Track = true }, cancellationToken)
                .ConfigureAwait(false);

            foreach (var finding in parent?.Tasks.Where(t => t.RepairWorkOrderId == id) ?? [])
            {
                finding.RepairWorkOrderId = null;
            }
        }

        workOrders.Remove(order);

        await workOrders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Deleted work order {WorkOrderId}", id);

        return Result<bool>.Ok(true);
    }

    /// <inheritdoc />
    public async Task<Result<WorkOrderResponse>> CompleteWorkOrderAsync(Guid id, CompleteWorkOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await workOrders
            .GetByIdAsync(id, new QuerySpec<WorkOrder> { Includes = TaskIncludes, Track = true }, cancellationToken)
            .ConfigureAwait(false);

        if (order is null)
        {
            return Result<WorkOrderResponse>.NotFound("Work order not found.");
        }

        if (!order.IsOpen())
        {
            return Result<WorkOrderResponse>.Conflict("This work order is already closed.");
        }

        if (order.Tasks.Any(t => t.Phase == TaskPhase.Before) && order.PreCheckSubmittedAt is null)
        {
            return Result<WorkOrderResponse>.Invalid("Submit the pre-check before completing the work order.");
        }

        if (order.Tasks.Any(t => t.Phase == TaskPhase.After) && order.PostCheckSubmittedAt is null)
        {
            return Result<WorkOrderResponse>.Invalid("Submit the post-check before completing the work order.");
        }

        if (order.Tasks.Any(t => t.IsRequired && !t.IsComplete))
        {
            return Result<WorkOrderResponse>.Invalid("Every required task must be checked off before the work order can be completed.");
        }

        var mower = await mowers
            .GetByIdAsync(order.MowerId, new QuerySpec<Mower> { Track = true }, cancellationToken)
            .ConfigureAwait(false);

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

    /// <summary>
    /// Gathers the inspections for a new work order: the ones assigned to the service, plus any that apply by age or engine
    /// hours, each once.
    /// </summary>
    /// <param name="template">The service template (inspections and their items loaded).</param>
    /// <param name="mower">The equipment the order is for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The active inspections, with their items.</returns>
    private async Task<IReadOnlyList<InspectionTemplate>> CollectInspectionsAsync(ServiceTemplate template, Mower mower, CancellationToken cancellationToken)
    {
        var typeId = mower.AssetTypeId;

        var automatic = await inspections
            .ListAsync(
                new QuerySpec<InspectionTemplate>
                {
                    Filter = t => t.IsActive && (t.AssetTypeId == null || t.AssetTypeId == typeId),
                    Includes = [nameof(InspectionTemplate.Tasks), nameof(InspectionTemplate.AssetType)],
                    OrderBy = t => t.Name,
                },
                cancellationToken)
            .ConfigureAwait(false);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var assigned = template.Inspections
            .Where(i => i.IsActive && (i.AssetTypeId is null || i.AssetTypeId == typeId));

        return assigned
            .Concat(automatic.Where(i => i.AppliesTo(mower, today)))
            .GroupBy(i => i.Id)
            .Select(g => g.First())
            .OrderBy(i => i.Name)
            .ToList();
    }

    /// <summary>Adds the items of some inspections to a work order as check items.</summary>
    /// <param name="order">The work order being built.</param>
    /// <param name="checks">The inspections to add.</param>
    /// <param name="phase">Pre-check or post-check.</param>
    /// <param name="number">The next sort order, advanced as items are added.</param>
    private static void AddCheckItems(WorkOrder order, IEnumerable<InspectionTemplate> checks, TaskPhase phase, ref int number)
    {
        foreach (var inspection in checks)
        {
            foreach (var item in inspection.Tasks.OrderBy(t => t.SortOrder))
            {
                order.Tasks.Add(new WorkOrderTask
                {
                    WorkOrderId = order.Id,
                    Source = inspection.Name,
                    Phase = phase,
                    Description = item.Description,
                    SortOrder = number++,
                    IsRequired = item.IsRequired,
                    ItemType = item.ItemType,
                    Choices = item.Choices,
                    RepairOn = item.RepairOn,
                });
            }
        }
    }

    /// <summary>Marks a check item as answered.</summary>
    /// <param name="item">The item.</param>
    /// <param name="at">When it was answered.</param>
    /// <param name="userId">Who answered it.</param>
    private static void MarkDone(WorkOrderTask item, DateTimeOffset at, Guid userId)
    {
        item.IsComplete = true;

        item.CompletedAt = at;

        item.CompletedByUserId = userId;
    }

    /// <summary>Moves a scheduled order to in progress once any work or check has been done, taking the mower's current hours as the start.</summary>
    /// <param name="order">The tracked work order (tasks loaded).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task.</returns>
    private async Task StartIfNeededAsync(WorkOrder order, CancellationToken cancellationToken)
    {
        var wasScheduled = order.Status == WorkOrderStatus.Scheduled;

        order.Status = order.Tasks.Any(t => t.IsComplete) || order.PreCheckSubmittedAt is not null
            ? WorkOrderStatus.InProgress
            : WorkOrderStatus.Scheduled;

        if (wasScheduled && order.Status == WorkOrderStatus.InProgress)
        {
            // Work is starting now: take the mower's current hours as the starting reading.
            var mower = await mowers
                .GetByIdAsync(order.MowerId, null, cancellationToken)
                .ConfigureAwait(false);

            order.EngineHoursAtStart = mower?.EngineHours ?? order.EngineHoursAtStart;
        }
    }

    /// <summary>Cuts text to a column's length.</summary>
    /// <param name="text">The text.</param>
    /// <param name="length">The longest allowed.</param>
    /// <returns>The text, shortened if needed.</returns>
    private static string Truncate(string text, int length)
    {
        return text.Length <= length ? text : text[..length];
    }

    /// <summary>Computes how close a mower is to being due for a template, from its last completed service.</summary>
    /// <param name="mower">The mower (asset type loaded).</param>
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

        return new MaintenanceDueResponse(mower.Id, mower.Name, template.Id, template.TitleFor(mower.AssetType?.Name), hoursUntil, daysUntil, isOverdue, hasOpen);
    }
}
