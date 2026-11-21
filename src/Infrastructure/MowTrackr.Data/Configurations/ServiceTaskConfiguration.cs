namespace MowTrackr.Data.Configurations;

/// <summary>
/// EF Core mapping for <see cref="ServiceTask"/>.
/// </summary>
internal sealed class ServiceTaskConfiguration : IEntityTypeConfiguration<ServiceTask>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ServiceTask> builder)
    {
        builder.ToTable("ServiceTasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Description).HasMaxLength(300).IsRequired();

        builder.Property(t => t.IsRequired).HasDefaultValue(true);

        builder.Property(t => t.QuantityRequirement).HasConversion<string>().HasMaxLength(20).HasDefaultValue(DetailRequirement.None);

        builder.Property(t => t.QuantityUnitSource).HasConversion<string>().HasMaxLength(20).HasDefaultValue(QuantityUnitSource.StepUnit);

        builder.Property(t => t.PartNumberRequirement).HasConversion<string>().HasMaxLength(20).HasDefaultValue(DetailRequirement.None);

        builder.Property(t => t.NotesRequirement).HasConversion<string>().HasMaxLength(20).HasDefaultValue(DetailRequirement.None);

        builder.HasOne(t => t.Unit).WithMany().HasForeignKey(t => t.UnitId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.Condition).HasConversion<string>().HasMaxLength(20).HasDefaultValue(ServiceTaskCondition.Always);
    }
}