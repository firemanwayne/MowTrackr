namespace MowTrackr.Client.Abstractions;

/// <summary>
/// Shared HTTP plumbing for the typed API services: JSON in/out and uniform error handling.
/// </summary>
/// <param name="http">The HTTP client (base address and bearer handler are configured at registration).</param>
/// <param name="localizer">The string localizer for user-facing error messages.</param>
internal abstract class ApiServiceBase(HttpClient http, IStringLocalizer<SharedResource> localizer)
{
    /// <summary>Sends a GET and reads the JSON payload.</summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="url">The relative URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result.</returns>
    protected async Task<ApiResult<T>> GetAsync<T>(string url, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        return await SendAsync<T>(request, cancellationToken);
    }

    /// <summary>Sends a request with a JSON body and reads the JSON payload.</summary>
    /// <typeparam name="TRequest">The body type.</typeparam>
    /// <typeparam name="TResponse">The payload type.</typeparam>
    /// <param name="method">The HTTP method.</param>
    /// <param name="url">The relative URL.</param>
    /// <param name="body">The body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result.</returns>
    protected async Task<ApiResult<TResponse>> SendAsync<TRequest, TResponse>(HttpMethod method, string url, TRequest body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, url) { Content = JsonContent.Create(body) };

        return await SendAsync<TResponse>(request, cancellationToken);
    }

    /// <summary>Sends a body-less request (POST/DELETE) and reads the JSON payload, if any.</summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="method">The HTTP method.</param>
    /// <param name="url">The relative URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result.</returns>
    protected async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, url);

        return await SendAsync<T>(request, cancellationToken);
    }

    /// <summary>Sends a prepared request and maps the response (including failures) to an <see cref="ApiResult{T}"/>.</summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result.</returns>
    protected async Task<ApiResult<T>> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await http.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var value = response.Content.Headers.ContentLength is 0 || response.StatusCode == HttpStatusCode.NoContent
                    ? default
                    : await response.Content.ReadFromJsonAsync<T>(cancellationToken);

                return new ApiResult<T>(response.StatusCode, value, null);
            }

            return await ReadFailureAsync<T>(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return new ApiResult<T>(HttpStatusCode.ServiceUnavailable, default, localizer["Unable to reach the API at {0}. Check that it is running and the browser trusts its certificate.", http.BaseAddress?.ToString() ?? string.Empty].Value);
        }
    }

    /// <summary>Reads a failed response. A 409 whose body matches <typeparamref name="T"/> keeps that body as the value.</summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="response">The failed response.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The failed result.</returns>
    private async Task<ApiResult<T>> ReadFailureAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var text = await response.Content.ReadAsStringAsync(cancellationToken);

        T? detail = default;

        string? error = null;

        if (!string.IsNullOrWhiteSpace(text))
        {
            try
            {
                var problem = System.Text.Json.JsonSerializer.Deserialize<ProblemResponse>(text, System.Text.Json.JsonSerializerOptions.Web);

                error = problem?.Detail ?? problem?.Title;

                if (response.StatusCode == HttpStatusCode.Conflict && problem?.Title is null)
                {
                    detail = System.Text.Json.JsonSerializer.Deserialize<T>(text, System.Text.Json.JsonSerializerOptions.Web);
                }
            }
            catch (System.Text.Json.JsonException)
            {
                error = null;
            }
        }

        error ??= response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => localizer["Please sign in again."].Value,
            HttpStatusCode.Forbidden => localizer["You do not have permission to do that."].Value,
            _ => localizer["The request failed ({0}).", (int)response.StatusCode].Value,
        };

        return new ApiResult<T>(response.StatusCode, detail, error);
    }
}