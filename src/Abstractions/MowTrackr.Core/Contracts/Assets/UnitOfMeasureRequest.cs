namespace MowTrackr.Core.Contracts.Assets;

/// <summary>
/// Request to add or rename a unit of measure.
/// </summary>
public sealed class UnitOfMeasureRequest
{
    /// <summary>Gets or sets the unit name, for example "Quart".</summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the short label shown next to a quantity, for example "qt".</summary>
    [Required]
    [StringLength(20)]
    public string Abbreviation { get; set; } = string.Empty;
}
