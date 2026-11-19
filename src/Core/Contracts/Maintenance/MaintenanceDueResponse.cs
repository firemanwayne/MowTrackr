namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// A mower/template pair that is due (or coming due) for service.
/// </summary>
/// <param name="MowerId">The mower key.</param>
/// <param name="MowerName">The mower name.</param>
/// <param name="ServiceTemplateId">The template key.</param>
/// <param name="TemplateName">The template name.</param>
/// <param name="HoursUntilDue">Engine hours until due (negative when overdue); null if the template has no hour interval.</param>
/// <param name="DaysUntilDue">Days until due (negative when overdue); null if the template has no day interval.</param>
/// <param name="IsOverdue">Whether either interval has already passed.</param>
/// <param name="HasOpenWorkOrder">Whether a scheduled or in-progress work order already exists.</param>
public sealed record MaintenanceDueResponse(
    Guid MowerId,
    string MowerName,
    Guid ServiceTemplateId,
    string TemplateName,
    decimal? HoursUntilDue,
    int? DaysUntilDue,
    bool IsOverdue,
    bool HasOpenWorkOrder);