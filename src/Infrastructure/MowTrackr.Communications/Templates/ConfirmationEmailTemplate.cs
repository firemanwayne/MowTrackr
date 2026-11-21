using MowTrackr.Core.Constants;

namespace MowTrackr.Communications.Templates;

/// <summary>
/// Builds the email-address confirmation email in English or Spanish.
/// </summary>
internal static class ConfirmationEmailTemplate
{
    private const string AppName = "MowTrackr";

    /// <summary>Builds the subject and bodies for a confirmation email.</summary>
    /// <param name="email">The confirmation details.</param>
    /// <returns>The subject, plain-text body and HTML body.</returns>
    public static (string Subject, string PlainText, string Html) Build(EmailConfirmationEmail email)
    {
        var spanish = string.Equals(email.Language, AppLanguages.Spanish, StringComparison.OrdinalIgnoreCase);

        var subject = spanish ? $"Confirma tu correo para {AppName}" : $"Confirm your email address for {AppName}";

        var intro = spanish ? $"Hola {email.DisplayName}, confirma tu correo para terminar de configurar tu cuenta de {AppName}." : $"Hi {email.DisplayName}, confirm your email address to finish setting up your {AppName} account.";

        var action = spanish ? "Confirmar correo" : "Confirm email address";

        var footer = spanish ? "Este enlace funciona durante 24 horas. Si no creaste una cuenta, puedes ignorar este correo." : "This link works for 24 hours. If you did not create an account, you can ignore this email.";

        var linkHelp = spanish ? "Si el botón no funciona, copia este enlace en tu navegador:" : "If the button does not work, copy this link into your browser:";

        var plain = $"{intro}\n\n{action}: {email.ConfirmUrl}\n\n{footer}\n";

        var html = EmailLayout.Render(new EmailLayoutContent(subject, spanish, intro, action, email.ConfirmUrl, linkHelp, footer));

        return (subject, plain, html);
    }
}