using MowTrackr.Core.Options;
using MowTrackr.Identity.Interfaces;
using MowTrackr.Identity.Services;

namespace MowTrackr.Identity.UnitTests.Fakes;

/// <summary>
/// A test host with the real Identity configuration (password policy, token providers, lockout) over the in-memory
/// provider, and substitutes for the token, email and SMS services. Records what was sent.
/// </summary>
internal sealed class IdentityTestHost : IDisposable
{
    public const string StrongPassword = "Tr1cky-Meadow-Gate!";

    private static readonly DateTimeOffset Start = new(2027, 1, 10, 12, 0, 0, TimeSpan.Zero);

    private readonly ServiceProvider _provider;

    private IdentityTestHost(ServiceProvider provider)
    {
        _provider = provider;

        Users = provider.GetRequiredService<UserManager<ApplicationUser>>();

        Tokens.CreateToken(Arg.Any<ApplicationUser>(), Arg.Any<IReadOnlyList<string>>()).Returns(("jwt-token", Start.AddHours(8)));

        Email.SendEmailConfirmationAsync(Arg.Do<EmailConfirmationEmail>(ConfirmationEmails.Add), Arg.Any<CancellationToken>()).Returns(true);

        Sms.SendVerificationCodeAsync(Arg.Do<SmsVerification>(Texts.Add), Arg.Any<CancellationToken>()).Returns(true);
    }

    public UserManager<ApplicationUser> Users { get; }

    public InMemoryRepository<UserInvitation> Invitations { get; } = new();

    public ITokenService Tokens { get; } = Substitute.For<ITokenService>();

    public IEmailService Email { get; } = Substitute.For<IEmailService>();

    public ISmsService Sms { get; } = Substitute.For<ISmsService>();

    public FakeTimeProvider Time { get; } = new(Start);

    public List<EmailConfirmationEmail> ConfirmationEmails { get; } = [];

    public List<SmsVerification> Texts { get; } = [];

    public static async Task<IdentityTestHost> CreateAsync()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDataProtection();

        services.AddDbContext<IdentityTestDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddIdentityServices();

        new IdentityBuilder(typeof(ApplicationUser), typeof(ApplicationRole), services).AddEntityFrameworkStores<IdentityTestDbContext>();

        var provider = services.BuildServiceProvider();

        var roles = provider.GetRequiredService<RoleManager<ApplicationRole>>();

        foreach (var role in AppRoles.All)
        {
            await roles.CreateAsync(new ApplicationRole { Name = role });
        }

        return new IdentityTestHost(provider);
    }

    public AuthService CreateAuth()
    {
        return new AuthService(
            Users,
            Tokens,
            Invitations,
            Email,
            new OptionsWrapper<ClientOptions>(new ClientOptions { BaseUrl = "https://client.test" }),
            Time,
            NullLogger<AuthService>.Instance);
    }

    public UserService CreateUsers()
    {
        return new UserService(Users, Sms, NullLogger<UserService>.Instance);
    }

    public async Task<ApplicationUser> AddUserAsync(string email, string role, bool emailConfirmed = true, string password = StrongPassword)
    {
        var user = new ApplicationUser { UserName = email, Email = email, DisplayName = "Test User", EmailConfirmed = emailConfirmed };

        var created = await Users.CreateAsync(user, password);

        Assert.True(created.Succeeded, string.Join(" ", created.Errors.Select(e => e.Description)));

        await Users.AddToRoleAsync(user, role);

        return user;
    }

    public UserInvitation AddInvitation(string email, string role)
    {
        var invitation = new UserInvitation
        {
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            Role = role,
            TokenHash = Guid.NewGuid().ToString("N"),
            InvitedAt = Start,
            ExpiresAt = Start.AddDays(7),
        };

        Invitations.AddAsync(invitation, CancellationToken.None).GetAwaiter().GetResult();

        return invitation;
    }

    public void Dispose()
    {
        _provider.Dispose();
    }
}