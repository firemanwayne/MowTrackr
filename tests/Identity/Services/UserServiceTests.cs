using MowTrackr.Identity.UnitTests.Fakes;

namespace MowTrackr.Identity.UnitTests.Services;

public sealed class UserServiceTests
{
    [Fact]
    public async Task StartPhoneVerificationAsync_ValidNumber_TextsACodeToTheNormalizedNumber()
    {
        using var host = await IdentityTestHost.CreateAsync();

        var user = await host.AddUserAsync("pat@example.com", AppRoles.Technician);

        var result = await host.CreateUsers().StartPhoneVerificationAsync(user.Id, new StartPhoneVerificationRequest { PhoneNumber = "+1 (555) 123-4567" }, CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.True(result.Value!.CodeSent);

        Assert.Equal("+15551234567", host.Texts.Single().To);
    }

    [Theory]
    [InlineData("5551234567")]
    [InlineData("+0555123456")]
    [InlineData("+1555")]
    [InlineData("call me maybe")]
    public async Task StartPhoneVerificationAsync_NotInternationalFormat_IsRejectedAndNothingIsSent(string number)
    {
        using var host = await IdentityTestHost.CreateAsync();

        var user = await host.AddUserAsync("pat@example.com", AppRoles.Technician);

        var result = await host.CreateUsers().StartPhoneVerificationAsync(user.Id, new StartPhoneVerificationRequest { PhoneNumber = number }, CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);

        Assert.Empty(host.Texts);
    }

    [Fact]
    public async Task VerifyPhoneAsync_CorrectCode_SavesTheNumberAsVerified()
    {
        using var host = await IdentityTestHost.CreateAsync();

        var user = await host.AddUserAsync("pat@example.com", AppRoles.Technician);

        var users = host.CreateUsers();

        await users.StartPhoneVerificationAsync(user.Id, new StartPhoneVerificationRequest { PhoneNumber = "+15551234567" }, CancellationToken.None);

        var result = await users.VerifyPhoneAsync(user.Id, new VerifyPhoneRequest { PhoneNumber = "+15551234567", Code = host.Texts.Single().Code }, CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.Equal("+15551234567", result.Value!.PhoneNumber);

        Assert.True(result.Value.PhoneNumberConfirmed);
    }

    [Fact]
    public async Task VerifyPhoneAsync_WrongCode_ChangesNothing()
    {
        using var host = await IdentityTestHost.CreateAsync();

        var user = await host.AddUserAsync("pat@example.com", AppRoles.Technician);

        var users = host.CreateUsers();

        await users.StartPhoneVerificationAsync(user.Id, new StartPhoneVerificationRequest { PhoneNumber = "+15551234567" }, CancellationToken.None);

        var wrong = host.Texts.Single().Code == "000000" ? "111111" : "000000";

        var result = await users.VerifyPhoneAsync(user.Id, new VerifyPhoneRequest { PhoneNumber = "+15551234567", Code = wrong }, CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);

        var stored = await host.Users.FindByIdAsync(user.Id.ToString());

        Assert.False(stored!.PhoneNumberConfirmed);

        Assert.Null(stored.PhoneNumber);
    }

    [Fact]
    public async Task VerifyPhoneAsync_CodeForADifferentNumber_IsRejected()
    {
        using var host = await IdentityTestHost.CreateAsync();

        var user = await host.AddUserAsync("pat@example.com", AppRoles.Technician);

        var users = host.CreateUsers();

        await users.StartPhoneVerificationAsync(user.Id, new StartPhoneVerificationRequest { PhoneNumber = "+15551234567" }, CancellationToken.None);

        var result = await users.VerifyPhoneAsync(user.Id, new VerifyPhoneRequest { PhoneNumber = "+15557654321", Code = host.Texts.Single().Code }, CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task UpdateProfileAsync_CannotSetAnUnverifiedPhoneNumber()
    {
        using var host = await IdentityTestHost.CreateAsync();

        var user = await host.AddUserAsync("pat@example.com", AppRoles.Technician);

        var result = await host.CreateUsers().UpdateProfileAsync(user.Id, new UpdateProfileRequest { DisplayName = "Pat", Language = "en", Theme = "Light" }, CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.Null(result.Value!.PhoneNumber);
    }
}