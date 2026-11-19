namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Sends the application's emails (through Azure Communication Services in Azure, or the log locally).
/// </summary>
public interface IEmailService
{
    /// <summary>Sends an invitation email.</summary>
    /// <param name="email">The recipient, link and wording.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> if the mail service accepted the message.</returns>
    Task<bool> SendInvitationAsync(InvitationEmail email, CancellationToken cancellationToken);
}