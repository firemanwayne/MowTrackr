using Microsoft.AspNetCore.Http;
using MowTrackr.Core.Constants;

namespace MowTrackr.Data.Services;

/// <summary>
/// Reads the signed-in user's key from the current HTTP request's claims.
/// </summary>
internal static class CurrentUserReader
{
    /// <summary>Gets the signed-in user's key.</summary>
    /// <param name="accessor">The HTTP context accessor.</param>
    /// <returns>The user key, or <see langword="null"/> outside a signed-in request.</returns>
    public static Guid? Read(IHttpContextAccessor accessor)
    {
        var value = accessor.HttpContext?.User.FindFirst(AppClaims.UserId)?.Value;

        return Guid.TryParse(value, out var id) ? id : null;
    }
}
