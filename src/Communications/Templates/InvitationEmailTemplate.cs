using System.Globalization;
using MowTrackr.Core.Constants;

namespace MowTrackr.Communications.Templates;

/// <summary>
/// Builds the invitation email in English or Spanish.
/// </summary>
internal static class InvitationEmailTemplate
{
    private const string AppName = "MowTrackr";

    /// <summary>Builds the subject and bodies for an invitation.</summary>
    /// <param name="email">The invitation details.</param>
    /// <returns>The subject, plain-text body and HTML body.</returns>
    public static (string Subject, string PlainText, string Html) Build(InvitationEmail email)
    {
        var spanish = string.Equals(email.Language, AppLanguages.Spanish, StringComparison.OrdinalIgnoreCase);

        var culture = CultureInfo.GetCultureInfo(spanish ? "es-ES" : "en-US");

        var expires = email.ExpiresAt.UtcDateTime.ToString("D", culture);

        var subject = spanish ? $"{email.InviterName} te invitó a {AppName}" : $"{email.InviterName} invited you to {AppName}";

        var intro = spanish ? $"{email.InviterName} te invitó a unirte a {AppName}, para seguir el mantenimiento de cortacéspedes." : $"{email.InviterName} invited you to join {AppName}, to track lawn mower maintenance.";

        var action = spanish ? "Aceptar invitación" : "Accept invitation";

        var expiry = spanish ? $"Esta invitación vence el {expires} (UTC)." : $"This invitation expires on {expires} (UTC).";

        var ignore = spanish ? "Si no la esperabas, puedes ignorar este correo." : "If you were not expecting it, you can ignore this email.";

        var linkHelp = spanish ? "Si el botón no funciona, copia este enlace en tu navegador:" : "If the button does not work, copy this link into your browser:";

        var plain = $"{intro}\n\n{action}: {email.AcceptUrl}\n\n{expiry}\n{ignore}\n";

        var html = EmailLayout.Render(new EmailLayoutContent(subject, spanish, intro, action, email.AcceptUrl, linkHelp, $"{expiry} {ignore}"));

        return (subject, plain, html);
    }
}