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
}