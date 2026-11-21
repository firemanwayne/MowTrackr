namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// One item of an inspection (check) template.
/// </summary>
/// <param name="Id">The item key.</param>
/// <param name="Description">The item text.</param>
/// <param name="SortOrder">The display order.</param>
/// <param name="IsRequired">Whether it must be answered.</param>
/// <param name="ItemType">The kind of answer.</param>
/// <param name="Choices">The choices, separated by commas, for a choice item.</param>
/// <param name="RepairOn">The answers that mean something needs repair, separated by commas.</param>
public sealed record InspectionTaskResponse(Guid Id, string Description, int SortOrder, bool IsRequired, CheckItemType ItemType, string? Choices, string? RepairOn);
