namespace MowTrackr.Core.Enums;

/// <summary>
/// Severity of a service interval validation finding.
/// </summary>
public enum IssueSeverity
{
    /// <summary>Informational overlap; the template can still be saved.</summary>
    Warning,

    /// <summary>A conflict that blocks saving.</summary>
    Error,
}