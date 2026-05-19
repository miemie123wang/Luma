using Luma.Localization;

var errors = TranslationValidator.ValidateAll();

if (errors.Count == 0)
{
    Console.WriteLine($"Translation validation passed for {TranslationValidator.SupportedCultures.Count} cultures: {string.Join(", ", TranslationValidator.SupportedCultures)}");
    return 0;
}

Console.Error.WriteLine($"Translation validation failed with {errors.Count} issue(s):");
foreach (var error in errors)
    Console.Error.WriteLine($"- {error}");

return 1;