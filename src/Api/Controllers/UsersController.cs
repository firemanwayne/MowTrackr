using Microsoft.AspNetCore.RateLimiting;

using MowTrackr.Api.Extensions;
using MowTrackr.Core.Contracts.Identity;

namespace MowTrackr.Api.Controllers;

/// <summary>
/// The signed-in user's profile, plus user and role administration.
/// </summary>
/// <param name="users">The user service.</param>
[ApiController]
[Authorize]
[Route(ApiRoutes.Users)]
public sealed class UsersController(IUserService users) : ControllerBase
{
    /// <summary>Gets the signed-in user's profile.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The profile.</returns>
    [HttpGet("me")]
    public async Task<IActionResult> GetMeAsync(CancellationToken cancellationToken)
    {
        var result = await users
            .GetAsync(User.GetUserId(), cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Edits the signed-in user's profile.</summary>
    /// <param name="request">The new profile values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated profile.</returns>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMeAsync([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await users
            .UpdateProfileAsync(User.GetUserId(), request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Texts a verification code to a phone number the user wants to use.</summary>
    /// <param name="request">The phone number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Whether the text was sent.</returns>
    [HttpPost("me/phone/send-code")]
    [EnableRateLimiting(RateLimitPolicies.Sms)]
    public async Task<IActionResult> StartPhoneVerificationAsync([FromBody] StartPhoneVerificationRequest request, CancellationToken cancellationToken)
    {
        var result = await users
            .StartPhoneVerificationAsync(User.GetUserId(), request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Finishes phone verification with the texted code and saves the number as verified.</summary>
    /// <param name="request">The number and the code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated profile.</returns>
    [HttpPost("me/phone/verify")]
    [EnableRateLimiting(RateLimitPolicies.PhoneVerify)]
    public async Task<IActionResult> VerifyPhoneAsync([FromBody] VerifyPhoneRequest request, CancellationToken cancellationToken)
    {
        var result = await users
            .VerifyPhoneAsync(User.GetUserId(), request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Lists all users (managers need this to assign work).</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The users.</returns>
    [HttpGet]
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        var result = await users
            .ListAsync(cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Replaces a user's roles.</summary>
    /// <param name="id">The user key.</param>
    /// <param name="request">The new role set.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated profile.</returns>
    [HttpPut("{id:guid}/roles")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> SetRolesAsync(Guid id, [FromBody] SetRolesRequest request, CancellationToken cancellationToken)
    {
        var result = await users
            .SetRolesAsync(id, request, cancellationToken)
            .ConfigureAwait(false);

        return result.ToActionResult();
    }
}