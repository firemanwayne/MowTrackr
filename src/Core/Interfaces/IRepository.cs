namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Generic persistence abstraction so domain libraries stay independent of EF Core.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IRepository<T>
    where T : EntityBase
{
    /// <summary>Gets one entity by key.</summary>
    /// <param name="id">The key.</param>
    /// <param name="spec">Optional includes and tracking options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity, or <see langword="null"/>.</returns>
    Task<T?> GetByIdAsync(Guid id, QuerySpec<T>? spec, CancellationToken cancellationToken);

    /// <summary>Lists entities matching a specification.</summary>
    /// <param name="spec">Optional filter, includes and ordering.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching entities.</returns>
    Task<IReadOnlyList<T>> ListAsync(QuerySpec<T>? spec, CancellationToken cancellationToken);

    /// <summary>Stages a new entity.</summary>
    /// <param name="entity">The entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task.</returns>
    Task AddAsync(T entity, CancellationToken cancellationToken);

    /// <summary>Stages the removal of an entity.</summary>
    /// <param name="entity">The entity.</param>
    void Remove(T entity);

    /// <summary>Persists all staged changes.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}