namespace MowTrackr.Data.Factories;

/// <summary>
/// Design-time factory so <c>dotnet ef</c> can build the context without running the app host.
/// </summary>
internal sealed class MowTrackrDbContextFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<MowTrackrDbContext>
{
    private const string DesignTimeConnection = "Server=(localdb)\\MSSQLLocalDB;Database=MowTrackrDesign;Trusted_Connection=True;";

    /// <inheritdoc />
    public MowTrackrDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MowTrackrDbContext>().UseSqlServer(DesignTimeConnection).Options;

        return new MowTrackrDbContext(options);
    }
}