namespace MowTrackr.Identity.Models;

/// <summary>
/// An application role (see <c>AppRoles</c>).
/// </summary>
public sealed class ApplicationRole : IdentityRole<Guid>
{
    /// <summary>Initializes a new instance of the <see cref="ApplicationRole"/> class.</summary>
    public ApplicationRole()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ApplicationRole"/> class with a name.</summary>
    /// <param name="roleName">The role name.</param>
    public ApplicationRole(string roleName)
        : base(roleName)
    {
    }
}