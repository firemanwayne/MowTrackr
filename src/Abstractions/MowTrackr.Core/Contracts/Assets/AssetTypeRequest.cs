namespace MowTrackr.Core.Contracts.Assets;

/// <summary>
/// Payload for adding an asset type.
/// </summary>
public sealed class AssetTypeRequest
{
    /// <summary>Gets or sets the type name (e.g. "Chain Saw").</summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}
