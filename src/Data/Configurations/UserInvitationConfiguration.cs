namespace MowTrackr.Data.Configurations;

/// <summary>
/// EF Core mapping for <see cref="UserInvitation"/>.
/// </summary>
internal sealed class UserInvitationConfiguration : IEntityTypeConfiguration<UserInvitation>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserInvitation> builder)
    {
        builder.ToTable("UserInvitations");

        builder.Property(i => i.Email).HasMaxLength(256).IsRequired();

        builder.Property(i => i.NormalizedEmail).HasMaxLength(256).IsRequired();

        builder.Property(i => i.Role).HasMaxLength(256).IsRequired();

        builder.Property(i => i.Language).HasMaxLength(10).IsRequired().HasDefaultValue("en");

        builder.Property(i => i.TokenHash).HasMaxLength(64).IsRequired();

        builder.Property(i => i.InvitedByName).HasMaxLength(256).IsRequired();

        builder.HasIndex(i => i.TokenHash).IsUnique();

        builder.HasIndex(i => i.NormalizedEmail);
    }
}