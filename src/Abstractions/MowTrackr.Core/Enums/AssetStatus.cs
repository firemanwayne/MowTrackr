namespace MowTrackr.Core.Enums;

/// <summary>
/// Whether a piece of equipment can be used, worked out from its work orders.
/// </summary>
public enum AssetStatus
{
    /// <summary>No open repair work orders.</summary>
    Available,

    /// <summary>At least one repair work order is not complete.</summary>
    NeedsRepair,

    /// <summary>Taken out of service.</summary>
    Retired,
}
