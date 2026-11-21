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
}
