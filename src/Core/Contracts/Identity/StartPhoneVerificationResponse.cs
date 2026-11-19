namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// Outcome of starting phone verification.
/// </summary>
/// <param name="CodeSent">Whether the text message was handed to the SMS service.</param>
public sealed record StartPhoneVerificationResponse(bool CodeSent);