namespace MowTrackr.Assets.UnitTests.Services;

public sealed class OilTypeServiceTests
{
    private readonly IRepository<OilType> _repository = Substitute.For<IRepository<OilType>>();

    [Fact]
    public async Task DeleteAsync_OilTypeRequiredByEquipment_IsRefusedAndNothingIsRemoved()
    {
        var oil = new OilType { Name = "SAE 10W-30" };

        _repository.GetByIdAsync(oil.Id, Arg.Any<QuerySpec<OilType>?>(), Arg.Any<CancellationToken>()).Returns(oil);

        var result = await CreateService(Usage("2 mower(s)")).DeleteAsync(oil.Id, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);

        Assert.Contains("2 mower(s)", result.Error);

        _repository.DidNotReceive().Remove(Arg.Any<OilType>());
    }

    [Fact]
    public async Task DeleteAsync_OilTypeNotUsed_RemovesIt()
    {
        var oil = new OilType { Name = "SAE 30" };

        _repository.GetByIdAsync(oil.Id, Arg.Any<QuerySpec<OilType>?>(), Arg.Any<CancellationToken>()).Returns(oil);

        var result = await CreateService(Usage(null)).DeleteAsync(oil.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);

        _repository.Received(1).Remove(oil);
    }

    [Fact]
    public async Task CreateAsync_NameAlreadyTaken_IsAConflict()
    {
        _repository.ListAsync(Arg.Any<QuerySpec<OilType>?>(), Arg.Any<CancellationToken>()).Returns([new OilType { Name = "SAE 30" }]);

        var result = await CreateService().CreateAsync(new OilTypeRequest { Name = "SAE 30" }, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
    }

    private OilTypeService CreateService(params IOilTypeUsage[] usages)
    {
        return new OilTypeService(_repository, usages, NullLogger<OilTypeService>.Instance);
    }

    private static IOilTypeUsage Usage(string? description)
    {
        var usage = Substitute.For<IOilTypeUsage>();

        usage.DescribeUseAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(description);

        return usage;
    }
}
