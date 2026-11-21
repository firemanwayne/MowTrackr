namespace MowTrackr.Data.UnitTests.Interceptors;

public sealed class AuditSaveChangesInterceptorTests
{
    private readonly string _database = Guid.NewGuid().ToString();

    private Guid? _user;

    [Fact]
    public async Task SaveChangesAsync_AddingAnEntity_StampsTheSignedInUser()
    {
        var alice = Guid.NewGuid();

        _user = alice;

        await using var context = CreateContext();

        var type = new AssetType { Name = "Chain Saw" };

        context.AssetTypes.Add(type);

        await context.SaveChangesAsync();

        Assert.Equal(alice, type.UpdatedBy);
    }

    [Fact]
    public async Task SaveChangesAsync_ChangingAnEntity_StampsTheUserWhoChangedIt()
    {
        var alice = Guid.NewGuid();

        var bob = Guid.NewGuid();

        _user = alice;

        await using var context = CreateContext();

        var type = new AssetType { Name = "Blower" };

        context.AssetTypes.Add(type);

        await context.SaveChangesAsync();

        var firstStamp = type.UpdatedAt;

        _user = bob;

        type.Name = "Leaf Blower";

        await context.SaveChangesAsync();

        Assert.Equal(bob, type.UpdatedBy);

        Assert.True(type.UpdatedAt >= firstStamp);
    }

    [Fact]
    public async Task SaveChangesAsync_NoSignedInUser_LeavesUpdatedByEmpty()
    {
        _user = null;

        await using var context = CreateContext();

        var type = new AssetType { Name = "Weed Eater" };

        context.AssetTypes.Add(type);

        await context.SaveChangesAsync();

        Assert.Null(type.UpdatedBy);
    }

    [Fact]
    public async Task SaveChangesAsync_UntouchedEntity_KeepsItsUpdatedBy()
    {
        var alice = Guid.NewGuid();

        _user = alice;

        await using var context = CreateContext();

        var first = new AssetType { Name = "Push Mower" };

        var second = new AssetType { Name = "Riding Mower" };

        context.AssetTypes.AddRange(first, second);

        await context.SaveChangesAsync();

        _user = Guid.NewGuid();

        second.Name = "Zero-Turn Mower";

        await context.SaveChangesAsync();

        Assert.Equal(alice, first.UpdatedBy);

        Assert.NotEqual(alice, second.UpdatedBy);
    }

    [Fact]
    public void SaveChanges_Synchronously_AlsoStampsTheUser()
    {
        var alice = Guid.NewGuid();

        _user = alice;

        using var context = CreateContext();

        var type = new AssetType { Name = "Hedge Trimmer" };

        context.AssetTypes.Add(type);

        context.SaveChanges();

        Assert.Equal(alice, type.UpdatedBy);
    }

    private MowTrackrDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MowTrackrDbContext>()
            .UseInMemoryDatabase(_database)
            .AddInterceptors(new AuditSaveChangesInterceptor(() => _user))
            .Options;

        return new MowTrackrDbContext(options);
    }
}
