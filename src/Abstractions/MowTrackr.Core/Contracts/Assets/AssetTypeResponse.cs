namespace MowTrackr.Core.Contracts.Assets;

/// <summary>
/// An asset type as returned by the API.
/// </summary>
/// <param name="Id">The type key.</param>
/// <param name="Name">The type name.</param>
/// <param name="HasTires">Whether the type has tires.</param>
public sealed record AssetTypeResponse(Guid Id, string Name, bool HasTires = false);
