using System.Net;

namespace MowTrackr.Communications.Templates;

/// <summary>
/// The shared look of MowTrackr emails: a branded header, one paragraph, one button and a small print line.
/// </summary>
internal static class EmailLayout
{
    private const string AppName = "MowTrackr";

    private const string InkColor = "#14181a";

    private const string AccentColor = "#6cc24a";

    private const string MutedColor = "#5d6a61";

    /// <summary>Renders the HTML body.</summary>
    /// <param name="content">What to show.</param>
    /// <returns>The HTML document.</returns>
    public static string Render(EmailLayoutContent content)
    {
        var url = WebUtility.HtmlEncode(content.ActionUrl);

        return $"""
            <!DOCTYPE html>
            <html lang="{(content.Spanish ? "es" : "en")}">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1.0">
              <title>{WebUtility.HtmlEncode(content.Subject)}</title>
            </head>
            <body style="margin:0;padding:24px;background:#f3f5f1;font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:{InkColor};line-height:1.55;">
              <div style="max-width:560px;margin:0 auto;background:#ffffff;border:1px solid #dfe4da;border-radius:10px;overflow:hidden;">
                <div style="background:{InkColor};padding:20px 28px;">
                  <span style="color:{AccentColor};font-size:22px;font-weight:600;letter-spacing:-0.02em;">{AppName}</span>
                </div>
                <div style="padding:28px;">
                  <p style="margin:0 0 20px;font-size:16px;">{WebUtility.HtmlEncode(content.Intro)}</p>
                  <p style="margin:0 0 24px;">
                    <a href="{url}" style="display:inline-block;background:{AccentColor};color:{InkColor};text-decoration:none;font-weight:600;padding:13px 26px;border-radius:6px;">{WebUtility.HtmlEncode(content.ActionLabel)}</a>
                  </p>
                  <p style="margin:0 0 6px;font-size:13px;color:{MutedColor};">{WebUtility.HtmlEncode(content.LinkHelp)}</p>
                  <p style="margin:0 0 24px;font-size:13px;word-break:break-all;"><a href="{url}" style="color:{InkColor};">{url}</a></p>
                  <hr style="border:none;border-top:1px solid #dfe4da;margin:0 0 16px;">
                  <p style="margin:0;font-size:12px;color:{MutedColor};">{WebUtility.HtmlEncode(content.Footer)}</p>
                </div>
              </div>
            </body>
            </html>
            """;
    }
}