namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// One answer in a submitted check.
/// </summary>
public sealed class CheckAnswerRequest
{
    /// <summary>Gets or sets the work order task the answer is for.</summary>
    public Guid TaskId { get; set; }

    /// <summary>Gets or sets the answer: Yes or No, one of the choices, a number of hours, or text. Blank for an optional item left out.</summary>
    [StringLength(1000)]
    public string? Answer { get; set; }
}
