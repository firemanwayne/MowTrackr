using MowTrackr.Maintenance.Options;
using MowTrackr.Maintenance.Services;
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

        services.TryAddScoped<IMaintenanceService, MaintenanceService>();

        return services;
    }
}