using MowTrackr.Core.Contracts.Maintenance;
using MowTrackr.Core.Enums;
using MowTrackr.Maintenance.Lookups;
using MowTrackr.Maintenance.Options;
using MowTrackr.Maintenance.Services;
using MowTrackr.Maintenance.UnitTests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace MowTrackr.Maintenance.UnitTests.Services;

public sealed class MaintenanceServiceTests
{
    private static readonly Guid _technician = Guid.NewGuid();

    private readonly InMemoryRepository<Mower> _mowers = new();

    private readonly InMemoryRepository<ServiceTemplate> _templates = new();

    private readonly InMemoryRepository<WorkOrder> _workOrders = new();

    private readonly InMemoryRepository<InspectionTemplate> _inspections = new();

    private readonly AssetType _pushMower = new() { Name = "Push Mower", HasTires = false };

    private readonly AssetType _ridingMower = new() { Name = "Riding Mower", HasTires = true };

    private readonly AssetType _chainSaw = new() { Name = "Chain Saw", HasTires = false };

    [Fact]
    public async Task CreateWorkOrderAsync_ServiceWithChecks_BuildsPreCheckThenServiceThenPostCheck()
    {
        var order = await CreateAnnualAsync(_pushMower);

        var phases = order.Tasks.OrderBy(t => t.SortOrder).Select(t => t.Phase).Distinct().ToList();

        Assert.Equal([TaskPhase.Before, TaskPhase.Service, TaskPhase.After], phases);
    }

    [Fact]
    public async Task CreateWorkOrderAsync_UnitOfAType_TitleStartsWithThatType()
    {
        var order = await CreateAnnualAsync(_pushMower);

        Assert.Equal("Push Mower Annual Inspection", order.TemplateName);
    }

    [Fact]
    public async Task CreateWorkOrderAsync_TypeWithoutTires_LeavesOutTheTireRotation()
    {
        var order = await CreateAnnualAsync(_pushMower);

        Assert.DoesNotContain(order.Tasks, t => t.Description == "Rotate tires");
    }

    [Fact]
    public async Task CreateWorkOrderAsync_TypeWithTires_IncludesTheTireRotation()
    {
        var order = await CreateAnnualAsync(_ridingMower);

        Assert.Contains(order.Tasks, t => t.Description == "Rotate tires" && t.Phase == TaskPhase.Service);
    }

    [Fact]
    public async Task CreateWorkOrderAsync_TypeTheServiceDoesNotCover_IsInvalid()
    {
        var template = AnnualTemplate();

        var saw = AddMower(_chainSaw);

        var result = await Service().CreateWorkOrderAsync(new WorkOrderCreateRequest { MowerId = saw.Id, ServiceTemplateId = template.Id }, CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task SetTaskCompletionAsync_PreCheckNotSubmitted_IsRejected()
    {
        var order = await CreateAnnualAsync(_pushMower);

        var step = order.Tasks.First(t => t.Phase == TaskPhase.Service);

        var result = await Service().SetTaskCompletionAsync(new TaskCompletionCommand(order.Id, step.Id, true, _technician, true), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task SubmitCheckAsync_ValidPreCheck_UnlocksTheServiceSteps()
    {
        var order = await CreateAnnualAsync(_pushMower);

        var submitted = await SubmitPreCheckAsync(order);

        Assert.True(submitted.IsSuccess);

        var step = submitted.Value!.Tasks.First(t => t.Phase == TaskPhase.Service);

        var result = await Service().SetTaskCompletionAsync(new TaskCompletionCommand(order.Id, step.Id, true, _technician, true), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SubmitCheckAsync_ValidPreCheck_SavesAnswersAndMovesTheOrderToInProgress()
    {
        var order = await CreateAnnualAsync(_pushMower);

        var submitted = (await SubmitPreCheckAsync(order)).Value!;

        Assert.Equal(WorkOrderStatus.InProgress, submitted.Status);

        Assert.NotNull(submitted.PreCheckSubmittedAt);

        Assert.Equal("Half", submitted.Tasks.First(t => t.Description == "Oil level").Answer);
    }

    [Fact]
    public async Task SubmitCheckAsync_MissingRequiredAnswer_IsInvalid()
    {
        var order = await CreateAnnualAsync(_pushMower);

        (string Description, string? Answer)[] answers = [("No existing damage", "Yes")];

        var result = await SubmitAsync(order, InspectionTiming.Before, answers);

        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task SubmitCheckAsync_AnswerThatIsNotAChoice_IsInvalid()
    {
        var order = await CreateAnnualAsync(_pushMower);

        var result = await SubmitAsync(order, InspectionTiming.Before, PreCheckAnswers(oil: "Quarter"));

        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task SubmitCheckAsync_HoursAnswer_UpdatesTheMowersEngineHours()
    {
        var order = await CreateAnnualAsync(_pushMower, hours: 100m);

        await SubmitAsync(order, InspectionTiming.Before, PreCheckAnswers(hours: "112.5"));

        Assert.Equal(112.5m, _mowers.Items[0].EngineHours);
    }

    [Fact]
    public async Task SubmitCheckAsync_HoursLowerThanRecorded_IsInvalidAndLeavesTheHours()
    {
        var order = await CreateAnnualAsync(_pushMower, hours: 100m);

        var result = await SubmitAsync(order, InspectionTiming.Before, PreCheckAnswers(hours: "90"));

        Assert.Equal(ResultStatus.Invalid, result.Status);

        Assert.Equal(100m, _mowers.Items[0].EngineHours);
    }

    [Fact]
    public async Task SubmitCheckAsync_PostCheckBeforeTheServiceStepsAreDone_IsAConflict()
    {
        var order = await CreateAnnualAsync(_pushMower);

        await SubmitPreCheckAsync(order);

        var result = await SubmitAsync(order, InspectionTiming.After, PostCheckAnswers());

        Assert.Equal(ResultStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task CompleteWorkOrderAsync_PostCheckNotSubmitted_IsInvalid()
    {
        var order = await CreateAnnualAsync(_pushMower);

        await SubmitPreCheckAsync(order);

        await CompleteServiceStepsAsync(order);

        var result = await Service().CompleteWorkOrderAsync(order.Id, new CompleteWorkOrderRequest { EngineHoursAtService = 10m }, CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task CompleteWorkOrderAsync_AfterBothChecksAndTheServiceSteps_Completes()
    {
        var order = await CreateAnnualAsync(_pushMower);

        await SubmitPreCheckAsync(order);

        await CompleteServiceStepsAsync(order);

        var post = await SubmitAsync(order, InspectionTiming.After, PostCheckAnswers());

        Assert.True(post.IsSuccess);

        var result = await Service().CompleteWorkOrderAsync(order.Id, new CompleteWorkOrderRequest { EngineHoursAtService = 10m }, CancellationToken.None);

        Assert.Equal(WorkOrderStatus.Completed, result.Value!.Status);
    }

    [Fact]
    public async Task SubmitCheckAsync_ServiceStepsCompleteItem_IsAnsweredAutomatically()
    {
        var order = await CreateAnnualAsync(_pushMower);

        await SubmitPreCheckAsync(order);

        await CompleteServiceStepsAsync(order);

        var post = (await SubmitAsync(order, InspectionTiming.After, PostCheckAnswers())).Value!;

        Assert.Equal("Yes", post.Tasks.First(t => t.Description == "All service items completed").Answer);
    }

    [Fact]
    public async Task SubmitCheckAsync_BadAnswerToARepairItem_IsReportedAsAFinding()
    {
        var order = await CreateAnnualAsync(_pushMower);

        var submitted = (await SubmitAsync(order, InspectionTiming.Before, PreCheckAnswers(damage: "No"))).Value!;

        var finding = Assert.Single(submitted.Tasks, t => t.IsFinding);

        Assert.Equal("No existing damage", finding.Description);
    }

    [Fact]
    public async Task CreateRepairAsync_ConfirmedFinding_CreatesALinkedRepairWorkOrder()
    {
        var order = await CreateAnnualAsync(_pushMower);

        var submitted = (await SubmitAsync(order, InspectionTiming.Before, PreCheckAnswers(damage: "No"))).Value!;

        var finding = submitted.Tasks.First(t => t.IsFinding);

        var result = await Service().CreateRepairAsync(new RepairCommand(order.Id, finding.Id, _technician, true), CancellationToken.None);

        var repair = result.Value!;

        Assert.Equal(WorkOrderKind.Repair, repair.Kind);

        Assert.Equal(order.Id, repair.ParentWorkOrderId);

        Assert.Equal(order.MowerId, repair.MowerId);

        Assert.Null(repair.ServiceTemplateId);
    }

    [Fact]
    public async Task CreateRepairAsync_SameFindingTwice_IsAConflict()
    {
        var order = await CreateAnnualAsync(_pushMower);

        var finding = (await SubmitAsync(order, InspectionTiming.Before, PreCheckAnswers(damage: "No"))).Value!.Tasks.First(t => t.IsFinding);

        await Service().CreateRepairAsync(new RepairCommand(order.Id, finding.Id, _technician, true), CancellationToken.None);

        var again = await Service().CreateRepairAsync(new RepairCommand(order.Id, finding.Id, _technician, true), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, again.Status);
    }

    [Fact]
    public async Task CreateRepairAsync_ItemThatIsNotAFinding_IsInvalid()
    {
        var order = await CreateAnnualAsync(_pushMower);

        var submitted = (await SubmitPreCheckAsync(order)).Value!;

        var item = submitted.Tasks.First(t => t.Description == "No existing damage");

        var result = await Service().CreateRepairAsync(new RepairCommand(order.Id, item.Id, _technician, true), CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task OpenRepairLookup_RepairNotFinished_ListsTheMower()
    {
        var order = await CreateAnnualAsync(_pushMower);

        var finding = (await SubmitAsync(order, InspectionTiming.Before, PreCheckAnswers(damage: "No"))).Value!.Tasks.First(t => t.IsFinding);

        await Service().CreateRepairAsync(new RepairCommand(order.Id, finding.Id, _technician, true), CancellationToken.None);

        var needing = await new OpenRepairLookup(_workOrders).GetMowerIdsNeedingRepairAsync(CancellationToken.None);

        Assert.Contains(order.MowerId, needing);
    }

    [Fact]
    public async Task OpenRepairLookup_ServiceWorkOrderOnly_DoesNotListTheMower()
    {
        var order = await CreateAnnualAsync(_pushMower);

        var needing = await new OpenRepairLookup(_workOrders).GetMowerIdsNeedingRepairAsync(CancellationToken.None);

        Assert.DoesNotContain(order.MowerId, needing);
    }

    [Fact]
    public async Task CompleteWorkOrderAsync_AnnualServiceWithAnOpenRepair_StillCompletesAndTheMowerStaysUnavailable()
    {
        var order = await CreateAnnualAsync(_pushMower);

        var finding = (await SubmitAsync(order, InspectionTiming.Before, PreCheckAnswers(damage: "No"))).Value!.Tasks.First(t => t.IsFinding);

        await Service().CreateRepairAsync(new RepairCommand(order.Id, finding.Id, _technician, true), CancellationToken.None);

        await CompleteServiceStepsAsync(order);

        await SubmitAsync(order, InspectionTiming.After, PostCheckAnswers());

        var completed = await Service().CompleteWorkOrderAsync(order.Id, new CompleteWorkOrderRequest { EngineHoursAtService = 10m }, CancellationToken.None);

        var needing = await new OpenRepairLookup(_workOrders).GetMowerIdsNeedingRepairAsync(CancellationToken.None);

        Assert.True(completed.IsSuccess);

        Assert.Contains(order.MowerId, needing);
    }

    [Fact]
    public async Task DeleteWorkOrderAsync_ExistingOrder_RemovesIt()
    {
        var order = await CreateAnnualAsync(_pushMower);

        var result = await Service().DeleteWorkOrderAsync(order.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.Empty(_workOrders.Items);
    }

    [Fact]
    public async Task DeleteWorkOrderAsync_UnknownOrder_IsNotFound()
    {
        var result = await Service().DeleteWorkOrderAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task DeleteWorkOrderAsync_ServiceOrderThatRaisedARepair_KeepsTheRepairWithoutALink()
    {
        var (order, repair) = await CreateOrderWithRepairAsync();

        await Service().DeleteWorkOrderAsync(order.Id, CancellationToken.None);

        var remaining = Assert.Single(_workOrders.Items);

        Assert.Equal(repair.Id, remaining.Id);

        Assert.Null(remaining.ParentWorkOrderId);
    }

    [Fact]
    public async Task DeleteWorkOrderAsync_RepairRaisedFromAFinding_FreesTheFindingForANewRepair()
    {
        var (order, repair) = await CreateOrderWithRepairAsync();

        await Service().DeleteWorkOrderAsync(repair.Id, CancellationToken.None);

        var finding = _workOrders.Items.Single().Tasks.Single(t => t.Description == "No existing damage");

        Assert.Null(finding.RepairWorkOrderId);

        var again = await Service().CreateRepairAsync(new RepairCommand(order.Id, finding.Id, _technician, true), CancellationToken.None);

        Assert.True(again.IsSuccess);
    }

    [Fact]
    public async Task DeleteWorkOrderAsync_OpenRepair_MowerNoLongerNeedsRepair()
    {
        var (order, repair) = await CreateOrderWithRepairAsync();

        await Service().DeleteWorkOrderAsync(repair.Id, CancellationToken.None);

        var needing = await new OpenRepairLookup(_workOrders).GetMowerIdsNeedingRepairAsync(CancellationToken.None);

        Assert.DoesNotContain(order.MowerId, needing);
    }

    private IMaintenanceService Service()
    {
        return new MaintenanceService(_mowers, _templates, _workOrders, _inspections, new OptionsWrapper<MaintenanceOptions>(new MaintenanceOptions()), NullLogger<MaintenanceService>.Instance);
    }

    private async Task<(WorkOrderResponse Order, WorkOrderResponse Repair)> CreateOrderWithRepairAsync()
    {
        var order = await CreateAnnualAsync(_pushMower);

        var finding = (await SubmitAsync(order, InspectionTiming.Before, PreCheckAnswers(damage: "No"))).Value!.Tasks.First(t => t.IsFinding);

        var repair = (await Service().CreateRepairAsync(new RepairCommand(order.Id, finding.Id, _technician, true), CancellationToken.None)).Value!;

        return (order, repair);
    }

    private async Task<WorkOrderResponse> CreateAnnualAsync(AssetType type, decimal hours = 0m)
    {
        var template = AnnualTemplate();

        var mower = AddMower(type, hours);

        var result = await Service().CreateWorkOrderAsync(new WorkOrderCreateRequest { MowerId = mower.Id, ServiceTemplateId = template.Id }, CancellationToken.None);

        return result.Value!;
    }

    private Mower AddMower(AssetType type, decimal hours = 0m)
    {
        var mower = new Mower { Name = "Mower 1", AssetTypeId = type.Id, AssetType = type, EngineHours = hours, PurchaseDate = new DateOnly(2024, 1, 1), IsActive = true };

        _mowers.AddAsync(mower, CancellationToken.None);

        return mower;
    }

    private ServiceTemplate AnnualTemplate()
    {
        var existing = _templates.Items.FirstOrDefault();

        if (existing is not null)
        {
            return existing;
        }

        var template = new ServiceTemplate
        {
            Name = "Annual Inspection",
            AssetTypes = [_pushMower, _ridingMower],
            Inspections = [PreCheck(), PostCheck()],
            Tasks =
            [
                new ServiceTask { Description = "Change engine oil", SortOrder = 0 },
                new ServiceTask { Description = "Change air filter", SortOrder = 1 },
                new ServiceTask { Description = "Change fuel filter", SortOrder = 2 },
                new ServiceTask { Description = "Rotate tires", SortOrder = 3, Condition = ServiceTaskCondition.AssetHasTires },
            ],
        };

        _templates.AddAsync(template, CancellationToken.None);

        return template;
    }

    private static InspectionTemplate PreCheck()
    {
        return new InspectionTemplate
        {
            Name = "Annual Pre-Check",
            Timing = InspectionTiming.Before,
            Tasks =
            [
                new InspectionTask { Description = "No existing damage", SortOrder = 0, ItemType = CheckItemType.YesNo, RepairOn = "No" },
                new InspectionTask { Description = "Oil level", SortOrder = 1, ItemType = CheckItemType.Choice, Choices = "Full, Half, None", RepairOn = "None" },
                new InspectionTask { Description = "Current hours", SortOrder = 2, ItemType = CheckItemType.EngineHours, IsRequired = false },
                new InspectionTask { Description = "Fuel level", SortOrder = 3, ItemType = CheckItemType.Choice, Choices = "Empty, Half, Full" },
                new InspectionTask { Description = "Notes", SortOrder = 4, ItemType = CheckItemType.Text, IsRequired = false },
            ],
        };
    }

    private static InspectionTemplate PostCheck()
    {
        return new InspectionTemplate
        {
            Name = "Annual Post-Check",
            Timing = InspectionTiming.After,
            Tasks =
            [
                new InspectionTask { Description = "Does it start?", SortOrder = 0, ItemType = CheckItemType.YesNo, RepairOn = "No" },
                new InspectionTask { Description = "Oil level after", SortOrder = 1, ItemType = CheckItemType.Choice, Choices = "Full, Half, None", RepairOn = "None" },
                new InspectionTask { Description = "No new damage", SortOrder = 2, ItemType = CheckItemType.YesNo, RepairOn = "No" },
                new InspectionTask { Description = "All service items completed", SortOrder = 3, ItemType = CheckItemType.ServiceStepsComplete },
                new InspectionTask { Description = "Current hours after", SortOrder = 4, ItemType = CheckItemType.EngineHours, IsRequired = false },
            ],
        };
    }

    private static (string Description, string? Answer)[] PreCheckAnswers(string damage = "Yes", string oil = "Half", string? hours = null)
    {
        return
        [
            ("No existing damage", damage),
            ("Oil level", oil),
            ("Current hours", hours),
            ("Fuel level", "Full"),
        ];
    }

    private static (string Description, string? Answer)[] PostCheckAnswers()
    {
        return
        [
            ("Does it start?", "Yes"),
            ("Oil level after", "Full"),
            ("No new damage", "Yes"),
        ];
    }

    private Task<Result<WorkOrderResponse>> SubmitPreCheckAsync(WorkOrderResponse order)
    {
        return SubmitAsync(order, InspectionTiming.Before, PreCheckAnswers());
    }

    private Task<Result<WorkOrderResponse>> SubmitAsync(WorkOrderResponse order, InspectionTiming timing, IEnumerable<(string Description, string? Answer)> answers)
    {
        var current = _workOrders.Items.First(w => w.Id == order.Id);

        var phase = timing == InspectionTiming.Before ? TaskPhase.Before : TaskPhase.After;

        var list = answers
            .Select(a => new CheckAnswerRequest { TaskId = current.Tasks.First(t => t.Phase == phase && t.Description == a.Description).Id, Answer = a.Answer })
            .ToList();

        return Service().SubmitCheckAsync(new CheckSubmissionCommand(order.Id, timing, list, _technician, true), CancellationToken.None);
    }

    private async Task CompleteServiceStepsAsync(WorkOrderResponse order)
    {
        var current = _workOrders.Items.First(w => w.Id == order.Id);

        foreach (var step in current.Tasks.Where(t => t.Phase == TaskPhase.Service).ToList())
        {
            await Service().SetTaskCompletionAsync(new TaskCompletionCommand(order.Id, step.Id, true, _technician, true), CancellationToken.None);
        }
    }
}
