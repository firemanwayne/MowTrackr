using MowTrackr.Identity.Interfaces;
using MowTrackr.Identity.Services;
using MowTrackr.Identity.UnitTests.Fakes;

namespace MowTrackr.Identity.UnitTests.Services;

public sealed class AuthServiceTests
{
    private static readonly DateTimeOffset Now = new(2027, 1, 10, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetRegistrationStatusAsync_NoAccounts_IsOpen()
    {
        using var harness = await Harness.CreateAsync();

        var result = await harness.Auth.GetRegistrationStatusAsync(CancellationToken.None);

        Assert.True(result.Value!.IsOpen);
    }

    [Fact]
    public async Task GetRegistrationStatusAsync_AfterTheFirstAccount_IsClosed()
    {
        using var harness = await Harness.CreateAsync();

        await harness.AddUserAsync("admin@example.com", AppRoles.Admin);

        var result = await harness.Auth.GetRegistrationStatusAsync(CancellationToken.None);

        Assert.False(result.Value!.IsOpen);
    }

    [Fact]
    public async Task RegisterAsync_FirstAccount_BecomesAdmin()
    {
        using var harness = await Harness.CreateAsync();

        var result = await harness.Auth.RegisterAsync(Register("first@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.Contains(AppRoles.Admin, result.Value!.User.Roles);
    }

    [Fact]
    public async Task RegisterAsync_AfterTheFirstAccount_IsForbidden()
    {
        using var harness = await Harness.CreateAsync();

        await harness.AddUserAsync("admin@example.com", AppRoles.Admin);

        var result = await harness.Auth.RegisterAsync(Register("stranger@example.com"), CancellationToken.None);

        Assert.Equal(ResultStatus.Forbidden, result.Status);

        Assert.Null(await harness.Users.FindByEmailAsync("stranger@example.com"));
    }

    [Fact]
    public async Task ExternalLoginAsync_FirstEverAccount_BecomesAdminWithoutAnInvitation()
    {
        using var harness = await Harness.CreateAsync();

        var result = await harness.Auth.ExternalLoginAsync(External("first@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.Contains(AppRoles.Admin, result.Value!.User.Roles);
    }

    [Fact]
    public async Task ExternalLoginAsync_NewUserWithoutInvitation_IsRefused()
    {
        using var harness = await Harness.CreateAsync();

        await harness.AddUserAsync("admin@example.com", AppRoles.Admin);

        var result = await harness.Auth.ExternalLoginAsync(External("stranger@example.com"), CancellationToken.None);

        Assert.False(result.IsSuccess);

        Assert.Contains("invitation", result.Error, StringComparison.OrdinalIgnoreCase);

        Assert.Null(await harness.Users.FindByEmailAsync("stranger@example.com"));
    }

    [Fact]
    public async Task ExternalLoginAsync_NewUserWithPendingInvitation_GetsTheInvitedRoleAndAcceptsIt()
    {
        using var harness = await Harness.CreateAsync();

        await harness.AddUserAsync("admin@example.com", AppRoles.Admin);

        var invitation = harness.AddInvitation("pat@example.com", AppRoles.Manager);

        var result = await harness.Auth.ExternalLoginAsync(External("PAT@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.Equal([AppRoles.Manager], result.Value!.User.Roles);

        Assert.Equal(InvitationStatus.Accepted, invitation.GetStatus(Now));
    }

    [Fact]
    public async Task ExternalLoginAsync_RevokedInvitation_IsRefused()
    {
        using var harness = await Harness.CreateAsync();

        await harness.AddUserAsync("admin@example.com", AppRoles.Admin);

        var invitation = harness.AddInvitation("pat@example.com", AppRoles.Manager);

        invitation.RevokedAt = Now;

        var result = await harness.Auth.ExternalLoginAsync(External("pat@example.com"), CancellationToken.None);

        Assert.False(result.IsSuccess);

        Assert.Null(await harness.Users.FindByEmailAsync("pat@example.com"));
    }

    [Fact]
    public async Task ExternalLoginAsync_ExpiredInvitation_IsRefused()
    {
        using var harness = await Harness.CreateAsync();

        await harness.AddUserAsync("admin@example.com", AppRoles.Admin);

        var invitation = harness.AddInvitation("pat@example.com", AppRoles.Technician);

        invitation.ExpiresAt = Now.AddMinutes(-1);

        var result = await harness.Auth.ExternalLoginAsync(External("pat@example.com"), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    private static RegisterRequest Register(string email)
    {
        return new RegisterRequest { Email = email, Password = "Sup3rSecret!", DisplayName = "Someone" };
    }

    private static ExternalLoginCommand External(string email)
    {
        return new ExternalLoginCommand("Google", $"google-{email}", email, "Someone");
    }

    /// <summary>A real <see cref="UserManager{TUser}"/> over the in-memory provider, with the roles seeded.</summary>
    private sealed class Harness : IDisposable
    {
        private readonly ServiceProvider _provider;

        private Harness(ServiceProvider provider)
        {
            _provider = provider;

            Users = provider.GetRequiredService<UserManager<ApplicationUser>>();

            var tokens = Substitute.For<ITokenService>();

            tokens.CreateToken(Arg.Any<ApplicationUser>(), Arg.Any<IReadOnlyList<string>>()).Returns(("jwt-token", Now.AddHours(8)));

            Auth = new AuthService(Users, tokens, Invitations, new FakeTimeProvider(Now), NullLogger<AuthService>.Instance);
        }

        public UserManager<ApplicationUser> Users { get; }

        public InMemoryRepository<UserInvitation> Invitations { get; } = new();

        public AuthService Auth { get; }

        public static async Task<Harness> CreateAsync()
        {
            var services = new ServiceCollection();

            services.AddLogging();

            services.AddDbContext<IdentityTestDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            services
                .AddIdentityCore<ApplicationUser>(options => options.User.RequireUniqueEmail = true)
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<IdentityTestDbContext>();

            var provider = services.BuildServiceProvider();

            var roles = provider.GetRequiredService<RoleManager<ApplicationRole>>();

            foreach (var role in AppRoles.All)
            {
                await roles.CreateAsync(new ApplicationRole { Name = role });
            }

            return new Harness(provider);
        }

        public async Task AddUserAsync(string email, string role)
        {
            var user = new ApplicationUser { UserName = email, Email = email, DisplayName = email };

            await Users.CreateAsync(user);

            await Users.AddToRoleAsync(user, role);
        }

        public UserInvitation AddInvitation(string email, string role)
        {
            var invitation = new UserInvitation
            {
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                Role = role,
                TokenHash = Guid.NewGuid().ToString("N"),
                InvitedAt = Now,
                ExpiresAt = Now.AddDays(7),
            };

            Invitations.AddAsync(invitation, CancellationToken.None).GetAwaiter().GetResult();

            return invitation;
        }

        public void Dispose()
        {
            _provider.Dispose();
        }
    }
}