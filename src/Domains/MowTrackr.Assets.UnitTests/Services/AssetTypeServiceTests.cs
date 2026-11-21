namespace MowTrackr.Assets.UnitTests.Services;

public sealed class AssetTypeServiceTests
{
    private readonly IRepository<AssetType> _repository = Substitute.For<IRepository<AssetType>>();

    [Fact]
    public async Task DeleteAsync_TypeIsUsed_IsRefusedAndNothingIsRemoved()
    {
        var type = new AssetType { Name = "Chain Saw" };

        _repository.GetByIdAsync(type.Id, Arg.Any<QuerySpec<AssetType>?>(), Arg.Any<CancellationToken>()).Returns(type);

        var service = CreateService(Usage("3 equipment record(s)"), Usage(null), Usage("1 service template(s)"));

        var result = await service.DeleteAsync(type.Id, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);

        Assert.Contains("3 equipment record(s) and 1 service template(s)", result.Error);

        _repository.DidNotReceive().Remove(Arg.Any<AssetType>());
    }

    [Fact]
    public async Task DeleteAsync_TypeIsUnused_RemovesIt()
    {
        var type = new AssetType { Name = "Blower" };

        _repository.GetByIdAsync(type.Id, Arg.Any<QuerySpec<AssetType>?>(), Arg.Any<CancellationToken>()).Returns(type);

        var service = CreateService(Usage(null), Usage(null));

        var result = await service.DeleteAsync(type.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);

        _repository.Received(1).Remove(type);
    }

    [Fact]
    public async Task DeleteAsync_UnknownType_IsNotFound()
    {
        var service = CreateService();

        var result = await service.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task UpdateAsync_NameTakenByAnotherType_IsAConflict()
    {
        var type = new AssetType { Name = "Blower" };

        _repository.GetByIdAsync(type.Id, Arg.Any<QuerySpec<AssetType>?>(), Arg.Any<CancellationToken>()).Returns(type);

        _repository.ListAsync(Arg.Any<QuerySpec<AssetType>?>(), Arg.Any<CancellationToken>()).Returns([new AssetType { Name = "Chain Saw" }]);

        var service = CreateService();

        var result = await service.UpdateAsync(type.Id, new AssetTypeRequest { Name = "Chain Saw" }, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);

        Assert.Equal("Blower", type.Name);
    }

    [Fact]
    public async Task UpdateAsync_NewUniqueName_RenamesAndTrims()
    {
        var type = new AssetType { Name = "Blower" };

        _repository.GetByIdAsync(type.Id, Arg.Any<QuerySpec<AssetType>?>(), Arg.Any<CancellationToken>()).Returns(type);

        _repository.ListAsync(Arg.Any<QuerySpec<AssetType>?>(), Arg.Any<CancellationToken>()).Returns([]);

        var service = CreateService();

        var result = await service.UpdateAsync(type.Id, new AssetTypeRequest { Name = "  Leaf Blower " }, CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.Equal("Leaf Blower", type.Name);

        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static IAssetTypeUsage Usage(string? description)
    {
        var usage = Substitute.For<IAssetTypeUsage>();

        usage.DescribeUseAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(description);

        return usage;
    }

    private AssetTypeService CreateService(params IAssetTypeUsage[] usages)
    {
        return new AssetTypeService(_repository, usages, NullLogger<AssetTypeService>.Instance);
    }
}
