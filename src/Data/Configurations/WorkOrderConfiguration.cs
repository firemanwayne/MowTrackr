namespace MowTrackr.Data.Configurations;

/// <summary>
/// EF Core mapping for <see cref="WorkOrder"/> and its checklist.
/// </summary>
internal sealed class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrders");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id).ValueGeneratedNever();

        builder.Property(w => w.MowerName).HasMaxLength(100).IsRequired();

        builder.Property(w => w.TemplateName).HasMaxLength(100).IsRequired();

        builder.Property(w => w.Notes).HasMaxLength(1000);

        builder.Property(w => w.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(w => w.EngineHoursAtService).HasPrecision(10, 2);

        builder.HasIndex(w => new { w.MowerId, w.ServiceTemplateId, w.Status });

        builder.HasIndex(w => w.AssignedToUserId);

        builder.HasMany(w => w.Tasks).WithOne().HasForeignKey(t => t.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
    }
}