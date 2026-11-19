using MowTrackr.Communications.Templates;

namespace MowTrackr.Communications.UnitTests.Templates;

public sealed class SmsMessageTemplateTests
{
    [Fact]
    public void Build_English_ContainsTheCodeAndWarnsNotToShareIt()
    {
        var text = SmsMessageTemplate.Build(new SmsVerification("+15551234567", "482913", "en"));

        Assert.Contains("482913", text, StringComparison.Ordinal);

        Assert.Contains("Do not share", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_Spanish_UsesSpanishWording()
    {
        var text = SmsMessageTemplate.Build(new SmsVerification("+15551234567", "482913", "es"));

        Assert.Contains("482913", text, StringComparison.Ordinal);

        Assert.Contains("No lo compartas", text, StringComparison.Ordinal);
    }
}