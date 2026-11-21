namespace MowTrackr.Core.Constants;

/// <summary>
/// Names of the application roles.
/// </summary>
public static class AppRoles
{
    /// <summary>Full access, including user and role management.</summary>
    public const string Admin = "Admin";

    /// <summary>Manages mowers, service templates and schedules work orders.</summary>
    public const string Manager = "Manager";

    /// <summary>Performs maintenance and checks off work order tasks.</summary>
    public const string Technician = "Technician";

    /// <summary>Comma-separated list for <c>[Authorize(Roles = ...)]</c> on manager-level endpoints.</summary>
    public const string ManagerOrAdmin = Admin + "," + Manager;

    /// <summary>Every role, in privilege order (highest first).</summary>
    public static readonly IReadOnlyList<string> All = [Admin, Manager, Technician];
}