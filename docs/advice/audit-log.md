# Advice Audit Log

Category index: [Advice Docs](README.md)

Workflow reference: [Advice Audit Workflow](audit.md)

This file records what we learned while auditing Luma's local shooting advice. It is more detailed than the workflow doc on purpose: use it as the working memory for future advice quality passes.

## Status

First-layer high-risk audit is complete.

Scope completed:

- Generated 7 high-risk advice outputs with `tools/Luma.AdviceAudit`.
- Added `--out` support so audit outputs can be written to local review files.
- Reviewed those outputs with the audit prompt.
- Fixed all `Wrong` findings.
- Fixed high-value `Risky` findings that affected common or confusing paths.
- Ran a targeted re-audit.
- Cleaned up remaining minor wording/routing issues from the targeted re-audit.

Validation completed:

```powershell
dotnet run --project .\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj
dotnet run --project .\tools\Luma.LocalizationCheck\Luma.LocalizationCheck.csproj
dotnet build .\Luma\Luma.csproj
```

Local generated output and external review files should live under:

```text
docs/advice/generated/
```

This folder is ignored by git. Use it for temporary review artifacts such as:

```text
high-risk-output.md
high-risk-review.md
regression-output.md
regression-review.md
```

The useful findings from those files should be summarized in this log before committing code or documentation changes.

## Why This Audit Exists

The local advice is one of the most user-facing parts of Luma. If it is wrong, the app can feel confident but untrustworthy.

The goal of audit is not to make every suggestion perfect. The goal is to catch advice that is obviously wrong, mismatched to the user's device, unsafe for the light level, or internally contradictory.

## First-Layer Review Summary

Initial review result:

- 2 OK.
- 4 Risky.
- 1 Wrong.

Main systemic issue:

Generic fallback bullets were appearing beside scenario-specific bullets. This was worse than a simple missing rule because the correct specific branch could be present while a generic bullet gave a conflicting priority.

Examples:

- A handheld night landscape could correctly warn about stability while also showing a generic low-ISO landscape starting point.
- A foggy blue-hour urban scene could get a full-night handheld exposure template while also showing a fog note later.

## Fixes Applied

### First Test Focus

When a handheld low-light feasibility warning is active for landscape or night sky, `First test shot` now stays focused on the matching exposure branch. It no longer appends generic safe-start and device-operation bullets.

Reason:

If the app says the plan is difficult or unreliable, the next line should not add a generic path that sounds equally recommended.

### Blue Hour Camera Exposure

Blue-hour handheld camera scenes now have their own exposure branch instead of reusing full-night handheld exposure.

Current intent:

- Blue hour is dim, but not full night.
- Full-frame handheld blue hour can start around `ISO 800-1600`, `f/2.8-f/4`, and about `1/80s` for still subjects.
- Moving subjects still need a faster shutter or an explicit tradeoff.
- Fog and signs should not be over-brightened.

### Night Sky Tripod Risk

Night sky with tripod now leads with star trailing, missed focus, and noisy shadows instead of blown highlights.

Reason:

Clear weather is good for night sky, but the generic clear-sky rule pushed the risk toward daylight highlight language. For night sky, focus, exposure length, and noise are more important.

### Fog And Low Visibility Priority

Fog or low visibility now takes priority over generic handheld blur when selecting the leading risk, unless a higher-priority risk such as moving subject applies.

Reason:

Fog changes the visual problem. The user needs to think about flat contrast, shape, and diffusion before simply brightening the image.

### Moving Beginner Feasibility

Beginner moving-subject scenarios now get a short feasibility note about burst or continuous capture.

Reason:

Moving subjects are a real edge case for beginners, especially in mixed or changing light. The app should say this plainly before giving ordinary portrait or scene advice.

### Action Cam Night Moving Warning

Action cam + full night + moving subject now gets a feasibility warning.

Reason:

This combination has a high failure rate. The user should expect noise and blur and should look for street lights and controlled movement.

### Device-Matched Moving Advice

Phone and action cam moving-subject advice now uses burst, action mode, sport mode, or video language instead of a manual `1/500s` shutter-speed instruction.

Reason:

`1/500s` is useful for interchangeable-lens camera users, but it is device-mismatched for auto-exposure phone and action cam users.

### Night Sky Weather Notes

Generic clear-sky weather notes are suppressed for night sky cases.

Reason:

The daylight clear-sky note talks about strong direction and highlight control. That is not the right priority for night sky.

## Case Outcomes

### Case 1: Night Handheld Landscape

Initial verdict: Risky.

Issue:

First test shot mixed the correct handheld branch with generic low-ISO fallback bullets.

Fix:

Keep first test focused when handheld low-light feasibility is active.

Status:

Resolved.

### Case 2: Night Sky With Tripod

Initial verdict: Risky.

Issue:

The leading risk was blown highlights because clear weather reused a daylight risk path.

Fix:

Add night-sky-specific risk and adjustment text for tripod night sky.

Targeted re-audit note:

The remaining generic still-subject tripod bullet about low ISO and depth of field was also removed for night sky tripod.

Status:

Resolved.

### Case 3: Night Sky Handheld

Initial verdict: OK.

Optional issue:

The first version still included a daylight-style clear-sky weather note.

Fix:

Suppress generic clear-sky weather notes for night sky cases.

Status:

Resolved enough for first layer. Future wording can mention laying the phone flat or propping it securely.

### Case 4: Golden Hour Moving Portrait

Initial verdict: Risky.

Issue:

Beginner with moving subject had no feasibility note.

Fix:

Add beginner moving-subject feasibility note.

Targeted re-audit note:

The `1/500s` moving-subject note was too manual for Phone Pro beginner use. It now uses burst/action/sport/video language for phones and action cams.

Status:

Resolved.

### Case 5: Midday Landscape

Initial verdict: OK.

Issue:

None.

Fix:

None.

Status:

Keep as-is.

### Case 6: Foggy Blue Hour Urban

Initial verdict: Wrong.

Issue:

Blue hour reused the full-night handheld exposure template and fog was not treated as the leading concern.

Fix:

Add blue-hour handheld camera exposure branch and prioritize contrast/fog risk for low visibility.

Status:

Resolved.

### Case 7: Action Cam Night Urban

Initial verdict: Risky.

Issue:

Action cam in full night with moving subject had no feasibility warning.

Fix:

Add action-cam night moving-subject feasibility note.

Targeted re-audit note:

The `1/500s` moving-subject note was device-mismatched. It now uses burst/action/sport/video language for phones and action cams.

Status:

Resolved.

## Current Rule Principles Learned

Use these as guardrails before changing `ShootingAdviceService` again:

- Feasibility should come before parameter confidence.
- If feasibility says a plan is difficult, avoid adding generic fallback bullets that sound equally recommended.
- Light phase should branch before shooting style when the phase changes exposure reality.
- Weather can become the main visual problem; fog and low visibility are not just extra notes.
- Device language matters as much as parameter correctness.
- Phones and action cams should usually get operational modes and capture tactics, not manual shutter-speed instructions.
- Night sky should prioritize stability, focus, exposure length, star trailing, and noise.
- Clear weather means different things in daylight and night sky.

## Second-Layer Audit Plan

The next layer should expand from 7 high-risk cases to about 24-36 representative regression cases.

Do not generate the full matrix yet. The full matrix is useful only when checking for missing branches, duplicate text, or unexpected collisions.

Recommended second-layer groups:

- Low light handheld camera: landscape, urban, portrait.
- Low light tripod camera: landscape, urban, night sky.
- Phone basic: daylight, low light, moving subject.
- Phone Pro: portrait, night sky, moving subject.
- Action cam: daylight harsh light, night urban, moving subject.
- Weather modifiers: clear, heavy cloud, fog, rain, mixed cloud.
- Experience modifiers: beginner step depth, intermediate balance, professional concision.
- Support/motion modifiers: handheld still, handheld moving, tripod still.

Suggested case count:

- 12 cases for light and style coverage.
- 8 cases for device-specific language.
- 6 cases for weather modifiers.
- 4-10 cases for experience/support/motion edge cases.

## Next Tooling Ideas

Useful additions to `tools/Luma.AdviceAudit` later:

- Add `--set high-risk` and `--set regression` switches.
- Add `--culture en|es|zh-Hans|zh-Hant` for translation spot checks.
- Add richer markdown output with stable headings and optional metadata for each case.
- Add a `--review-template` option that writes an empty review file next to the generated output.
- Add a simple duplicate-line detector to find repeated generic bullets.
- Add a device-language check that flags shutter-speed strings in phone/action-cam moving-subject cases.

## Commit Boundary Guidance

Good commit boundaries for future advice work:

- Commit audit tooling separately when possible.
- Commit one audit layer and its fixes together when the fixes are small and directly tied to the findings.
- Avoid mixing advice rule changes with UI refactors.
- Update this log when a review result directly changes a rule.