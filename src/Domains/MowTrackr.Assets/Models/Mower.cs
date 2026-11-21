namespace MowTrackr.Assets.Models;

/// <summary>
/// A mower tracked in the asset register.
/// </summary>
public sealed class Mower : EntityBase
{
    /// <summary>Gets or sets the friendly name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the asset type key.</summary>
    public Guid AssetTypeId { get; set; }

    /// <summary>Gets or sets the asset type.</summary>
    public AssetType? AssetType { get; set; }

    /// <summary>Gets or sets the manufacturer key.</summary>
    public Guid ManufacturerId { get; set; }

    /// <summary>Gets or sets the manufacturer.</summary>
    public Manufacturer? Manufacturer { get; set; }

    /// <summary>Gets or sets the model key.</summary>
    public Guid ManufacturerModelId { get; set; }

    /// <summary>Gets or sets the model.</summary>
    public ManufacturerModel? Model { get; set; }

    /// <summary>Gets or sets the serial number.</summary>
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the purchase date.</summary>
    public DateOnly PurchaseDate { get; set; }

    /// <summary>Gets or sets the current engine hours.</summary>
    public decimal EngineHours { get; set; }

    /// <summary>Gets or sets the oil type.</summary>
    public string OilType { get; set; } = string.Empty;

    /// <summary>Gets or sets the last oil change date.</summary>
    public DateOnly? LastOilChange { get; set; }

    /// <summary>Gets or sets the oil capacity in quarts.</summary>
    public decimal OilQuantityQuarts { get; set; }

    /// <summary>Gets or sets a value indicating whether the mower is in service.</summary>
    public bool IsActive { get; set; } = true;
}