namespace MowTrackr.Core.Contracts.Assets;

/// <summary>
/// A mower (or other piece of equipment) as returned by the API.
/// </summary>
/// <param name="Id">The mower key.</param>
/// <param name="Name">The mower name.</param>
/// <param name="AssetTypeId">The asset type key.</param>
/// <param name="AssetType">The asset type name.</param>
/// <param name="ManufacturerId">The manufacturer key.</param>
/// <param name="Manufacturer">The manufacturer name.</param>
/// <param name="ManufacturerModelId">The model key.</param>
/// <param name="Model">The model name.</param>
/// <param name="SerialNumber">The serial number.</param>
/// <param name="PurchaseDate">The purchase date.</param>
/// <param name="EngineHours">The engine hours.</param>
/// <param name="OilType">The name of the oil type the engine requires.</param>
/// <param name="LastOilChange">The last oil change date.</param>
/// <param name="OilQuantity">The oil capacity, measured in the oil unit.</param>
/// <param name="IsActive">Whether the unit is in service.</param>
/// <param name="Status">Available, Needs Repair (an open repair work order) or Retired.</param>
/// <param name="OilTypeId">The key of the oil type the engine requires.</param>
/// <param name="OilUnitId">The key of the unit the oil quantity is measured in.</param>
/// <param name="OilUnit">The short label of that unit, such as "qt".</param>
public sealed record MowerResponse(
    Guid Id,
    string Name,
    Guid AssetTypeId,
    string AssetType,
    Guid ManufacturerId,
    string Manufacturer,
    Guid ManufacturerModelId,
    string Model,
    string SerialNumber,
    DateOnly PurchaseDate,
    decimal EngineHours,
    string OilType,
    DateOnly? LastOilChange,
    decimal OilQuantity,
    bool IsActive,
    AssetStatus Status = AssetStatus.Available,
    Guid? OilUnitId = null,
    string OilUnit = "",
    Guid? OilTypeId = null);
