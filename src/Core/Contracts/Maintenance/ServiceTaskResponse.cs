namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// One checklist item of a service template as returned by the API.
/// </summary>
/// <param name="Id">The task key.</param>
/// <param name="Description">The task description.</param>
/// <param name="SortOrder">The display order.</param>
public sealed record ServiceTaskResponse(Guid Id, string Description, int SortOrder);