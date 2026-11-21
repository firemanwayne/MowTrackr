namespace MowTrackr.Data.Recorders;

/// <summary>
/// Turns the context's pending creates, updates and deletes into <see cref="ChangeRecord"/> rows, so they are saved in
/// the same transaction as the changes they describe.
/// </summary>
internal static class ChangeRecorder
{
    private static readonly HashSet<string> BookkeepingProperties =
    [
        nameof(EntityBase.Id),
        nameof(EntityBase.CreatedAt),
        nameof(EntityBase.UpdatedAt),
        nameof(EntityBase.UpdatedBy),
    ];

    /// <summary>Adds a history row for every entity that is about to be created, changed or deleted.</summary>
    /// <param name="context">The context being saved.</param>
    /// <param name="at">When the changes are being saved.</param>
    /// <param name="by">The signed-in user, or <see langword="null"/> when there is none.</param>
    public static void Record(DbContext context, DateTimeOffset at, Guid? by)
    {
        var records = new List<ChangeRecord>();

        foreach (var entry in context.ChangeTracker.Entries<EntityBase>())
        {
            var record = Describe(entry, at, by);

            if (record is not null)
            {
                records.Add(record);
            }
        }

        context.Set<ChangeRecord>().AddRange(records);
    }

    /// <summary>Builds the history row for one entity.</summary>
    /// <param name="entry">The tracked entity.</param>
    /// <param name="at">When the change is being saved.</param>
    /// <param name="by">The signed-in user, if any.</param>
    /// <returns>The row, or <see langword="null"/> when nothing worth recording changed.</returns>
    private static ChangeRecord? Describe(EntityEntry<EntityBase> entry, DateTimeOffset at, Guid? by)
    {
        var action = entry.State switch
        {
            EntityState.Added => ChangeAction.Created,
            EntityState.Modified => ChangeAction.Updated,
            EntityState.Deleted => ChangeAction.Deleted,
            _ => (ChangeAction?)null,
        };

        if (action is null)
        {
            return null;
        }

        var changes = CollectChanges(entry, action.Value);

        if (action == ChangeAction.Updated && changes.Count == 0)
        {
            return null;
        }

        return new ChangeRecord
        {
            EntityType = entry.Metadata.ClrType.Name,
            EntityId = entry.Entity.Id,
            Action = action.Value,
            ChangedAt = at,
            ChangedBy = by,
            Changes = changes.Count == 0 ? null : JsonSerializer.Serialize(changes, JsonSerializerOptions.Web),
        };
    }

    /// <summary>Lists the audited values that differ for one entity.</summary>
    /// <param name="entry">The tracked entity.</param>
    /// <param name="action">What is happening to it.</param>
    /// <returns>Property name to its old and new value.</returns>
    private static Dictionary<string, PropertyChange> CollectChanges(EntityEntry<EntityBase> entry, ChangeAction action)
    {
        var changes = new Dictionary<string, PropertyChange>();

        foreach (var property in entry.Properties)
        {
            var name = property.Metadata.Name;

            if (BookkeepingProperties.Contains(name) || IsNotAudited(property.Metadata))
            {
                continue;
            }

            var before = action == ChangeAction.Created ? null : Format(property.OriginalValue);

            var after = action == ChangeAction.Deleted ? null : Format(property.CurrentValue);

            var changed = action == ChangeAction.Updated ? property.IsModified && before != after : before != after;

            if (changed)
            {
                changes[name] = new PropertyChange(before, after);
            }
        }

        return changes;
    }

    /// <summary>Checks whether a property opted out of the history.</summary>
    /// <param name="property">The mapped property.</param>
    /// <returns><see langword="true"/> when it is marked <see cref="NotAuditedAttribute"/>.</returns>
    private static bool IsNotAudited(IProperty property)
    {
        return property.PropertyInfo?.IsDefined(typeof(NotAuditedAttribute), inherit: true) == true;
    }

    /// <summary>Writes a value as culture-independent text.</summary>
    /// <param name="value">The value, which may be <see langword="null"/>.</param>
    /// <returns>The text, or <see langword="null"/> for an empty value.</returns>
    private static string? Format(object? value)
    {
        return value switch
        {
            null => null,
            DateTimeOffset offset => offset.ToString("O", CultureInfo.InvariantCulture),
            DateTime time => time.ToString("O", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };
    }
}
