namespace MowTrackr.Core.Enums;

/// <summary>
/// Where the unit of a step's quantity comes from.
/// </summary>
public enum QuantityUnitSource
{
    /// <summary>A unit chosen on the step itself (each, gallon, ...).</summary>
    StepUnit,

    /// <summary>The oil unit defined on the equipment being serviced (quarts for one engine, ounces for another). The oil capacity is the starting quantity.</summary>
    AssetOil,
}
