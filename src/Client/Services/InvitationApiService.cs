namespace MowTrackr.Client.Services;

/// <summary>
/// Default <see cref="IInvitationApiService"/>.
/// </summary>
/// <param name="http">The HTTP client.</param>
/// <param name="localizer">The string localizer for error messages.</param>
internal sealed class InvitationApiService(HttpClient http, IStringLocalizer<SharedResource> localizer) : ApiServiceBase(http, localizer), IInvitationApiService
{
    /// <inheritdoc />
    public Task<ApiResult<List<InvitationResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        return GetAsync<List<InvitationResponse>>(ApiRoutes.Invitations, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<InvitationSentResponse>> CreateAsync(CreateInvitationRequest request, CancellationToken cancellationToken)
    {
        return SendAsync<CreateInvitationRequest, InvitationSentResponse>(HttpMethod.Post, ApiRoutes.Invitations, request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<InvitationSentResponse>> ResendAsync(Guid id, CancellationToken cancellationToken)
    {
        return SendAsync<InvitationSentResponse>(HttpMethod.Post, $"{ApiRoutes.Invitations}/{id}/resend", cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<InvitationResponse>> RevokeAsync(Guid id, CancellationToken cancellationToken)
    {
        return SendAsync<InvitationResponse>(HttpMethod.Delete, $"{ApiRoutes.Invitations}/{id}", cancellationToken);
    }
}