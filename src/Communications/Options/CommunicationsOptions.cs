namespace MowTrackr.Communications.Options;

/// <summary>
/// Azure Communication Services email settings, bound from the "Communications" configuration section.
/// When the connection string is empty (local development) emails are logged instead of sent.
/// </summary>
public sealed class CommunicationsOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Communications";

    /// <summary>Gets or sets the Communication Services connection string.</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Gets or sets the verified sender address, e.g. DoNotReply@xxxx.azurecomm.net.</summary>
    public string SenderAddress { get; set; } = string.Empty;

    /// <summary>Gets or sets the Communication Services phone number text messages are sent from, in E.164 format.</summary>
    public string SmsSenderNumber { get; set; } = string.Empty;

    /// <summary>Gets a value indicating whether real email sending is configured.</summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ConnectionString) && !string.IsNullOrWhiteSpace(SenderAddress);

    /// <summary>Gets a value indicating whether real text messaging is configured.</summary>
    public bool IsSmsConfigured => !string.IsNullOrWhiteSpace(ConnectionString) && !string.IsNullOrWhiteSpace(SmsSenderNumber);
}