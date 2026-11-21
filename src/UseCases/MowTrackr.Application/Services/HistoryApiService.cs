namespace MowTrackr.Application.Services;

/// <summary>
/// Default <see cref="IHistoryApiService"/>.
/// </summary>
/// <param name="http">The HTTP client.</param>
/// <param name="localizer">The string localizer for error messages.</param>
internal sealed class HistoryApiService(HttpClient http, IStringLocalizer<SharedResource> localizer) : ApiServiceBase(http, localizer), IHistoryApiService
{
    /// <inheritdoc />
    public Task<ApiResult<List<ChangeHistoryResponse>>> ListAsync(string entityType, Guid entityId, CancellationToken cancellationToken)
    {
        return GetAsync<List<ChangeHistoryResponse>>($"{ApiRoutes.History}/{Uri.EscapeDataString(entityType)}/{entityId}", cancellationToken);
    }
}
