namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// Payload for replacing a user's roles.
/// </summary>
public sealed class SetRolesRequest
{
    /// <summary>Gets or sets the complete set of roles the user should have.</summary>
    public List<string> Roles { get; set; } = [];
}