namespace MowTrackr.Data.Configurations;

/// <summary>
/// EF Core mapping for <see cref="InspectionTask"/>.
/// </summary>
internal sealed class InspectionTaskConfiguration : IEntityTypeConfiguration<InspectionTask>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<InspectionTask> builder)
    {
        builder.ToTable("InspectionTasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Description).HasMaxLength(300).IsRequired();

        builder.Property(t => t.IsRequired).HasDefaultValue(true);

        builder.Property(t => t.ItemType).HasConversion<string>().HasMaxLength(30).HasDefaultValue(CheckItemType.YesNo);

        builder.Property(t => t.Choices).HasMaxLength(300);

        builder.Property(t => t.RepairOn).HasMaxLength(300);
    }
}
