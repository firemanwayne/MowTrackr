namespace MowTrackr.Data.Configurations;

/// <summary>
/// EF Core mapping for <see cref="ChangeRecord"/>.
/// </summary>
internal sealed class ChangeRecordConfiguration : IEntityTypeConfiguration<ChangeRecord>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ChangeRecord> builder)
    {
        builder.ToTable("ChangeHistory");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.EntityType).HasMaxLength(100).IsRequired();

        builder.Property(c => c.Action).HasConversion<string>().HasMaxLength(20);

        // No foreign key to the changed row: history must outlive the row it describes.
        builder.HasIndex(c => new { c.EntityType, c.EntityId, c.ChangedAt });

        builder.HasIndex(c => c.ChangedBy);
    }
}
