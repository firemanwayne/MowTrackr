using MowTrackr.Core.Contracts.Maintenance;

namespace MowTrackr.Core.Interfaces;

/// <summary>
/// Detects duplicate and overlapping service intervals across templates.
/// </summary>
public interface IIntervalValidator
{
    /// <summary>Validates a candidate interval against existing templates.</summary>
    /// <param name="candidate">The candidate interval.</param>
    /// <param name="existing">The other templates to compare with.</param>
    /// <returns>Errors (block saving) and warnings (informational).</returns>
    IReadOnlyList<IntervalIssue> Validate(IntervalCheckRequest candidate, IReadOnlyCollection<IntervalDefinition> existing);
}