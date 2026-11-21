namespace MowTrackr.Core.Contracts.Assets;

/// <summary>
/// Request to add or rename an oil type.
/// </summary>
public sealed class OilTypeRequest
{
    /// <summary>Gets or sets the oil type name, for example "SAE 10W-30".</summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
}
