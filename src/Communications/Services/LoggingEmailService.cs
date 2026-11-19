using Microsoft.Extensions.Hosting;

namespace MowTrackr.Communications.Services;

/// <summary>
/// Stand-in used when Azure Communication Services is not configured. It sends nothing; in Development it logs the
/// invitation link so the flow can be tried locally. Elsewhere the link (a secret) is never written to the log.
/// </summary>
/// <param name="environment">The host environment.</param>
/// <param name="logger">The logger.</param>
internal sealed class LoggingEmailService(IHostEnvironment environment, ILogger<LoggingEmailService> logger) : IEmailService
{
    /// <inheritdoc />
    public Task<bool> SendInvitationAsync(InvitationEmail email, CancellationToken cancellationToken)
    {
        if (environment.IsDevelopment())
        {
            logger.LogInformation("[Local] Invitation for {Recipient} not emailed; accept link: {AcceptUrl}", email.To, email.AcceptUrl);
        }
        else
        {
            logger.LogWarning("Invitation for {Recipient} was not emailed because Communications settings are missing", email.To);
        }

        return Task.FromResult(false);
    }
}