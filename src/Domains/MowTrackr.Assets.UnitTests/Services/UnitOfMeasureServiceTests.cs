namespace MowTrackr.Assets.UnitTests.Services;

public sealed class UnitOfMeasureServiceTests
{
    private readonly IRepository<UnitOfMeasure> _repository = Substitute.For<IRepository<UnitOfMeasure>>();

    [Fact]
    public async Task DeleteAsync_UnitIsUsed_IsRefusedAndNothingIsRemoved()
    {
        var unit = new UnitOfMeasure { Name = "Quart", Abbreviation = "qt" };

        _repository.GetByIdAsync(unit.Id, Arg.Any<QuerySpec<UnitOfMeasure>?>(), Arg.Any<CancellationToken>()).Returns(unit);

        var service = CreateService(Usage("4 mower(s)"), Usage(null), Usage("1 service template(s)"));

        var result = await service.DeleteAsync(unit.Id, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);

        Assert.Contains("4 mower(s) and 1 service template(s)", result.Error);

        _repository.DidNotReceive().Remove(Arg.Any<UnitOfMeasure>());
    }

    [Fact]
    public async Task DeleteAsync_UnitIsUnused_RemovesIt()
    {
        var unit = new UnitOfMeasure { Name = "Pint", Abbreviation = "pt" };

        _repository.GetByIdAsync(unit.Id, Arg.Any<QuerySpec<UnitOfMeasure>?>(), Arg.Any<CancellationToken>()).Returns(unit);

        var result = await CreateService(Usage(null)).DeleteAsync(unit.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);

        _repository.Received(1).Remove(unit);
    }

    [Fact]
    public async Task CreateAsync_NameOrLabelAlreadyTaken_IsAConflict()
    {
        _repository.ListAsync(Arg.Any<QuerySpec<UnitOfMeasure>?>(), Arg.Any<CancellationToken>()).Returns([new UnitOfMeasure { Name = "Quart", Abbreviation = "qt" }]);

        var result = await CreateService().CreateAsync(new UnitOfMeasureRequest { Name = "Quart", Abbreviation = "q" }, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task CreateAsync_NewUnit_TrimsTheNameAndLabelAndSavesIt()
    {
        _repository.ListAsync(Arg.Any<QuerySpec<UnitOfMeasure>?>(), Arg.Any<CancellationToken>()).Returns([]);

        var result = await CreateService().CreateAsync(new UnitOfMeasureRequest { Name = "  Gallon ", Abbreviation = " gal " }, CancellationToken.None);

        Assert.Equal(("Gallon", "gal"), (result.Value!.Name, result.Value.Abbreviation));

        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private UnitOfMeasureService CreateService(params IUnitOfMeasureUsage[] usages)
    {
        return new UnitOfMeasureService(_repository, usages, NullLogger<UnitOfMeasureService>.Instance);
    }

    private static IUnitOfMeasureUsage Usage(string? description)
    {
        var usage = Substitute.For<IUnitOfMeasureUsage>();

        usage.DescribeUseAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(description);

        return usage;
    }
}
