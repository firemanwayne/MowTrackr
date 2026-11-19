namespace MowTrackr.Core.Models;

/// <summary>
/// A verification code to text to a phone number.
/// </summary>
/// <param name="To">The phone number in E.164 format.</param>
/// <param name="Code">The code to send.</param>
/// <param name="Language">The language code of the message (en or es).</param>
public sealed record SmsVerification(string To, string Code, string Language);