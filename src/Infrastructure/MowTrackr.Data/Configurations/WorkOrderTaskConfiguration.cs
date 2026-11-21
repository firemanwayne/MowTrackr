namespace MowTrackr.Data.Configurations;

/// <summary>
/// EF Core mapping for <see cref="WorkOrderTask"/>.
/// </summary>
internal sealed class WorkOrderTaskConfiguration : IEntityTypeConfiguration<WorkOrderTask>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WorkOrderTask> builder)
    {
        builder.ToTable("WorkOrderTasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Source).HasMaxLength(200);

        builder.Property(t => t.Description).HasMaxLength(300).IsRequired();

        builder.Property(t => t.IsRequired).HasDefaultValue(true);

        builder.Property(t => t.Phase).HasConversion<string>().HasMaxLength(20).HasDefaultValue(TaskPhase.Service);

        builder.Property(t => t.ItemType).HasConversion<string>().HasMaxLength(30).HasDefaultValue(CheckItemType.YesNo);

        builder.Property(t => t.Choices).HasMaxLength(300);

        builder.Property(t => t.RepairOn).HasMaxLength(300);

        builder.Property(t => t.Answer).HasMaxLength(1000);
    }
}