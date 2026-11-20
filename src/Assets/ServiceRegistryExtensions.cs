using MowTrackr.Assets.Services;

namespace MowTrackr.Assets;

/// <summary>
/// Registers the Assets domain services.
/// </summary>
public static class ServiceRegistryExtensions
{
    /// <summary>Adds the mower asset services.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection AddAssetsServices(this IServiceCollection services)
    {
        services.TryAddScoped<IMowerService, MowerService>();

        services.TryAddScoped<IManufacturerService, ManufacturerService>();

        return services;
    }
}