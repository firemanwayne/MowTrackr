using MowTrackr.Application.Handlers;
using MowTrackr.Application.Providers;
using MowTrackr.Application.Services;
using MowTrackr.Application.Stores;
using MowTrackr.Localization;

namespace MowTrackr.Application;

/// <summary>
/// Registers the client's services: token storage, authentication state and the typed API clients.
/// </summary>
internal static class ServiceRegistryExtensions
{
    /// <summary>Adds authentication and the API clients, all pointed at the configured API base address.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Sets the <see cref="ApiClientOptions"/>; <see cref="ApiClientOptions.BaseAddress"/> is required.</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, Action<ApiClientOptions> configure)
    {
        services.Configure(configure);

        services.AddLocalizationServices();

        services.AddAuthorizationCore();

        services.AddCascadingAuthenticationState();

        services.TryAddSingleton<ITokenStore, LocalStorageTokenStore>();

        services.TryAddSingleton<ISessionExpiry, SessionExpiry>();

        services.TryAddScoped<JwtAuthenticationStateProvider>();

        services.TryAddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());

        services.TryAddScoped<ISessionService, SessionService>();

        services.TryAddScoped<IPreferencesService, PreferencesService>();

        services.TryAddTransient<BearerTokenHandler>();

        services
            .AddApiClient<IAuthApiService, AuthApiService>()
            .AddApiClient<IMowerApiService, MowerApiService>()
            .AddApiClient<IManufacturerApiService, ManufacturerApiService>()
            .AddApiClient<IAssetTypeApiService, AssetTypeApiService>()
            .AddApiClient<IServiceTemplateApiService, ServiceTemplateApiService>()
            .AddApiClient<IInspectionTemplateApiService, InspectionTemplateApiService>()
            .AddApiClient<IMaintenanceApiService, MaintenanceApiService>()
            .AddApiClient<IUserApiService, UserApiService>()
            .AddApiClient<IInvitationApiService, InvitationApiService>();

        return services;
    }

    /// <summary>Registers one typed HTTP client with the base address and bearer handler.</summary>
    /// <typeparam name="TInterface">The service interface.</typeparam>
    /// <typeparam name="TImplementation">The implementation.</typeparam>
    /// <param name="services">The service collection.</param>
    private static IServiceCollection AddApiClient<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services
            .AddHttpClient<TInterface, TImplementation>((provider, client) => client.BaseAddress = ResolveBaseAddress(provider))
            .AddHttpMessageHandler<BearerTokenHandler>();

        return services;
    }

    /// <summary>Reads the configured API base address, failing loudly if the host never set one.</summary>
    /// <param name="provider">The service provider.</param>
    /// <returns>The API base address.</returns>
    private static Uri ResolveBaseAddress(IServiceProvider provider)
    {
        var options = provider
            .GetRequiredService<IOptions<ApiClientOptions>>()
            .Value;

        return options.BaseAddress ?? throw new InvalidOperationException($"{nameof(ApiClientOptions)}.{nameof(ApiClientOptions.BaseAddress)} was not set in the {nameof(AddApplicationServices)} callback.");
    }
}