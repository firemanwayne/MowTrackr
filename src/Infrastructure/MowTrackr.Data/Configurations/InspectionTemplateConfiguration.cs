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

        // The database default (After) backfills existing rows. EF would otherwise treat the first enum value, Before, as "not
        // set" and save the default instead, so a sentinel that is never a real value makes it always send the chosen timing.
        builder.Property(t => t.Timing).HasConversion<string>().HasMaxLength(10).HasDefaultValue(InspectionTiming.After).HasSentinel((InspectionTiming)(-1));

        builder.HasOne(t => t.AssetType).WithMany().HasForeignKey(t => t.AssetTypeId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Tasks).WithOne().HasForeignKey(t => t.InspectionTemplateId).OnDelete(DeleteBehavior.Cascade);
    }
}
