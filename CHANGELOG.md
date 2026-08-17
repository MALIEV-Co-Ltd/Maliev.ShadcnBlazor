# Changelog

All notable changes to this project are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and releases follow
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.8] - 2026-08-17

### Fixed

- Preserved Ghost bubble sizing and reaction spacing so conversation content
  remains readable in every variant.
- Reworked streaming marker shimmer to move left-to-right without a masking
  rectangle and to respect reduced-motion and forced-colors preferences.
- Aligned message avatars to the message body, compacted footer actions, and
  kept copy/reply actions and sent status in their intended positions.
- Moved the Message Scroller composer into the chat window and kept its
  auto-follow, direction, and streaming behavior interactive.
- Refreshed responsive conversation visual proofs and regression coverage for
  the six annotation fixes.

## [1.0.7] - 2026-08-17

### Fixed

- Completed the annotation-driven documentation and component polish pass across
  conversation, data-display, form, action, feedback, and semantic examples.
- Synchronized interactive previews and source snippets, including unbound form
  controls, table borders, chart configuration, carousel motion, and message
  actions.
- Refreshed responsive visual-proof baselines for all 64 completed dossiers.

## [1.0.6] - 2026-08-16

### Fixed

- Kept bundled Geist, Noto Sans Thai, and JetBrains Mono typography offline by
  loading Google Fonts only when a remote preset is explicitly selected.
- Refined the documentation workbench so the rails stay at the viewport edges,
  the reading column remains centered, and active navigation uses quiet emphasis.
- Made the documentation brand mark round and enlarged it, kept resting
  attachment previews quiet while preserving their keyboard focus ring, and
  kept chart bars square in the theme mock.

## [1.0.5] - 2026-08-16

### Fixed

- Synchronized interactive Card, Progress, Toast, DatePicker, Toggle, and Chart
  documentation sources with their live previews.
- Kept chart bars square by default and restored slider drag behavior across
  interactive Blazor rerenders.
- Refreshed responsive visual proofs for the updated component demos and fixed
  public-surface validation on Windows PowerShell.

## [1.0.4] - 2026-08-16

### Fixed

- Added editor-style Razor and C# syntax colors to documentation code blocks.
- Made copy feedback transient and repeatable, returning to the copy icon after
  each successful copy.
- Removed the unintended uploading attachment outline while preserving the
  keyboard focus ring.

## [1.0.2] - 2026-08-16

### Fixed

- Added catalog-wide contracts proving every completed documentation route
  renders its real Maliev.ShadcnBlazor component.

## [1.0.1] - 2026-08-15

### Fixed

- Improved interactive Showcase examples, visual states, and responsive layouts.
- Added the reusable syntax-colored `ShadcnCodeBlock` documentation primitive.
- Corrected component loading, selection, overlay, calendar, conversation, and data-display interactions.

## [1.0.0] - 2026-08-14

### Added

- Initial public release with 64 reviewed Blazor component families.
- Light and dark theming, LTR and RTL direction, reduced-motion and
  forced-colors behavior.
- Standalone component Showcase, unit tests, and real-browser regression tests.
- Signed-source metadata, symbol package, MIT license, and third-party notices.

[Unreleased]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/compare/v1.0.8...HEAD
[1.0.8]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.8
[1.0.7]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.7
[1.0.6]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.6
[1.0.5]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.5
[1.0.4]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.4
[1.0.2]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.2
[1.0.1]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.1
[1.0.0]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.0
