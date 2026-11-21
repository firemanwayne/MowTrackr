namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// An inspection template assigned to a service template.
/// </summary>
/// <param name="Id">The inspection template key.</param>
/// <param name="Name">The inspection name.</param>
/// <param name="Timing">Whether it is the pre-check or the post-check.</param>
public sealed record InspectionSummaryResponse(Guid Id, string Name, InspectionTiming Timing);
