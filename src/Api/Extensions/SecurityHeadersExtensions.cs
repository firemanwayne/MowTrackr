namespace MowTrackr.Api.Extensions;

/// <summary>
/// Adds the response headers every API response should carry.
/// </summary>
public static class SecurityHeadersExtensions
{
    /// <summary>
    /// Adds <c>X-Content-Type-Options</c>, <c>X-Frame-Options</c>, <c>Referrer-Policy</c>, a locked-down
    /// <c>Content-Security-Policy</c> (the API serves only JSON), <c>Permissions-Policy</c> and
    /// <c>Cache-Control: no-store</c> (responses carry tokens and personal data).
    /// </summary>
    /// <param name="app">The application.</param>
    /// <returns>The same application, for chaining.</returns>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;

            headers.XContentTypeOptions = "nosniff";

            headers.XFrameOptions = "DENY";

            headers["Referrer-Policy"] = "no-referrer";

            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'";

            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";

            headers.CacheControl = "no-store";

            await next(context).ConfigureAwait(false);
        });
    }
}