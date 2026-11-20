namespace MowTrackr.Data.Configurations;

/// <summary>
/// EF Core mapping for <see cref="ManufacturerModel"/>.
/// </summary>
internal sealed class ManufacturerModelConfiguration : IEntityTypeConfiguration<ManufacturerModel>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ManufacturerModel> builder)
    {
        builder.ToTable("ManufacturerModels");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Name).HasMaxLength(100).IsRequired();

        builder.HasIndex(m => new { m.ManufacturerId, m.Name }).IsUnique();
    }
}
