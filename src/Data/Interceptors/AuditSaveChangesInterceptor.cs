using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MowTrackr.Data.Interceptors;

/// <summary>
/// Stamps <see cref="EntityBase.UpdatedAt"/> and <see cref="EntityBase.UpdatedBy"/> on every entity that is added or
/// modified, so no service has to remember to. It is a single shared instance (the context is pooled), so the signed-in
/// user is looked up on every save rather than captured.
/// </summary>
/// <param name="currentUser">Returns the signed-in user's key, or <see langword="null"/> when there is none.</param>
internal sealed class AuditSaveChangesInterceptor(Func<Guid?> currentUser) : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>Stamps the time and the user on every added or modified entity in the context.</summary>
    /// <param name="context">The context being saved, if any.</param>
    private void Stamp(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var user = currentUser();

        foreach (var entry in context.ChangeTracker.Entries<EntityBase>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            entry.Entity.UpdatedAt = now;

            entry.Entity.UpdatedBy = user;
        }
    }
}
