namespace MowTrackr.Core.Contracts.Assets;

/// <summary>
/// Request to add or rename an asset type.
/// </summary>
public sealed class AssetTypeRequest
{
    /// <summary>Gets or sets the type name, for example "Chain Saw".</summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether this type has tires (so tire service steps apply to it).</summary>
    public bool HasTires { get; set; }
}
