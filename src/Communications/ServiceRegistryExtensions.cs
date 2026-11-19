using MowTrackr.Communications.Options;
using MowTrackr.Communications.Services;

namespace MowTrackr.Communications;

/// <summary>
/// Registers the email sender: Azure Communication Services when configured, a logging stand-in otherwise.
/// </summary>
public static class ServiceRegistryExtensions
{
    /// <summary>Adds <see cref="IEmailService"/>, choosing the implementation from the "Communications" settings.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection AddCommunicationServices(this IServiceCollection services)
    {
        services.AddOptions<CommunicationsOptions>().BindConfiguration(CommunicationsOptions.SectionName);

        services.TryAddSingleton<IEmailService>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<CommunicationsOptions>>().Value;

            return settings.IsConfigured
                ? ActivatorUtilities.CreateInstance<AzureEmailService>(sp)
                : ActivatorUtilities.CreateInstance<LoggingEmailService>(sp);
        });

        services.TryAddSingleton<ISmsService>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<CommunicationsOptions>>().Value;

            return settings.IsSmsConfigured
                ? ActivatorUtilities.CreateInstance<AzureSmsService>(sp)
                : ActivatorUtilities.CreateInstance<LoggingSmsService>(sp);
        });

        return services;
    }
}