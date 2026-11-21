namespace MowTrackr.Application.Models;

/// <summary>
/// The subset of the API's ProblemDetails body the client reads.
/// </summary>
/// <param name="Title">The short title.</param>
/// <param name="Detail">The human-readable detail.</param>
public sealed record ProblemResponse(string? Title, string? Detail);