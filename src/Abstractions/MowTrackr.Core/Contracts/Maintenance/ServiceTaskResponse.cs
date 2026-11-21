namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// One checklist item of a service template.
/// </summary>
/// <param name="Id">The task key.</param>
/// <param name="Description">The task description.</param>
/// <param name="SortOrder">The display order.</param>
/// <param name="IsRequired">Whether the task must be checked off.</param>
/// <param name="Condition">When the step applies.</param>
/// <param name="QuantityRequirement">Whether the step asks for a quantity.</param>
/// <param name="QuantityUnitSource">Where the quantity's unit comes from.</param>
/// <param name="UnitId">The unit of the quantity when it comes from the step.</param>
/// <param name="Unit">The label of that unit.</param>
/// <param name="PartNumberRequirement">Whether the step asks for a part number.</param>
/// <param name="NotesRequirement">Whether the step asks for notes.</param>
public sealed record ServiceTaskResponse(
    Guid Id,
    string Description,
    int SortOrder,
    bool IsRequired,
    ServiceTaskCondition Condition = ServiceTaskCondition.Always,
    DetailRequirement QuantityRequirement = DetailRequirement.None,
    QuantityUnitSource QuantityUnitSource = QuantityUnitSource.StepUnit,
    Guid? UnitId = null,
    string? Unit = null,
    DetailRequirement PartNumberRequirement = DetailRequirement.None,
    DetailRequirement NotesRequirement = DetailRequirement.None);
