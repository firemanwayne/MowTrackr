namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// One finding from interval validation.
/// </summary>
/// <param name="Severity">Error blocks saving; warning is informational.</param>
/// <param name="Message">A human-readable explanation.</param>
public sealed record IntervalIssue(IssueSeverity Severity, string Message);