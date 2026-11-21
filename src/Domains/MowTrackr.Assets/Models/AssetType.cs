namespace MowTrackr.Assets.Models;

/// <summary>
/// A kind of equipment (Weed Eater, Blower, Push Mower, Riding Mower, Chain Saw, ...). Mowers and service templates belong to
/// types, so a chain saw can have different maintenance from a mower.
/// </summary>
public sealed class AssetType : EntityBase
{
    /// <summary>Gets or sets the type name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether this type has tires, so service steps that need tires (rotation) apply to it.</summary>
    public bool HasTires { get; set; }
}
