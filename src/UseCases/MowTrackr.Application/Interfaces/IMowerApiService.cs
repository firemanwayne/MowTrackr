namespace MowTrackr.Application.Interfaces;

/// <summary>
/// Client for the mower endpoints.
/// </summary>
internal interface IMowerApiService
{
    /// <summary>Lists mowers.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The mowers.</returns>
    Task<ApiResult<List<MowerResponse>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Adds a mower.</summary>
    /// <param name="request">The details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created mower.</returns>
    Task<ApiResult<MowerResponse>> CreateAsync(MowerRequest request, CancellationToken cancellationToken);

    /// <summary>Updates a mower.</summary>
    /// <param name="id">The mower key.</param>
    /// <param name="request">The new details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated mower.</returns>
    Task<ApiResult<MowerResponse>> UpdateAsync(Guid id, MowerRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a mower.</summary>
    /// <param name="id">The mower key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result.</returns>
    Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}