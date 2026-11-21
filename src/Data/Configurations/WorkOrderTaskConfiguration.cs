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
    }
}