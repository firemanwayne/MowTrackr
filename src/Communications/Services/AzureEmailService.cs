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
    public async Task<bool> SendInvitationAsync(InvitationEmail email, CancellationToken cancellationToken)
    {
        var (subject, plainText, html) = InvitationEmailTemplate.Build(email);

        var content = new EmailContent(subject)
        {
            PlainText = plainText,
            Html = html,
        };

        var message = new EmailMessage(_options.SenderAddress, email.To, content);

        var operation = await _client.SendAsync(WaitUntil.Completed, message, cancellationToken).ConfigureAwait(false);

        var succeeded = operation.HasCompleted && operation.Value.Status == EmailSendStatus.Succeeded;

        if (succeeded)
        {
            logger.LogInformation("Invitation email sent (operation {OperationId})", operation.Id);
        }
        else
        {
            logger.LogWarning("Invitation email was not sent (operation {OperationId}, status {Status})", operation.Id, operation.Value.Status);
        }

        return succeeded;
    }
}