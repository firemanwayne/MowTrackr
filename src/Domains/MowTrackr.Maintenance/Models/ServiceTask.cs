using MowTrackr.Assets.Models;

namespace MowTrackr.Maintenance.Models;

/// <summary>
/// One checklist item belonging to a <see cref="ServiceTemplate"/>.
/// </summary>
public sealed class ServiceTask : EntityBase
{
    /// <summary>Gets or sets the owning template key.</summary>
    public Guid ServiceTemplateId { get; set; }

    /// <summary>Gets or sets the task description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the display order.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets a value indicating whether the task must be checked off to complete a work order.</summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>Gets or sets when the step applies, for example only to asset types that have tires.</summary>
    public ServiceTaskCondition Condition { get; set; } = ServiceTaskCondition.Always;

    /// <summary>Gets or sets whether the step asks for a quantity.</summary>
    public DetailRequirement QuantityRequirement { get; set; } = DetailRequirement.None;

    /// <summary>Gets or sets where the quantity's unit comes from: a unit on the step, or the equipment's oil unit.</summary>
    public QuantityUnitSource QuantityUnitSource { get; set; } = QuantityUnitSource.StepUnit;

    /// <summary>Gets or sets the key of the quantity's unit when it comes from the step.</summary>
    public Guid? UnitId { get; set; }

    /// <summary>Gets or sets the quantity's unit when it comes from the step.</summary>
    public UnitOfMeasure? Unit { get; set; }

    /// <summary>Gets or sets whether the step asks for a part number.</summary>
    public DetailRequirement PartNumberRequirement { get; set; } = DetailRequirement.None;

    /// <summary>Gets or sets whether the step asks for notes.</summary>
    public DetailRequirement NotesRequirement { get; set; } = DetailRequirement.None;
}
