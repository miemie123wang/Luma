using System.Text.RegularExpressions;

namespace Luma.Localization;

public static partial class TranslationValidator
{
    public static IReadOnlyList<string> SupportedCultures => Translations.All.Keys.Order().ToArray();

    public static IReadOnlyList<string> ValidateAll(string referenceCulture = "en")
    {
        var errors = new List<string>();

        if (!Translations.All.TryGetValue(referenceCulture, out var reference))
        {
            errors.Add($"Reference culture '{referenceCulture}' is missing.");
            return errors;
        }

        foreach (var (culture, translations) in Translations.All.OrderBy(item => item.Key))
        {
            ValidateKeys(referenceCulture, reference, culture, translations, errors);
            ValidatePlaceholders(referenceCulture, reference, culture, translations, errors);
        }

        return errors;
    }

    public static void ThrowIfInvalid(string referenceCulture = "en")
    {
        var errors = ValidateAll(referenceCulture);
        if (errors.Count > 0)
            throw new InvalidOperationException("Translation validation failed:" + Environment.NewLine + string.Join(Environment.NewLine, errors));
    }

    private static void ValidateKeys(
        string referenceCulture,
        IReadOnlyDictionary<string, string> reference,
        string culture,
        IReadOnlyDictionary<string, string> translations,
        List<string> errors)
    {
        var referenceKeys = reference.Keys.ToHashSet(StringComparer.Ordinal);
        var cultureKeys = translations.Keys.ToHashSet(StringComparer.Ordinal);

        foreach (var missingKey in referenceKeys.Except(cultureKeys).Order())
            errors.Add($"[{culture}] Missing key from {referenceCulture}: {missingKey}");

        foreach (var extraKey in cultureKeys.Except(referenceKeys).Order())
            errors.Add($"[{culture}] Extra key not present in {referenceCulture}: {extraKey}");
    }

    private static void ValidatePlaceholders(
        string referenceCulture,
        IReadOnlyDictionary<string, string> reference,
        string culture,
        IReadOnlyDictionary<string, string> translations,
        List<string> errors)
    {
        foreach (var (key, referenceValue) in reference.OrderBy(item => item.Key))
        {
            if (!translations.TryGetValue(key, out var value))
                continue;

            var expected = GetPlaceholderSignature(referenceValue);
            var actual = GetPlaceholderSignature(value);

            if (!expected.SetEquals(actual))
            {
                errors.Add(
                    $"[{culture}] Placeholder mismatch for {key}; {referenceCulture}=({string.Join(", ", expected.Order())}), {culture}=({string.Join(", ", actual.Order())})");
            }
        }
    }

    private static ISet<int> GetPlaceholderSignature(string value)
    {
        return PlaceholderRegex()
            .Matches(value)
            .Select(match => int.Parse(match.Groups[1].Value))
            .ToHashSet();
    }

    [GeneratedRegex(@"(?<!\{)\{(\d+)(?:[^{}]*)\}(?!\})")]
    private static partial Regex PlaceholderRegex();
}