using MowTrackr.Core.Contracts.Assets;

namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Manages the mower asset register.
/// </summary>
public interface IMowerService
{
    /// <summary>Lists all mowers.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The mowers.</returns>
    Task<Result<IReadOnlyList<MowerResponse>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Gets one mower.</summary>
    /// <param name="id">The mower key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The mower.</returns>
    Task<Result<MowerResponse>> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Adds a mower.</summary>
    /// <param name="request">The mower details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created mower.</returns>
    Task<Result<MowerResponse>> CreateAsync(MowerRequest request, CancellationToken cancellationToken);

    /// <summary>Updates a mower.</summary>
    /// <param name="id">The mower key.</param>
    /// <param name="request">The new details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated mower.</returns>
    Task<Result<MowerResponse>> UpdateAsync(Guid id, MowerRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a mower.</summary>
    /// <param name="id">The mower key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}