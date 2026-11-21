namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// One checklist item of a service template.
/// </summary>
public sealed class ServiceTaskRequest
{
    /// <summary>Gets or sets the task description (e.g. "Replace air filter").</summary>
    [Required]
    [StringLength(300)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the task must be checked off before the work order can be completed.</summary>
    public bool IsRequired { get; set; } = true;
}