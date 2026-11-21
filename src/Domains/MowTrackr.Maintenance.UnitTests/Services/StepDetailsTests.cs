using Microsoft.Extensions.Logging.Abstractions;
using MowTrackr.Core.Contracts.Maintenance;
using MowTrackr.Core.Enums;
using MowTrackr.Maintenance.Options;
using MowTrackr.Maintenance.Services;
using MowTrackr.Maintenance.UnitTests.Fakes;

namespace MowTrackr.Maintenance.UnitTests.Services;

public sealed class StepDetailsTests
{
    private static readonly Guid _technician = Guid.NewGuid();

    private readonly InMemoryRepository<Mower> _mowers = new();

    private readonly InMemoryRepository<ServiceTemplate> _templates = new();

    private readonly InMemoryRepository<WorkOrder> _workOrders = new();

    private readonly InMemoryRepository<InspectionTemplate> _inspections = new();

    private readonly UnitOfMeasure _quart = new() { Name = "Quart", Abbreviation = "qt" };

    private readonly UnitOfMeasure _ounce = new() { Name = "Ounce", Abbreviation = "oz" };

    private readonly UnitOfMeasure _each = new() { Name = "Each", Abbreviation = "ea" };

    private readonly AssetType _pushMower = new() { Name = "Push Mower" };

    [Fact]
    public async Task CreateWorkOrderAsync_OilStep_StartsWithTheEquipmentsOilUnitQuantityAndType()
    {
        var order = await CreateOrderAsync(_quart);

        var step = OilStep(order);

        Assert.Equal("qt", step.QuantityUnit);

        Assert.Equal(2m, step.Quantity);

        Assert.Equal("SAE 10W-30", step.Product);
    }

    [Fact]
    public async Task CreateWorkOrderAsync_EquipmentWhoseOilIsMeasuredInOunces_UsesOunces()
    {
        var order = await CreateOrderAsync(_ounce, oilQuantity: 20m);

        var step = OilStep(order);

        Assert.Equal("oz", step.QuantityUnit);

        Assert.Equal(20m, step.Quantity);
    }

    [Fact]
    public async Task CreateWorkOrderAsync_StepWithItsOwnUnit_UsesThatUnit()
    {
        var order = await CreateOrderAsync(_quart);

        var filter = order.Tasks.Single(t => t.Description == "Change air filter");

        Assert.Equal("ea", filter.QuantityUnit);

        Assert.Null(filter.Product);
    }

    [Fact]
    public async Task SetTaskCompletionAsync_RequiredPartNumberMissing_IsInvalid()
    {
        var order = await CreateOrderAsync(_quart);

        var result = await CheckOffAsync(order, OilStep(order), new TaskDetailsRequest { Quantity = 2m });

        Assert.Equal(ResultStatus.Invalid, result.Status);

        Assert.Contains("part number", result.Error);
    }

    [Fact]
    public async Task SetTaskCompletionAsync_RequiredDetailsGiven_CompletesAndSavesThem()
    {
        var order = await CreateOrderAsync(_quart);

        var result = await CheckOffAsync(order, OilStep(order), new TaskDetailsRequest { Quantity = 1.5m, PartNumber = " 100-ABC ", Notes = "Used the last jug" });

        var step = result.Value!.Tasks.Single(t => t.Description == "Change engine oil");

        Assert.True(step.IsComplete);

        Assert.Equal(1.5m, step.Quantity);

        Assert.Equal("100-ABC", step.PartNumber);

        Assert.Equal("Used the last jug", step.StepNotes);
    }

    [Fact]
    public async Task SetTaskCompletionAsync_RequiredQuantityZero_IsInvalid()
    {
        var order = await CreateOrderAsync(_quart);

        var result = await CheckOffAsync(order, OilStep(order), new TaskDetailsRequest { Quantity = 0m, PartNumber = "100-ABC" });

        Assert.Equal(ResultStatus.Invalid, result.Status);

        Assert.Contains("quantity", result.Error);
    }

    [Fact]
    public async Task SetTaskCompletionAsync_NegativeQuantity_IsInvalid()
    {
        var order = await CreateOrderAsync(_quart);

        var result = await CheckOffAsync(order, OilStep(order), new TaskDetailsRequest { Quantity = -1m, PartNumber = "100-ABC" });

        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task SetTaskCompletionAsync_UncheckingAStep_NeedsNoDetails()
    {
        var order = await CreateOrderAsync(_quart);

        var checkedOff = await CheckOffAsync(order, OilStep(order), new TaskDetailsRequest { Quantity = 2m, PartNumber = "100-ABC" });

        var result = await Service().SetTaskCompletionAsync(
            new TaskCompletionCommand(order.Id, OilStep(checkedOff.Value!).Id, false, _technician, true),
            CancellationToken.None);

        Assert.False(result.Value!.Tasks.Single(t => t.Description == "Change engine oil").IsComplete);
    }

    [Fact]
    public async Task SetTaskCompletionAsync_DetailsTheStepDoesNotAskFor_AreNotSaved()
    {
        var order = await CreateOrderAsync(_quart);

        var filter = order.Tasks.Single(t => t.Description == "Change air filter");

        var result = await CheckOffAsync(order, filter, new TaskDetailsRequest { Quantity = 1m, PartNumber = "should be ignored", Notes = "ignored too" });

        var saved = result.Value!.Tasks.Single(t => t.Description == "Change air filter");

        Assert.Equal(1m, saved.Quantity);

        Assert.Null(saved.PartNumber);

        Assert.Null(saved.StepNotes);
    }

    [Fact]
    public async Task SetTaskCompletionAsync_SavingDetailsOnACompletedStep_KeepsItComplete()
    {
        var order = await CreateOrderAsync(_quart);

        await CheckOffAsync(order, OilStep(order), new TaskDetailsRequest { Quantity = 2m, PartNumber = "100-ABC" });

        var result = await CheckOffAsync(order, OilStep(order), new TaskDetailsRequest { Quantity = 2m, PartNumber = "200-XYZ" });

        var step = result.Value!.Tasks.Single(t => t.Description == "Change engine oil");

        Assert.True(step.IsComplete);

        Assert.Equal("200-XYZ", step.PartNumber);
    }

    private static WorkOrderTaskResponse OilStep(WorkOrderResponse order)
    {
        return order.Tasks.Single(t => t.Description == "Change engine oil");
    }

    private IMaintenanceService Service()
    {
        return new MaintenanceService(_mowers, _templates, _workOrders, _inspections, new OptionsWrapper<MaintenanceOptions>(new MaintenanceOptions()), NullLogger<MaintenanceService>.Instance);
    }

    private async Task<Result<WorkOrderResponse>> CheckOffAsync(WorkOrderResponse order, WorkOrderTaskResponse step, TaskDetailsRequest details)
    {
        return await Service().SetTaskCompletionAsync(new TaskCompletionCommand(order.Id, step.Id, true, _technician, true, details), CancellationToken.None);
    }

    private async Task<WorkOrderResponse> CreateOrderAsync(UnitOfMeasure oilUnit, decimal oilQuantity = 2m)
    {
        var mower = new Mower
        {
            Name = "Mower 1",
            AssetTypeId = _pushMower.Id,
            AssetType = _pushMower,
            OilQuantity = oilQuantity,
            OilUnitId = oilUnit.Id,
            OilUnit = oilUnit,
            OilTypeId = Guid.NewGuid(),
            OilType = new OilType { Name = "SAE 10W-30" },
            PurchaseDate = new DateOnly(2024, 1, 1),
            IsActive = true,
        };

        await _mowers.AddAsync(mower, CancellationToken.None);

        var template = new ServiceTemplate
        {
            Name = "Annual Service",
            AssetTypes = [_pushMower],
            Tasks =
            [
                new ServiceTask
                {
                    Description = "Change engine oil",
                    SortOrder = 0,
                    QuantityRequirement = DetailRequirement.Required,
                    QuantityUnitSource = QuantityUnitSource.AssetOil,
                    PartNumberRequirement = DetailRequirement.Required,
                    NotesRequirement = DetailRequirement.Optional,
                },
                new ServiceTask
                {
                    Description = "Change air filter",
                    SortOrder = 1,
                    QuantityRequirement = DetailRequirement.Optional,
                    QuantityUnitSource = QuantityUnitSource.StepUnit,
                    UnitId = _each.Id,
                    Unit = _each,
                },
            ],
        };

        await _templates.AddAsync(template, CancellationToken.None);

        var result = await Service().CreateWorkOrderAsync(new WorkOrderCreateRequest { MowerId = mower.Id, ServiceTemplateId = template.Id }, CancellationToken.None);

        return result.Value!;
    }
}
