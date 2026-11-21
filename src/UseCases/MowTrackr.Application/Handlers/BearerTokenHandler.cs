using System.Net.Http.Headers;

namespace MowTrackr.Application.Handlers;

/// <summary>
/// Adds the stored JWT to every outgoing API request, and ends the session when the API says the token is no longer
/// valid (expired, or signed with a key the API no longer accepts).
/// </summary>
/// <param name="tokens">The token store.</param>
/// <param name="expiry">The session-expiry signal.</param>
internal sealed class BearerTokenHandler(ITokenStore tokens, ISessionExpiry expiry) : DelegatingHandler
{
    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await tokens.GetAsync();

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Only a request that carried a token can mean "your session is gone"; a wrong password at sign-in is also a 401.
        if (response.StatusCode == HttpStatusCode.Unauthorized && !string.IsNullOrEmpty(token))
        {
            await tokens.ClearAsync();

            expiry.SignalExpired();
        }

        return response;
    }
}