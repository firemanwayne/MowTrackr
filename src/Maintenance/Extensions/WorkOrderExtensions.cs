using MowTrackr.Maintenance.Models;

namespace MowTrackr.Maintenance.Extensions;

/// <summary>
/// Mapping helpers for <see cref="WorkOrder"/>.
/// </summary>
internal static class WorkOrderExtensions
{
    /// <summary>Maps a work order to its API response.</summary>
    /// <param name="order">The entity (tasks must be loaded).</param>
    /// <returns>The response.</returns>
    public static WorkOrderResponse ToResponse(this WorkOrder order)
    {
        var tasks = order.Tasks
            .OrderBy(t => t.SortOrder)
            .Select(t => new WorkOrderTaskResponse(t.Id, t.Description, t.SortOrder, t.IsRequired, t.IsComplete, t.CompletedAt, t.CompletedByUserId))
            .ToList();

        return new WorkOrderResponse(
            order.Id,
            order.MowerId,
            order.MowerName,
            order.ServiceTemplateId,
            order.TemplateName,
            order.Status,
            order.ScheduledFor,
            order.AssignedToUserId,
            order.CompletedAt,
            order.EngineHoursAtStart,
            order.EngineHoursAtService,
            order.Notes,
            tasks);
    }

    /// <summary>Gets a value indicating whether the order is still open (scheduled or in progress).</summary>
    /// <param name="order">The entity.</param>
    /// <returns><see langword="true"/> if open.</returns>
    public static bool IsOpen(this WorkOrder order)
    {
        return order.Status is WorkOrderStatus.Scheduled or WorkOrderStatus.InProgress;
    }
}