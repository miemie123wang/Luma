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

## Review Prompt

Copy this prompt into an external AI tool, then paste or upload the generated case output file after it.

Ask the reviewer to return Markdown content that can be saved as `docs/advice/generated/high-risk-review.md`.

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
dotnet run --project .\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj
```

For longer reviews, write the output to a file instead of copying from the terminal:

```powershell
dotnet run --project .\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj -- --out .\docs\advice\generated\high-risk-output.md
```

Ask the reviewer to return Markdown content for a review file too. Save that file next to the generated output, for example:

```text
docs/advice/generated/high-risk-review.md
```

This lets the coding agent read the review file directly and convert only meaningful findings into small rule changes.

The external reviewer does not need access to this repository; it only needs to return Markdown that you save at that path.

If your terminal is already inside the app folder (`Luma/Luma`), use this path instead:

```powershell
dotnet run --project ..\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj
```

From the app folder, write output to a file with:

```powershell
dotnet run --project ..\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj -- --out ..\docs\advice\generated\high-risk-output.md
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
# Advice Audit Review: high-risk

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

1. Run `tools/Luma.AdviceAudit -- --out ...` to create a generated output file.
2. Send the review prompt and generated output file to an external reviewer.
3. Save the returned Markdown review as `docs/advice/generated/*-review.md`.
4. Let the coding agent read the review file and triage only meaningful `Wrong` and high-value `Risky` findings.
5. Convert findings into small rule or wording changes.
6. Re-run the same cases after each fix.
