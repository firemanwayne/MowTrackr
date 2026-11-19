using Azure;
using Azure.Communication.Email;
using MowTrackr.Communications.Options;
using MowTrackr.Communications.Templates;

namespace MowTrackr.Communications.Services;

/// <summary>
/// Sends email through Azure Communication Services.
/// </summary>
/// <param name="options">The Communication Services settings.</param>
/// <param name="logger">The logger.</param>
internal sealed class AzureEmailService(IOptions<CommunicationsOptions> options, ILogger<AzureEmailService> logger) : IEmailService
{
    private readonly CommunicationsOptions _options = options.Value;

    private readonly EmailClient _client = new(options.Value.ConnectionString);

    /// <inheritdoc />
    public Task<bool> SendInvitationAsync(InvitationEmail email, CancellationToken cancellationToken)
    {
        return SendAsync(email.To, InvitationEmailTemplate.Build(email), "Invitation", cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> SendEmailConfirmationAsync(EmailConfirmationEmail email, CancellationToken cancellationToken)
    {
        return SendAsync(email.To, ConfirmationEmailTemplate.Build(email), "Confirmation", cancellationToken);
    }

    /// <summary>Sends one message and reports whether the mail service accepted it.</summary>
    /// <param name="to">The recipient address.</param>
    /// <param name="message">The subject and bodies.</param>
    /// <param name="kind">What kind of email this is, for the log.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> if the message was accepted.</returns>
    private async Task<bool> SendAsync(string to, (string Subject, string PlainText, string Html) message, string kind, CancellationToken cancellationToken)
    {
        var content = new EmailContent(message.Subject)
        {
            PlainText = message.PlainText,
            Html = message.Html,
        };

        var emailMessage = new EmailMessage(_options.SenderAddress, to, content);

        var operation = await _client.SendAsync(WaitUntil.Completed, emailMessage, cancellationToken).ConfigureAwait(false);

        var succeeded = operation.HasCompleted && operation.Value.Status == EmailSendStatus.Succeeded;

        if (succeeded)
        {
            logger.LogInformation("{Kind} email sent (operation {OperationId})", kind, operation.Id);
        }
        else
        {
            logger.LogWarning("{Kind} email was not sent (operation {OperationId}, status {Status})", kind, operation.Id, operation.Value.Status);
        }

        return succeeded;
    }
}