namespace MowTrackr.Core.Constants;

/// <summary>
/// Names of the supported external sign-in providers (matching their authentication scheme names).
/// </summary>
public static class AuthProviders
{
    /// <summary>Google sign-in.</summary>
    public const string Google = "Google";

    /// <summary>Facebook sign-in.</summary>
    public const string Facebook = "Facebook";

    /// <summary>LinkedIn sign-in.</summary>
    public const string LinkedIn = "LinkedIn";

    /// <summary>Every supported provider.</summary>
    public static readonly IReadOnlyList<string> All = [Google, Facebook, LinkedIn];
}