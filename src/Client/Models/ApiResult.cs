namespace MowTrackr.Client.Models;

/// <summary>
/// Outcome of an API call as seen by the UI.
/// </summary>
/// <typeparam name="T">The success payload type.</typeparam>
/// <param name="StatusCode">The HTTP status code.</param>
/// <param name="Value">The payload (also set for some 409 responses that carry detail).</param>
/// <param name="Error">A human-readable error, if the call failed.</param>
public sealed record ApiResult<T>(HttpStatusCode StatusCode, T? Value, string? Error)
{
    /// <summary>Gets a value indicating whether the call succeeded.</summary>
    public bool IsSuccess => (int)StatusCode is >= 200 and < 300;
}