namespace MowTrackr.Data;

/// <summary>
/// The application's EF Core context: ASP.NET Core Identity tables plus the Assets and Maintenance domains.
/// </summary>
/// <param name="options">The context options.</param>
internal sealed class MowTrackrDbContext(DbContextOptions<MowTrackrDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    /// <summary>Gets the mowers.</summary>
    public DbSet<Mower> Mowers => Set<Mower>();

    /// <summary>Gets the service templates.</summary>
    public DbSet<ServiceTemplate> ServiceTemplates => Set<ServiceTemplate>();

    /// <summary>Gets the service template checklist tasks.</summary>
    public DbSet<ServiceTask> ServiceTasks => Set<ServiceTask>();

    /// <summary>Gets the work orders.</summary>
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    /// <summary>Gets the work order checklist tasks.</summary>
    public DbSet<WorkOrderTask> WorkOrderTasks => Set<WorkOrderTask>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(MowTrackrDbContext).Assembly);
    }
}