namespace MowTrackr.Core.Contracts.Assets;

/// <summary>
/// A unit of measure as returned by the API.
/// </summary>
/// <param name="Id">The unit key.</param>
/// <param name="Name">The unit name.</param>
/// <param name="Abbreviation">The short label.</param>
public sealed record UnitOfMeasureResponse(Guid Id, string Name, string Abbreviation);
