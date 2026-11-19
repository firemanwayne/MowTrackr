using MowTrackr.Localization.Services;

namespace MowTrackr.Localization;

/// <summary>
/// Registers the Localization services.
/// </summary>
public static class ServiceRegistryExtensions
{
    /// <summary>Adds <c>IStringLocalizer</c> support and the culture service.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection AddLocalizationServices(this IServiceCollection services)
    {
        services.AddLocalization();

        services.TryAddSingleton<ICultureService, CultureService>();

        return services;
    }
}