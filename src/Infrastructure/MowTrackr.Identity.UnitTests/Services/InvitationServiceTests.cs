using MowTrackr.Core.Options;
using MowTrackr.Identity.Interfaces;
using MowTrackr.Identity.Options;
using MowTrackr.Identity.Services;
using MowTrackr.Identity.UnitTests.Fakes;

namespace MowTrackr.Identity.UnitTests.Services;

public sealed class InvitationServiceTests
{
    private static readonly Guid InviterId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task CreateAsync_UnknownRole_ReturnsInvalid()
    {
        var harness = new Harness();

        var result = await harness.Service.CreateAsync(Request("new@example.com", "Superuser"), InviterId, CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);

        Assert.Empty(harness.Invitations.Items);
    }

    [Fact]
    public async Task CreateAsync_EmailBelongsToExistingUser_ReturnsConflict()
    {
        var harness = new Harness();

        harness.Users.FindByEmailAsync("taken@example.com").Returns(new ApplicationUser { Email = "taken@example.com" });

        var result = await harness.Service.CreateAsync(Request("taken@example.com", AppRoles.Technician), InviterId, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);

        Assert.Empty(harness.Invitations.Items);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_StoresOnlyTheTokenHashAndEmailsTheLink()
    {
        var harness = new Harness();

        var result = await harness.Service.CreateAsync(Request("New@Example.com", AppRoles.Manager), InviterId, CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.True(result.Value!.EmailSent);

        var stored = Assert.Single(harness.Invitations.Items);

        var token = harness.SentTokens.Single();

        Assert.Equal(InvitationService.HashToken(token), stored.TokenHash);

        Assert.DoesNotContain(token, stored.TokenHash, StringComparison.Ordinal);

        Assert.StartsWith("https://client.test/accept-invitation?token=", harness.SentEmails.Single().AcceptUrl, StringComparison.Ordinal);

        Assert.Equal(AppRoles.Manager, stored.Role);

        Assert.Equal("Wayne", stored.InvitedByName);

        Assert.Equal(harness.Time.GetUtcNow().AddDays(7), stored.ExpiresAt);
    }

    [Fact]
    public async Task CreateAsync_PendingInvitationAlreadyExists_ReturnsConflict()
    {
        var harness = new Harness();

        await harness.Service.CreateAsync(Request("new@example.com", AppRoles.Technician), InviterId, CancellationToken.None);

        var second = await harness.Service.CreateAsync(Request("NEW@example.com", AppRoles.Technician), InviterId, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, second.Status);

        Assert.Single(harness.Invitations.Items);
    }

    [Fact]
    public async Task CreateAsync_PreviousInvitationExpired_AllowsANewOne()
    {
        var harness = new Harness();

        await harness.Service.CreateAsync(Request("new@example.com", AppRoles.Technician), InviterId, CancellationToken.None);

        harness.Time.Advance(TimeSpan.FromDays(8));

        var second = await harness.Service.CreateAsync(Request("new@example.com", AppRoles.Technician), InviterId, CancellationToken.None);

        Assert.True(second.IsSuccess);

        Assert.Equal(2, harness.Invitations.Items.Count);
    }

    [Fact]
    public async Task CreateAsync_EmailCannotBeSent_KeepsTheInvitationAndReportsIt()
    {
        var harness = new Harness();

        harness.Email.SendInvitationAsync(Arg.Any<InvitationEmail>(), Arg.Any<CancellationToken>()).Returns<Task<bool>>(_ => throw new InvalidOperationException("mail down"));

        var result = await harness.Service.CreateAsync(Request("new@example.com", AppRoles.Technician), InviterId, CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.False(result.Value!.EmailSent);

        Assert.Single(harness.Invitations.Items);
    }

    [Fact]
    public async Task ResendAsync_OpenInvitation_IssuesANewLinkAndKillsTheOldOne()
    {
        var harness = new Harness();

        await harness.Service.CreateAsync(Request("new@example.com", AppRoles.Technician), InviterId, CancellationToken.None);

        var oldToken = harness.SentTokens.Single();

        harness.Time.Advance(TimeSpan.FromDays(3));

        var resent = await harness.Service.ResendAsync(harness.Invitations.Items.Single().Id, InviterId, CancellationToken.None);

        Assert.True(resent.IsSuccess);

        var newToken = harness.SentTokens.Last();

        Assert.NotEqual(oldToken, newToken);

        Assert.Equal(ResultStatus.NotFound, (await harness.Service.PreviewAsync(oldToken, CancellationToken.None)).Status);

        Assert.True((await harness.Service.PreviewAsync(newToken, CancellationToken.None)).IsSuccess);

        Assert.Equal(harness.Time.GetUtcNow().AddDays(7), harness.Invitations.Items.Single().ExpiresAt);
    }

    [Fact]
    public async Task ResendAsync_RevokedInvitation_ReturnsConflict()
    {
        var harness = new Harness();

        await harness.Service.CreateAsync(Request("new@example.com", AppRoles.Technician), InviterId, CancellationToken.None);

        var id = harness.Invitations.Items.Single().Id;

        await harness.Service.RevokeAsync(id, CancellationToken.None);

        var result = await harness.Service.ResendAsync(id, InviterId, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task RevokeAsync_PendingInvitation_StopsTheLinkWorkingAndIsIdempotent()
    {
        var harness = new Harness();

        await harness.Service.CreateAsync(Request("new@example.com", AppRoles.Technician), InviterId, CancellationToken.None);

        var id = harness.Invitations.Items.Single().Id;

        var first = await harness.Service.RevokeAsync(id, CancellationToken.None);

        var second = await harness.Service.RevokeAsync(id, CancellationToken.None);

        Assert.True(first.IsSuccess);

        Assert.True(second.IsSuccess);

        Assert.Equal(InvitationStatus.Revoked, second.Value!.Status);

        Assert.Equal(ResultStatus.NotFound, (await harness.Service.PreviewAsync(harness.SentTokens.Single(), CancellationToken.None)).Status);
    }

    [Fact]
    public async Task RevokeAsync_UnknownInvitation_ReturnsNotFound()
    {
        var harness = new Harness();

        var result = await harness.Service.RevokeAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task PreviewAsync_UnknownOrBlankToken_ReturnsNotFound()
    {
        var harness = new Harness();

        Assert.Equal(ResultStatus.NotFound, (await harness.Service.PreviewAsync("nope", CancellationToken.None)).Status);

        Assert.Equal(ResultStatus.NotFound, (await harness.Service.PreviewAsync(" ", CancellationToken.None)).Status);
    }

    [Fact]
    public async Task PreviewAsync_ExpiredInvitation_ReturnsNotFound()
    {
        var harness = new Harness();

        await harness.Service.CreateAsync(Request("new@example.com", AppRoles.Technician), InviterId, CancellationToken.None);

        harness.Time.Advance(TimeSpan.FromDays(8));

        var result = await harness.Service.PreviewAsync(harness.SentTokens.Single(), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task AcceptAsync_ValidToken_CreatesTheUserWithTheInvitedRoleAndSignsThemIn()
    {
        var harness = new Harness();

        await harness.Service.CreateAsync(Request("new@example.com", AppRoles.Manager), InviterId, CancellationToken.None);

        var result = await harness.Service.AcceptAsync(Accept(harness.SentTokens.Single()), CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.Equal("jwt-token", result.Value!.Token);

        await harness.Users.Received(1).CreateAsync(Arg.Is<ApplicationUser>(u => u.Email == "new@example.com" && u.EmailConfirmed && u.DisplayName == "Pat Smith"), "Sup3rSecret!");

        await harness.Users.Received(1).AddToRoleAsync(Arg.Any<ApplicationUser>(), AppRoles.Manager);

        Assert.Equal(InvitationStatus.Accepted, harness.Invitations.Items.Single().GetStatus(harness.Time.GetUtcNow()));
    }

    [Fact]
    public async Task AcceptAsync_TokenAlreadyUsed_ReturnsNotFound()
    {
        var harness = new Harness();

        await harness.Service.CreateAsync(Request("new@example.com", AppRoles.Technician), InviterId, CancellationToken.None);

        var token = harness.SentTokens.Single();

        await harness.Service.AcceptAsync(Accept(token), CancellationToken.None);

        var again = await harness.Service.AcceptAsync(Accept(token), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, again.Status);
    }

    [Fact]
    public async Task AcceptAsync_PasswordRejectedByIdentity_ReturnsInvalidAndKeepsTheInvitationOpen()
    {
        var harness = new Harness();

        harness.Users.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Failed(new IdentityError { Description = "Passwords must have at least one digit." }));

        await harness.Service.CreateAsync(Request("new@example.com", AppRoles.Technician), InviterId, CancellationToken.None);

        var result = await harness.Service.AcceptAsync(Accept(harness.SentTokens.Single()), CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);

        Assert.Contains("digit", result.Error, StringComparison.Ordinal);

        Assert.Equal(InvitationStatus.Pending, harness.Invitations.Items.Single().GetStatus(harness.Time.GetUtcNow()));
    }

    [Fact]
    public async Task AcceptAsync_EmailRegisteredMeanwhile_ReturnsConflict()
    {
        var harness = new Harness();

        await harness.Service.CreateAsync(Request("new@example.com", AppRoles.Technician), InviterId, CancellationToken.None);

        harness.Users.FindByEmailAsync("new@example.com").Returns(new ApplicationUser { Email = "new@example.com" });

        var result = await harness.Service.AcceptAsync(Accept(harness.SentTokens.Single()), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
    }

    private static CreateInvitationRequest Request(string email, string role)
    {
        return new CreateInvitationRequest { Email = email, Role = role };
    }

    private static AcceptInvitationRequest Accept(string token)
    {
        return new AcceptInvitationRequest { Token = token, DisplayName = " Pat Smith ", Password = "Sup3rSecret!" };
    }

    /// <summary>Builds the service under test with fakes and records the emails it sends.</summary>
    private sealed class Harness
    {
        private readonly List<InvitationEmail> _sent = [];

        public Harness()
        {
            Users = Substitute.For<UserManager<ApplicationUser>>(Substitute.For<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            Users.NormalizeEmail(Arg.Any<string>()).Returns(call => call.Arg<string>().ToUpperInvariant());

            Users.FindByIdAsync(InviterId.ToString()).Returns(new ApplicationUser { Id = InviterId, DisplayName = "Wayne", Email = "wayne@example.com" });

            Users.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);

            Users.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);

            Users.GetRolesAsync(Arg.Any<ApplicationUser>()).Returns(call => (IList<string>)new List<string> { AppRoles.Technician });

            Tokens = Substitute.For<ITokenService>();

            Tokens.CreateToken(Arg.Any<ApplicationUser>(), Arg.Any<IReadOnlyList<string>>()).Returns(("jwt-token", Time.GetUtcNow().AddHours(8)));

            Email = Substitute.For<IEmailService>();

            Email.SendInvitationAsync(Arg.Do<InvitationEmail>(_sent.Add), Arg.Any<CancellationToken>()).Returns(true);

            Service = new InvitationService(
                Invitations,
                Users,
                Tokens,
                Email,
                new OptionsWrapper<ClientOptions>(new ClientOptions { BaseUrl = "https://client.test/" }),
                new OptionsWrapper<InvitationOptions>(new InvitationOptions()),
                Time,
                NullLogger<InvitationService>.Instance);
        }

        public InMemoryRepository<UserInvitation> Invitations { get; } = new();

        public UserManager<ApplicationUser> Users { get; }

        public ITokenService Tokens { get; }

        public IEmailService Email { get; }

        public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2027, 1, 10, 12, 0, 0, TimeSpan.Zero));

        public InvitationService Service { get; }

        public IReadOnlyList<InvitationEmail> SentEmails => _sent;

        public IReadOnlyList<string> SentTokens => _sent.Select(e => Uri.UnescapeDataString(e.AcceptUrl[(e.AcceptUrl.IndexOf("token=", StringComparison.Ordinal) + "token=".Length)..])).ToList();
    }
}