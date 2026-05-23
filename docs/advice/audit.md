# Advice Audit Workflow

This workflow is for checking whether Luma's hard-coded local advice is reasonable enough. It is not a replacement for unit tests. It is a lightweight product review process for catching advice that is obviously wrong, misleading, or mismatched to the user's device.

## Goal

Use a small set of representative cases to review the exact text Luma generates.

We are not asking an AI to redesign the whole rule system. We are asking it to sanity-check whether the local output is too unreasonable.

## Current Audit Strategy

The advice system is a rule-composition system, not a table of fully hand-written scenarios. The theoretical input matrix is large:

```text
14 light phases * about 6 weather states * 4 shooting styles * 5 camera types * 3 experience levels * 2 support modes * 2 subject states = about 20,160 combinations
```

This does not mean Luma has 20,000 separate advice cases. Most inputs fold into broader rule groups such as low light, harsh light, clear weather, fog, tripod, handheld, moving subject, and device type.

The practical audit target is therefore smaller:

- First pass: review the 7 high-risk cases below.
- Second pass: expand to about 24-36 representative regression cases.
- Full matrix generation is not useful yet unless we need to find duplicate text, missing branches, or unexpected rule collisions.

The current case sets are risk-guided manual samples, not random samples and not formal combinatorial coverage. They are chosen by looking at dimensions where rule-composed advice is most likely to conflict:

- Low light, night, and night sky.
- Handheld versus tripod support.
- Phone, manual camera, and action cam wording.
- Still versus moving subjects.
- Beginner versus professional guidance depth.
- Weather modifiers such as fog, heavy cloud, clear sky, and harsh light.

This means the audit has coverage logic, but it does not claim exhaustive coverage. Extreme combinations such as `ActionCam + NightSky + Tripod` can still be blind spots until invariant checks and broader matrix scans exist.

Current hard-coded advice is roughly 50-plus localized advice fragments that are assembled into a card:

- Feasibility warnings.
- First test exposure guidance.
- Safe starting point guidance.
- Device-specific operation guidance.
- Risk warnings.
- Adjustment steps.
- Weather notes.
- Experience-level notes.
- Capture-condition notes for support mode and subject movement.
- Beginner field steps.

Audit should focus on combinations where these fragments can conflict, especially night or low-light scenes, phone versus camera wording, handheld versus tripod guidance, and moving subjects.

## Review Limits And Invariants

Layered review is an exploration process, not a proof that every possible combination is correct.

Because the advice is assembled from shared rule fragments, a fix for one reviewed case can affect earlier cases and unreviewed combinations. Re-running previous audit sets lowers that risk, but it cannot guarantee that the full input matrix has no regressions.

This project should not claim that it can give perfect photography advice. The realistic quality target is to make the advice useful, safe, and device-appropriate for common travel-photography paths, while catching obvious rule collisions before they reach users.

Use separate quality bars instead of one vague "correctness" number:

- Basic safety bar: advice should not be obviously wrong, unsafe for the light level, or impossible on the selected device. After the current audit passes, this is the bar Luma should try to push toward roughly 80-90% on common paths, then improve with automated checks.
- Practical usefulness bar: advice should give a reasonable first test shot, a clear risk to watch, and one useful adjustment. This is likely lower than the safety bar because the app has limited context about lens, exact phone model, subject speed, local lighting, and user skill.
- Coaching-quality bar: advice should feel like a strong photographer tailored it to the exact scene. This is not the current promise of the app and should not be treated as the release gate.

The product positioning should stay honest: Luma is a travel photography starting-point assistant, not a perfect photography answer engine. It should help the user decide what to try first, what to watch first, and what to change first.

Use review to discover new classes of problems. Once a problem repeats or becomes clear enough to describe as a rule, convert it into an automated invariant check.

Run the invariant checker with:

```powershell
dotnet run --project .\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj -- --set high-risk --check-invariants
dotnet run --project .\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj -- --set regression --check-invariants
dotnet run --project .\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj -- --set travel-t6-sept-iles --check-invariants
dotnet run --project .\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj -- --set matrix-smoke --check-invariants
```

The checker prints only failures. A clean run prints `No invariant failures found.` and exits with code 0; failures are listed by case and return a non-zero exit code. The `matrix-smoke` set generates a broader representative scan for machine checks only; do not use it as a manual review file unless a specific failure needs inspection.

For the normal local quality gate, run the wrapper script instead of typing every command manually:

```powershell
.\tools\check-advice.ps1
```

The script runs `matrix-smoke`, the named regression/travel invariant sets, localization validation, and the app build. It stops at the first failure so the failing command output stays visible.

Examples of invariants learned from review:

- `PhoneBasic` output should not give manual camera controls such as ISO, aperture, shutter speed, or depth-of-field instructions as the user's action.
- `ActionCam` output should not depend on tap-to-focus, manual shutter speed, or manual ISO control.
- `NightSky` output should not reuse daylight highlight-first guidance.
- Tripod low-light manual-camera output should not contain handheld-only rules such as `1/focal length handheld`.
- Auto-exposure devices should use mode, bracing, light-source, timer, burst, action-mode, sport-mode, or video language.
- Night tripod landscape should lead with noise, focus, or long-exposure concerns before daylight-style highlight risk.
- Beginner handheld night manual-camera scenarios should show a feasibility warning.

Full matrix generation is useful for machine checks, not for manual review. The long-term workflow should be:

1. Use external review to find new quality issues.
2. Fix meaningful `Wrong` and high-value `Risky` findings with small rules or wording changes.
3. Re-run prior audit sets as regression samples.
4. Convert repeated issues into automated invariant checks.
5. Use full or large-matrix generation only to run those checks and report failures.

Use three quality layers for different error types:

1. Risk-guided manual review catches semantic and product-judgment problems, such as misleading direction, mismatched tone, or advice that is technically possible but unhelpful.
2. Invariant machine scans catch known structural rule violations, such as `PhoneBasic` output containing `ISO`, `ActionCam` output containing manual shutter-speed guidance, or night tripod landscape leading with daylight highlight risk.
3. Full matrix generation catches coverage and collision problems, such as missing output, empty sections, duplicate output, or adjacent cases that should differ but do not.

Do not start with full matrix review as the main quality gate. Run full or broad matrix checks after invariants are stable; otherwise the output can be too noisy to triage well.

## Next Development Direction

The next stage should move from case-by-case fixing to explicit quality gates.

Priority order:

1. Keep `tools/check-advice.ps1` green before committing advice-rule changes.
2. Expand toward a fuller generated matrix only after a new invariant justifies broader coverage.
3. Split advice rules by device capability only when a shared category changes the user's real action.
4. Add metadata to generated audit output so each case can show which advice keys were used.
5. Continue external review by theme instead of asking for one broad overall review.

Complexity control rule:

Do not split rules just to make the model look more precise. Split only when shared wording would make the user do something different, impossible, or misleading for the selected device or scene. Prefer adding an invariant first; let repeated invariant failures justify new branches.

Initial invariants to implement:

- `PhoneBasic` advice must not ask the user to set ISO, aperture, shutter speed, or depth of field.
- `ActionCam` advice must not use tap-to-focus, manual ISO, manual shutter speed, burst-still, action-mode, or sport-mode language as the primary moving-subject path.
- `ActionCam` moving-subject advice should prefer video, high-frame-rate capture, stabilization, and pulling a frame later.
- `NightSky` advice should not lead with daylight highlight-protection guidance.
- Night tripod landscape should not use highlight risk as the first watch item; it should lead with noise, focus, stability, or long-exposure concerns.
- Tripod low-light advice should not contain handheld-only `1/focal length` guidance.
- Beginner handheld night manual-camera advice should include a feasibility warning.

The important design lesson from the second-layer audit is that `IsAutoExposureDevice` is useful but too broad for all wording decisions. It is safe for avoiding manual camera controls, but it is not precise enough for motion advice because phones and action cameras have different practical workflows.

Future rule design should prefer capability-style questions when possible:

```text
CanUseManualExposure
CanUseTapExposure
CanUseBurst
PrefersVideoForMotion
HasRemoteOrTimerTrigger
```

This does not mean creating a separate hand-written case for every input combination. It means splitting only the dimensions that change the meaning of the advice.

Keep the rule system in three layers:

1. Broad rules for common photographic realities such as low light, harsh light, moving subjects, tripod, handheld, and night sky.
2. Capability overrides for manual cameras, phones, action cameras, tripod workflow, handheld workflow, and video-preferred motion capture.
3. Small exception rules only when review or invariants show a real mismatch, such as `ActionCam + Moving`, `NightSky + Tripod`, `PhoneBasic + LowLight`, or `Beginner + Handheld Night`.

Layer 3 should stay small. If a proposed branch only makes wording more polished but does not prevent a real user-facing mismatch, leave it out until review or invariant output proves it matters.

## Pre-Trip Canon EOS T6 Travel Check

Before the upcoming travel use case, prioritize a practical field check for the user's actual camera and destination: Canon EOS T6 with an 18-55mm APS-C kit zoom, traveling to Sept-Iles, mostly shooting landscapes with some street photography and a small amount of portrait work. This is a real user path, so it should come before another broad advice review.

Goal:

- Make Luma useful as a travel starting-point assistant for an APS-C kit-zoom camera.
- Confirm coastal landscape, fog, heavy cloud, rain, harbour street, moving street, portrait, sunset tripod, and night harbour paths have reasonable first-shot guidance, risk warnings, and adjustment steps.
- Avoid spending this pass on lower-priority device paths such as ActionCam unless an invariant catches a known issue.

Current audit set names:

```text
travel-aps-c-landscape
travel-t6-sept-iles
```

The earlier `travel-fullframe-landscape` set can remain as broader coverage, but it is no longer the priority field-use path.

Suggested Sept-Iles cases:

1. APS-C + Midday + Landscape + Handheld + Still + Heavy cloud coastal landscape.
2. APS-C + BlueHour + Landscape + Handheld + Still + Fog or low visibility.
3. APS-C + GoldenHour + Urban + Handheld + Still + Mixed cloud harbour street.
4. APS-C + Afternoon + Urban + Handheld + Moving + Mixed cloud street scene.
5. APS-C + Afternoon + Urban + Handheld + Still + Rainy street scene.
6. APS-C + GoldenHour + Portrait + Handheld + Still + Mixed cloud casual portrait.
7. APS-C + Midday + Portrait + Handheld + Still + Heavy cloud casual portrait.
8. APS-C + Sunset + Landscape + Tripod + Still + Clear coast.
9. APS-C + Night + Urban + Tripod + Still + Mixed harbour lights.
10. APS-C + Midday + Landscape + Handheld + Still + Clear shoreline.

Review focus:

- Daylight and harsh light should protect highlights without overcomplicating the field workflow.
- Fog and heavy cloud should prioritize contrast, shape, and separation instead of simply brightening the frame.
- Blue hour should stay stable and avoid over-brightening signs, fog, or sky.
- Night tripod landscape should lead with noise, focus, stability, or long-exposure concerns before highlight risk.
- Night sky tripod should prioritize focus, star trailing, exposure length, and noise.
- Moving street scenes should protect shutter speed first and raise ISO before allowing subject blur.
- APS-C portrait guidance should respect the 18-55mm kit zoom's actual aperture range instead of implying f/2.8 is available.
- Handheld night or blue-hour landscape should be honest about stability limits.

This travel set should be treated as a targeted product-readiness pass, not a replacement for the broader invariant checker. If time is short, run this set manually first, then implement `--check-invariants` after the trip-focused path is usable.

## Review Prompt

Copy this prompt into an external AI tool, then paste or upload the generated case output file after it.

Ask the reviewer to return Markdown content that can be saved as the matching review file, such as `docs/advice/generated/high-risk-review.md` or `docs/advice/generated/regression-review.md`.

```text
You are a photography coach. Review these local hard-coded photography advice outputs.

The goal is not to create perfect advice. The goal is to catch advice that is obviously wrong, misleading, unsafe for the scenario, or mismatched to the user's device.

For each case, respond with:
- Verdict: OK / Risky / Wrong
- Main issue: one sentence describing the largest problem
- Suggested fix: the smallest rule or wording change needed
- Must fix before commit: yes/no

Return the review as Markdown suitable for saving to a local review file. Do not rewrite the advice text unless a short quote is needed to identify the issue.

Rules for judging:
1. Night and low-light scenes must not reuse daylight landscape settings.
2. Handheld night scenes should prioritize stable support.
3. Night sky generally requires a tripod or stable surface.
4. Moving subjects need enough shutter speed or an explicit tradeoff.
5. Basic phone advice should not depend on camera-only aperture/shutter controls.
6. Phone Pro advice can mention Pro or Night mode, but should not assume every phone has adjustable aperture.
7. Beginner advice should be concrete and step-based.
8. Professional advice can be shorter and more tradeoff-oriented.
9. Parameters do not need to be perfect; they just must not be clearly unreasonable.
10. Do not rewrite every paragraph. Flag only meaningful problems.
```

## Case Format

Use this template for each case.

```text
Case N: short name
Phase:
Weather:
Style:
Camera:
Experience:
Support:
Subject:

Local output:
Feasibility:
First test shot:
Watch first:
If it is not working:
Steps:
```

## Generate Local Outputs

Run this command from the repository root to generate the current local outputs for the high-risk cases:

```powershell
dotnet run --project .\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj -- --set high-risk
```

For longer reviews, write the output to a file instead of copying from the terminal:

```powershell
dotnet run --project .\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj -- --set high-risk --out .\docs\advice\generated\high-risk-output.md
```

For the second-layer regression pass, use:

```powershell
dotnet run --project .\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj -- --set regression --out .\docs\advice\generated\regression-output.md
```

Ask the reviewer to return Markdown content for a review file too. Save that file next to the generated output, for example:

```text
docs/advice/generated/high-risk-review.md
```

This lets the coding agent read the review file directly and convert only meaningful findings into small rule changes.

The external reviewer does not need access to this repository; it only needs to return Markdown that you save at that path.

If your terminal is already inside the app folder (`Luma/Luma`), use this path instead:

```powershell
dotnet run --project ..\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj -- --set high-risk
```

From the app folder, write output to a file with:

```powershell
dotnet run --project ..\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj -- --set high-risk --out ..\docs\advice\generated\high-risk-output.md
```

From the app folder, generate second-layer regression output with:

```powershell
dotnet run --project ..\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj -- --set regression --out ..\docs\advice\generated\regression-output.md
```

Paste or upload the generated output file after the review prompt above when asking for review. The generator currently uses English output so the photography review can focus on advice quality instead of translation quality.

Recommended file pair for a review pass:

```text
docs/advice/generated/high-risk-output.md
docs/advice/generated/high-risk-review.md
```

For the second-layer regression pass, use:

```text
docs/advice/generated/regression-output.md
docs/advice/generated/regression-review.md
```

Do not commit files under `docs/advice/generated/`; they are local review artifacts.

## Review Result File Format

Ask the reviewer to use this format when returning a review file:

```text
# Advice Audit Review: audit-set-name

Overall summary:
- OK: N
- Risky: N
- Wrong: N
- Must-fix before commit: yes/no

## Case 1: case name

Verdict: OK / Risky / Wrong
Main issue: one sentence, or none
Suggested fix: smallest rule or wording change, or none
Must fix before commit: yes/no

## Case 2: case name

Verdict: OK / Risky / Wrong
Main issue: one sentence, or none
Suggested fix: smallest rule or wording change, or none
Must fix before commit: yes/no
```

For targeted re-audits, use this shorter format:

```text
# Advice Audit Re-Review: high-risk

## Case N: case name

Resolved: yes/no
Remaining issue: one sentence, or none
Must fix before commit: yes/no
```

## High-Risk Cases For Next Review

Start with these cases before expanding the matrix.

### Case 1: Night Handheld Landscape

```text
Phase: Night
Weather: clear or mixed cloud
Style: Landscape
Camera: Full frame
Experience: Beginner
Support: Handheld
Subject: Still
```

Expected review focus:

- Must show a feasibility warning.
- Must not recommend daylight landscape defaults as the main path.
- Should prioritize stable support over low ISO.

### Case 2: Night Sky With Tripod

```text
Phase: Night
Weather: clear
Style: Night sky
Camera: APS-C
Experience: Intermediate
Support: Tripod
Subject: Still
```

Expected review focus:

- Should use tripod/stability language.
- Should give a reasonable first test such as high ISO, wider aperture, and around 10 seconds.
- Should warn about star trailing or focus.

### Case 3: Night Sky Handheld

```text
Phase: Night
Weather: clear
Style: Night sky
Camera: Phone Pro
Experience: Beginner
Support: Handheld
Subject: Still
```

Expected review focus:

- Must warn that handheld night sky is not reliable.
- Phone language should be operational, not DSLR-style.
- Should mention Night mode or stable support.

### Case 4: Golden Hour Moving Portrait

```text
Phase: Golden hour
Weather: mixed cloud
Style: Portrait
Camera: Phone Pro
Experience: Beginner
Support: Handheld
Subject: Moving
```

Expected review focus:

- Should prioritize subject sharpness.
- Should keep phone-specific controls.
- Should still mention face exposure or highlight protection.

### Case 5: Midday Landscape

```text
Phase: Daylight
Weather: clear, low cloud cover
Style: Landscape
Camera: APS-C
Experience: Professional
Support: Handheld
Subject: Still
```

Expected review focus:

- Should protect highlights.
- Should not overexplain beginner steps.
- Should be acceptable with ISO 100, landscape aperture, and safe shutter.

### Case 6: Foggy Urban Scene

```text
Phase: Blue hour
Weather: fog or low visibility
Style: Urban
Camera: Full frame
Experience: Intermediate
Support: Handheld
Subject: Still
```

Expected review focus:

- Should mention contrast/shape risk.
- Should not over-brighten foggy scenes.
- Should keep handheld blur risk in mind.

### Case 7: Action Cam Low Light

```text
Phase: Night
Weather: mixed cloud
Style: Urban
Camera: Action cam
Experience: Beginner
Support: Handheld
Subject: Moving
```

Expected review focus:

- Should warn that low light is weak for action cameras.
- Should emphasize stabilization, bright areas, and avoiding underexposed motion.
- Should not give interchangeable-lens camera aperture advice.

## What To Do With Review Results

Use this triage:

- `Wrong`: fix before shipping or demoing that scenario.
- `Risky`: improve if it touches a common user path.
- `OK`: leave it unless the wording is confusing.

When fixing, prefer changing one rule branch or one localized phrase. Avoid rewriting the whole service because one case sounds awkward.

After fixing a review result, regenerate both the current set and any earlier set that the change might affect. If a fix changes a shared rule such as device wording, risk selection, exposure selection, or adjustment wording, regenerate at least `high-risk` and `regression` before committing.

For fixes that touch a broad shared rule, ask for a targeted re-review instead of a full manual review. The re-review should check only the affected cases and ask whether the previous must-fix issue is resolved and whether a new must-fix was introduced.

## First Review Findings

First high-risk review summary:

- 2 cases were OK.
- 4 cases were Risky.
- 1 case was Wrong.

The largest systemic issue was generic fallback bullets appearing beside more specific scenario guidance. This is risky because the specific branch may be correct while the extra generic bullet gives the user a conflicting priority.

Priority fixes from this pass:

- Case 6, Foggy Urban Scene: treat blue hour camera exposure as distinct from night handheld exposure.
- Case 1, Night Handheld Landscape: when feasibility warning triggers, keep the first test shot focused on the matching exposure branch instead of appending generic safe-start and device-operation bullets.

Detailed findings:

- Case 1, Night Handheld Landscape: Risky. First test shot mixed the correct handheld branch with generic low-ISO fallback bullets. Fixed by keeping first test focused when a handheld low-light feasibility warning is active.
- Case 2, Night Sky Tripod: Risky. Clear weather pushed the leading risk toward blown highlights, which is misleading for night sky. Fixed by giving tripod night sky its own star-trailing, focus, and noise risk.
- Case 3, Night Sky Handheld: OK. Keep as-is for now; optional future wording can mention laying the phone flat or propping it securely.
- Case 4, Golden Hour Moving Portrait: Risky. Beginner moving-subject scenarios need a short heads-up about burst or continuous capture. Fixed with a beginner moving-subject feasibility note.
- Case 5, Midday Landscape: OK. Keep as-is.
- Case 6, Foggy Blue Hour Urban: Wrong. Blue hour should not reuse the full night handheld exposure template, and fog should be a leading concern. Fixed with a blue-hour handheld exposure branch and contrast-first risk ordering for fog or low visibility.
- Case 7, Action Cam Night Urban: Risky. Beginner action cam users in full night with moving subjects need an explicit high-failure-rate warning. Fixed with an action-cam night moving-subject feasibility note.

Targeted re-audit result:

- No remaining issue was marked as must-fix before commit.
- Case 2 had a minor remaining generic tripod bullet about low ISO and depth of field. Fixed by not appending still-subject tripod guidance to night sky tripod cases.
- Case 3 had a low-priority daylight clear-sky weather note. Fixed by suppressing generic clear-sky weather notes for night sky cases.
- Case 4 and Case 7 still had a device-mismatched `1/500s` moving-subject note for phone or action cam users. Fixed by using a phone/action-cam friendly burst, action mode, sport mode, or video note for auto-exposure devices.

## Next Starting Point

1. Implement `--check-invariants` in `tools/Luma.AdviceAudit`.
2. Start with known device-language and low-light invariants from the first two review rounds.
3. Run the invariant checker against `high-risk`, `regression`, and then a broader generated matrix.
4. Fix only invariant failures that represent real user-facing mismatches.
5. Add case metadata or advice-key tracing to the generated output after the first invariant checker is working.
6. Use the next external review as a theme-based review, such as device-language review or night/low-light review, instead of another broad review pass.
