namespace MowTrackr.Core.Models;

/// <summary>
/// Describes a repository query (filter, eager-load paths, ordering) without exposing EF Core to domain libraries.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public sealed class QuerySpec<T>
    where T : EntityBase
{
    /// <summary>Gets or sets an optional filter.</summary>
    public Expression<Func<T, bool>>? Filter { get; set; }

    /// <summary>Gets or sets navigation property paths to eager-load.</summary>
    public IReadOnlyList<string> Includes { get; set; } = [];

    /// <summary>Gets or sets an optional ascending ordering key.</summary>
    public Expression<Func<T, object>>? OrderBy { get; set; }

    /// <summary>Gets or sets a value indicating whether the entities will be modified (tracked).</summary>
    public bool Track { get; set; }
}