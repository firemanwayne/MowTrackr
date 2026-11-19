using System.Globalization;
using System.Net;
using MowTrackr.Core.Constants;

namespace MowTrackr.Communications.Templates;

/// <summary>
/// Builds the invitation email in English or Spanish.
/// </summary>
internal static class InvitationEmailTemplate
{
    private const string AppName = "MowTrackr";

    private const string InkColor = "#14181a";

    private const string AccentColor = "#6cc24a";

    private const string MutedColor = "#5d6a61";

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

        var html = $"""
            <!DOCTYPE html>
            <html lang="{(spanish ? "es" : "en")}">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1.0">
              <title>{WebUtility.HtmlEncode(subject)}</title>
            </head>
            <body style="margin:0;padding:24px;background:#f3f5f1;font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:{InkColor};line-height:1.55;">
              <div style="max-width:560px;margin:0 auto;background:#ffffff;border:1px solid #dfe4da;border-radius:10px;overflow:hidden;">
                <div style="background:{InkColor};padding:20px 28px;">
                  <span style="color:{AccentColor};font-size:22px;font-weight:600;letter-spacing:-0.02em;">{AppName}</span>
                </div>
                <div style="padding:28px;">
                  <p style="margin:0 0 20px;font-size:16px;">{WebUtility.HtmlEncode(intro)}</p>
                  <p style="margin:0 0 24px;">
                    <a href="{WebUtility.HtmlEncode(email.AcceptUrl)}" style="display:inline-block;background:{AccentColor};color:{InkColor};text-decoration:none;font-weight:600;padding:13px 26px;border-radius:6px;">{action}</a>
                  </p>
                  <p style="margin:0 0 6px;font-size:13px;color:{MutedColor};">{linkHelp}</p>
                  <p style="margin:0 0 24px;font-size:13px;word-break:break-all;"><a href="{WebUtility.HtmlEncode(email.AcceptUrl)}" style="color:{InkColor};">{WebUtility.HtmlEncode(email.AcceptUrl)}</a></p>
                  <hr style="border:none;border-top:1px solid #dfe4da;margin:0 0 16px;">
                  <p style="margin:0;font-size:12px;color:{MutedColor};">{expiry} {ignore}</p>
                </div>
              </div>
            </body>
            </html>
            """;

        return (subject, plain, html);
    }
}