namespace MowTrackr.Core.Enums;

/// <summary>
/// The kind of answer a check item takes.
/// </summary>
public enum CheckItemType
{
    /// <summary>Yes or no. Phrase the item so that Yes is the good answer ("No existing damage", "Does it start?").</summary>
    YesNo,

    /// <summary>One of a list of choices, such as Full, Half, None.</summary>
    Choice,

    /// <summary>The unit's current engine hours. Recording it also updates the unit's hours.</summary>
    EngineHours,

    /// <summary>Free-text notes.</summary>
    Text,

    /// <summary>Worked out by the system: Yes once every required service step is done.</summary>
    ServiceStepsComplete,
}
