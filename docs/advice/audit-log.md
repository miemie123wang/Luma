# Advice Audit Log

Category index: [Advice Docs](README.md)

Workflow reference: [Advice Audit Workflow](audit.md)

This file records what we learned while auditing Luma's local shooting advice. It is more detailed than the workflow doc on purpose: use it as the working memory for future advice quality passes.

## Status

First-layer high-risk audit is complete. Second-layer regression audit tooling is in place, and the first regression review fixes have been applied.

Scope completed:

- Generated 7 high-risk advice outputs with `tools/Luma.AdviceAudit`.
- Added `--out` support so audit outputs can be written to local review files.
- Added `--set high-risk|regression` support so the second-layer case set can be generated separately.
- Reviewed those outputs with the audit prompt.
- Fixed all `Wrong` findings.
- Fixed high-value `Risky` findings that affected common or confusing paths.
- Ran a targeted re-audit.
- Cleaned up remaining minor wording/routing issues from the targeted re-audit.

Validation completed:

```powershell
dotnet run --project .\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj -- --set high-risk
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

## Developer Framing

This work is not just editing photography copy. It is turning a subjective product capability into an engineering system that can be reviewed, improved, and protected from regression.

The advice module has several difficult qualities:

- It is user-visible and affects trust quickly.
- It is partly subjective because photography guidance depends on taste, equipment, skill, and scene context.
- It has a large theoretical input matrix.
- It cannot honestly promise 100% correct advice.
- It still needs to avoid advice that is obviously wrong, impossible on the selected device, or misleading in high-risk scenes.

The engineering task is to convert vague quality judgment into explicit boundaries:

- Which scenarios are high risk.
- Which outputs are clearly wrong.
- Which device-language mismatches are unacceptable.
- Which findings must be fixed before commit.
- Which findings are optional polish.
- Which repeated review findings should become automated invariants.
- When a shared rule should be split.
- When splitting would only add complexity without preventing a real user-facing mismatch.

In practical terms, this work is building a review-driven quality workflow for a rule-composition advice engine:

- Generate representative high-risk and regression cases.
- Use external AI review as a discovery step, not as proof.
- Triage findings manually.
- Convert meaningful findings into small rule or wording changes.
- Validate localization and builds.
- Record product and engineering decisions in docs.
- Convert repeated failures into future invariant checks.

The case sets are manually selected, but they are not arbitrary. They are risk-guided samples chosen around dimensions where rule composition is likely to collide: low light, night sky, handheld support, tripod support, phone wording, action cam wording, moving subjects, beginner guidance, professional concision, fog, harsh light, and clear weather. This is representative coverage, not exhaustive coverage. It can miss extreme combinations such as `ActionCam + NightSky + Tripod`.

The quality strategy has three layers:

- Manual review finds semantic and product-judgment issues: wrong advice direction, misleading tone, confusing priorities, or guidance that a human photographer would flag even if no simple text rule catches it.
- Invariant machine scans prevent known structural failures from returning: device-language mismatches, forbidden manual controls for auto devices, highlight-first night tripod guidance, handheld-only rules in tripod contexts, or missing feasibility warnings in known hard scenarios.
- Full matrix generation should come after invariants are stable. Its job is coverage and collision discovery: missing output, empty sections, duplicate output, or neighboring cases that collapse into the same advice when they should differ.

The important sequence is: use manual review to discover issue classes, use invariants to lock known classes down, then use full matrix generation to broaden coverage. Running a full matrix too early would produce many unranked findings without enough rules to triage them.

This is the useful part of AI-assisted development: AI helps discover issue classes in subjective output, while engineering judgment decides what is a real bug, what is acceptable risk, what should become a rule, and what should not be over-engineered.

A concise resume-style summary of this work:

```text
Built a review-driven quality workflow for a rule-based photography advice engine: designed representative audit sets, integrated external AI review as a discovery step, triaged findings into rule-level fixes, added localization validation, documented correctness limits, and planned invariant-based regression checks to prevent known advice-quality failures across a large input matrix.
```

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

## Review Coverage Limits

Layered external review is useful for discovering problem classes, but it is not enough as the final quality gate.

Reasons:

- The input matrix is large enough that manual review cannot cover it well.
- Many combinations share the same rule fragments, so a fix in one branch can affect reviewed and unreviewed cases.
- External review can miss issues or apply standards inconsistently between passes.
- Full matrix output is too large for high-quality human review.

The long-term direction is to turn repeated review findings into machine-checkable invariants. Review should find new issue classes; invariant checks should prevent known issue classes from coming back.

### Correctness Expectations

Discussion note:

We explicitly decided that Luma should not frame its local advice as 100% correct photography guidance. The input matrix is too large, the real world has more variables than the app currently models, and even expert photographers would disagree on exact settings for some scenes.

The more useful question is which level of correctness the product is promising.

Estimated quality bars:

- Basic safety and device fit: roughly 80-90% on common paths after the current review passes, with the goal of pushing this higher through invariant checks.
- Practical usefulness as a starting point: roughly 70-85% on common paths, because the app can usually give a sane first attempt and adjustment direction.
- Coaching-quality scene specificity: roughly 55-70%, because the app does not know exact lens, phone model, subject speed, scene contrast, available supports, local artificial light, wind, crowd movement, or the user's real technique.
- Full matrix correctness across 20,000-plus theoretical combinations: not a meaningful promise. Even a small percentage of rule collisions can represent many combinations, so manual review cannot be the proof mechanism.

This is a product and engineering decision, not an excuse to accept weak advice. The practical goal is to make bad advice rare, device-mismatched advice unacceptable, high-risk scenarios clearly warned, and common travel-photography paths genuinely useful.

The positioning that best matches the implementation is:

```text
Luma is a travel photography starting-point assistant.
It helps the user decide what to try first, what risk to watch first, and what adjustment to make first.
It should not claim to produce the single best answer for every scene.
```

This is also an important AI-assisted development record. The advice audit process shows the work moving from subjective review to explicit quality bars, documented assumptions, regression cases, and future automated invariants. That progression is worth preserving because it demonstrates product judgment, risk analysis, and practical use of AI review without pretending that AI review is a formal proof.

Candidate invariants:

- PhoneBasic output must not ask the user to set ISO, shutter speed, aperture, or depth of field.
- ActionCam output must not use tap-to-focus, manual ISO, or manual shutter-speed instructions.
- Phone and action cam moving-subject advice should use burst, action mode, sport mode, video, stabilization, or light-source language.
- Tripod low-light output should not include handheld-only `1/focal length` advice.
- NightSky output should not lead with daylight highlight-protection rules.
- Night tripod landscape should lead with noise, focus, or long-exposure risk before highlight risk.
- Beginner handheld night manual-camera output should include feasibility warning language.
- Nautical twilight should not blindly reuse full-night handheld ISO values.

When these invariants are implemented, they should run across a broad matrix and output only failures, not every generated advice card.

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

## Second-Layer Regression Review

The first regression review used 24 generated cases from:

```powershell
dotnet run --project .\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj -- --set regression --out .\docs\advice\generated\regression-output.md
```

Review summary:

- 2 cases were marked `Wrong`.
- 13 cases were marked `Risky`.
- 9 cases were marked `OK`.
- 9 cases were marked must-fix before commit.

The recurring root causes were rule bleed between manual cameras and auto-exposure devices, action cam movement language reusing phone/camera concepts, and night/tripod cases inheriting daylight highlight priorities.

Must-fix changes applied:

- PhoneBasic landscape starting points no longer mention ISO or depth of field; they now use tap-to-expose and reframe language.
- PhoneBasic low-light blur adjustments now suggest bracing, moving toward light, avoiding zoom, or using night/action mode instead of shutter/ISO changes.
- Phone Pro night-sky tripod output no longer uses daylight highlight pull-down exposure language.
- ActionCam moving feasibility now uses video, supported burst mode, stabilization, and automatic exposure language instead of tap-to-focus or generic burst still advice.
- ActionCam night still scenarios now show a feasibility warning about noise, blur, lit areas, and keeping the camera steady.
- Beginner manual-camera night handheld scenarios now show a general handheld night feasibility warning, including portrait cases.
- Night tripod landscape now leads with noise instead of blown highlights.
- Nautical dawn handheld APS-C exposure now uses `ISO 1600-3200` instead of full-night `ISO 6400`.
- PhoneBasic tripod watch-first guidance now uses automatic exposure plus timer/remote-trigger language instead of ISO/shutter control.

Additional low-cost cleanup applied from non-must-fix findings:

- Auto-exposure devices now get device-appropriate blur risk and adjustment wording.
- Professional night-sky advice no longer uses the generic highlight-protection experience fallback.

Targeted regression re-review result:

- Cases 7, 8, 10, 13, 15, 21, 22, 23, and 24 were re-reviewed.
- All 9 original must-fix issues were marked resolved.
- One new must-fix was found in Case 13, Action Cam Midday Moving Urban: the watch-first text still used phone-like `burst`, `action mode`, or `sport mode` wording for an action cam.
- Fixed by adding an ActionCam-specific moving-subject condition that recommends video or high-frame-rate capture and pulling a frame later.
- Also tightened ActionCam moving feasibility wording from video or supported burst mode to video or high-frame-rate capture.

Remaining non-must-fix notes from the re-review:

- Phone Pro night sky tripod could eventually prefer Pro or Manual mode wording over automatic exposure wording.
- Action cam tripod/still branches could eventually prefer voice control or app trigger wording over a generic timer.
- Beginner handheld night portrait could be simplified by suppressing the generic `1/focal length` line when a more specific handheld night line already appears.

## Next Tooling Ideas

Useful additions to `tools/Luma.AdviceAudit` later:

- Add `--check-invariants` to scan a broad matrix and report known rule violations.
- Add a full or large matrix mode for machine checks, not manual review.
- Add `--culture en|es|zh-Hans|zh-Hant` for translation spot checks.
- Add richer markdown output with stable headings and optional metadata for each case.
- Add a `--review-template` option that writes an empty review file next to the generated output.
- Add a simple duplicate-line detector to find repeated generic bullets.
- Add a device-language check that flags shutter-speed strings in phone/action-cam moving-subject cases.

## Next Session Plan

The next work session should start with automation, not another manual review.

Recommended order:

1. Implement `--check-invariants` in `tools/Luma.AdviceAudit`.
2. Make it generate cases internally and print only failures.
3. Add the first invariant group from known review findings.
4. Run it against `high-risk` and `regression` sets.
5. Expand it to a broad matrix only after the known sets pass.
6. Document any failures as either a true advice bug or an invariant that was too strict.

First invariant group:

- PhoneBasic manual-control language check.
- ActionCam moving-language check.
- NightSky daylight-highlight lead check.
- Night tripod landscape should not lead with highlight risk; it should lead with noise, focus, stability, or long-exposure concerns.
- Tripod low-light handheld-rule check.
- Beginner handheld night feasibility-warning check.

The ActionCam re-review is the reason this should be the next step. The first fix moved ActionCam out of manual-camera wording, but it still inherited phone-style wording through the broader auto-exposure branch. An invariant checker would catch that class of issue repeatedly and cheaply.

After the checker exists, the next structural improvement is to separate device capability questions from device family checks. `IsAutoExposureDevice` should remain useful for broad manual-control avoidance, but specific advice should use narrower intent such as phone workflow, action-camera workflow, manual-exposure support, tap exposure, burst support, or video-preferred motion capture.

Complexity control note:

There is a real risk of over-splitting the advice system. The goal is not to create a branch for every device, light phase, style, support mode, and experience combination. The rule for splitting should be: split only when shared wording changes the user's real action or creates a device/scene mismatch.

Good split:

- Phone moving-subject advice can mention burst, action mode, sport mode, or video.
- ActionCam moving-subject advice should prefer video, high-frame-rate capture, stabilization, and pulling a frame later.

This split is justified because the user workflow is different.

Bad split:

- Splitting two daylight still-landscape branches only because one wording could sound slightly more tailored.

This should wait until review or invariant output shows a real problem.

The preferred process is: write an invariant first, observe repeated failures, then add the smallest rule branch that fixes the real mismatch.

Theme-based external reviews should come after that. Good themes:

- Device-language review.
- Night and low-light review.
- Moving-subject review.
- Beginner clarity review.
- Professional concision review.

This preserves the useful part of AI review: finding new problem classes. It also avoids using AI review as a false proof that every generated combination is correct.

## Commit Boundary Guidance

Good commit boundaries for future advice work:

- Commit audit tooling separately when possible.
- Commit one audit layer and its fixes together when the fixes are small and directly tied to the findings.
- Avoid mixing advice rule changes with UI refactors.
- Update this log when a review result directly changes a rule.