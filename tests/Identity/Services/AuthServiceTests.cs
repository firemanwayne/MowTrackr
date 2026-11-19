using MowTrackr.Identity.UnitTests.Fakes;

namespace MowTrackr.Identity.UnitTests.Services;

public sealed class AuthServiceTests
{
    private static readonly DateTimeOffset Now = new(2027, 1, 10, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetRegistrationStatusAsync_NoAccounts_IsOpen()
    {
        using var host = await IdentityTestHost.CreateAsync();

        var result = await host.CreateAuth().GetRegistrationStatusAsync(CancellationToken.None);

        Assert.True(result.Value!.IsOpen);
    }

    [Fact]
    public async Task GetRegistrationStatusAsync_AfterTheFirstAccount_IsClosed()
    {
        using var host = await IdentityTestHost.CreateAsync();

        await host.AddUserAsync("admin@example.com", AppRoles.Admin);

        var result = await host.CreateAuth().GetRegistrationStatusAsync(CancellationToken.None);

        Assert.False(result.Value!.IsOpen);
    }

    [Fact]
    public async Task RegisterAsync_FirstAccount_BecomesAnUnconfirmedAdminAndIsEmailedALink()
    {
        using var host = await IdentityTestHost.CreateAsync();

        var result = await host.CreateAuth().RegisterAsync(Register("first@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.True(result.Value!.ConfirmationEmailSent);

        var user = await host.Users.FindByEmailAsync("first@example.com");

        Assert.False(user!.EmailConfirmed);

        Assert.Contains(AppRoles.Admin, await host.Users.GetRolesAsync(user));

        Assert.StartsWith("https://client.test/confirm-email?userId=", host.ConfirmationEmails.Single().ConfirmUrl, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RegisterAsync_AfterTheFirstAccount_IsForbidden()
    {
        using var host = await IdentityTestHost.CreateAsync();

        await host.AddUserAsync("admin@example.com", AppRoles.Admin);

        var result = await host.CreateAuth().RegisterAsync(Register("stranger@example.com"), CancellationToken.None);

        Assert.Equal(ResultStatus.Forbidden, result.Status);

        Assert.Null(await host.Users.FindByEmailAsync("stranger@example.com"));
    }

    [Theory]
    [InlineData("Short1!")]
    [InlineData("alllowercase-and-long-1!")]
    [InlineData("ALLUPPERCASE-AND-LONG-1!")]
    [InlineData("NoDigitsInThisOne-Here!")]
    [InlineData("NoSymbolsInThis0neHere")]
    [InlineData("Password12345!")]
    [InlineData("Someone-Loves-Mowing-1!")]
    public async Task RegisterAsync_PasswordBreaksThePolicy_IsRejectedAndNothingIsCreated(string password)
    {
        using var host = await IdentityTestHost.CreateAsync();

        var request = Register("first@example.com");

        request.Password = password;

        var result = await host.CreateAuth().RegisterAsync(request, CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);

        Assert.Null(await host.Users.FindByEmailAsync("first@example.com"));
    }

    [Fact]
    public async Task LoginAsync_ConfirmedAccount_SignsIn()
    {
        using var host = await IdentityTestHost.CreateAsync();

        await host.AddUserAsync("admin@example.com", AppRoles.Admin);

        var result = await host.CreateAuth().LoginAsync(new LoginRequest { Email = "admin@example.com", Password = IdentityTestHost.StrongPassword }, CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.Equal("jwt-token", result.Value!.Token);
    }

    [Fact]
    public async Task LoginAsync_UnconfirmedAccountWithTheRightPassword_IsForbidden()
    {
        using var host = await IdentityTestHost.CreateAsync();

        await host.AddUserAsync("admin@example.com", AppRoles.Admin, emailConfirmed: false);

        var result = await host.CreateAuth().LoginAsync(new LoginRequest { Email = "admin@example.com", Password = IdentityTestHost.StrongPassword }, CancellationToken.None);

        Assert.Equal(ResultStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task LoginAsync_UnconfirmedAccountWithTheWrongPassword_DoesNotRevealThatItExists()
    {
        using var host = await IdentityTestHost.CreateAsync();

        await host.AddUserAsync("admin@example.com", AppRoles.Admin, emailConfirmed: false);

        var auth = host.CreateAuth();

        var wrongPassword = await auth.LoginAsync(new LoginRequest { Email = "admin@example.com", Password = "Not-The-Password-1!" }, CancellationToken.None);

        var unknownEmail = await auth.LoginAsync(new LoginRequest { Email = "nobody@example.com", Password = "Not-The-Password-1!" }, CancellationToken.None);

        Assert.Equal(ResultStatus.Unauthorized, wrongPassword.Status);

        Assert.Equal(ResultStatus.Unauthorized, unknownEmail.Status);

        Assert.Equal(unknownEmail.Error, wrongPassword.Error);
    }

    [Fact]
    public async Task LoginAsync_AfterTooManyFailures_LockedOutWithTheSameMessage()
    {
        using var host = await IdentityTestHost.CreateAsync();

        await host.AddUserAsync("admin@example.com", AppRoles.Admin);

        var auth = host.CreateAuth();

        var wrong = new LoginRequest { Email = "admin@example.com", Password = "Not-The-Password-1!" };

        LoginRequest right = new() { Email = "admin@example.com", Password = IdentityTestHost.StrongPassword };

        Result<AuthResponse> first = await auth.LoginAsync(wrong, CancellationToken.None);

        for (var i = 0; i < 5; i++)
        {
            await auth.LoginAsync(wrong, CancellationToken.None);
        }

        var locked = await auth.LoginAsync(right, CancellationToken.None);

        Assert.Equal(ResultStatus.Unauthorized, locked.Status);

        Assert.Equal(first.Error, locked.Error);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ValidLink_ConfirmsTheAccountAndAllowsSignIn()
    {
        using var host = await IdentityTestHost.CreateAsync();

        var auth = host.CreateAuth();

        await auth.RegisterAsync(Register("first@example.com"), CancellationToken.None);

        var (userId, token) = ParseLink(host.ConfirmationEmails.Single().ConfirmUrl);

        var confirmed = await auth.ConfirmEmailAsync(new ConfirmEmailRequest { UserId = userId, Token = token }, CancellationToken.None);

        Assert.True(confirmed.IsSuccess);

        var login = await auth.LoginAsync(new LoginRequest { Email = "first@example.com", Password = IdentityTestHost.StrongPassword }, CancellationToken.None);

        Assert.True(login.IsSuccess);
    }

    [Fact]
    public async Task ConfirmEmailAsync_TamperedOrWrongUserLink_IsRejected()
    {
        using var host = await IdentityTestHost.CreateAsync();

        var auth = host.CreateAuth();

        await auth.RegisterAsync(Register("first@example.com"), CancellationToken.None);

        var (userId, token) = ParseLink(host.ConfirmationEmails.Single().ConfirmUrl);

        var tampered = await auth.ConfirmEmailAsync(new ConfirmEmailRequest { UserId = userId, Token = token[..^2] + "AA" }, CancellationToken.None);

        var wrongUser = await auth.ConfirmEmailAsync(new ConfirmEmailRequest { UserId = Guid.NewGuid(), Token = token }, CancellationToken.None);

        var garbage = await auth.ConfirmEmailAsync(new ConfirmEmailRequest { UserId = userId, Token = "***" }, CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, tampered.Status);

        Assert.Equal(ResultStatus.Invalid, wrongUser.Status);

        Assert.Equal(ResultStatus.Invalid, garbage.Status);

        Assert.False((await host.Users.FindByEmailAsync("first@example.com"))!.EmailConfirmed);
    }

    [Fact]
    public async Task ResendConfirmationAsync_UnconfirmedAccount_SendsANewLink()
    {
        using var host = await IdentityTestHost.CreateAsync();

        await host.AddUserAsync("admin@example.com", AppRoles.Admin, emailConfirmed: false);

        var result = await host.CreateAuth().ResendConfirmationAsync(new ResendConfirmationRequest { Email = "admin@example.com" }, CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.Single(host.ConfirmationEmails);
    }

    [Fact]
    public async Task ResendConfirmationAsync_UnknownOrConfirmedAccount_AnswersTheSameAndSendsNothing()
    {
        using var host = await IdentityTestHost.CreateAsync();

        await host.AddUserAsync("admin@example.com", AppRoles.Admin);

        var auth = host.CreateAuth();

        var confirmed = await auth.ResendConfirmationAsync(new ResendConfirmationRequest { Email = "admin@example.com" }, CancellationToken.None);

        var unknown = await auth.ResendConfirmationAsync(new ResendConfirmationRequest { Email = "nobody@example.com" }, CancellationToken.None);

        Assert.True(confirmed.IsSuccess);

        Assert.True(unknown.IsSuccess);

        Assert.Equal(confirmed.Value!.Message, unknown.Value!.Message);

        Assert.Empty(host.ConfirmationEmails);
    }

    [Fact]
    public async Task ExternalLoginAsync_FirstEverAccount_BecomesAdminWithoutAnInvitation()
    {
        using var host = await IdentityTestHost.CreateAsync();

        var result = await host.CreateAuth().ExternalLoginAsync(External("first@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.Contains(AppRoles.Admin, result.Value!.User.Roles);
    }

    [Fact]
    public async Task ExternalLoginAsync_NewUserWithoutInvitation_IsRefused()
    {
        using var host = await IdentityTestHost.CreateAsync();

        await host.AddUserAsync("admin@example.com", AppRoles.Admin);

        var result = await host.CreateAuth().ExternalLoginAsync(External("stranger@example.com"), CancellationToken.None);

        Assert.False(result.IsSuccess);

        Assert.Contains("invitation", result.Error, StringComparison.OrdinalIgnoreCase);

        Assert.Null(await host.Users.FindByEmailAsync("stranger@example.com"));
    }

    [Fact]
    public async Task ExternalLoginAsync_NewUserWithPendingInvitation_GetsTheInvitedRoleAndAcceptsIt()
    {
        using var host = await IdentityTestHost.CreateAsync();

        await host.AddUserAsync("admin@example.com", AppRoles.Admin);

        var invitation = host.AddInvitation("pat@example.com", AppRoles.Manager);

        var result = await host.CreateAuth().ExternalLoginAsync(External("PAT@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.Equal([AppRoles.Manager], result.Value!.User.Roles);

        Assert.Equal(InvitationStatus.Accepted, invitation.GetStatus(Now));
    }

    [Fact]
    public async Task ExternalLoginAsync_RevokedInvitation_IsRefused()
    {
        using var host = await IdentityTestHost.CreateAsync();

        await host.AddUserAsync("admin@example.com", AppRoles.Admin);

        var invitation = host.AddInvitation("pat@example.com", AppRoles.Manager);

        invitation.RevokedAt = Now;

        var result = await host.CreateAuth().ExternalLoginAsync(External("pat@example.com"), CancellationToken.None);

        Assert.False(result.IsSuccess);

        Assert.Null(await host.Users.FindByEmailAsync("pat@example.com"));
    }

    [Fact]
    public async Task ExternalLoginAsync_ExpiredInvitation_IsRefused()
    {
        using var host = await IdentityTestHost.CreateAsync();

        await host.AddUserAsync("admin@example.com", AppRoles.Admin);

        var invitation = host.AddInvitation("pat@example.com", AppRoles.Technician);

        invitation.ExpiresAt = Now.AddMinutes(-1);

        var result = await host.CreateAuth().ExternalLoginAsync(External("pat@example.com"), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExternalLoginAsync_ExistingAccountWithAnUnconfirmedEmail_IsNotTakenOver()
    {
        using var host = await IdentityTestHost.CreateAsync();

        await host.AddUserAsync("owner@example.com", AppRoles.Admin, emailConfirmed: false);

        var result = await host.CreateAuth().ExternalLoginAsync(External("owner@example.com"), CancellationToken.None);

        Assert.False(result.IsSuccess);

        Assert.Empty(await host.Users.GetLoginsAsync((await host.Users.FindByEmailAsync("owner@example.com"))!));
    }

    [Fact]
    public async Task ExternalLoginAsync_ExistingConfirmedAccount_LinksTheProvider()
    {
        using var host = await IdentityTestHost.CreateAsync();

        await host.AddUserAsync("owner@example.com", AppRoles.Admin);

        var result = await host.CreateAuth().ExternalLoginAsync(External("owner@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    private static RegisterRequest Register(string email)
    {
        return new RegisterRequest { Email = email, Password = IdentityTestHost.StrongPassword, DisplayName = "Someone" };
    }

    private static ExternalLoginCommand External(string email)
    {
        return new ExternalLoginCommand("Google", $"google-{email}", email, "Someone");
    }

    private static (Guid UserId, string Token) ParseLink(string url)
    {
        var query = new Uri(url).Query.TrimStart('?').Split('&').Select(p => p.Split('=', 2)).ToDictionary(p => p[0], p => Uri.UnescapeDataString(p[1]));

        return (Guid.Parse(query["userId"]), query["token"]);
    }
}