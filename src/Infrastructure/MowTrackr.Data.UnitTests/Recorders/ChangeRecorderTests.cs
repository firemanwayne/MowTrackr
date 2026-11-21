namespace MowTrackr.Data.UnitTests.Recorders;

public sealed class ChangeRecorderTests
{
    private readonly string _database = Guid.NewGuid().ToString();

    private Guid? _user;

    [Fact]
    public async Task SaveChangesAsync_AddingAnEntity_RecordsACreatedRowWithTheNewValues()
    {
        await using var context = CreateContext();

        var type = new AssetType { Name = "Chain Saw" };

        context.AssetTypes.Add(type);

        await context.SaveChangesAsync();

        var record = Assert.Single(await HistoryAsync());

        Assert.Equal(ChangeAction.Created, record.Action);

        Assert.Equal(nameof(AssetType), record.EntityType);

        Assert.Equal(type.Id, record.EntityId);

        var changes = ReadChanges(record);

        Assert.Equal(new PropertyChange(null, "Chain Saw"), changes[nameof(AssetType.Name)]);
    }

    [Fact]
    public async Task SaveChangesAsync_ChangingAValue_RecordsTheOldAndNewValueOnly()
    {
        await using var context = CreateContext();

        var type = new AssetType { Name = "Blower" };

        context.AssetTypes.Add(type);

        await context.SaveChangesAsync();

        type.Name = "Leaf Blower";

        await context.SaveChangesAsync();

        var update = Assert.Single(await HistoryAsync(), r => r.Action == ChangeAction.Updated);

        var changes = ReadChanges(update);

        Assert.Equal(new PropertyChange("Blower", "Leaf Blower"), Assert.Single(changes).Value);
    }

    [Fact]
    public async Task SaveChangesAsync_NothingChanged_AddsNoRow()
    {
        await using var context = CreateContext();

        var type = new AssetType { Name = "Push Mower" };

        context.AssetTypes.Add(type);

        await context.SaveChangesAsync();

        type.Name = "Push Mower";

        await context.SaveChangesAsync();

        Assert.Single(await HistoryAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_DeletingAnEntity_RecordsADeletedRowWithTheOldValues()
    {
        await using var context = CreateContext();

        var type = new AssetType { Name = "Weed Eater" };

        context.AssetTypes.Add(type);

        await context.SaveChangesAsync();

        context.AssetTypes.Remove(type);

        await context.SaveChangesAsync();

        var deleted = Assert.Single(await HistoryAsync(), r => r.Action == ChangeAction.Deleted);

        Assert.Equal(type.Id, deleted.EntityId);

        Assert.Equal(new PropertyChange("Weed Eater", null), ReadChanges(deleted)[nameof(AssetType.Name)]);
    }

    [Fact]
    public async Task SaveChangesAsync_SignedInUser_IsRecordedAsWhoChangedIt()
    {
        var alice = Guid.NewGuid();

        _user = alice;

        await using var context = CreateContext();

        context.AssetTypes.Add(new AssetType { Name = "Riding Mower" });

        await context.SaveChangesAsync();

        var record = Assert.Single(await HistoryAsync());

        Assert.Equal(alice, record.ChangedBy);
    }

    [Fact]
    public async Task SaveChangesAsync_NoSignedInUser_RecordsNoUser()
    {
        _user = null;

        await using var context = CreateContext();

        context.AssetTypes.Add(new AssetType { Name = "Hedge Trimmer" });

        await context.SaveChangesAsync();

        Assert.Null(Assert.Single(await HistoryAsync()).ChangedBy);
    }

    [Fact]
    public async Task SaveChangesAsync_NotAuditedProperty_NeverAppearsInTheHistory()
    {
        await using var context = CreateContext();

        var invitation = new UserInvitation { Email = "a@example.com", TokenHash = "secret-hash" };

        context.UserInvitations.Add(invitation);

        await context.SaveChangesAsync();

        var record = Assert.Single(await HistoryAsync());

        Assert.DoesNotContain(nameof(UserInvitation.TokenHash), record.Changes);

        Assert.DoesNotContain("secret-hash", record.Changes);

        Assert.Contains(nameof(UserInvitation.Email), record.Changes);
    }

    [Fact]
    public async Task SaveChangesAsync_AddingSeveralEntities_RecordsOneRowEachAndNeverRecordsTheHistoryItself()
    {
        await using var context = CreateContext();

        context.AssetTypes.AddRange(new AssetType { Name = "One" }, new AssetType { Name = "Two" });

        await context.SaveChangesAsync();

        Assert.Equal(2, (await HistoryAsync()).Count);
    }

    private async Task<List<ChangeRecord>> HistoryAsync()
    {
        await using var reader = CreateContext();

        return await reader
            .ChangeHistory
            .OrderBy(r => r.ChangedAt)
            .ToListAsync();
    }

    private static Dictionary<string, PropertyChange> ReadChanges(ChangeRecord record)
    {
        return JsonSerializer.Deserialize<Dictionary<string, PropertyChange>>(record.Changes!, JsonSerializerOptions.Web)!;
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
