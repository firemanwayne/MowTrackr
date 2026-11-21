using MowTrackr.Maintenance.Models;

namespace MowTrackr.Maintenance.Validators;

/// <summary>
/// The rules for answers to check items: what a valid answer looks like, and which answers mean something needs repair.
/// </summary>
internal static class CheckAnswerValidator
{
    /// <summary>The good answer to a yes/no item.</summary>
    public const string Yes = "Yes";

    /// <summary>The bad answer to a yes/no item.</summary>
    public const string No = "No";

    private const char ListSeparator = ',';

    /// <summary>Splits a comma-separated list (choices, or the answers that need repair).</summary>
    /// <param name="text">The text, which may be <see langword="null"/>.</param>
    /// <returns>The trimmed, non-empty entries.</returns>
    public static IReadOnlyList<string> SplitList(string? text)
    {
        return text is null
            ? []
            : text.Split(ListSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>Checks an answer against the item's type.</summary>
    /// <param name="task">The check item.</param>
    /// <param name="answer">The trimmed, non-blank answer.</param>
    /// <returns>An error message, or <see langword="null"/> when the answer is valid.</returns>
    public static string? Validate(WorkOrderTask task, string answer)
    {
        return task.ItemType switch
        {
            CheckItemType.YesNo => IsOneOf(answer, [Yes, No]) ? null : $"\"{task.Description}\": answer Yes or No.",
            CheckItemType.Choice => IsOneOf(answer, SplitList(task.Choices)) ? null : $"\"{task.Description}\": choose one of {task.Choices}.",
            CheckItemType.EngineHours => TryParseHours(answer, out _) ? null : $"\"{task.Description}\": enter the engine hours as a number.",
            _ => null,
        };
    }

    /// <summary>Writes an answer the way it is stored: the template's own spelling for Yes/No and choices, invariant numbers for hours.</summary>
    /// <param name="task">The check item.</param>
    /// <param name="answer">The valid answer.</param>
    /// <returns>The stored answer.</returns>
    public static string Canonical(WorkOrderTask task, string answer)
    {
        switch (task.ItemType)
        {
            case CheckItemType.YesNo:
                return string.Equals(answer, Yes, StringComparison.OrdinalIgnoreCase) ? Yes : No;

            case CheckItemType.Choice:
                return SplitList(task.Choices).First(c => string.Equals(c, answer, StringComparison.OrdinalIgnoreCase));

            case CheckItemType.EngineHours:
                TryParseHours(answer, out var hours);

                return hours.ToString("0.##", CultureInfo.InvariantCulture);

            default:
                return answer;
        }
    }

    /// <summary>Reads an engine-hours answer.</summary>
    /// <param name="answer">The text.</param>
    /// <param name="hours">The hours when the text is a non-negative number.</param>
    /// <returns><see langword="true"/> when the text is valid.</returns>
    public static bool TryParseHours(string answer, out decimal hours)
    {
        return decimal.TryParse(answer, NumberStyles.Number, CultureInfo.InvariantCulture, out hours) && hours >= 0;
    }

    /// <summary>Checks whether an item's answer means something needs repair.</summary>
    /// <param name="task">The check item.</param>
    /// <returns><see langword="true"/> when the answer is one of the item's repair answers.</returns>
    public static bool IsFinding(WorkOrderTask task)
    {
        return task.ItemType is CheckItemType.YesNo or CheckItemType.Choice
            && !string.IsNullOrWhiteSpace(task.Answer)
            && IsOneOf(task.Answer, SplitList(task.RepairOn));
    }

    /// <summary>Compares an answer with a list of options, ignoring case.</summary>
    /// <param name="answer">The answer.</param>
    /// <param name="options">The options.</param>
    /// <returns><see langword="true"/> when the answer is one of them.</returns>
    private static bool IsOneOf(string answer, IEnumerable<string> options)
    {
        return options.Any(o => string.Equals(o, answer, StringComparison.OrdinalIgnoreCase));
    }
}
