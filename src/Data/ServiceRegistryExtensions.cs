using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Hosting;
using MowTrackr.Data.HostedServices;
using MowTrackr.Data.Repositories;

namespace MowTrackr.Data;

/// <summary>
/// Registers persistence: the Azure SQL context (via Aspire), Identity stores, repositories and migrations.
/// </summary>
public static class ServiceRegistryExtensions
{
    /// <summary>The Aspire connection name of the application database.</summary>
    public const string DatabaseConnectionName = "mowtrackr";

    /// <summary>The data-protection application name, so every instance shares one key ring.</summary>
    private const string DataProtectionApplicationName = "MowTrackr";

    /// <summary>
    /// Adds the SQL Server context (connection string <c>ConnectionStrings:mowtrackr</c>, supplied by Aspire locally
    /// and by app settings in Azure), the Identity EF stores, the generic repository and the startup migration.
    /// Call this before <c>AddIdentityServices</c> so migrations run before roles are seeded.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IHostApplicationBuilder AddDataServices(this IHostApplicationBuilder builder)
    {
        builder.AddSqlServerDbContext<MowTrackrDbContext>(DatabaseConnectionName);

        new IdentityBuilder(typeof(ApplicationUser), typeof(ApplicationRole), builder.Services)
            .AddEntityFrameworkStores<MowTrackrDbContext>();

        builder.Services
            .AddDataProtection()
            .SetApplicationName(DataProtectionApplicationName)
            .PersistKeysToDbContext<MowTrackrDbContext>();

        builder.Services.TryAddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        builder.Services.AddHostedService<DatabaseMigrationHostedService>();

        return builder;
    }
}