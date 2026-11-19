using System.Security.Claims;
using System.Text.Json;

namespace MowTrackr.Client.Providers;

/// <summary>
/// Builds the Blazor authentication state from the stored JWT (no server round-trip; the API validates the token itself).
/// </summary>
/// <param name="tokens">The token store.</param>
/// <param name="expiry">The session-expiry signal.</param>
internal sealed class JwtAuthenticationStateProvider(ITokenStore tokens, ISessionExpiry expiry) : AuthenticationStateProvider, IDisposable
{
    private const string AuthenticationType = "jwt";

    private static readonly TimeSpan MaxTimerDelay = TimeSpan.FromDays(24);

    private static readonly TimeSpan ExpiryGrace = TimeSpan.FromSeconds(1);

    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    private Timer? _expiryTimer;

    private bool _subscribed;

    /// <inheritdoc />
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        Subscribe();

        var token = await tokens.GetAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return Anonymous;
        }

        var claims = TryParseClaims(token);

        if (claims is null)
        {
            await tokens.ClearAsync();

            // There was a token and it is no longer good (expired or damaged): tell the user, do not just show the login page.
            expiry.FlagExpired();

            return Anonymous;
        }

        ScheduleExpiry(claims);

        var identity = new ClaimsIdentity(claims, AuthenticationType, AppClaims.Name, AppClaims.Role);

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _expiryTimer?.Dispose();

        expiry.Expired -= NotifyStateChanged;
    }

    /// <summary>Tells the UI that the session changed so it re-evaluates <c>AuthorizeView</c>s.</summary>
    public void NotifyStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    /// <summary>Starts listening for the session ending unexpectedly (once).</summary>
    private void Subscribe()
    {
        if (_subscribed)
        {
            return;
        }

        _subscribed = true;

        expiry.Expired += NotifyStateChanged;
    }

    /// <summary>Arranges for the app to notice the moment the token expires, even if the page sits idle.</summary>
    /// <param name="claims">The token's claims.</param>
    private void ScheduleExpiry(List<Claim> claims)
    {
        _expiryTimer?.Dispose();

        var expiryClaim = claims.FirstOrDefault(c => c.Type == "exp");

        if (expiryClaim is null || !long.TryParse(expiryClaim.Value, out var seconds))
        {
            return;
        }

        // A moment after the expiry, not on it: a timer can fire a few milliseconds early, and the token only counts as expired once its second has passed.
        var remaining = DateTimeOffset.FromUnixTimeSeconds(seconds) - DateTimeOffset.UtcNow + ExpiryGrace;

        if (remaining <= ExpiryGrace || remaining > MaxTimerDelay)
        {
            return;
        }

        _expiryTimer = new Timer(_ => expiry.SignalExpired(), null, remaining, Timeout.InfiniteTimeSpan);
    }

    /// <summary>Reads the claims out of a JWT payload, returning <see langword="null"/> if it is malformed or expired.</summary>
    /// <param name="token">The JWT.</param>
    /// <returns>The claims, or <see langword="null"/>.</returns>
    private static List<Claim>? TryParseClaims(string token)
    {
        var parts = token.Split('.');

        if (parts.Length != 3)
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(DecodeBase64Url(parts[1]));

            var claims = new List<Claim>();

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    claims.AddRange(property.Value.EnumerateArray().Select(v => new Claim(property.Name, v.ToString())));

                    continue;
                }

                claims.Add(new Claim(property.Name, property.Value.ToString()));
            }

            var expiry = claims.FirstOrDefault(c => c.Type == "exp");

            if (expiry is not null && long.TryParse(expiry.Value, out var seconds) && DateTimeOffset.FromUnixTimeSeconds(seconds) <= DateTimeOffset.UtcNow)
            {
                return null;
            }

            return claims;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (FormatException)
        {
            return null;
        }
    }

    /// <summary>Decodes a base64url segment.</summary>
    /// <param name="segment">The segment.</param>
    /// <returns>The decoded bytes.</returns>
    private static byte[] DecodeBase64Url(string segment)
    {
        var padded = segment.Replace('-', '+').Replace('_', '/');

        padded = padded.PadRight(padded.Length + ((4 - (padded.Length % 4)) % 4), '=');

        return Convert.FromBase64String(padded);
    }
}