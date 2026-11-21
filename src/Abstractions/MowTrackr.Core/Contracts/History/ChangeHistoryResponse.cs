namespace MowTrackr.Core.Contracts.History;

/// <summary>
/// One entry in an item's change history: a create, update or delete.
/// </summary>
/// <param name="Id">The entry key.</param>
/// <param name="Action">What happened: <c>Created</c>, <c>Updated</c> or <c>Deleted</c>.</param>
/// <param name="ChangedAt">When the change was saved.</param>
/// <param name="ChangedBy">The key of the user who made it, or <see langword="null"/> when there was none.</param>
/// <param name="ChangedByName">That user's display name, or <see langword="null"/> when unknown.</param>
/// <param name="Changes">The values that changed.</param>
public sealed record ChangeHistoryResponse(
    Guid Id,
    string Action,
    DateTimeOffset ChangedAt,
    Guid? ChangedBy,
    string? ChangedByName,
    IReadOnlyList<PropertyChangeResponse> Changes);
