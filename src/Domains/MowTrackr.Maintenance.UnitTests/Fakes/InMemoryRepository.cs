namespace MowTrackr.Maintenance.UnitTests.Fakes;

/// <summary>
/// A list-backed <see cref="IRepository{T}"/> for tests. Filters run as compiled predicates; entities are shared, so
/// changes made through a tracked query are visible to later queries just like with EF Core's identity map. Navigation
/// properties are whatever the test set on the entities, so <c>Includes</c> are ignored.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
internal sealed class InMemoryRepository<T> : IRepository<T>
    where T : EntityBase
{
    private readonly List<T> _items = [];

    /// <summary>Gets the stored entities.</summary>
    public IReadOnlyList<T> Items => _items;

    /// <inheritdoc />
    public Task<T?> GetByIdAsync(Guid id, QuerySpec<T>? spec, CancellationToken cancellationToken)
    {
        return Task.FromResult(_items.FirstOrDefault(e => e.Id == id));
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<T>> ListAsync(QuerySpec<T>? spec, CancellationToken cancellationToken)
    {
        IEnumerable<T> query = _items;

        if (spec?.Filter is not null)
        {
            query = query.Where(spec.Filter.Compile());
        }

        return Task.FromResult<IReadOnlyList<T>>(query.ToList());
    }

    /// <inheritdoc />
    public Task AddAsync(T entity, CancellationToken cancellationToken)
    {
        _items.Add(entity);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Remove(T entity)
    {
        _items.Remove(entity);
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(0);
    }
}
