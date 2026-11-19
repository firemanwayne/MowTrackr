namespace MowTrackr.Core.Contracts.Identity;

/// <summary>
/// Whether open self-registration is available. It is only while no account exists yet (to create the first Admin);
/// after that, accounts come from invitations.
/// </summary>
/// <param name="IsOpen">Whether anyone can register right now.</param>
public sealed record RegistrationStatusResponse(bool IsOpen);