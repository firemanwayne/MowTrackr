namespace MowTrackr.Core.Enums;

/// <summary>
/// What a work order is for.
/// </summary>
public enum WorkOrderKind
{
    /// <summary>Scheduled maintenance from a service template.</summary>
    Service,

    /// <summary>A repair, usually raised from something found during a check.</summary>
    Repair,
}
