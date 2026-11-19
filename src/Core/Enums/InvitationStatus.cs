namespace MowTrackr.Core.Enums;

/// <summary>
/// Where an invitation is in its life.
/// </summary>
public enum InvitationStatus
{
    /// <summary>Sent and still usable.</summary>
    Pending = 0,

    /// <summary>Used to create an account.</summary>
    Accepted = 1,

    /// <summary>Withdrawn by an administrator.</summary>
    Revoked = 2,

    /// <summary>Not used before its expiry time.</summary>
    Expired = 3,
}