using Microsoft.Extensions.Hosting;
using MowTrackr.Core.Constants;
using MowTrackr.Identity.Models;

namespace MowTrackr.Identity.Services;

/// <summary>
/// Hosted service that makes sure every application role exists at startup.
/// </summary>
/// <param name="scopeFactory">The scope factory (roles are scoped services).</param>
internal sealed class RoleSeedService(IServiceScopeFactory scopeFactory) : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();

        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        foreach (var name in AppRoles.All)
        {
            if (!await roles.RoleExistsAsync(name).ConfigureAwait(false))
            {
                await roles.CreateAsync(new ApplicationRole(name)).ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}