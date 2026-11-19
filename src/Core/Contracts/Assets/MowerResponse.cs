namespace MowTrackr.Core.Contracts.Assets;

/// <summary>
/// A mower as returned by the API.
/// </summary>
/// <param name="Id">The mower key.</param>
/// <param name="Name">The friendly name.</param>
/// <param name="Manufacturer">The manufacturer.</param>
/// <param name="Model">The model.</param>
/// <param name="SerialNumber">The serial number.</param>
/// <param name="PurchaseDate">The purchase date.</param>
/// <param name="EngineHours">The current engine hours.</param>
/// <param name="OilType">The oil type.</param>
/// <param name="LastOilChange">The last oil change date.</param>
/// <param name="OilQuantityQuarts">The oil capacity in quarts.</param>
/// <param name="IsActive">Whether the mower is in service.</param>
public sealed record MowerResponse(
    Guid Id,
    string Name,
    string Manufacturer,
    string Model,
    string SerialNumber,
    DateOnly PurchaseDate,
    decimal EngineHours,
    string OilType,
    DateOnly? LastOilChange,
    decimal OilQuantityQuarts,
    bool IsActive);