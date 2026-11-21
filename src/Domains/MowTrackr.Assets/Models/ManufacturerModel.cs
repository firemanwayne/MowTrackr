namespace MowTrackr.Assets.Models;

/// <summary>
/// An equipment model made by a <see cref="Manufacturer"/>.
/// </summary>
public sealed class ManufacturerModel : EntityBase
{
    /// <summary>Gets or sets the owning manufacturer key.</summary>
    public Guid ManufacturerId { get; set; }

    /// <summary>Gets or sets the owning manufacturer.</summary>
    public Manufacturer? Manufacturer { get; set; }

    /// <summary>Gets or sets the model name.</summary>
    public string Name { get; set; } = string.Empty;
}
