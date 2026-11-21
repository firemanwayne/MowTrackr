namespace MowTrackr.Application.Options;

/// <summary>
/// Settings for the typed API clients, supplied through the <c>AddClientServices</c> configure callback.
/// </summary>
public sealed class ApiClientOptions
{
    /// <summary>Gets or sets the API base address every typed client is pointed at. Required.</summary>
    public Uri? BaseAddress { get; set; }
}
