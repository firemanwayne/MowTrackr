using System.Net;
using MowTrackr.Client.Handlers;
using MowTrackr.Client.Services;
using MowTrackr.Client.UnitTests.Fakes;

namespace MowTrackr.Client.UnitTests.Handlers;

public sealed class BearerTokenHandlerTests
{
    [Fact]
    public async Task SendAsync_WithAToken_SendsItAsABearerHeader()
    {
        var inner = new StubHandler(HttpStatusCode.OK);

        using var client = CreateClient(new InMemoryTokenStore("abc.def.ghi"), new SessionExpiry(), inner);

        await client.GetAsync("https://api.test/x");

        Assert.Equal("Bearer abc.def.ghi", inner.LastRequest!.Headers.Authorization!.ToString());
    }

    [Fact]
    public async Task SendAsync_ApiAnswers401ToASignedInRequest_EndsTheSession()
    {
        var store = new InMemoryTokenStore("abc.def.ghi");

        var expiry = new SessionExpiry();

        var raised = 0;

        expiry.Expired += () => raised++;

        using var client = CreateClient(store, expiry, new StubHandler(HttpStatusCode.Unauthorized));

        var response = await client.GetAsync("https://api.test/mowers");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        Assert.Null(store.Current);

        Assert.Equal(1, raised);

        Assert.True(expiry.ConsumeNotice());
    }

    [Fact]
    public async Task SendAsync_ApiAnswers401WithoutAToken_IsJustAFailedSignIn()
    {
        var expiry = new SessionExpiry();

        var raised = 0;

        expiry.Expired += () => raised++;

        using var client = CreateClient(new InMemoryTokenStore(), expiry, new StubHandler(HttpStatusCode.Unauthorized));

        await client.PostAsync("https://api.test/api/auth/login", new StringContent("{}"));

        Assert.Equal(0, raised);

        Assert.False(expiry.ConsumeNotice());
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task SendAsync_OtherStatuses_KeepTheSession(HttpStatusCode status)
    {
        var store = new InMemoryTokenStore("abc.def.ghi");

        var expiry = new SessionExpiry();

        using var client = CreateClient(store, expiry, new StubHandler(status));

        await client.GetAsync("https://api.test/x");

        Assert.Equal("abc.def.ghi", store.Current);

        Assert.False(expiry.ConsumeNotice());
    }

    private static HttpClient CreateClient(InMemoryTokenStore store, SessionExpiry expiry, StubHandler inner)
    {
        return new HttpClient(new BearerTokenHandler(store, expiry) { InnerHandler = inner });
    }

    private sealed class StubHandler(HttpStatusCode status) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;

            return Task.FromResult(new HttpResponseMessage(status));
        }
    }
}