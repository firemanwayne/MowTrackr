namespace MowTrackr.Core.UnitTests.Constants;

public sealed class PasswordPolicyTests
{
    private static readonly string[] Info = ["pat.lee@example.com", "Pat Lee"];

    [Theory]
    [InlineData("Tr1cky-Meadow-Gate!")]
    [InlineData("correct Horse 7 battery Staple")]
    [InlineData("Xk9#vLq2$wZp8@mN")]
    public void Failures_StrongPassword_HasNone(string password)
    {
        Assert.Empty(PasswordPolicy.Failures(password, Info));

        Assert.True(PasswordPolicy.IsAcceptable(password, Info));
    }

    [Fact]
    public void Failures_TooShort_ReportsMinimumLength()
    {
        Assert.Contains(PasswordRule.MinimumLength, PasswordPolicy.Failures("Ab1!xyz", Info));
    }

    [Fact]
    public void Failures_ExactlyTheMinimumLength_PassesTheLengthRule()
    {
        var password = "Aa1!" + new string('x', PasswordPolicy.MinLength - 4);

        Assert.DoesNotContain(PasswordRule.MinimumLength, PasswordPolicy.Failures(password, Info));
    }

    [Fact]
    public void Failures_LongerThanTheMaximum_ReportsMaximumLength()
    {
        var password = "Aa1!" + new string('x', PasswordPolicy.MaxLength);

        Assert.Contains(PasswordRule.MaximumLength, PasswordPolicy.Failures(password, Info));
    }

    [Theory]
    [InlineData("alllowercase-and-long-1!", PasswordRule.Uppercase)]
    [InlineData("ALLUPPERCASE-AND-LONG-1!", PasswordRule.Lowercase)]
    [InlineData("NoDigitsInThisOne-Here!", PasswordRule.Digit)]
    [InlineData("NoSymbolsInThis0neHere", PasswordRule.Symbol)]
    public void Failures_MissingACharacterClass_ReportsThatRule(string password, PasswordRule expected)
    {
        Assert.Contains(expected, PasswordPolicy.Failures(password, Info));
    }

    [Theory]
    [InlineData("Password12345!")]
    [InlineData("P@ssword-1234567")]
    [InlineData("Qwerty-123456789!")]
    [InlineData("MowTrackr2027!!")]
    [InlineData("Passw0rd!2027")]
    [InlineData("aaaaAAAA1111!!!!")]
    public void Failures_CommonOrGuessablePassword_ReportsNotCommon(string password)
    {
        Assert.Contains(PasswordRule.NotCommon, PasswordPolicy.Failures(password, Info));
    }

    [Theory]
    [InlineData("pat.lee@example.com", "Pat-Loves-Mowing-99!")]
    [InlineData("pat.lee@example.com", "My-Name-Is-Lee-2027!")]
    [InlineData("someone@example.com", "Fancy-Someone-Rides-7!")]
    public void Failures_ContainsTheUsersOwnDetails_ReportsNoPersonalInfo(string email, string password)
    {
        Assert.Contains(PasswordRule.NoPersonalInfo, PasswordPolicy.Failures(password, [email, "Pat Lee"]));
    }

    [Fact]
    public void Failures_ShortNameFragments_AreNotTreatedAsPersonalInfo()
    {
        Assert.DoesNotContain(PasswordRule.NoPersonalInfo, PasswordPolicy.Failures("Tr1cky-Meadow-Gate!", ["jo@example.com", "Jo Bo"]));
    }

    [Fact]
    public void Failures_NullOrEmpty_FailsWithoutThrowing()
    {
        Assert.NotEmpty(PasswordPolicy.Failures(null));

        Assert.NotEmpty(PasswordPolicy.Failures(string.Empty));
    }

    [Fact]
    public void Failures_NoPersonalInfoSupplied_StillAppliesEveryOtherRule()
    {
        Assert.Empty(PasswordPolicy.Failures("Tr1cky-Meadow-Gate!"));

        Assert.Contains(PasswordRule.Digit, PasswordPolicy.Failures("NoDigitsInThisOne-Here!"));
    }
}