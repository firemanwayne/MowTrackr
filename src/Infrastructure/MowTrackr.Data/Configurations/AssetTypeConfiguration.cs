namespace MowTrackr.Data.Configurations;

/// <summary>
/// EF Core mapping for <see cref="AssetType"/>.
/// </summary>
internal sealed class AssetTypeConfiguration : IEntityTypeConfiguration<AssetType>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AssetType> builder)
    {
        builder.ToTable("AssetTypes");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();

        builder.HasIndex(t => t.Name).IsUnique();
    }
}
