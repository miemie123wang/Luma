# Luma Docs

This folder is organized by product area so we can change the app by following the relevant document instead of rediscovering decisions each time.

## Categories

### Advice

Local rule-based photography advice, output structure, prompt copy behavior, and AI-assisted review workflow.

- [Advice overview](advice/README.md)
- [Advice design](advice/design.md)
- [Advice audit workflow](advice/audit.md)

### Engineering

Build, structure, completed cleanup, deployment, localization, and resilience notes.

- [Codebase review notes](engineering/codebase-review.md)

## How To Use These Docs

1. Start from this index when picking up work.
2. Open the category README for the feature area.
3. Update the category doc before or alongside code changes.
4. Keep README files short; put detailed decisions in category documents.

## Documentation Maintenance

When changing product behavior, local advice rules, UI flow, testing workflow, deployment assumptions, or other decisions that affect future work, update the relevant docs in the same change.

- Product behavior: update the feature category document.
- User-facing feature summary: update the root README files.
- Cross-feature navigation: update this index or the category README.
- Advice rules, prompt behavior, and audit cases: update `docs/advice/`.

Small visual tweaks, typo fixes, and internal-only refactors do not need documentation updates unless they change how the product should be understood or maintained.

## Planned Categories

These folders do not need to exist until we start active work there.

- `planning/`: trip planning product rules and UX flow.
- `ui/`: visual system, layout rules, and responsive behavior.
- `engineering/`: additional build, deployment, localization, and resilience notes.
