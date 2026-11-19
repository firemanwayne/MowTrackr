using System.Net.Http.Headers;

namespace MowTrackr.Client.Handlers;

/// <summary>
/// Adds the stored JWT to every outgoing API request.
/// </summary>
/// <param name="tokens">The token store.</param>
internal sealed class BearerTokenHandler(ITokenStore tokens) : DelegatingHandler
{
    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await tokens.GetAsync();

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}