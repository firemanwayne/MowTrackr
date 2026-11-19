namespace MowTrackr.Core.Enums;

/// <summary>
/// Outcome category of a service call, mapped to an HTTP status at the API boundary.
/// </summary>
public enum ResultStatus
{
    /// <summary>The operation succeeded.</summary>
    Ok,

    /// <summary>The target does not exist.</summary>
    NotFound,

    /// <summary>The input failed validation.</summary>
    Invalid,

    /// <summary>The input conflicts with existing data.</summary>
    Conflict,

    /// <summary>The caller is not authenticated or the credentials are wrong.</summary>
    Unauthorized,

    /// <summary>The caller is authenticated but not permitted.</summary>
    Forbidden,
}