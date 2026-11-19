using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MowTrackr.Core.Constants;
using MowTrackr.Identity.Interfaces;
using MowTrackr.Identity.Models;
using MowTrackr.Identity.Options;
using MowTrackr.Identity.Services;

namespace MowTrackr.Identity;

/// <summary>
/// Registers the Identity domain services: ASP.NET Core Identity, JWT issuing and JWT bearer validation.
/// </summary>
public static class ServiceRegistryExtensions
{
    /// <summary>
    /// Adds Identity (users and roles; the EF Core stores are attached by the Data library), JWT bearer
    /// authentication, the role-based authorization policies and the auth/user services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddOptions<JwtOptions>().BindConfiguration(JwtOptions.SectionName);

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;

                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddRoles<ApplicationRole>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
            {
                var settings = jwt.Value;

                bearer.MapInboundClaims = false;

                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = AppClaims.Name,
                    RoleClaimType = AppClaims.Role,
                };
            });

        services.AddAuthorization();

        services.TryAddSingleton<ITokenService, TokenService>();

        services.TryAddScoped<IAuthService, AuthService>();

        services.TryAddScoped<IUserService, UserService>();

        services.AddHostedService<RoleSeedService>();

        return services;
    }
}