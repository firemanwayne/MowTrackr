using Azure.Communication.Sms;
using MowTrackr.Communications.Options;
using MowTrackr.Communications.Templates;

namespace MowTrackr.Communications.Services;

/// <summary>
/// Sends text messages through Azure Communication Services SMS.
/// </summary>
/// <param name="options">The Communication Services settings.</param>
/// <param name="logger">The logger.</param>
internal sealed class AzureSmsService(IOptions<CommunicationsOptions> options, ILogger<AzureSmsService> logger) : ISmsService
{
    private readonly CommunicationsOptions _options = options.Value;

    private readonly SmsClient _client = new(options.Value.ConnectionString);

    /// <inheritdoc />
    public bool IsConfigured => true;

    /// <inheritdoc />
    public async Task<bool> SendVerificationCodeAsync(SmsVerification message, CancellationToken cancellationToken)
    {
        var response = await _client.SendAsync(_options.SmsSenderNumber, message.To, SmsMessageTemplate.Build(message), new SmsSendOptions(enableDeliveryReport: false), cancellationToken).ConfigureAwait(false);

        var succeeded = response.Value.Successful;

        if (succeeded)
        {
            logger.LogInformation("Verification text sent (message {MessageId})", response.Value.MessageId);
        }
        else
        {
            logger.LogWarning("Verification text was not sent (status {Status})", response.Value.HttpStatusCode);
        }

        return succeeded;
    }
}