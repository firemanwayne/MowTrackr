using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MowTrackr.Core.Constants;
using MowTrackr.Identity.Interfaces;
using MowTrackr.Identity.Models;
using MowTrackr.Identity.Options;

namespace MowTrackr.Identity.Services;

/// <summary>
/// Default <see cref="ITokenService"/> that issues HMAC-SHA256 signed JWTs.
/// </summary>
/// <param name="options">The JWT settings.</param>
internal sealed class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    /// <inheritdoc />
    public (string Token, DateTimeOffset ExpiresAt) CreateToken(ApplicationUser user, IReadOnlyList<string> roles)
    {
        var settings = options.Value;

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(settings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(AppClaims.UserId, user.Id.ToString()),
            new(AppClaims.Email, user.Email ?? string.Empty),
            new(AppClaims.Name, user.DisplayName),
        };

        claims.AddRange(roles.Select(r => new Claim(AppClaims.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);

        return (token, expiresAt);
    }
}