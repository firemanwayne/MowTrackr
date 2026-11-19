namespace MowTrackr.Core.Constants;

/// <summary>
/// JWT claim names used by the API when issuing tokens and by the client when reading them.
/// </summary>
public static class AppClaims
{
    /// <summary>The user key claim.</summary>
    public const string UserId = "sub";

    /// <summary>The email claim.</summary>
    public const string Email = "email";

    /// <summary>The display name claim.</summary>
    public const string Name = "name";

    /// <summary>The role claim (one per role).</summary>
    public const string Role = "role";
}