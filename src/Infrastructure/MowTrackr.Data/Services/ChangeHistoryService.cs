namespace MowTrackr.Data.Services;

/// <summary>
/// Default <see cref="IChangeHistoryService"/>: reads the <see cref="ChangeRecord"/> rows the audit interceptor writes.
/// </summary>
/// <param name="db">The database context.</param>
internal sealed class ChangeHistoryService(MowTrackrDbContext db) : IChangeHistoryService
{
    /// <summary>The most entries returned for one item.</summary>
    private const int MaxEntries = 200;

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ChangeHistoryResponse>>> ListAsync(string entityType, Guid entityId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(entityType))
        {
            return Result<IReadOnlyList<ChangeHistoryResponse>>.Invalid("An entity type is required.");
        }

        var records = await db
            .ChangeHistory
            .AsNoTracking()
            .Where(r => r.EntityType == entityType && r.EntityId == entityId)
            .OrderByDescending(r => r.ChangedAt)
            .Take(MaxEntries)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var userIds = records
            .Where(r => r.ChangedBy is not null)
            .Select(r => r.ChangedBy!.Value)
            .Distinct()
            .ToList();

        var names = await db
            .Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, cancellationToken)
            .ConfigureAwait(false);

        IReadOnlyList<ChangeHistoryResponse> entries = records
            .Select(r => ToResponse(r, names))
            .ToList();

        return Result<IReadOnlyList<ChangeHistoryResponse>>.Ok(entries);
    }

    /// <summary>Maps a stored row to its response.</summary>
    /// <param name="record">The stored row.</param>
    /// <param name="names">User display names by key.</param>
    /// <returns>The response.</returns>
    private static ChangeHistoryResponse ToResponse(ChangeRecord record, Dictionary<Guid, string> names)
    {
        string? name = null;

        if (record.ChangedBy is { } user && names.TryGetValue(user, out var found))
        {
            name = found;
        }

        return new ChangeHistoryResponse(record.Id, record.Action.ToString(), record.ChangedAt, record.ChangedBy, name, ReadChanges(record.Changes));
    }

    /// <summary>Reads the stored JSON into a list, treating a missing or unreadable value as no changes.</summary>
    /// <param name="json">The stored JSON.</param>
    /// <returns>The changed values.</returns>
    private static List<PropertyChangeResponse> ReadChanges(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            var changes = JsonSerializer.Deserialize<Dictionary<string, PropertyChange>>(json, JsonSerializerOptions.Web) ?? [];

            return changes
                .Select(pair => new PropertyChangeResponse(pair.Key, pair.Value.Old, pair.Value.New))
                .ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
