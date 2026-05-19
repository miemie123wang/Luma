# Luma Shooting Advice Design

## Goal

Luma should give real-time photography advice without relying on an AI API in the MVP stage.

The goal is not to hard-code one exact camera setting. The goal is to hard-code a reliable photography decision process:

```text
Based on current light and shooting style, give a safe starting point.
Based on the user's device, translate that into usable actions.
Based on weather, warn about exposure, stability, and contrast risks.
Based on experience level, adjust explanation depth.
If the result is wrong, tell the user what to change first.
```

This keeps the advice useful while avoiding overconfident recommendations such as a single fixed ISO, aperture, and shutter speed.

## Product Principle

Luma should sound like a practical photography assistant, not a parameter calculator.

Avoid advice like:

```text
Best settings: ISO 200, f/2.8, 1/250s.
```

Prefer advice like:

```text
Start by exposing for the face. If the highlights clip, lower exposure first. If the subject blurs, raise shutter speed or stabilize the camera before trying to lower ISO.
```

The app does not know the user's exact lens, focal length, maximum aperture, hand stability, subject speed, or creative intent. Therefore, advice should use safe ranges, priorities, and adjustment order.

## Inputs

### Real-Time Context

- Current light phase
- Weather: cloud cover, precipitation, wind speed, visibility, temperature
- Location and altitude
- Shooting style selected on the Home page
- Camera support mode selected on the Home page: handheld or tripod
- Subject motion selected on the Home page: still or moving

### Persisted Settings

- Camera type
- Experience level
- Language

Shooting style, support mode, and subject motion should stay on the Home page. They are session/context choices, not long-term user settings.

## Output Structure

The advice card should have three user-facing sections.

### Safe Starting Point

What to try first so the user gets a usable shot.

Examples:

```text
Start by exposing for the face, then use side light or backlight to add shape without losing skin detail.
```

```text
Start with a stable frame, low ISO if possible, and protect the brightest sky detail before lifting shadows.
```

### Watch First

The most likely failure mode in the current context.

Examples:

```text
The main risk is blur, so protect shutter speed or stability before trying to reduce noise.
```

```text
The main risk is blown highlights, especially sky, skin, wet ground, or bright signs.
```

### If It Is Not Working

The first adjustment the user should make.

Examples:

```text
If it looks blurry, raise shutter speed, stabilize the camera, or accept higher ISO before slowing the shutter.
```

```text
If highlights clip, lower exposure first; then change angle or add light to the subject instead of brightening everything.
```

## Decision Flow

The first version should stay intentionally small.

```text
GetPrimaryRisk()
GetSafeStartingPoint()
GetDeviceOperation()
GetWeatherNote()
GetAdjustmentStep()
GetExperienceNote()
```

Each method should do one thing. Avoid building a large rule engine until the product direction is clearer.

## Primary Risks

Start with four risk categories.

### Blur Risk

Triggered by:

- Handheld + low light
- Moving subject
- Night sky without tripod

Advice priority:

1. Protect shutter speed or stability.
2. Accept higher ISO if needed.
3. Do not slow shutter first unless using a tripod or intentionally creating motion blur.

### Highlight Risk

Triggered by:

- Harsh daylight
- Clear sky
- Bright sky, skin, wet ground, or signs

Advice priority:

1. Lower exposure first.
2. Change angle or move subject.
3. Add light to the subject instead of brightening the whole scene.

### Noise Risk

Triggered by:

- Low light
- Night scenes
- Small-sensor devices in dark conditions

Advice priority:

1. Add stability or light first.
2. Use longer exposure only if stable.
3. Lower ISO after the image is stable enough.

### Contrast Risk

Triggered by:

- Heavy cloud cover
- Fog or low visibility
- Flat light

Advice priority:

1. Simplify the background.
2. Use stronger subject shape.
3. Add light/dark separation through position or composition.

## Safe Starting Point Rules

Safe starting points should mostly depend on light phase and shooting style.

### Portrait

Default:

```text
Start by exposing for the face, then use side light or backlight to add shape without losing skin detail.
```

Harsh light:

```text
Start in open shade or turn the face away from direct sun; keep skin highlights under control before chasing a bright background.
```

### Landscape

Default:

```text
Start with a clean foreground and a controlled sky; keep ISO low and choose depth of field before adjusting brightness.
```

Low light:

```text
Start with a stable frame, low ISO if possible, and protect the brightest sky detail before lifting shadows.
```

### Urban

Default:

```text
Start by finding a strong light/shadow edge, then wait for a subject to enter the frame.
```

Low light:

```text
Start around bright signs, windows, or street lights, and keep highlights from clipping before brightening shadows.
```

### Night Sky

```text
Start with stability, manual focus near infinity, and one short test exposure before committing to a long shot.
```

## Device Translation

The same photography logic should be translated differently for different devices.

### Phone / Phone Pro

Use operational language:

- Use 1x or 2x.
- Keep HDR on.
- Tap to focus/expose.
- Lower exposure slightly if the sky is bright.
- Use night mode or Pro mode in low light.
- Avoid digital zoom, especially in low light.

### APS-C / Full Frame

Use parameter ranges:

- ISO 100-400 as a starting point.
- Portrait: f/2.8-f/4 or wider if needed.
- Landscape: f/8-f/11.
- Protect shutter speed for handheld or moving subjects.
- Use RAW when useful.
- Use exposure compensation when highlights are at risk.

### Action Cam

Use reliability advice:

- Keep stabilization on.
- Use wide mode.
- Avoid low light when possible.
- Avoid long direct shots into the sun.
- Keep the lens clean, especially in rain or near water.

## Weather Modifiers

Weather should modify the main advice, not replace it.

### Rain

```text
Rain can add reflections; protect the lens and watch for bright highlights on wet ground.
```

### Fog / Low Visibility

```text
Low visibility simplifies backgrounds; increase contrast slightly and keep a strong subject shape.
```

### Heavy Cloud

```text
Cloud cover softens shadows; portraits and detail shots will be easier than dramatic landscapes.
```

### Clear Sky

```text
Clear sky gives strong direction; control highlights and use shadows deliberately.
```

### Mixed Cloud

```text
Mixed cloud can change quickly; take a test shot and be ready when light breaks through.
```

## Experience Level

Experience level should change explanation depth, not photography facts.

### Beginner

```text
Keep it simple: take one safe shot first, then adjust exposure or composition.
```

### Intermediate

```text
Try one exposure variation and one composition variation before leaving the spot.
```

### Professional

```text
Use RAW, protect highlight detail, and make a deliberate technical tradeoff for the final look.
```

## Home Page Controls

The Home page should expose lightweight context choices.

### Shooting Style

- Landscape
- Urban
- Portrait
- Night Sky

### Camera Support Mode

- Handheld
- Tripod

### Subject Motion

- Still
- Moving

These should be single-choice controls. They are not persisted to settings.

## Why This Is Reliable

This system avoids pretending to know unavailable details:

- Exact lens
- Exact focal length
- Maximum aperture
- Real subject speed
- User hand stability
- Creative intent
- Whether the user can use RAW or manual mode

Instead, Luma gives:

- A safe starting point
- The most likely failure risk
- Device-specific操作 language
- The first adjustment to make when the shot is wrong

## Future Expansion

Do not start with a full rule engine. Keep the first version in C# methods.

Possible future steps:

1. Add more shooting styles, such as food, wildlife, architecture, and video.
2. Add a generated prompt that users can copy into an external AI tool.
3. Split hard-coded rules into data tables only after the rules become hard to maintain.
4. Track which advice sections users copy or interact with most.
5. Add optional Pro/BYOK AI advice only after product validation.

## One-Sentence Summary

Luma should not hard-code answers. It should hard-code a practical photography decision process: what is likely to fail, where to start safely, how this device should be used, and what to adjust first.
