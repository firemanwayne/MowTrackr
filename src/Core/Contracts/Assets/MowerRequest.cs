namespace MowTrackr.Core.Contracts.Assets;

/// <summary>
/// Payload for creating or updating a mower.
/// </summary>
public sealed class MowerRequest
{
    /// <summary>Gets or sets the friendly name (e.g. "Zone 3 Zero-Turn").</summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the manufacturer.</summary>
    [Required]
    [StringLength(100)]
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>Gets or sets the model.</summary>
    [Required]
    [StringLength(100)]
    public string Model { get; set; } = string.Empty;

    /// <summary>Gets or sets the serial number.</summary>
    [Required]
    [StringLength(100)]
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the purchase date.</summary>
    public DateOnly PurchaseDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>Gets or sets the current engine hours.</summary>
    [Range(0, 1_000_000)]
    public decimal EngineHours { get; set; }

    /// <summary>Gets or sets the oil type (e.g. "SAE 10W-30").</summary>
    [Required]
    [StringLength(50)]
    public string OilType { get; set; } = string.Empty;

    /// <summary>Gets or sets the date of the last oil change.</summary>
    public DateOnly? LastOilChange { get; set; }

    /// <summary>Gets or sets the oil capacity in quarts.</summary>
    [Range(0, 100)]
    public decimal OilQuantityQuarts { get; set; }

    /// <summary>Gets or sets a value indicating whether the mower is in service.</summary>
    public bool IsActive { get; set; } = true;
}