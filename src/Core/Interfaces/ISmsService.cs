namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Sends text messages (through Azure Communication Services in Azure, or the log locally).
/// </summary>
public interface ISmsService
{
    /// <summary>Gets a value indicating whether real text messages can be sent.</summary>
    bool IsConfigured { get; }

    /// <summary>Texts a verification code.</summary>
    /// <param name="message">The recipient and code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> if the SMS service accepted the message.</returns>
    Task<bool> SendVerificationCodeAsync(SmsVerification message, CancellationToken cancellationToken);
}