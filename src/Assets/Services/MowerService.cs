using MowTrackr.Assets.Extensions;
using MowTrackr.Assets.Models;

namespace MowTrackr.Assets.Services;

/// <summary>
/// Default <see cref="IMowerService"/> backed by the generic repository.
/// </summary>
/// <param name="mowers">The mower repository.</param>
/// <param name="logger">The logger.</param>
internal sealed class MowerService(IRepository<Mower> mowers, ILogger<MowerService> logger) : IMowerService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<MowerResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var spec = new QuerySpec<Mower> { OrderBy = m => m.Name };

        var items = await mowers.ListAsync(spec, cancellationToken).ConfigureAwait(false);

        return Result<IReadOnlyList<MowerResponse>>.Ok(items.Select(m => m.ToResponse()).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<MowerResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var mower = await mowers.GetByIdAsync(id, null, cancellationToken).ConfigureAwait(false);

        if (mower is null)
        {
            return Result<MowerResponse>.NotFound("Mower not found.");
        }

        return Result<MowerResponse>.Ok(mower.ToResponse());
    }

    /// <inheritdoc />
    public async Task<Result<MowerResponse>> CreateAsync(MowerRequest request, CancellationToken cancellationToken)
    {
        var duplicate = await SerialExistsAsync(request.SerialNumber, null, cancellationToken).ConfigureAwait(false);

        if (duplicate)
        {
            return Result<MowerResponse>.Conflict("A mower with that serial number already exists.");
        }

        var mower = new Mower();

        mower.Apply(request);

        await mowers.AddAsync(mower, cancellationToken).ConfigureAwait(false);

        await mowers.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Created mower {MowerId} ({MowerName})", mower.Id, mower.Name);

        return Result<MowerResponse>.Ok(mower.ToResponse());
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

        mower.Apply(request);

        await mowers.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Updated mower {MowerId}", mower.Id);

        return Result<MowerResponse>.Ok(mower.ToResponse());
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
}