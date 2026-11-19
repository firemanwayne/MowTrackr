using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace MowTrackr.Api.Extensions;

/// <summary>
/// Rate limiting: a global per-address ceiling plus tighter policies for the endpoints attackers target.
/// </summary>
public static class RateLimitingExtensions
{
    private const int GlobalPerMinute = 240;

    private const int AuthPerMinute = 10;

    private const int SmsPerWindow = 3;

    private const int PhoneVerifyPerWindow = 8;

    private const string UnknownClient = "unknown";

    /// <summary>Adds the rate limiter and its policies.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(System.Globalization.CultureInfo.InvariantCulture);
                }

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too many requests",
                    Detail = "Too many attempts. Wait a moment and try again.",
                };

                await context.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken).ConfigureAwait(false);
            };

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
                RateLimitPartition.GetFixedWindowLimiter($"global:{ClientAddress(http)}", _ => Window(GlobalPerMinute, TimeSpan.FromMinutes(1))));

            options.AddPolicy(RateLimitPolicies.Auth, http =>
                RateLimitPartition.GetFixedWindowLimiter($"auth:{ClientAddress(http)}", _ => Window(AuthPerMinute, TimeSpan.FromMinutes(1))));

            options.AddPolicy(RateLimitPolicies.Sms, http =>
                RateLimitPartition.GetFixedWindowLimiter($"sms:{UserOrAddress(http)}", _ => Window(SmsPerWindow, TimeSpan.FromMinutes(10))));

            options.AddPolicy(RateLimitPolicies.PhoneVerify, http =>
                RateLimitPartition.GetFixedWindowLimiter($"verify:{UserOrAddress(http)}", _ => Window(PhoneVerifyPerWindow, TimeSpan.FromMinutes(10))));
        });

        return services;
    }

    /// <summary>Builds a fixed-window limiter that rejects instead of queueing.</summary>
    /// <param name="permits">The requests allowed per window.</param>
    /// <param name="window">The window length.</param>
    /// <returns>The limiter options.</returns>
    private static FixedWindowRateLimiterOptions Window(int permits, TimeSpan window)
    {
        return new FixedWindowRateLimiterOptions { PermitLimit = permits, Window = window, QueueLimit = 0 };
    }

    /// <summary>Gets the client's address (the real one when forwarded headers are enabled).</summary>
    /// <param name="http">The request.</param>
    /// <returns>The address.</returns>
    private static string ClientAddress(HttpContext http)
    {
        return http.Connection.RemoteIpAddress?.ToString() ?? UnknownClient;
    }

    /// <summary>Gets the signed-in user's key, or the client address when anonymous.</summary>
    /// <param name="http">The request.</param>
    /// <returns>The partition key.</returns>
    private static string UserOrAddress(HttpContext http)
    {
        var id = http.User.GetUserId();

        return id == Guid.Empty ? ClientAddress(http) : id.ToString();
    }
}