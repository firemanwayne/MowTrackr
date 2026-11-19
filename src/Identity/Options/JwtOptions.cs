namespace MowTrackr.Identity.Options;

/// <summary>
/// JWT issuing/validation settings, bound from the "Jwt" configuration section.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Jwt";

    /// <summary>Gets or sets the token issuer.</summary>
    public string Issuer { get; set; } = "MowTrackr";

    /// <summary>Gets or sets the token audience.</summary>
    public string Audience { get; set; } = "MowTrackr.Client";

    /// <summary>Gets or sets the HMAC signing key (at least 32 characters). Supply via secrets/app settings, never source control.</summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>Gets or sets how long an issued token is valid, in minutes.</summary>
    public int ExpiryMinutes { get; set; } = 480;
}