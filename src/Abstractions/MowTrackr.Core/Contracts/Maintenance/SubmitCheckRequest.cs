namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// The answers to a pre-check or post-check.
/// </summary>
public sealed class SubmitCheckRequest
{
    /// <summary>Gets or sets the answers.</summary>
    public List<CheckAnswerRequest> Answers { get; set; } = [];
}
