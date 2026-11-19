using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MowTrackr.Core.Constants;
using MowTrackr.Identity.Interfaces;
using MowTrackr.Identity.Models;
using MowTrackr.Identity.Options;
using MowTrackr.Identity.Services;
using MowTrackr.Identity.Validators;

namespace MowTrackr.Identity;

/// <summary>
/// Registers the Identity domain services: ASP.NET Core Identity, JWT issuing and JWT bearer validation.
/// </summary>
public static class ServiceRegistryExtensions
{
    private const int MinimumSigningKeyBytes = 32;

    /// <summary>
    /// Adds Identity (users and roles; the EF Core stores are attached by the Data library), JWT bearer
    /// authentication, the role-based authorization policies and the auth/user services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services
            .AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .Validate(o => Encoding.UTF8.GetByteCount(o.SigningKey) >= MinimumSigningKeyBytes, $"Jwt:SigningKey must be at least {MinimumSigningKeyBytes} bytes.")
            .ValidateOnStart();

        services.AddOptions<InvitationOptions>().BindConfiguration(InvitationOptions.SectionName);

        services.TryAddSingleton(TimeProvider.System);

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                // Password rules live in AppPasswordValidator (shared with the client), so the built-in checks are relaxed to avoid duplicate messages.
                options.Password.RequiredLength = 1;
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<ApplicationRole>()
            .AddPasswordValidator<AppPasswordValidator>()
            .AddDefaultTokenProviders();

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
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
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

        services.TryAddScoped<IInvitationService, InvitationService>();

        services.AddHostedService<RoleSeedService>();

        return services;
    }
}