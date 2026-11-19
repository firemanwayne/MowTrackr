namespace MowTrackr.Core.Models;

/// <summary>
/// Everything needed to send an email-confirmation message.
/// </summary>
/// <param name="To">The recipient address.</param>
/// <param name="DisplayName">The recipient's name.</param>
/// <param name="ConfirmUrl">The link that confirms the address.</param>
/// <param name="Language">The language code of the email (en or es).</param>
public sealed record EmailConfirmationEmail(string To, string DisplayName, string ConfirmUrl, string Language);