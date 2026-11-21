using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MowTrackr.Identity.UnitTests.Fakes;

/// <summary>
/// A minimal Identity context on the in-memory provider, so tests can use a real <see cref="UserManager{TUser}"/>.
/// </summary>
/// <param name="options">The context options.</param>
internal sealed class IdentityTestDbContext(DbContextOptions<IdentityTestDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
}