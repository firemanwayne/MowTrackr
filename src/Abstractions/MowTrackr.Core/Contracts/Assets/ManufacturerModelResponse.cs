namespace MowTrackr.Core.Contracts.Assets;

/// <summary>
/// An equipment model as returned by the API.
/// </summary>
/// <param name="Id">The model key.</param>
/// <param name="ManufacturerId">The owning manufacturer key.</param>
/// <param name="Name">The model name.</param>
public sealed record ManufacturerModelResponse(Guid Id, Guid ManufacturerId, string Name);
