using Microsoft.AspNetCore.Authentication.Cookies;

namespace MowTrackr.Api.Extensions;

/// <summary>
/// Registers the optional Google, Facebook and LinkedIn sign-in handlers. A provider is only enabled when its
/// client id and secret are configured (section <c>Authentication:{Provider}</c>), so a bare checkout still runs.
/// </summary>
public static class ExternalAuthenticationExtensions
{
    /// <summary>The temporary cookie scheme that carries the provider identity between the provider and our callback.</summary>
    public const string ExternalScheme = "External";

    /// <summary>Adds the external provider handlers that have credentials configured.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration (read once at startup).</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection AddExternalProviders(this IServiceCollection services, IConfiguration configuration)
    {
        var auth = services.AddAuthentication();

        auth.AddCookie(ExternalScheme, options =>
        {
            options.Cookie.SameSite = SameSiteMode.Lax;

            options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        });

        var google = configuration.GetSection($"Authentication:{AuthProviders.Google}");

        if (HasCredentials(google))
        {
            auth.AddGoogle(AuthProviders.Google, options =>
            {
                options.SignInScheme = ExternalScheme;

                options.ClientId = google["ClientId"]!;

                options.ClientSecret = google["ClientSecret"]!;
            });
        }

        var facebook = configuration.GetSection($"Authentication:{AuthProviders.Facebook}");

        if (HasCredentials(facebook))
        {
            auth.AddFacebook(AuthProviders.Facebook, options =>
            {
                options.SignInScheme = ExternalScheme;

                options.AppId = facebook["ClientId"]!;

                options.AppSecret = facebook["ClientSecret"]!;
            });
        }

        var linkedIn = configuration.GetSection($"Authentication:{AuthProviders.LinkedIn}");

        if (HasCredentials(linkedIn))
        {
            auth.AddLinkedIn(AuthProviders.LinkedIn, options =>
            {
                options.SignInScheme = ExternalScheme;

                options.ClientId = linkedIn["ClientId"]!;

                options.ClientSecret = linkedIn["ClientSecret"]!;
            });
        }

        return services;
    }

    /// <summary>Checks that a provider section has both a client id and secret.</summary>
    /// <param name="section">The provider configuration section.</param>
    /// <returns><see langword="true"/> when both are present.</returns>
    private static bool HasCredentials(IConfigurationSection section)
    {
        return !string.IsNullOrWhiteSpace(section["ClientId"]) && !string.IsNullOrWhiteSpace(section["ClientSecret"]);
    }
}