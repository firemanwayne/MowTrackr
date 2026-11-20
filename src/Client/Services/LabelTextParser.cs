using System.Text.RegularExpressions;

namespace MowTrackr.Client.Services;

/// <summary>
/// Picks the manufacturer, model and serial number out of the text an OCR engine read from an equipment label.
/// Labels vary a lot, so this is a best effort: the technician always checks the result before it is saved.
/// </summary>
internal static partial class LabelTextParser
{
    private const int MinimumSerialLength = 5;

    private const int MinimumModelLength = 3;

    private static readonly string[] Brands =
    [
        "Briggs & Stratton", "John Deere", "Cub Cadet", "Troy-Bilt", "Dixie Chopper", "Bad Boy", "Billy Goat",
        "Husqvarna", "Exmark", "Kawasaki", "Greenworks", "Shindaiwa", "Craftsman", "Simplicity", "Swisher",
        "Kubota", "Gravely", "Hustler", "Redmax", "Snapper", "Ferris", "Makita", "Poulan", "Murray", "Kohler",
        "Yamaha", "Ariens", "Bobcat", "Walker", "Honda", "Stihl", "Scag", "Toro", "Echo", "Ego", "Stiga",
    ];

    /// <summary>Reads the label text.</summary>
    /// <param name="text">The text recognized from the photo.</param>
    /// <returns>What was found; empty if nothing useful.</returns>
    public static LabelReading Parse(string? text)
    {
        var lines = (text ?? string.Empty)
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => WhitespacePattern().Replace(line.Trim(), " "))
            .Where(line => line.Length > 0)
            .ToList();

        // A machine's own serial/PIN beats an engine serial plate, which is only used when nothing else is found.
        var serial = FindValue(lines, SerialKeyPattern(), MinimumSerialLength, requireDigit: true, allowEngine: false)
            ?? FindProductIdentificationNumber(lines)
            ?? FindValue(lines, SerialKeyPattern(), MinimumSerialLength, requireDigit: true, allowEngine: true);

        var model = FindValue(lines, ModelKeyPattern(), MinimumModelLength, requireDigit: false, allowEngine: true);

        return new LabelReading(FindBrand(lines), model, serial);
    }

    /// <summary>Finds the value that follows a keyword such as "SERIAL NO." on the same line or the next one.</summary>
    /// <param name="lines">The label's lines.</param>
    /// <param name="key">Matches the keyword.</param>
    /// <param name="minimumLength">The shortest acceptable value.</param>
    /// <param name="requireDigit">Whether the value must contain a digit (serial numbers always do).</param>
    /// <param name="allowEngine">Whether a keyword line mentioning the engine may be used.</param>
    /// <returns>The value, or <see langword="null"/>.</returns>
    private static string? FindValue(List<string> lines, Regex key, int minimumLength, bool requireDigit, bool allowEngine)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            var match = key.Match(lines[i]);

            if (!match.Success)
            {
                continue;
            }

            if (!allowEngine && lines[i].Contains("engine", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var candidate = FirstToken(lines[i][(match.Index + match.Length)..]);

            if (candidate is null && i + 1 < lines.Count)
            {
                candidate = FirstToken(lines[i + 1]);
            }

            if (candidate is not null && candidate.Length >= minimumLength && (!requireDigit || candidate.Any(char.IsDigit)))
            {
                return candidate;
            }
        }

        return null;
    }

    /// <summary>Looks for a bare 17-character identification number (the VIN-style PIN on tractors and zero-turns) anywhere on the label.</summary>
    /// <param name="lines">The label's lines.</param>
    /// <returns>The number, or <see langword="null"/>.</returns>
    private static string? FindProductIdentificationNumber(List<string> lines)
    {
        foreach (var line in lines)
        {
            foreach (Match match in IdentificationNumberPattern().Matches(line))
            {
                if (match.Value.Any(char.IsDigit) && match.Value.Any(char.IsLetter))
                {
                    return match.Value.ToUpperInvariant();
                }
            }
        }

        return null;
    }

    /// <summary>Takes the first value-like word, skipping separators and a leading "no." or "#".</summary>
    /// <param name="text">The text after a keyword.</param>
    /// <returns>The word, or <see langword="null"/>.</returns>
    private static string? FirstToken(string text)
    {
        var stripped = LeadingNoisePattern().Replace(text, string.Empty);

        var match = TokenPattern().Match(stripped);

        if (!match.Success)
        {
            return null;
        }

        var token = match.Value.TrimEnd('.', '-', '/');

        return KeywordOnlyPattern().IsMatch(token) ? null : token;
    }

    /// <summary>Finds the first known brand name in the text.</summary>
    /// <param name="lines">The label's lines.</param>
    /// <returns>The brand with its usual capitalization, or <see langword="null"/>.</returns>
    private static string? FindBrand(List<string> lines)
    {
        var joined = string.Join(' ', lines);

        string? best = null;

        var bestIndex = int.MaxValue;

        foreach (var brand in Brands)
        {
            var index = joined.IndexOf(brand, StringComparison.OrdinalIgnoreCase);

            if (index < 0 || index >= bestIndex || !IsWholeWord(joined, index, brand.Length))
            {
                continue;
            }

            best = brand;

            bestIndex = index;
        }

        return best;
    }

    /// <summary>Checks that a match is not part of a longer word (so "Ego" is not found inside "Cargo").</summary>
    /// <param name="text">The text.</param>
    /// <param name="index">Where the match starts.</param>
    /// <param name="length">The match length.</param>
    /// <returns><see langword="true"/> if the match stands alone.</returns>
    private static bool IsWholeWord(string text, int index, int length)
    {
        var before = index == 0 || !char.IsLetterOrDigit(text[index - 1]);

        var after = index + length >= text.Length || !char.IsLetterOrDigit(text[index + length]);

        return before && after;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespacePattern();

    [GeneratedRegex(@"\b(?:serial|ser\.?|s/n|s\.n\.|sn|(?:product\s+)?identification|pin)(?=$|[\s:#.\-]|no)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SerialKeyPattern();

    [GeneratedRegex(@"\b(?:model|mod\.?|m/n|tractor)(?=$|[\s:#.\-]|no)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ModelKeyPattern();

    [GeneratedRegex(@"^[^A-Za-z0-9]*(?:(?:no|number|num)\b\.?[^A-Za-z0-9]*)?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex LeadingNoisePattern();

    [GeneratedRegex(@"(?<![A-Za-z0-9])[A-Za-z0-9]{17}(?![A-Za-z0-9])")]
    private static partial Regex IdentificationNumberPattern();

    [GeneratedRegex(@"^[A-Za-z0-9][A-Za-z0-9\-/.]*")]
    private static partial Regex TokenPattern();

    [GeneratedRegex(@"^(?:model|serial|no|number|type)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex KeywordOnlyPattern();
}