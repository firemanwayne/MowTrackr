namespace MowTrackr.Data.Models;

/// <summary>
/// What happened to a row.
/// </summary>
internal enum ChangeAction
{
    /// <summary>The row was added.</summary>
    Created,

    /// <summary>One or more values on the row changed.</summary>
    Updated,

    /// <summary>The row was removed.</summary>
    Deleted,
}
