namespace MowTrackr.Core.Contracts.Assets;

/// <summary>
/// An oil type as returned by the API.
/// </summary>
/// <param name="Id">The oil type key.</param>
/// <param name="Name">The oil type name.</param>
public sealed record OilTypeResponse(Guid Id, string Name);
