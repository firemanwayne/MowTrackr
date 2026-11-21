using MowTrackr.Assets.Services;
using MowTrackr.Assets.Usages;

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

        services.TryAddScoped<IAssetTypeService, AssetTypeService>();

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAssetTypeUsage, MowerAssetTypeUsage>());

        services.TryAddScoped<IOilTypeService, OilTypeService>();

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IOilTypeUsage, MowerOilTypeUsage>());

        services.TryAddScoped<IUnitOfMeasureService, UnitOfMeasureService>();

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IUnitOfMeasureUsage, MowerUnitUsage>());

        return services;
    }
}