using System.Security.Claims;
using System.Text.Json;

namespace MowTrackr.Client.Providers;

/// <summary>
/// Builds the Blazor authentication state from the stored JWT (no server round-trip; the API validates the token itself).
/// </summary>
/// <param name="tokens">The token store.</param>
internal sealed class JwtAuthenticationStateProvider(ITokenStore tokens) : AuthenticationStateProvider
{
    private const string AuthenticationType = "jwt";

    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    /// <inheritdoc />
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokens.GetAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return Anonymous;
        }

        var claims = TryParseClaims(token);

        if (claims is null)
        {
            await tokens.ClearAsync();

            return Anonymous;
        }

        var identity = new ClaimsIdentity(claims, AuthenticationType, AppClaims.Name, AppClaims.Role);

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    /// <summary>Tells the UI that the session changed so it re-evaluates <c>AuthorizeView</c>s.</summary>
    public void NotifyStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
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