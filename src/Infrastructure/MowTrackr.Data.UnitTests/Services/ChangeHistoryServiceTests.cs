using MowTrackr.Core.Constants;

namespace MowTrackr.Data.UnitTests.Services;

public sealed class ChangeHistoryServiceTests
{
    private readonly string _database = Guid.NewGuid().ToString();

    private Guid? _user;

    [Fact]
    public async Task ListAsync_ChangesToTheItem_AreReturnedNewestFirstWithWhoAndValues()
    {
        var alice = await AddUserAsync("Alice");

        _user = alice;

        var type = await SaveAsync(new AssetType { Name = "Blower" });

        await Task.Delay(5);

        await RenameAsync(type, "Leaf Blower");

        var result = await ListAsync(nameof(AssetType), type.Id);

        var entries = result.Value!;

        Assert.Equal(2, entries.Count);

        Assert.Equal("Updated", entries[0].Action);

        Assert.Equal("Alice", entries[0].ChangedByName);

        Assert.Equal(alice, entries[0].ChangedBy);

        Assert.Equal(new PropertyChangeResponse(nameof(AssetType.Name), "Blower", "Leaf Blower"), Assert.Single(entries[0].Changes));

        Assert.Equal("Created", entries[1].Action);
    }

    [Fact]
    public async Task ListAsync_ChangesToOtherItems_AreNotIncluded()
    {
        var mine = await SaveAsync(new AssetType { Name = "Chain Saw" });

        await SaveAsync(new AssetType { Name = "Push Mower" });

        var result = await ListAsync(nameof(AssetType), mine.Id);

        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task ListAsync_UserNoLongerExists_ReturnsTheEntryWithoutAName()
    {
        _user = Guid.NewGuid();

        var type = await SaveAsync(new AssetType { Name = "Weed Eater" });

        var entry = Assert.Single((await ListAsync(nameof(AssetType), type.Id)).Value!);

        Assert.Equal(_user, entry.ChangedBy);

        Assert.Null(entry.ChangedByName);
    }

    [Fact]
    public async Task ListAsync_BlankEntityType_IsInvalid()
    {
        var result = await ListAsync(" ", Guid.NewGuid());

        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task ListAsync_ItemWithNoHistory_ReturnsAnEmptyList()
    {
        var result = await ListAsync(nameof(AssetType), Guid.NewGuid());

        Assert.True(result.IsSuccess);

        Assert.Empty(result.Value!);
    }

    [Fact]
    public void MowerEntityType_MatchesTheEntityName_SoTheClientFindsTheHistory()
    {
        Assert.Equal(HistoryEntityTypes.Mower, typeof(Mower).Name);
    }

    private async Task<Result<IReadOnlyList<ChangeHistoryResponse>>> ListAsync(string entityType, Guid entityId)
    {
        await using var context = CreateContext();

        return await new ChangeHistoryService(context).ListAsync(entityType, entityId, CancellationToken.None);
    }

    private async Task<Guid> AddUserAsync(string displayName)
    {
        await using var context = CreateContext();

        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = displayName, DisplayName = displayName };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        return user.Id;
    }

    private async Task<AssetType> SaveAsync(AssetType type)
    {
        await using var context = CreateContext();

        context.AssetTypes.Add(type);

        await context.SaveChangesAsync();

        return type;
    }

    private async Task RenameAsync(AssetType type, string name)
    {
        await using var context = CreateContext();

        var tracked = await context.AssetTypes.FirstAsync(t => t.Id == type.Id);

        tracked.Name = name;

        await context.SaveChangesAsync();
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
