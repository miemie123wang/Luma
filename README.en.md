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
- Uses a three-part advice flow: safe starting point, what to watch first, and what to adjust if the shot is not working
- Translates the same photography logic into different actions for phone, phone pro, APS-C, full frame, and action camera users
- Design notes are documented in [docs/shooting-advice-design.md](docs/shooting-advice-design.md)

### User Settings
All settings are stored in `localStorage`; no account is required.

- Camera type: phone, phone pro, APS-C, full frame, action camera
- Experience level: beginner, intermediate, professional
- UI language: English, Spanish, Simplified Chinese, Traditional Chinese

### Planner
The planner page exists as a placeholder. Full planning logic is not implemented yet.

---

## Planned Work

### Copy AI Prompt
- Generate a prompt from current light, weather, place, shooting style, camera type, experience level, and local advice
- Let users copy it into an external AI tool without Luma paying for an AI API
- Reuse the same local advice context instead of maintaining a separate prompt-only logic path

### Trip Planner
- Enter a location and date range
- Analyze daily light quality and weather
- Highlight the best shooting days and time windows

---

## Tech Stack

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
├── Layout/        # App layout and navigation
├── Localization/  # In-memory localization implementation and translations
├── Models/        # Location, light phase, weather, settings, and advice models
├── Pages/         # Home, planner, and settings pages
├── Services/      # Light phase, settings, shooting advice, SunCalc, and weather services
├── wwwroot/       # CSS, JavaScript interop, local SunCalc, and entry HTML
├── Program.cs     # App startup and service registration
└── Luma.csproj    # Project file
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
- [ ] Trip planner
- [ ] Copy AI Prompt
- [ ] GitHub Pages deployment

---

*Last updated: 2026-05-18*
