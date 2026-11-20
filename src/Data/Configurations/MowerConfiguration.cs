namespace MowTrackr.Data.Configurations;

/// <summary>
/// EF Core mapping for <see cref="Mower"/>.
/// </summary>
internal sealed class MowerConfiguration : IEntityTypeConfiguration<Mower>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Mower> builder)
    {
        builder.ToTable("Mowers");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Name).HasMaxLength(100).IsRequired();

        builder.HasOne(m => m.Manufacturer).WithMany().HasForeignKey(m => m.ManufacturerId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Model).WithMany().HasForeignKey(m => m.ManufacturerModelId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(m => m.SerialNumber).HasMaxLength(100).IsRequired();

        builder.Property(m => m.OilType).HasMaxLength(50).IsRequired();

        builder.Property(m => m.EngineHours).HasPrecision(10, 2);

        builder.Property(m => m.OilQuantityQuarts).HasPrecision(6, 2);

        builder.HasIndex(m => m.SerialNumber).IsUnique();
    }
}