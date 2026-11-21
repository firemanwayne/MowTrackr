using MowTrackr.Communications.Templates;

namespace MowTrackr.Communications.UnitTests.Templates;

public sealed class ConfirmationEmailTemplateTests
{
    private const string Url = "https://client.test/confirm-email?userId=abc&token=xyz_123";

    [Fact]
    public void Build_English_GreetsByNameAndLinksToConfirmation()
    {
        var (subject, plain, html) = ConfirmationEmailTemplate.Build(new EmailConfirmationEmail("pat@example.com", "Pat", Url, "en"));

        Assert.Equal("Confirm your email address for MowTrackr", subject);

        Assert.Contains("Hi Pat", plain, StringComparison.Ordinal);

        Assert.Contains(Url, plain, StringComparison.Ordinal);

        Assert.Contains("Confirm email address", html, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_Spanish_UsesSpanishWording()
    {
        var (subject, _, html) = ConfirmationEmailTemplate.Build(new EmailConfirmationEmail("pat@example.com", "Pat", Url, "es"));

        Assert.Equal("Confirma tu correo para MowTrackr", subject);

        Assert.Contains("Confirmar correo", html, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_NameWithMarkup_IsHtmlEncoded()
    {
        var (_, _, html) = ConfirmationEmailTemplate.Build(new EmailConfirmationEmail("pat@example.com", "<img src=x onerror=alert(1)>", Url, "en"));

        Assert.DoesNotContain("<img", html, StringComparison.Ordinal);
    }
}