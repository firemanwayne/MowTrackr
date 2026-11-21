namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// Request to create or update a service template.
/// </summary>
public sealed class ServiceTemplateRequest
{
    /// <summary>Gets or sets the template name.</summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the asset types the template applies to (at least one).</summary>
    public List<Guid> AssetTypeIds { get; set; } = [];

    /// <summary>Gets or sets the inspection templates (pre-check and post-check) assigned to this service.</summary>
    public List<Guid> InspectionTemplateIds { get; set; } = [];

    /// <summary>Gets or sets an optional description.</summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>Gets or sets the engine-hours interval.</summary>
    [Range(0.1, 100_000)]
    public decimal? IntervalHours { get; set; }

    /// <summary>Gets or sets the calendar interval in days.</summary>
    [Range(1, 3650)]
    public int? IntervalDays { get; set; }

    /// <summary>Gets or sets a value indicating whether the template participates in due checks.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the checklist.</summary>
    public List<ServiceTaskRequest> Tasks { get; set; } = [];
}
