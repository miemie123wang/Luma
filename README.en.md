# Luma 🌅
> A light assistant for travel photographers

---

## Project Overview

Luma is a light assistant app for travel photographers. It helps users understand the current light phase, local weather, and the next useful shooting window while traveling.

The target users are photographers on the go who want to catch good light without spending lots of time researching.

### Differentiation

| Existing tools (utility) | Luma (assistant) |
|---|---|
| PhotoPills, The Photographer's Ephemeris, GoldenHour.One | Combines location + weather + light |
| Tells you "sunrise at X time" | Tells you "what the light is like now and when it gets better" |

---

## Current Features

### Realtime Light Assistant
- Gets the user's current browser location
- Uses SunCalc to calculate sunrise, sunset, blue hour, golden hour, and related solar times
- Classifies the current light phase and shows its name, description, next phase, and 1-5 shooting rating
- Uses Open-Meteo for cloud cover, precipitation, weather condition, temperature, wind speed, and visibility
- Uses OpenStreetMap Nominatim for reverse geocoding
- Shows a high-altitude warning when relevant
- Lets the user choose the current shooting style, support mode (handheld / tripod), and subject motion (still / moving) on the home page; these choices are not stored as long-term settings

### Local Shooting Advice
- Uses local rule-based logic with no AI API or paid service required
- Generates advice from current light, weather, shooting style, camera type, experience level, support mode, and subject motion
- Uses a modular advice flow: feasibility warning, first test shot, what to watch first, what to adjust if the shot is not working, and beginner-only steps
- Starts with conservative test settings: camera users get ISO, aperture, shutter speed, and exposure compensation; phone users get lens, mode, exposure action, and stability guidance
- Low light and night scenes branch first by light phase and handheld/tripod support, so night scenes do not reuse daylight landscape settings; handheld night scenes prioritize stable support
- Translates the same photography logic into different actions for phone, phone pro, APS-C, full frame, and action camera users
- Experience level changes the operating mode and explanation depth: beginners get safer A/Av guidance, intermediate users can use A/Av or M, and professional users get RAW/manual/bracketing-oriented tradeoffs
- Docs are organized by feature area. Advice design lives in [docs/advice/design.md](docs/advice/design.md), and the AI review workflow lives in [docs/advice/audit.md](docs/advice/audit.md)

### Copy AI Prompt
- The advice card includes a `Copy AI prompt` button in the top-right corner
- Copies current time, light phase, place, weather, camera, experience level, shooting style, support mode, and subject motion
- Copies only the real context, not the local hard-coded advice, so external AI tools can reason independently
- Shows a short copied/failed status that disappears automatically

### User Settings
All settings are stored in `localStorage`; no account is required.

- Camera type: phone, phone pro, APS-C, full frame, action camera
- Experience level: beginner, intermediate, professional
- UI language: English, Spanish, Simplified Chinese, Traditional Chinese

### Planner
The planner page exists as a placeholder. Full planning logic is not implemented yet.

---

## Future Work

### Shooting Advice Improvements
- Add focal length context: wide, standard, telephoto
- Add maximum aperture context: f/1.8, f/2.8, f/4, f/5.6
- Add RAW / JPEG preference
- Adjust advice based on remaining time before sunrise or sunset
- Split broad styles into more specific scenes, such as street, architecture, traffic trails, single/group/child portraits, moon phase, and light pollution

### Trip Planner
- Enter a location and date range
- Analyze daily light quality and weather
- Highlight the best shooting days and time windows

---

## Tech Stack

See [docs/README.md](docs/README.md) for the categorized documentation index.

| Module | Choice | Notes |
|---|---|---|
| Framework | Blazor WebAssembly / .NET 9 | Static frontend app |
| UI | MudBlazor | Dark Material Design UI |
| Weather | Open-Meteo | Free, no API key |
| Sun times | Local SunCalc + C# services | Solar times in JS, phase classification in C# |
| Location | Browser Geolocation API | Via JS interop |
| Place names | OpenStreetMap Nominatim | Reverse geocoding; falls back to coordinates when unavailable |
| User data | localStorage | No account required |
| Localization | Custom `IStringLocalizer` + `Translations.cs` | English, Spanish, Simplified Chinese, Traditional Chinese |
| Hosting | GitHub Pages (planned) | Static deployment |

---

## Project Structure

```text
Luma/
├── Components/    # Feature-specific UI components
├── Layout/        # App layout and navigation
├── Localization/  # In-memory localization implementation and translations
├── Models/        # Location, light phase, weather, settings, and advice models
├── Pages/         # Home, planner, and settings pages
├── Services/      # Light phase, settings, shooting advice, SunCalc, and weather services
├── wwwroot/       # CSS, JavaScript interop, local SunCalc, and entry HTML
├── Program.cs     # App startup and service registration
└── Luma.csproj    # Project file

tools/
├── Luma.AdviceAudit/       # High-risk shooting advice output generator
└── Luma.LocalizationCheck/ # Localization key and placeholder validator
```

The root `global.json` pins the SDK to .NET 9 to avoid local .NET 10 SDK differences.

---

## Local Development

Requirements:
- .NET 9 SDK
- VS Code + C# Dev Kit

Start locally:
```powershell
git clone https://github.com/miemie123wang/Luma.git
cd Luma/Luma
dotnet run
```

The default development URL is `http://localhost:5284`.

Build check:
```powershell
cd Luma/Luma
dotnet build
```

Localization check:
```powershell
cd Luma
dotnet run --project tools/Luma.LocalizationCheck/Luma.LocalizationCheck.csproj
```

This verifies that every language has the same translation keys and compatible `{0}`, `{1}`-style format placeholders.

Advice audit output:
```powershell
cd Luma
dotnet run --project .\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj
```

If your terminal is already in the `Luma/Luma` app folder, use:

```powershell
dotnet run --project ..\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj
```

---

## MVP Status

- [x] Blazor WASM + MudBlazor foundation
- [x] Dark theme with warm golden-hour accents
- [x] Page structure for current light, planner, and settings
- [x] SunCalc JS interop
- [x] Current light phase calculation
- [x] Browser geolocation
- [x] User settings + localStorage
- [x] Open-Meteo weather integration
- [x] Localization for English, Spanish, Simplified Chinese, and Traditional Chinese
- [x] Local rule-based shooting advice
- [x] Copy AI Prompt
- [ ] Trip planner
- [ ] GitHub Pages deployment

---

*Last updated: 2026-05-19*
