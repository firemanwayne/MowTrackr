using MowTrackr.Client.Providers;
using MowTrackr.Client.Services;
using MowTrackr.Client.UnitTests.Fakes;

namespace MowTrackr.Client.UnitTests.Providers;

public sealed class JwtAuthenticationStateProviderTests
{
    [Fact]
    public async Task GetAuthenticationStateAsync_ValidToken_IsSignedInWithTheTokensClaims()
    {
        var expiry = new SessionExpiry();

        using var provider = new JwtAuthenticationStateProvider(new InMemoryTokenStore(TestJwt.Create(DateTimeOffset.UtcNow.AddHours(1), "Pat Lee", "Admin")), expiry);

        var state = await provider.GetAuthenticationStateAsync();

        Assert.True(state.User.Identity!.IsAuthenticated);

        Assert.Equal("Pat Lee", state.User.Identity.Name);

        Assert.True(state.User.IsInRole("Admin"));

        Assert.False(expiry.ConsumeNotice());
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_ExpiredToken_IsSignedOutClearedAndFlaggedForTheLoginPage()
    {
        var store = new InMemoryTokenStore(TestJwt.Create(DateTimeOffset.UtcNow.AddMinutes(-5)));

        var expiry = new SessionExpiry();

        using var provider = new JwtAuthenticationStateProvider(store, expiry);

        var state = await provider.GetAuthenticationStateAsync();

        Assert.False(state.User.Identity!.IsAuthenticated);

        Assert.Null(store.Current);

        Assert.True(expiry.ConsumeNotice());
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_NoToken_IsSignedOutWithoutAnExpiryNotice()
    {
        var expiry = new SessionExpiry();

        using var provider = new JwtAuthenticationStateProvider(new InMemoryTokenStore(), expiry);

        var state = await provider.GetAuthenticationStateAsync();

        Assert.False(state.User.Identity!.IsAuthenticated);

        Assert.False(expiry.ConsumeNotice());
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_DamagedToken_IsSignedOutAndCleared()
    {
        var store = new InMemoryTokenStore("not-a-jwt");

        using var provider = new JwtAuthenticationStateProvider(store, new SessionExpiry());

        var state = await provider.GetAuthenticationStateAsync();

        Assert.False(state.User.Identity!.IsAuthenticated);

        Assert.Null(store.Current);
    }

    [Fact]
    public async Task SignalExpired_WhileSignedIn_RaisesAnAuthenticationStateChangeThatIsSignedOut()
    {
        var store = new InMemoryTokenStore(TestJwt.Create(DateTimeOffset.UtcNow.AddHours(1)));

        var expiry = new SessionExpiry();

        using var provider = new JwtAuthenticationStateProvider(store, expiry);

        var changed = new TaskCompletionSource<Task<AuthenticationState>>();

        provider.AuthenticationStateChanged += task => changed.TrySetResult(task);

        await provider.GetAuthenticationStateAsync();

        // What the HTTP handler does when the API answers 401.
        await store.ClearAsync();

        expiry.SignalExpired();

        var state = await (await changed.Task.WaitAsync(TimeSpan.FromSeconds(5)));

        Assert.False(state.User.Identity!.IsAuthenticated);

        Assert.True(expiry.ConsumeNotice());
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_TokenExpiringWhileThePageIsOpen_SignsTheUserOutWhenItExpires()
    {
        var store = new InMemoryTokenStore(TestJwt.Create(DateTimeOffset.UtcNow.AddSeconds(2)));

        using var provider = new JwtAuthenticationStateProvider(store, new SessionExpiry());

        var changed = new TaskCompletionSource<Task<AuthenticationState>>();

        provider.AuthenticationStateChanged += task => changed.TrySetResult(task);

        var first = await provider.GetAuthenticationStateAsync();

        Assert.True(first.User.Identity!.IsAuthenticated);

        var state = await (await changed.Task.WaitAsync(TimeSpan.FromSeconds(10)));

        Assert.False(state.User.Identity!.IsAuthenticated);
    }
}