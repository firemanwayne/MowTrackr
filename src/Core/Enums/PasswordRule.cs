namespace MowTrackr.Core.Enums;

/// <summary>
/// A rule of the password policy.
/// </summary>
public enum PasswordRule
{
    /// <summary>At least <see cref="Constants.PasswordPolicy.MinLength"/> characters.</summary>
    MinimumLength = 0,

    /// <summary>At most <see cref="Constants.PasswordPolicy.MaxLength"/> characters.</summary>
    MaximumLength = 1,

    /// <summary>At least one upper-case letter.</summary>
    Uppercase = 2,

    /// <summary>At least one lower-case letter.</summary>
    Lowercase = 3,

    /// <summary>At least one digit.</summary>
    Digit = 4,

    /// <summary>At least one symbol (not a letter or digit).</summary>
    Symbol = 5,

    /// <summary>Does not contain the user's email name or display name.</summary>
    NoPersonalInfo = 6,

    /// <summary>Is not a common or easily guessed password.</summary>
    NotCommon = 7,
}