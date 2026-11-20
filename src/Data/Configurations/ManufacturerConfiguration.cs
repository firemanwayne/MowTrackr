namespace MowTrackr.Data.Configurations;

/// <summary>
/// EF Core mapping for <see cref="Manufacturer"/>.
/// </summary>
internal sealed class ManufacturerConfiguration : IEntityTypeConfiguration<Manufacturer>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Manufacturer> builder)
    {
        builder.ToTable("Manufacturers");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Name).HasMaxLength(100).IsRequired();

        builder.HasIndex(m => m.Name).IsUnique();

        builder.HasMany(m => m.Models).WithOne(m => m.Manufacturer).HasForeignKey(m => m.ManufacturerId).OnDelete(DeleteBehavior.Cascade);
    }
}
