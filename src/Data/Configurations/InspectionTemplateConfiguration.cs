namespace MowTrackr.Data.Configurations;

/// <summary>
/// EF Core mapping for <see cref="InspectionTemplate"/> and its checklist.
/// </summary>
internal sealed class InspectionTemplateConfiguration : IEntityTypeConfiguration<InspectionTemplate>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<InspectionTemplate> builder)
    {
        builder.ToTable("InspectionTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();

        builder.Property(t => t.Description).HasMaxLength(500);

        builder.Property(t => t.MinEngineHours).HasPrecision(10, 2);

        builder.HasOne(t => t.AssetType).WithMany().HasForeignKey(t => t.AssetTypeId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Tasks).WithOne().HasForeignKey(t => t.InspectionTemplateId).OnDelete(DeleteBehavior.Cascade);
    }
}
