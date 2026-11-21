using MowTrackr.Client.Handlers;
using MowTrackr.Client.Providers;
using MowTrackr.Client.Services;
using MowTrackr.Client.Stores;
using MowTrackr.Localization;

namespace MowTrackr.Client;

/// <summary>
/// Registers the client's services: token storage, authentication state and the typed API clients.
/// </summary>
internal static class ServiceRegistryExtensions
{
    /// <summary>Adds authentication and the API clients, all pointed at the configured API base address.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiBaseAddress">The API base address.</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection AddClientServices(this IServiceCollection services, Uri apiBaseAddress)
    {
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

        AddApiClient<IAuthApiService, AuthApiService>(services, apiBaseAddress);

        AddApiClient<IMowerApiService, MowerApiService>(services, apiBaseAddress);

        AddApiClient<IManufacturerApiService, ManufacturerApiService>(services, apiBaseAddress);

        AddApiClient<IAssetTypeApiService, AssetTypeApiService>(services, apiBaseAddress);

        AddApiClient<IServiceTemplateApiService, ServiceTemplateApiService>(services, apiBaseAddress);

        AddApiClient<IInspectionTemplateApiService, InspectionTemplateApiService>(services, apiBaseAddress);

        AddApiClient<IMaintenanceApiService, MaintenanceApiService>(services, apiBaseAddress);

        AddApiClient<IUserApiService, UserApiService>(services, apiBaseAddress);

        AddApiClient<IInvitationApiService, InvitationApiService>(services, apiBaseAddress);

        return services;
    }

    /// <summary>Registers one typed HTTP client with the base address and bearer handler.</summary>
    /// <typeparam name="TInterface">The service interface.</typeparam>
    /// <typeparam name="TImplementation">The implementation.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="apiBaseAddress">The API base address.</param>
    private static void AddApiClient<TInterface, TImplementation>(IServiceCollection services, Uri apiBaseAddress)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services
            .AddHttpClient<TInterface, TImplementation>(client => client.BaseAddress = apiBaseAddress)
            .AddHttpMessageHandler<BearerTokenHandler>();
    }
}