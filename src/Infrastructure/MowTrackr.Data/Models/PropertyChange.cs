namespace MowTrackr.Data.Models;

/// <summary>
/// One value that changed, as text. A <see langword="null"/> side means the value was empty (before a create, after a delete).
/// </summary>
/// <param name="Old">The value before the change.</param>
/// <param name="New">The value after the change.</param>
internal sealed record PropertyChange(string? Old, string? New);
