# Luma 🌅
> A light assistant for travel photographers

---

## Project Overview

Luma is a light assistant app aimed at travel photographers. It helps users find the best shooting times and lighting conditions while traveling.

The target users are photographers on the go who want to catch good light without spending lots of time researching.

### Differentiation

| Existing tools (utility) | Luma (assistant) |
|---|---|
| PhotoPills, The Photographer's Ephemeris, GoldenHour.One | Proactively combines itinerary + weather + light |
| Tells you "sunrise at X time" | Tells you "which day and time during this trip is worth getting up early to shoot" |

---

## Core Features

### 1. Realtime Light Assistant
- Automatically detects user's current time and position
- Shows the current light phase (blue hour / golden hour / noon / sunset)
- Tells the user what to shoot now and when the next good window is
- Gives parameter suggestions based on equipment and experience

### 2. Planner
- Enter places + date ranges
- Analyze light quality and weather for each day
- Highlight best photo days and time windows

---

## User Settings

All settings are stored in `localStorage`, no account required.

---

## Tech Stack

| Module | Choice |
|---|---|
| Framework | Blazor WASM |
| UI | MudBlazor |
| Weather | Open-Meteo |
| Sun times | SunCalc (JS) |
| User data | localStorage |

---

## Local Development

Requirements:
- .NET 9 SDK
- VS Code

Start locally:
```powershell
git clone https://github.com/miemie123wang/Luma.git
cd Luma/Luma
dotnet run
```

Open the browser at the address shown by `dotnet run` (e.g., `http://localhost:5284`).

---

*Last updated: 2026-05-18*
