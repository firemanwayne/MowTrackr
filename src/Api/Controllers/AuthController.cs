using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using MowTrackr.Api.Extensions;
using MowTrackr.Core.Contracts.Identity;
using MowTrackr.Core.Options;

namespace MowTrackr.Api.Controllers;

/// <summary>
/// Registration, sign-in and external provider flows. All of these return or redirect with a JWT.
/// </summary>
/// <param name="auth">The auth service.</param>
/// <param name="invitations">The invitation service (anonymous preview and accept).</param>
/// <param name="schemes">The authentication scheme provider (to discover enabled external providers).</param>
/// <param name="client">The client location settings.</param>
[ApiController]
[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.Auth)]
[Route(ApiRoutes.Auth)]
public sealed class AuthController(IAuthService auth, IInvitationService invitations, IAuthenticationSchemeProvider schemes, IOptions<ClientOptions> client) : ControllerBase
{
    private const string LoginProviderKey = "LoginProvider";

    /// <summary>Reports whether open registration is available (only until the first account exists).</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The registration status.</returns>
    [HttpGet("registration")]
    public async Task<IActionResult> GetRegistrationAsync(CancellationToken cancellationToken)
    {
        var result = await auth.GetRegistrationStatusAsync(cancellationToken).ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Describes the invitation behind a link token, so the accept page can confirm it.</summary>
    /// <param name="token">The secret token from the invitation link.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invitation preview.</returns>
    [HttpGet("invitation")]
    public async Task<IActionResult> GetInvitationAsync([FromQuery] string token, CancellationToken cancellationToken)
    {
        var result = await invitations.PreviewAsync(token, cancellationToken).ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Accepts an invitation: creates the account with the invited role and signs it in.</summary>
    /// <param name="request">The token and the new account's details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The signed-in session.</returns>
    [HttpPost("accept-invitation")]
    public async Task<IActionResult> AcceptInvitationAsync([FromBody] AcceptInvitationRequest request, CancellationToken cancellationToken)
    {
        var result = await invitations.AcceptAsync(request, cancellationToken).ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Confirms an email address from the emailed link.</summary>
    /// <param name="request">The user and token from the link.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A message, or why the link is not valid.</returns>
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await auth.ConfirmEmailAsync(request, cancellationToken).ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Sends a new confirmation email. Always answers the same, so it cannot be used to find accounts.</summary>
    /// <param name="request">The address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A message.</returns>
    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmationAsync([FromBody] ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        var result = await auth.ResendConfirmationAsync(request, cancellationToken).ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Creates the first account, which becomes an Admin, and emails a confirmation link. After that registration is by invitation only.</summary>
    /// <param name="request">The registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Where the confirmation link was sent.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await auth.RegisterAsync(request, cancellationToken).ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Signs in with email and password.</summary>
    /// <param name="request">The credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The signed-in session.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await auth.LoginAsync(request, cancellationToken).ConfigureAwait(false);

        return result.ToActionResult();
    }

    /// <summary>Lists the external providers that are configured and enabled.</summary>
    /// <returns>The provider names.</returns>
    [HttpGet("providers")]
    public async Task<IActionResult> GetProvidersAsync()
    {
        var all = await schemes.GetAllSchemesAsync().ConfigureAwait(false);

        var enabled = all.Select(s => s.Name).Where(n => AuthProviders.All.Contains(n)).ToList();

        return Ok(enabled);
    }

    /// <summary>Starts an external sign-in by redirecting to the provider.</summary>
    /// <param name="provider">The provider name (Google, Facebook or LinkedIn).</param>
    /// <returns>A challenge that redirects to the provider.</returns>
    [HttpGet("external/{provider}")]
    public async Task<IActionResult> ExternalAsync(string provider)
    {
        var scheme = await schemes.GetSchemeAsync(provider).ConfigureAwait(false);

        if (scheme is null || !AuthProviders.All.Contains(scheme.Name))
        {
            return NotFound(new ProblemDetails { Title = "Unknown or disabled provider", Status = StatusCodes.Status404NotFound });
        }

        var properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(ExternalCallbackAsync)) };

        properties.Items[LoginProviderKey] = scheme.Name;

        return Challenge(properties, scheme.Name);
    }

    /// <summary>Finishes an external sign-in: issues our JWT and redirects back to the client with it in the URL fragment.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A redirect to the client.</returns>
    [HttpGet("external-callback")]
    public async Task<IActionResult> ExternalCallbackAsync(CancellationToken cancellationToken)
    {
        var baseUrl = client.Value.BaseUrl.TrimEnd('/');

        var external = await HttpContext.AuthenticateAsync(ExternalAuthenticationExtensions.ExternalScheme).ConfigureAwait(false);

        if (!external.Succeeded || external.Principal is null)
        {
            return Redirect($"{baseUrl}/auth/callback#error={Uri.EscapeDataString("External sign-in failed.")}");
        }

        var principal = external.Principal;

        var provider = string.Empty;

        if (external.Properties is not null && external.Properties.Items.TryGetValue(LoginProviderKey, out var stored) && stored is not null)
        {
            provider = stored;
        }

        var command = new ExternalLoginCommand(
            provider,
            principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty);

        await HttpContext.SignOutAsync(ExternalAuthenticationExtensions.ExternalScheme).ConfigureAwait(false);

        var result = await auth.ExternalLoginAsync(command, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess || result.Value is null)
        {
            return Redirect($"{baseUrl}/auth/callback#error={Uri.EscapeDataString(result.Error ?? "Sign-in failed.")}");
        }

        return Redirect($"{baseUrl}/auth/callback#token={Uri.EscapeDataString(result.Value.Token)}");
    }
}