using MowTrackr.Maintenance.Lookups;
using MowTrackr.Maintenance.Options;
using MowTrackr.Maintenance.Services;
using MowTrackr.Maintenance.Usages;
using MowTrackr.Maintenance.Validators;

namespace MowTrackr.Maintenance;

/// <summary>
/// Registers the Maintenance domain services.
/// </summary>
public static class ServiceRegistryExtensions
{
    /// <summary>Adds service templates, interval validation, due checks and work orders.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection AddMaintenanceServices(this IServiceCollection services)
    {
        services.AddOptions<MaintenanceOptions>().BindConfiguration(MaintenanceOptions.SectionName);

        services.TryAddSingleton<IIntervalValidator, IntervalValidator>();

        services.TryAddScoped<IServiceTemplateService, ServiceTemplateService>();

        services.TryAddScoped<IInspectionTemplateService, InspectionTemplateService>();

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAssetTypeUsage, ServiceTemplateAssetTypeUsage>());

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAssetTypeUsage, InspectionTemplateAssetTypeUsage>());

        services.TryAddScoped<IMaintenanceService, MaintenanceService>();

        services.TryAddScoped<IOpenRepairLookup, OpenRepairLookup>();

        return services;
    }
}