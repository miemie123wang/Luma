using System.Globalization;
using Microsoft.Extensions.Localization;

namespace Luma.Localization;

public class InMemoryStringLocalizer : IStringLocalizer<SharedResource>
{
    public LocalizedString this[string name]
    {
        get
        {
            var value = Lookup(CultureInfo.CurrentUICulture.Name, name);
            return new LocalizedString(name, value ?? name, value is null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var ls = this[name];
            if (!ls.ResourceNotFound)
                return new LocalizedString(name, string.Format(ls.Value, arguments), false);
            return ls;
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var dict = GetDict(CultureInfo.CurrentUICulture.Name);
        return dict?.Select(kv => new LocalizedString(kv.Key, kv.Value, false))
               ?? [];
    }

    private static string? Lookup(string cultureName, string key)
    {
        var dict = GetDict(cultureName);
        if (dict is not null && dict.TryGetValue(key, out var val))
            return val;

        // Walk up the culture hierarchy (e.g. zh-CN → zh-Hans → en)
        var dash = cultureName.LastIndexOf('-');
        if (dash > 0)
            return Lookup(cultureName[..dash], key);

        // Final fallback: English
        if (cultureName != "en" && Translations.All.TryGetValue("en", out var en) && en.TryGetValue(key, out var enVal))
            return enVal;

        return null;
    }

    private static IReadOnlyDictionary<string, string>? GetDict(string culture) =>
        Translations.All.TryGetValue(culture, out var d) ? d : null;
}
