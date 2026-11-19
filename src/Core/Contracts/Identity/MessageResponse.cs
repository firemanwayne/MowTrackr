namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// A plain acknowledgement with a human-readable message.
/// </summary>
/// <param name="Message">The message.</param>
public sealed record MessageResponse(string Message);