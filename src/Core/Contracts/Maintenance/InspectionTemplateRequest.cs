namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// Payload for creating or updating an inspection template.
/// </summary>
public sealed class InspectionTemplateRequest
{
    /// <summary>Gets or sets the template name (e.g. "500 Hour Inspection").</summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the asset type this inspection applies to.</summary>
    [Required]
    public Guid? AssetTypeId { get; set; }

    /// <summary>Gets or sets an optional description.</summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>Gets or sets the engine hours at or above which the inspection applies, if any.</summary>
    [Range(0.1, 1_000_000)]
    public decimal? MinEngineHours { get; set; }

    /// <summary>Gets or sets the age in months, counted from the purchase date, at or above which the inspection applies, if any.</summary>
    [Range(1, 1200)]
    public int? MinAgeMonths { get; set; }

    /// <summary>Gets or sets a value indicating whether the inspection is added to work orders.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the ordered checklist.</summary>
    public List<ServiceTaskRequest> Tasks { get; set; } = [];
}
