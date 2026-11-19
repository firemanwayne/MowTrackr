namespace MowTrackr.Core.Models;

/// <summary>
/// Everything needed to send an invitation email.
/// </summary>
/// <param name="To">The recipient address.</param>
/// <param name="InviterName">Who invited them.</param>
/// <param name="AcceptUrl">The link that accepts the invitation.</param>
/// <param name="ExpiresAt">When the invitation stops working.</param>
/// <param name="Language">The language code of the email (en or es).</param>
public sealed record InvitationEmail(
    string To,
    string InviterName,
    string AcceptUrl,
    DateTimeOffset ExpiresAt,
    string Language);