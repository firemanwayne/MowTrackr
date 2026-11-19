namespace MowTrackr.Core.Models;

/// <summary>
/// The interval settings of an existing template, as seen by the interval validator.
/// </summary>
/// <param name="Id">The template key.</param>
/// <param name="Name">The template name.</param>
/// <param name="IntervalHours">The engine-hours interval.</param>
/// <param name="IntervalDays">The calendar interval in days.</param>
public sealed record IntervalDefinition(Guid Id, string Name, decimal? IntervalHours, int? IntervalDays);