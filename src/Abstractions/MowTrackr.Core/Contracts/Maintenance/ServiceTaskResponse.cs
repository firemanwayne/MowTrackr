namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// One checklist item of a service template.
/// </summary>
/// <param name="Id">The task key.</param>
/// <param name="Description">The task description.</param>
/// <param name="SortOrder">The display order.</param>
/// <param name="IsRequired">Whether the task must be checked off.</param>
/// <param name="Condition">When the step applies.</param>
public sealed record ServiceTaskResponse(Guid Id, string Description, int SortOrder, bool IsRequired, ServiceTaskCondition Condition = ServiceTaskCondition.Always);
