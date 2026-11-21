namespace MowTrackr.Assets.Models;

/// <summary>
/// A kind of equipment (e.g. Chain Saw, Push Mower). Assets and service templates each belong to one, so a chain saw
/// can have different maintenance from a riding mower.
/// </summary>
public sealed class AssetType : EntityBase
{
    /// <summary>Gets or sets the type name.</summary>
    public string Name { get; set; } = string.Empty;
}
