namespace MowTrackr.Data.Configurations;

/// <summary>
/// EF Core mapping for <see cref="OilType"/>.
/// </summary>
internal sealed class OilTypeConfiguration : IEntityTypeConfiguration<OilType>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OilType> builder)
    {
        builder.ToTable("OilTypes");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.Name).HasMaxLength(50).IsRequired();

        builder.HasIndex(o => o.Name).IsUnique();
    }
}
