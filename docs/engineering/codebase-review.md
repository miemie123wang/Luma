# Codebase Review Notes

Last reviewed: 2026-05-19

This document records structural cleanup opportunities found during a project scan. It is not a bug list. Use it to decide what to clean before the project grows.

## Overall State

The project is small and understandable. The main domain services are separated reasonably:

- `LightPhaseService`: light phase classification.
- `SunCalcService`: SunCalc, browser geolocation, and reverse geocoding interop.
- `WeatherService`: Open-Meteo current weather.
- `SettingsService`: localStorage settings.
- `ShootingAdviceService`: local rule-based photography advice.

The largest risk is not broken code. It is gradual drift: oversized page components, repeated UI patterns, and runtime-only localization problems.

Cleanup completed on 2026-05-19:

- Removed the unused standalone `NavMenu.razor` / `NavMenu.razor.css`; navigation is currently owned by `MainLayout.razor`.
- Removed unused Bootstrap static assets and unused Bootstrap-style global CSS helpers from `wwwroot/css/app.css`.
- Added `TranslationValidator` and `tools/Luma.LocalizationCheck` to catch missing translation keys and incompatible format placeholders.
- Extracted AI prompt generation from `Home.razor.cs` into `AiPromptBuilder`.
- Extracted the Home phase and detail cards into small child components.

## Highest-Value Cleanup

### 1. Home Page Responsibility Size

`Home.razor` and `Home.razor.cs` currently handle:

- Current light loading.
- Weather loading.
- Location and geocoding warnings.
- Shooting context state.
- Advice generation.
- AI prompt generation.
- Clipboard status.
- Large UI card composition.

Why it matters:

- The page is still manageable, but advice/prompt work will keep growing.
- Testing prompt generation and advice context will be harder while they live inside the page component.

Current split:

- AI prompt generation now lives in `AiPromptBuilder`.
- Current phase and location/weather details now live in small Home child components.

Recommended next split:

- Extract the advice card and capture-context controls into child components when the UI changes again.
- Keep data loading in the page until the app has more pages that need the same context.

### 2. Localization Guardrails

`Translations.cs` is a large in-memory dictionary for four languages. Missing keys can fall back silently, and placeholder mismatches are only found at runtime.

Why it matters:

- Advice text is changing quickly.
- Missing keys or wrong `{0}` placeholders can slip through visual testing.

Current validation:

- Run `dotnet run --project tools/Luma.LocalizationCheck/Luma.LocalizationCheck.csproj` from the repository root.
- The check validates that every language has the same keys as English.
- The check validates compatible `{0}`, `{1}`-style placeholders across languages.

Recommended next validation:

- Add targeted tests for advice generation in all supported cultures.

### 3. Culture Startup Has Two Paths

Culture is read in JavaScript before `Blazor.start`, and `Program.cs` also reads `localStorage` to set culture before `RunAsync`.

Related files:

- `Luma/wwwroot/index.html`
- `Luma/wwwroot/js/blazorCulture.js`
- `Luma/Program.cs`

Why it matters:

- Two startup paths can drift.
- It is harder to reason about which source is authoritative.

Recommended action:

- Pick one source of truth.
- Document the culture startup flow in engineering docs once chosen.

## Medium-Priority Improvements

### Error Reporting

Several places intentionally swallow exceptions or log only to console.

Examples:

- `Program.cs`: culture setup fallback.
- `SettingsService.cs`: localStorage fallback.
- `WeatherService.cs`: `Console.WriteLine` and null return.
- `MainLayout.razor`: language persistence fallback.

This is acceptable for an MVP, but production debugging will be easier with scoped logging or user-visible fallback reasons where appropriate.

### UI Style Duplication

Several pages repeat page title, card, and inline style patterns. This is okay while the app is small, but if Planner becomes real, move repeated visual patterns into shared classes or small components.

Examples:

- `Home.razor`
- `Settings.razor`
- `Planner.razor`
- `MainLayout.razor`

### Static HTML Localization

`wwwroot/index.html` has fixed `lang="zh"` and Chinese fallback error text. `App.razor` also has an English-only NotFound message.

Recommended action:

- Use a neutral default language or align it with startup culture where practical.
- Localize NotFound once routing grows.

### Package Version Alignment

The app targets `net9.0`, while `Microsoft.Extensions.Http` is referenced as `10.0.8`. The build currently succeeds, but aligning Microsoft package versions with the target framework reduces dependency surprise.

Recommended action:

- Check whether `Microsoft.Extensions.Http` is needed explicitly.
- If needed, consider using the matching `9.0.x` line for consistency.

## Low-Priority Cleanup

- `Advice_StartingPoint_Title` remains in translations after the advice UI switched to `Advice_FirstShot_Title`. Remove it after confirming no historical fallback needs it.
- `OptionItem<T>` is duplicated in `Home.razor.cs` and `Settings.razor.cs`. Keep for now; extract only if another page needs the same pattern.
- `sample-data/weather.json` appears unused. Keep if it is intended for future offline/demo work; otherwise remove later.

## Suggested Order

1. Add advice audit case generation.
2. Normalize culture startup.

## Do Not Rush

Do not refactor everything at once. The app is still small. Prefer one cleanup per feature pass, and keep behavior stable while the advice product direction is still changing.
