using MowTrackr.Assets.Extensions;
using MowTrackr.Assets.Models;

namespace MowTrackr.Assets.Services;

/// <summary>
/// Default <see cref="IMowerService"/> backed by the generic repository.
/// </summary>
/// <param name="mowers">The mower repository.</param>
/// <param name="models">The model repository, used to validate the manufacturer/model pairing.</param>
/// <param name="assetTypes">The asset type repository, used to validate the chosen type.</param>
/// <param name="openRepairs">Tells which units have an open repair, for the status.</param>
/// <param name="logger">The logger.</param>
internal sealed class MowerService(IRepository<Mower> mowers, IRepository<ManufacturerModel> models, IRepository<AssetType> assetTypes, IOpenRepairLookup openRepairs, ILogger<MowerService> logger) : IMowerService
{
    /// <summary>Navigation paths loaded so responses can carry the manufacturer and model names.</summary>
    private static readonly string[] _lookupIncludes = [nameof(Mower.AssetType), nameof(Mower.Manufacturer), nameof(Mower.Model)];

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<MowerResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var spec = new QuerySpec<Mower> { Includes = _lookupIncludes, OrderBy = m => m.Name };

        var items = await mowers.ListAsync(spec, cancellationToken).ConfigureAwait(false);

        var repairs = await openRepairs.GetMowerIdsNeedingRepairAsync(cancellationToken).ConfigureAwait(false);

        return Result<IReadOnlyList<MowerResponse>>.Ok(items.Select(m => m.ToResponse(repairs.Contains(m.Id))).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<MowerResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var mower = await mowers.GetByIdAsync(id, new QuerySpec<Mower> { Includes = _lookupIncludes }, cancellationToken).ConfigureAwait(false);

        if (mower is null)
        {
            return Result<MowerResponse>.NotFound("Mower not found.");
        }

        var repairs = await openRepairs.GetMowerIdsNeedingRepairAsync(cancellationToken).ConfigureAwait(false);

        return Result<MowerResponse>.Ok(mower.ToResponse(repairs.Contains(mower.Id)));
    }

    /// <inheritdoc />
    public async Task<Result<MowerResponse>> CreateAsync(MowerRequest request, CancellationToken cancellationToken)
    {
        var duplicate = await SerialExistsAsync(request.SerialNumber, null, cancellationToken).ConfigureAwait(false);

        if (duplicate)
        {
            return Result<MowerResponse>.Conflict("A mower with that serial number already exists.");
        }

        var pairingError = await ValidatePairingAsync(request, cancellationToken).ConfigureAwait(false);

        if (pairingError is not null)
        {
            return Result<MowerResponse>.Invalid(pairingError);
        }

        var mower = new Mower();

        mower.Apply(request);

        await mowers.AddAsync(mower, cancellationToken).ConfigureAwait(false);

        await mowers.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Created mower {MowerId} ({MowerName})", mower.Id, mower.Name);

        return await GetAsync(mower.Id, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<MowerResponse>> UpdateAsync(Guid id, MowerRequest request, CancellationToken cancellationToken)
    {
        var mower = await mowers.GetByIdAsync(id, new QuerySpec<Mower> { Track = true }, cancellationToken).ConfigureAwait(false);

        if (mower is null)
        {
            return Result<MowerResponse>.NotFound("Mower not found.");
        }

        var duplicate = await SerialExistsAsync(request.SerialNumber, id, cancellationToken).ConfigureAwait(false);

        if (duplicate)
        {
            return Result<MowerResponse>.Conflict("A mower with that serial number already exists.");
        }

        var pairingError = await ValidatePairingAsync(request, cancellationToken).ConfigureAwait(false);

        if (pairingError is not null)
        {
            return Result<MowerResponse>.Invalid(pairingError);
        }

        mower.Apply(request);

        await mowers.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Updated mower {MowerId}", mower.Id);

        return await GetAsync(mower.Id, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var mower = await mowers.GetByIdAsync(id, new QuerySpec<Mower> { Track = true }, cancellationToken).ConfigureAwait(false);

        if (mower is null)
        {
            return Result<bool>.NotFound("Mower not found.");
        }

        mowers.Remove(mower);

        await mowers.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Deleted mower {MowerId}", id);

        return Result<bool>.Ok(true);
    }

    /// <summary>Checks whether another mower already uses a serial number.</summary>
    /// <param name="serialNumber">The serial number.</param>
    /// <param name="excludeId">A mower to ignore (the one being edited).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> if the serial is taken.</returns>
    private async Task<bool> SerialExistsAsync(string serialNumber, Guid? excludeId, CancellationToken cancellationToken)
    {
        var serial = serialNumber.Trim();

        var spec = new QuerySpec<Mower> { Filter = m => m.SerialNumber == serial && m.Id != excludeId };

        var matches = await mowers.ListAsync(spec, cancellationToken).ConfigureAwait(false);

        return matches.Count > 0;
    }

    /// <summary>Checks that the chosen asset type exists and that the model belongs to the chosen manufacturer.</summary>
    /// <param name="request">The mower request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An error message, or <see langword="null"/> when the pairing is valid.</returns>
    private async Task<string?> ValidatePairingAsync(MowerRequest request, CancellationToken cancellationToken)
    {
        var typeId = request.AssetTypeId;

        var types = await assetTypes.ListAsync(new QuerySpec<AssetType> { Filter = t => t.Id == typeId }, cancellationToken).ConfigureAwait(false);

        if (types.Count == 0)
        {
            return "Choose an asset type.";
        }

        var modelId = request.ManufacturerModelId;

        var manufacturerId = request.ManufacturerId;

        var spec = new QuerySpec<ManufacturerModel> { Filter = m => m.Id == modelId && m.ManufacturerId == manufacturerId };

        var matches = await models.ListAsync(spec, cancellationToken).ConfigureAwait(false);

        return matches.Count == 0 ? "Choose a manufacturer and one of its models." : null;
    }
}
