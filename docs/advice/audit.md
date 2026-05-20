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

Copy this prompt into an external AI tool, then paste the cases below it.

```text
You are a photography coach. Review these local hard-coded photography advice outputs.

The goal is not to create perfect advice. The goal is to catch advice that is obviously wrong, misleading, unsafe for the scenario, or mismatched to the user's device.

For each case, respond with:
- Verdict: OK / Risky / Wrong
- Main issue: one sentence describing the largest problem
- Suggested fix: the smallest rule or wording change needed

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

## Tomorrow's Starting Point

1. Add a quick way to generate these cases from `ShootingAdviceService`.
2. Paste the generated outputs into this file under each case.
3. Run the review prompt with an external AI.
4. Convert only the meaningful findings into small rule changes.
