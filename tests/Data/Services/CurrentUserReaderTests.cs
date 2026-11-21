using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MowTrackr.Core.Constants;
using MowTrackr.Data.Services;

namespace MowTrackr.Data.UnitTests.Services;

public sealed class CurrentUserReaderTests
{
    [Fact]
    public void Read_SignedInRequest_ReturnsTheUserKeyFromTheClaim()
    {
        var userId = Guid.NewGuid();

        var registered = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(AppClaims.UserId, userId.ToString())], "test")) },
        };

        // The interceptor reads through its own accessor instance; it must still see the registered accessor's request.
        var other = new HttpContextAccessor();

        Assert.Equal(userId, CurrentUserReader.Read(other));

        registered.HttpContext = null;
    }

    [Fact]
    public void Read_NoRequest_ReturnsNull()
    {
        Assert.Null(CurrentUserReader.Read(new HttpContextAccessor { HttpContext = null }));
    }

    [Fact]
    public void Read_ClaimIsNotAKey_ReturnsNull()
    {
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(AppClaims.UserId, "not-a-guid")], "test")) },
        };

        Assert.Null(CurrentUserReader.Read(accessor));

        accessor.HttpContext = null;
    }
}
