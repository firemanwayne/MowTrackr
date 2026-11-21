using MowTrackr.Core.Contracts.Maintenance;
using MowTrackr.Maintenance.Models;

namespace MowTrackr.Maintenance.Validators;

/// <summary>
/// The rules for the details recorded on a service step: a step can only be checked off once its required details are in.
/// </summary>
internal static class TaskDetailValidator
{
    /// <summary>Checks the details of a step, and that required ones are present when the step is being checked off.</summary>
    /// <param name="task">The work order step.</param>
    /// <param name="details">The details sent with the request, or <see langword="null"/> to keep what is saved.</param>
    /// <param name="completing">Whether the step is being checked off.</param>
    /// <returns>An error message, or <see langword="null"/> when the details are fine.</returns>
    public static string? Validate(WorkOrderTask task, TaskDetailsRequest? details, bool completing)
    {
        var quantity = details is null ? task.Quantity : details.Quantity;

        var part = details is null ? task.PartNumber : Clean(details.PartNumber);

        var notes = details is null ? task.StepNotes : Clean(details.Notes);

        if (quantity is < 0)
        {
            return $"\"{task.Description}\": the quantity cannot be negative.";
        }

        if (!completing)
        {
            return null;
        }

        if (task.QuantityRequirement == DetailRequirement.Required && (quantity is null or <= 0 || string.IsNullOrWhiteSpace(task.QuantityUnit)))
        {
            return $"\"{task.Description}\": enter the quantity.";
        }

        if (task.PartNumberRequirement == DetailRequirement.Required && string.IsNullOrWhiteSpace(part))
        {
            return $"\"{task.Description}\": enter the part number.";
        }

        if (task.NotesRequirement == DetailRequirement.Required && string.IsNullOrWhiteSpace(notes))
        {
            return $"\"{task.Description}\": add a note.";
        }

        return null;
    }

    /// <summary>Saves the details on a step, only for the details the step asks for.</summary>
    /// <param name="task">The work order step.</param>
    /// <param name="details">The details sent with the request, or <see langword="null"/> to change nothing.</param>
    public static void Apply(WorkOrderTask task, TaskDetailsRequest? details)
    {
        if (details is null)
        {
            return;
        }

        if (task.QuantityRequirement != DetailRequirement.None)
        {
            task.Quantity = details.Quantity;
        }

        if (task.PartNumberRequirement != DetailRequirement.None)
        {
            task.PartNumber = Clean(details.PartNumber);
        }

        if (task.NotesRequirement != DetailRequirement.None)
        {
            task.StepNotes = Clean(details.Notes);
        }
    }

    /// <summary>Trims text and turns blank into <see langword="null"/>.</summary>
    /// <param name="text">The text.</param>
    /// <returns>The trimmed text, or <see langword="null"/>.</returns>
    private static string? Clean(string? text)
    {
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }
}
