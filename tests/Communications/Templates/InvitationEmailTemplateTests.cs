using MowTrackr.Communications.Templates;

namespace MowTrackr.Communications.UnitTests.Templates;

public sealed class InvitationEmailTemplateTests
{
    private const string Url = "https://client.test/accept-invitation?token=abc_DEF-123";

    private static readonly DateTimeOffset Expires = new(2027, 1, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Build_English_NamesTheInviterAndLinksToAcceptance()
    {
        var (subject, plain, html) = InvitationEmailTemplate.Build(new InvitationEmail("pat@example.com", "Wayne", Url, Expires, "en"));

        Assert.Equal("Wayne invited you to MowTrackr", subject);

        Assert.Contains(Url, plain, StringComparison.Ordinal);

        Assert.Contains($"href=\"{Url}\"", html, StringComparison.Ordinal);

        Assert.Contains("Accept invitation", html, StringComparison.Ordinal);

        Assert.Contains("January 17, 2027", plain, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_Spanish_UsesSpanishWording()
    {
        var (subject, plain, html) = InvitationEmailTemplate.Build(new InvitationEmail("pat@example.com", "Wayne", Url, Expires, "es"));

        Assert.Equal("Wayne te invitó a MowTrackr", subject);

        Assert.Contains("Aceptar invitación", html, StringComparison.Ordinal);

        Assert.Contains("lang=\"es\"", html, StringComparison.Ordinal);

        Assert.Contains("vence el", plain, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_InviterNameWithMarkup_IsHtmlEncoded()
    {
        var (_, _, html) = InvitationEmailTemplate.Build(new InvitationEmail("pat@example.com", "<script>alert(1)</script>", Url, Expires, "en"));

        Assert.DoesNotContain("<script>", html, StringComparison.Ordinal);

        Assert.Contains("&lt;script&gt;", html, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_UnknownLanguage_FallsBackToEnglish()
    {
        var (subject, _, _) = InvitationEmailTemplate.Build(new InvitationEmail("pat@example.com", "Wayne", Url, Expires, "fr"));

        Assert.Equal("Wayne invited you to MowTrackr", subject);
    }
}