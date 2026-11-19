namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// Outcome of creating the first account. The account cannot sign in until its email address is confirmed.
/// </summary>
/// <param name="Email">The address the confirmation link was sent to.</param>
/// <param name="ConfirmationEmailSent">Whether the mail service accepted the confirmation email.</param>
public sealed record RegisterResponse(string Email, bool ConfirmationEmailSent);