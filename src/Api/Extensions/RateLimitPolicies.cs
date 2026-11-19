namespace MowTrackr.Api.Extensions;

/// <summary>
/// Names of the rate-limiting policies applied with <c>[EnableRateLimiting]</c>.
/// </summary>
public static class RateLimitPolicies
{
    /// <summary>Sign-in, registration, invitations and email links: 10 requests a minute per client address.</summary>
    public const string Auth = "auth";

    /// <summary>Texting a verification code: 3 every 10 minutes per user (SMS costs money and is a fraud target).</summary>
    public const string Sms = "sms";

    /// <summary>Entering a texted code: 8 attempts every 10 minutes per user, so the 6-digit code cannot be guessed.</summary>
    public const string PhoneVerify = "phone-verify";
}