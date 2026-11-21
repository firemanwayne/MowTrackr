namespace MowTrackr.Core.Constants;

/// <summary>
/// Relative API routes shared by the API controllers and the client.
/// </summary>
public static class ApiRoutes
{
    /// <summary>Authentication endpoints.</summary>
    public const string Auth = "api/auth";

    /// <summary>Mower endpoints.</summary>
    public const string Mowers = "api/mowers";

    /// <summary>Asset type lookup endpoints.</summary>
    public const string AssetTypes = "api/asset-types";

    /// <summary>Manufacturer and model lookup endpoints.</summary>
    public const string Manufacturers = "api/manufacturers";

    /// <summary>Inspection template endpoints.</summary>
    public const string InspectionTemplates = "api/inspection-templates";

    /// <summary>Service template endpoints.</summary>
    public const string ServiceTemplates = "api/service-templates";

    /// <summary>Maintenance (due list and work order) endpoints.</summary>
    public const string Maintenance = "api/maintenance";

    /// <summary>User and profile endpoints.</summary>
    public const string Users = "api/users";

    /// <summary>Invitation administration endpoints (Admin only).</summary>
    public const string Invitations = "api/invitations";

    /// <summary>Change history endpoints (Admin only).</summary>
    public const string History = "api/history";
}