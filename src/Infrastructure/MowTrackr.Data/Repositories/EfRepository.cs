namespace MowTrackr.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IRepository{T}"/>. Queries are no-tracking unless the spec asks to track.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <param name="db">The database context.</param>
internal sealed class EfRepository<T>(MowTrackrDbContext db) : IRepository<T>
    where T : EntityBase
{
    /// <inheritdoc />
    public Task<T?> GetByIdAsync(Guid id, QuerySpec<T>? spec, CancellationToken cancellationToken)
    {
        return BuildQuery(spec).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> ListAsync(QuerySpec<T>? spec, CancellationToken cancellationToken)
    {
        return await BuildQuery(spec).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddAsync(T entity, CancellationToken cancellationToken)
    {
        await db.Set<T>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Remove(T entity)
    {
        db.Set<T>().Remove(entity);
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Translates a specification into an EF Core query.</summary>
    /// <param name="spec">The specification, or <see langword="null"/> for all rows (no tracking).</param>
    /// <returns>The query.</returns>
    private IQueryable<T> BuildQuery(QuerySpec<T>? spec)
    {
        IQueryable<T> query = db.Set<T>();

        if (spec is null)
        {
            return query.AsNoTracking();
        }

        if (!spec.Track)
        {
            query = query.AsNoTracking();
        }

        foreach (var include in spec.Includes)
        {
            query = query.Include(include);
        }

        if (spec.Filter is not null)
        {
            query = query.Where(spec.Filter);
        }

        if (spec.OrderBy is not null)
        {
            query = query.OrderBy(spec.OrderBy);
        }

        return query;
    }
}