namespace MowTrackr.Data.Configurations;

/// <summary>
/// EF Core mapping for <see cref="ServiceTemplate"/> and its checklist.
/// </summary>
internal sealed class ServiceTemplateConfiguration : IEntityTypeConfiguration<ServiceTemplate>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ServiceTemplate> builder)
    {
        builder.ToTable("ServiceTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();

        builder.HasOne(t => t.AssetType).WithMany().HasForeignKey(t => t.AssetTypeId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.Description).HasMaxLength(500);

        builder.Property(t => t.IntervalHours).HasPrecision(10, 2);

        builder.HasMany(t => t.Tasks).WithOne().HasForeignKey(t => t.ServiceTemplateId).OnDelete(DeleteBehavior.Cascade);
    }
}