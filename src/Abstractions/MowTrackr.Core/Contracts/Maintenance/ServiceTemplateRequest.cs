namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// Payload for creating or updating a service template (e.g. "Quarterly Maintenance").
/// </summary>
public sealed class ServiceTemplateRequest
{
    /// <summary>Gets or sets the template name.</summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the asset type this template applies to (e.g. Chain Saw).</summary>
    [Required]
    public Guid? AssetTypeId { get; set; }

    /// <summary>Gets or sets an optional description.</summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>Gets or sets the engine-hours interval (service due every N hours), if any.</summary>
    [Range(0.1, 100_000)]
    public decimal? IntervalHours { get; set; }

    /// <summary>Gets or sets the calendar interval in days (service due every N days), if any.</summary>
    [Range(1, 3650)]
    public int? IntervalDays { get; set; }

    /// <summary>Gets or sets a value indicating whether the template is used for due checks.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the ordered checklist.</summary>
    public List<ServiceTaskRequest> Tasks { get; set; } = [];
}