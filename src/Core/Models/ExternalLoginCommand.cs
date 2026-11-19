namespace MowTrackr.Core.Models;

/// <summary>
/// Identity details returned by an external provider, used to sign in or provision a local user.
/// </summary>
/// <param name="Provider">The provider name (see <c>AuthProviders</c>).</param>
/// <param name="ProviderKey">The user's unique id at the provider.</param>
/// <param name="Email">The email reported by the provider.</param>
/// <param name="DisplayName">The name reported by the provider.</param>
public sealed record ExternalLoginCommand(string Provider, string ProviderKey, string Email, string DisplayName);