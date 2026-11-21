namespace MowTrackr.Core.Contracts.History;

/// <summary>
/// One value that changed in a history entry.
/// </summary>
/// <param name="Property">The property name, such as <c>EngineHours</c>.</param>
/// <param name="Old">The value before the change, or <see langword="null"/> if it was empty.</param>
/// <param name="New">The value after the change, or <see langword="null"/> if it is empty.</param>
public sealed record PropertyChangeResponse(string Property, string? Old, string? New);
