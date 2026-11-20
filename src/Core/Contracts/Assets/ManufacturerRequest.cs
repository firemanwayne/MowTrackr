namespace MowTrackr.Core.Contracts.Assets;

/// <summary>
/// Payload for adding a manufacturer, or a model under a manufacturer (only the name is needed).
/// </summary>
public sealed class ManufacturerRequest
{
    /// <summary>Gets or sets the name.</summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}
