using MowTrackr.Core.Constants;

namespace MowTrackr.Communications.Templates;

/// <summary>
/// Builds the verification text message in English or Spanish.
/// </summary>
internal static class SmsMessageTemplate
{
    /// <summary>Builds the message text.</summary>
    /// <param name="message">The recipient and code.</param>
    /// <returns>The text to send.</returns>
    public static string Build(SmsVerification message)
    {
        var spanish = string.Equals(message.Language, AppLanguages.Spanish, StringComparison.OrdinalIgnoreCase);

        return spanish
            ? $"Tu código de MowTrackr es {message.Code}. Vence en unos minutos. No lo compartas con nadie."
            : $"Your MowTrackr code is {message.Code}. It expires in a few minutes. Do not share it with anyone.";
    }
}