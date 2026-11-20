namespace MowTrackr.Assets.Models;

/// <summary>
/// An equipment manufacturer (e.g. John Deere) that owns a set of <see cref="ManufacturerModel"/> values.
/// </summary>
public sealed class Manufacturer : EntityBase
{
    /// <summary>Gets or sets the manufacturer name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the models this manufacturer makes.</summary>
    public List<ManufacturerModel> Models { get; set; } = [];
}
