namespace MowTrackr.Data.Configurations;

/// <summary>
/// EF Core mapping for <see cref="UnitOfMeasure"/>.
/// </summary>
internal sealed class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("UnitsOfMeasure");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.Name).HasMaxLength(50).IsRequired();

        builder.Property(u => u.Abbreviation).HasMaxLength(20).IsRequired();

        builder.HasIndex(u => u.Name).IsUnique();

        builder.HasIndex(u => u.Abbreviation).IsUnique();
    }
}
