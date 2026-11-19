namespace MowTrackr.Core.Constants;

/// <summary>
/// The password policy, shared by the API (enforcement) and the client (live hints) so they cannot drift apart.
/// </summary>
public static class PasswordPolicy
{
    /// <summary>The minimum length.</summary>
    public const int MinLength = 12;

    /// <summary>The maximum length (also bounds the cost of hashing).</summary>
    public const int MaxLength = 128;

    private const int MinPersonalTokenLength = 3;

    private const int MaxExtraCharactersAroundCommonWord = 3;

    private static readonly HashSet<string> CommonWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "passw0rd", "passwort", "qwerty", "qwertyuiop", "qwertyuiopasdf", "asdfghjkl", "zxcvbnm", "letmein",
        "welcome", "admin", "administrator", "iloveyou", "monkey", "dragon", "football", "baseball", "basketball",
        "abc", "abcdef", "abcdefgh", "trustno", "sunshine", "princess", "master", "login", "changeme", "secret",
        "shadow", "summer", "winter", "spring", "autumn", "superman", "batman", "starwars", "whatever", "freedom",
        "computer", "internet", "michael", "jennifer", "jordan", "hunter", "ranger", "buster", "soccer", "hockey",
        "mowtrackr", "mowtracker", "lawnmower", "lawncare", "mowing", "mower", "grass", "landscape", "landscaping",
        "toro", "husqvarna", "exmark", "scag", "honda", "stihl", "greenfield", "welcome1", "test", "testing",
        "guest", "default", "user", "temp", "temporary", "company", "work", "office", "service", "maintenance",
    };

    /// <summary>Finds the rules a password breaks.</summary>
    /// <param name="password">The candidate password.</param>
    /// <param name="personalInfo">Values about the user (email, display name) the password must not contain.</param>
    /// <returns>The broken rules; empty when the password is acceptable.</returns>
    public static IReadOnlyList<PasswordRule> Failures(string? password, IEnumerable<string?>? personalInfo = null)
    {
        var value = password ?? string.Empty;

        var failures = new List<PasswordRule>();

        if (value.Length < MinLength)
        {
            failures.Add(PasswordRule.MinimumLength);
        }

        if (value.Length > MaxLength)
        {
            failures.Add(PasswordRule.MaximumLength);
        }

        if (!value.Any(char.IsUpper))
        {
            failures.Add(PasswordRule.Uppercase);
        }

        if (!value.Any(char.IsLower))
        {
            failures.Add(PasswordRule.Lowercase);
        }

        if (!value.Any(char.IsDigit))
        {
            failures.Add(PasswordRule.Digit);
        }

        if (!value.Any(c => !char.IsLetterOrDigit(c)))
        {
            failures.Add(PasswordRule.Symbol);
        }

        if (value.Length > 0 && ContainsPersonalInfo(value, personalInfo))
        {
            failures.Add(PasswordRule.NoPersonalInfo);
        }

        if (value.Length > 0 && IsCommon(value))
        {
            failures.Add(PasswordRule.NotCommon);
        }

        return failures;
    }

    /// <summary>Gets a value indicating whether a password meets every rule.</summary>
    /// <param name="password">The candidate password.</param>
    /// <param name="personalInfo">Values about the user the password must not contain.</param>
    /// <returns><see langword="true"/> if acceptable.</returns>
    public static bool IsAcceptable(string? password, IEnumerable<string?>? personalInfo = null)
    {
        return Failures(password, personalInfo).Count == 0;
    }

    /// <summary>Checks whether the password contains a word from the user's own details.</summary>
    /// <param name="password">The password.</param>
    /// <param name="personalInfo">Values about the user.</param>
    /// <returns><see langword="true"/> if it does.</returns>
    private static bool ContainsPersonalInfo(string password, IEnumerable<string?>? personalInfo)
    {
        if (personalInfo is null)
        {
            return false;
        }

        var lowered = password.ToLowerInvariant();

        foreach (var info in personalInfo)
        {
            if (string.IsNullOrWhiteSpace(info))
            {
                continue;
            }

            var atIndex = info.IndexOf('@', StringComparison.Ordinal);

            var source = atIndex > 0 ? info[..atIndex] : info;

            var tokens = source.Split([' ', '.', '_', '-', '+'], StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Any(t => t.Length >= MinPersonalTokenLength && lowered.Contains(t, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Checks whether the password is a common word, possibly with a few digits or symbols around it.</summary>
    /// <param name="password">The password.</param>
    /// <returns><see langword="true"/> if it is easily guessed.</returns>
    private static bool IsCommon(string password)
    {
        var plain = new string(password.Where(char.IsLetter).ToArray()).ToLowerInvariant();

        if (plain.Length == 0 || plain.Distinct().Count() <= 2)
        {
            return true;
        }

        // Also see through common letter substitutions (P@ssw0rd), trying 1 as both i and l. A number or symbols tacked
        // on the end (Password2027!) is the usual way to pad a common word, so it is dropped before looking.
        var core = password.TrimEnd(password.Where(c => !char.IsLetter(c)).Distinct().ToArray());

        return new[] { plain, Unleet(core, 'i'), Unleet(core, 'l') }.Any(IsCommonWordWithFewExtras);
    }

    /// <summary>Replaces common look-alike characters with the letters they stand for, then keeps only the letters.</summary>
    /// <param name="password">The password.</param>
    /// <param name="oneAs">The letter to use for the digit 1 (it can stand for i or l).</param>
    /// <returns>The lower-case letters.</returns>
    private static string Unleet(string password, char oneAs)
    {
        var mapped = password.Select(c => c switch
        {
            '@' or '4' => 'a',
            '3' => 'e',
            '0' => 'o',
            '$' or '5' => 's',
            '7' => 't',
            '1' => oneAs,
            _ => c,
        });

        return new string(mapped.Where(char.IsLetter).ToArray()).ToLowerInvariant();
    }

    /// <summary>Checks whether letters are a common word, or contain one with only a few extra letters around it.</summary>
    /// <param name="letters">Lower-case letters.</param>
    /// <returns><see langword="true"/> if easily guessed.</returns>
    private static bool IsCommonWordWithFewExtras(string letters)
    {
        if (CommonWords.Contains(letters))
        {
            return true;
        }

        return CommonWords.Any(word => word.Length >= 5 && letters.Contains(word, StringComparison.Ordinal) && letters.Length - word.Length <= MaxExtraCharactersAroundCommonWord);
    }
}