namespace MowTrackr.Communications.Templates;

/// <summary>
/// The wording and link for one <see cref="EmailLayout"/> email.
/// </summary>
/// <param name="Subject">The subject (also the document title).</param>
/// <param name="Spanish">Whether the email is in Spanish.</param>
/// <param name="Intro">The lead paragraph.</param>
/// <param name="ActionLabel">The button text.</param>
/// <param name="ActionUrl">Where the button goes.</param>
/// <param name="LinkHelp">The line above the pasted link.</param>
/// <param name="Footer">The small print.</param>
internal sealed record EmailLayoutContent(string Subject, bool Spanish, string Intro, string ActionLabel, string ActionUrl, string LinkHelp, string Footer);