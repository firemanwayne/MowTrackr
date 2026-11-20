namespace MowTrackr.Core.Contracts.Assets;

/// <summary>
/// A manufacturer and its models as returned by the API.
/// </summary>
/// <param name="Id">The manufacturer key.</param>
/// <param name="Name">The manufacturer name.</param>
/// <param name="Models">The models, ordered by name.</param>
public sealed record ManufacturerResponse(Guid Id, string Name, IReadOnlyList<ManufacturerModelResponse> Models);
