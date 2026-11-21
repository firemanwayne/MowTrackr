using MowTrackr.Core.Enums;
using MowTrackr.Assets.Models;

namespace MowTrackr.Assets.Extensions;

/// <summary>
/// Mapping helpers between <see cref="Mower"/> and its contracts.
/// </summary>
internal static class MowerExtensions
{
    /// <summary>Maps an entity to its API response.</summary>
    /// <param name="mower">The entity.</param>
    /// <param name="needsRepair">Whether the unit has an open repair work order.</param>
    /// <returns>The response.</returns>
    public static MowerResponse ToResponse(this Mower mower, bool needsRepair = false)
    {
        return new MowerResponse(
            mower.Id,
            mower.Name,
            mower.AssetTypeId,
            mower.AssetType?.Name ?? string.Empty,
            mower.ManufacturerId,
            mower.Manufacturer?.Name ?? string.Empty,
            mower.ManufacturerModelId,
            mower.Model?.Name ?? string.Empty,
            mower.SerialNumber,
            mower.PurchaseDate,
            mower.EngineHours,
            mower.OilType,
            mower.LastOilChange,
            mower.OilQuantityQuarts,
            mower.IsActive,
            mower.StatusFor(needsRepair));
    }

    /// <summary>Works out whether the unit is available: retired when out of service, needing repair when a repair work order is open.</summary>
    /// <param name="mower">The mower.</param>
    /// <param name="needsRepair">Whether it has an open repair work order.</param>
    /// <returns>The status.</returns>
    public static AssetStatus StatusFor(this Mower mower, bool needsRepair)
    {
        if (!mower.IsActive)
        {
            return AssetStatus.Retired;
        }

        return needsRepair ? AssetStatus.NeedsRepair : AssetStatus.Available;
    }

    /// <summary>Copies request values onto an entity.</summary>
    /// <param name="mower">The entity to modify.</param>
    /// <param name="request">The new values.</param>
    public static void Apply(this Mower mower, MowerRequest request)
    {
        mower.Name = request.Name.Trim();

        mower.AssetTypeId = request.AssetTypeId ?? Guid.Empty;

        mower.ManufacturerId = request.ManufacturerId ?? Guid.Empty;

        mower.ManufacturerModelId = request.ManufacturerModelId ?? Guid.Empty;

        mower.SerialNumber = request.SerialNumber.Trim();

        mower.PurchaseDate = request.PurchaseDate;

        mower.EngineHours = request.EngineHours;

        mower.OilType = request.OilType.Trim();

        mower.LastOilChange = request.LastOilChange;

        mower.OilQuantityQuarts = request.OilQuantityQuarts;

        mower.IsActive = request.IsActive;

        mower.UpdatedAt = DateTimeOffset.UtcNow;
    }
}