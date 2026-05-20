# Advice Docs

This category covers Luma's local photography advice system.

## Current Documents

- [Design](design.md): product principles, inputs, output structure, decision flow, and known limits.
- [Audit workflow](audit.md): how to enumerate hard-coded outputs and ask an external AI or human reviewer whether they are too unreasonable.
- [Audit log](audit-log.md): detailed findings, fixes, and working notes from completed advice audit passes.

## Current Direction

Luma does not call an AI API for advice in the MVP. It uses local rules to produce practical first-shot guidance and lets users copy real context to an external AI when they want a second opinion.

The local advice should prioritize:

- Feasibility before parameters.
- Light phase before shooting style.
- First test settings instead of broad ranges.
- Device-specific language.
- Clear adjustment order when a shot fails.

## Before Changing Advice Rules

1. Read [Design](design.md).
2. Add or update relevant cases in [Audit workflow](audit.md).
3. Generate the local advice for those cases.
4. Review the output with the audit prompt.
5. Change only the smallest rule needed to fix the issue.

## Current Audit Plan

The first 7 high-risk cases have been reviewed and fixed. See [Audit log](audit-log.md) for the detailed findings and decisions.

Next, expand to about 24-36 representative regression cases instead of trying to exhaust the full input matrix.
