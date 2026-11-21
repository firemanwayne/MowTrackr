namespace MowTrackr.Core.Enums;

/// <summary>
/// When a service step applies.
/// </summary>
public enum ServiceTaskCondition
{
    /// <summary>Always.</summary>
    Always,

    /// <summary>Only for asset types that have tires.</summary>
    AssetHasTires,
}
