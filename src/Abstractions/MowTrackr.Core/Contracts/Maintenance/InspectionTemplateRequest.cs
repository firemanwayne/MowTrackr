namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// Request to create or update an inspection (check) template.
/// </summary>
public sealed class InspectionTemplateRequest
{
    /// <summary>Gets or sets the template name.</summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the asset type it is limited to, or <see langword="null"/> for any type.</summary>
    public Guid? AssetTypeId { get; set; }

    /// <summary>Gets or sets whether it is a pre-check (before the work) or a post-check (after it).</summary>
    public InspectionTiming Timing { get; set; } = InspectionTiming.After;

    /// <summary>Gets or sets an optional description.</summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>Gets or sets the engine hours at or above which it is added to work orders automatically.</summary>
    [Range(0.1, 1_000_000)]
    public decimal? MinEngineHours { get; set; }

    /// <summary>Gets or sets the age in months at or above which it is added to work orders automatically.</summary>
    [Range(1, 1200)]
    public int? MinAgeMonths { get; set; }

    /// <summary>Gets or sets a value indicating whether the inspection is added to work orders.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the items.</summary>
    public List<InspectionTaskRequest> Tasks { get; set; } = [];
}
