using MowTrackr.Assets.Models;

namespace MowTrackr.Assets.Extensions;

/// <summary>
/// Mapping helpers between <see cref="Mower"/> and its contracts.
/// </summary>
internal static class MowerExtensions
{
    /// <summary>Maps an entity to its API response.</summary>
    /// <param name="mower">The entity.</param>
    /// <returns>The response.</returns>
    public static MowerResponse ToResponse(this Mower mower)
    {
        return new MowerResponse(
            mower.Id,
            mower.Name,
            mower.Manufacturer,
            mower.Model,
            mower.SerialNumber,
            mower.PurchaseDate,
            mower.EngineHours,
            mower.OilType,
            mower.LastOilChange,
            mower.OilQuantityQuarts,
            mower.IsActive);
    }

    /// <summary>Copies request values onto an entity.</summary>
    /// <param name="mower">The entity to modify.</param>
    /// <param name="request">The new values.</param>
    public static void Apply(this Mower mower, MowerRequest request)
    {
        mower.Name = request.Name.Trim();

        mower.Manufacturer = request.Manufacturer.Trim();

        mower.Model = request.Model.Trim();

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