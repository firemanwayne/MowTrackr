namespace MowTrackr.Core.Enums;

/// <summary>
/// Which part of a work order a task belongs to.
/// </summary>
public enum TaskPhase
{
    /// <summary>A service step: the work itself.</summary>
    Service,

    /// <summary>A pre-check item, answered before the work starts.</summary>
    Before,

    /// <summary>A post-check item, answered after the work is done.</summary>
    After,
}
