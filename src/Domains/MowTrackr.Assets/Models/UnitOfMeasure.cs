namespace MowTrackr.Assets.Models;

/// <summary>
/// A unit of measure (Quart, Ounce, Gallon, Each, ...). An asset's oil unit and a service step's quantity unit come from this list.
/// </summary>
public sealed class UnitOfMeasure : EntityBase
{
    /// <summary>Gets or sets the unit name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the short label shown next to a quantity, for example "qt".</summary>
    public string Abbreviation { get; set; } = string.Empty;
}
