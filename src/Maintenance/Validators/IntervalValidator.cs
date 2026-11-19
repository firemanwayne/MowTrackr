namespace MowTrackr.Maintenance.Validators;

/// <summary>
/// Default <see cref="IIntervalValidator"/>. Exact duplicates are errors; intervals that are multiples of
/// one another (and so come due together) are warnings.
/// </summary>
internal sealed class IntervalValidator : IIntervalValidator
{
    /// <inheritdoc />
    public IReadOnlyList<IntervalIssue> Validate(IntervalCheckRequest candidate, IReadOnlyCollection<IntervalDefinition> existing)
    {
        var issues = new List<IntervalIssue>();

        if (candidate.IntervalHours is null && candidate.IntervalDays is null)
        {
            issues.Add(new IntervalIssue(IssueSeverity.Error, "Set an engine-hours interval, a day interval, or both."));

            return issues;
        }

        if (candidate.IntervalHours is <= 0)
        {
            issues.Add(new IntervalIssue(IssueSeverity.Error, "The hours interval must be greater than zero."));
        }

        if (candidate.IntervalDays is <= 0)
        {
            issues.Add(new IntervalIssue(IssueSeverity.Error, "The days interval must be greater than zero."));
        }

        if (issues.Count > 0)
        {
            return issues;
        }

        foreach (var other in existing.Where(e => e.Id != candidate.ExcludeTemplateId))
        {
            CompareHours(candidate.IntervalHours, other, issues);

            CompareDays(candidate.IntervalDays, other, issues);
        }

        return issues;
    }

    /// <summary>Compares the hours interval against one other template.</summary>
    /// <param name="hours">The candidate hours interval.</param>
    /// <param name="other">The other template.</param>
    /// <param name="issues">The list to append findings to.</param>
    private static void CompareHours(decimal? hours, IntervalDefinition other, List<IntervalIssue> issues)
    {
        if (hours is not { } mine || other.IntervalHours is not { } theirs)
        {
            return;
        }

        if (mine == theirs)
        {
            issues.Add(new IntervalIssue(IssueSeverity.Error, $"'{other.Name}' already services every {theirs:0.##} hours."));

            return;
        }

        var (small, large) = mine < theirs ? (mine, theirs) : (theirs, mine);

        if (large % small == 0)
        {
            issues.Add(new IntervalIssue(
                IssueSeverity.Warning,
                $"{large:0.##} h is a multiple of {small:0.##} h ('{other.Name}'); both will come due together every {large:0.##} hours."));
        }
    }

    /// <summary>Compares the days interval against one other template.</summary>
    /// <param name="days">The candidate days interval.</param>
    /// <param name="other">The other template.</param>
    /// <param name="issues">The list to append findings to.</param>
    private static void CompareDays(int? days, IntervalDefinition other, List<IntervalIssue> issues)
    {
        if (days is not { } mine || other.IntervalDays is not { } theirs)
        {
            return;
        }

        if (mine == theirs)
        {
            issues.Add(new IntervalIssue(IssueSeverity.Error, $"'{other.Name}' already services every {theirs} days."));

            return;
        }

        var (small, large) = mine < theirs ? (mine, theirs) : (theirs, mine);

        if (large % small == 0)
        {
            issues.Add(new IntervalIssue(
                IssueSeverity.Warning,
                $"{large} days is a multiple of {small} days ('{other.Name}'); both will come due together every {large} days."));
        }
    }
}