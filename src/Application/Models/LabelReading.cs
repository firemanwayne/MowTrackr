namespace MowTrackr.Application.Models;

/// <summary>
/// What could be read from a photographed equipment label. Anything not found is <see langword="null"/>.
/// </summary>
/// <param name="Manufacturer">The maker, if a known brand name appears on the label.</param>
/// <param name="Model">The model number or name.</param>
/// <param name="SerialNumber">The serial number.</param>
public sealed record LabelReading(string? Manufacturer, string? Model, string? SerialNumber)
{
    /// <summary>Gets a value indicating whether nothing useful was found.</summary>
    public bool IsEmpty => Manufacturer is null && Model is null && SerialNumber is null;
}