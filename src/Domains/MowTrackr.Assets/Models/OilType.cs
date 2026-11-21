namespace MowTrackr.Assets.Models;

/// <summary>
/// A kind of oil (SAE 10W-30, synthetic 5W-30, 2-cycle mix, ...). Each piece of equipment requires one, and its oil change steps show it.
/// </summary>
public sealed class OilType : EntityBase
{
    /// <summary>Gets or sets the oil type name.</summary>
    public string Name { get; set; } = string.Empty;
}
