using Microsoft.Extensions.Hosting;

namespace MowTrackr.Data.HostedServices;

/// <summary>
/// Applies pending EF Core migrations at startup, before any other hosted service touches the database.
/// </summary>
/// <param name="scopeFactory">The scope factory (the context is scoped).</param>
/// <param name="logger">The logger.</param>
internal sealed class DatabaseMigrationHostedService(IServiceScopeFactory scopeFactory, ILogger<DatabaseMigrationHostedService> logger) : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<MowTrackrDbContext>();

        logger.LogInformation("Applying database migrations");

        await db.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}