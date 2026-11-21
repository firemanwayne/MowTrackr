using Microsoft.Extensions.Hosting;

namespace MowTrackr.Communications.Services;

/// <summary>
/// Stand-in used when SMS is not configured. It sends nothing; in Development it logs the code so phone verification
/// can be tried locally. Elsewhere the code (a secret) is never written to the log.
/// </summary>
/// <param name="environment">The host environment.</param>
/// <param name="logger">The logger.</param>
internal sealed class LoggingSmsService(IHostEnvironment environment, ILogger<LoggingSmsService> logger) : ISmsService
{
    /// <inheritdoc />
    public bool IsConfigured => false;

    /// <inheritdoc />
    public Task<bool> SendVerificationCodeAsync(SmsVerification message, CancellationToken cancellationToken)
    {
        if (environment.IsDevelopment())
        {
            logger.LogInformation("[Local] Verification text for {Recipient} not sent; code: {Code}", message.To, message.Code);
        }
        else
        {
            logger.LogWarning("A verification text for {Recipient} was not sent because the SMS settings are missing", message.To);
        }

        return Task.FromResult(false);
    }
}