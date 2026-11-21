namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// One checklist item of a service template, as sent by the client.
/// </summary>
public sealed class ServiceTaskRequest
{
    /// <summary>Gets or sets the task description.</summary>
    [Required]
    [StringLength(300)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the task must be checked off to complete a work order.</summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>Gets or sets when the step applies, for example only to asset types that have tires.</summary>
    public ServiceTaskCondition Condition { get; set; } = ServiceTaskCondition.Always;

    /// <summary>Gets or sets whether the step asks for a quantity.</summary>
    public DetailRequirement QuantityRequirement { get; set; } = DetailRequirement.None;

    /// <summary>Gets or sets where the quantity's unit comes from: a unit on the step, or the equipment's oil unit.</summary>
    public QuantityUnitSource QuantityUnitSource { get; set; } = QuantityUnitSource.StepUnit;

    /// <summary>Gets or sets the unit of the quantity when it comes from the step.</summary>
    public Guid? UnitId { get; set; }

    /// <summary>Gets or sets whether the step asks for a part number.</summary>
    public DetailRequirement PartNumberRequirement { get; set; } = DetailRequirement.None;

    /// <summary>Gets or sets whether the step asks for notes.</summary>
    public DetailRequirement NotesRequirement { get; set; } = DetailRequirement.None;
}
