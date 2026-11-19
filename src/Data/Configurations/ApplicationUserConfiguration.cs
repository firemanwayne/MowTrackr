namespace MowTrackr.Data.Configurations;

/// <summary>
/// EF Core mapping for <see cref="ApplicationUser"/> extras.
/// </summary>
internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
    }
}