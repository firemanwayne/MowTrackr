namespace MowTrackr.Core.Enums;

/// <summary>
/// Whether a service step asks for a detail (quantity, part number or notes), and whether the technician must fill it in.
/// </summary>
public enum DetailRequirement
{
    /// <summary>The step does not ask for it.</summary>
    None,

    /// <summary>The technician may fill it in.</summary>
    Optional,

    /// <summary>The step cannot be checked off until it is filled in.</summary>
    Required,
}
