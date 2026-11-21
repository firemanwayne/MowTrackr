namespace MowTrackr.Core.Enums;

/// <summary>
/// When an inspection (check) is done relative to the service work.
/// </summary>
public enum InspectionTiming
{
    /// <summary>Before the work starts: the pre-check.</summary>
    Before,

    /// <summary>After the work is done: the post-check.</summary>
    After,
}
