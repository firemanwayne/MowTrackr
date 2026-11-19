namespace MowTrackr.Api.Options;

/// <summary>
/// Where the Blazor client lives, bound from the "Client" configuration section. Used for CORS and the
/// redirect that finishes an external (Google/Facebook/LinkedIn) sign-in.
/// </summary>
public sealed class ClientOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Client";

    /// <summary>Gets or sets the client's base URL (no trailing slash).</summary>
    public string BaseUrl { get; set; } = "https://localhost:7072";
}