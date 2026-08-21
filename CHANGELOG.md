# Changelog

All notable changes to this project are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and releases follow
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.0] - 2026-08-21

### Added

- Added reusable message composition and action APIs:
  `ShadcnMessageBody`, `ShadcnMessageActions`, `ShadcnMessageCopyAction`,
  `ShadcnMessageReplyAction`, `ShadcnMessageReplyQuote`, and
  `ShadcnMessageStatus`.
- Added `ShadcnPaginationPages`, the `ShadcnTabsListVariant` API, data-table
  default-state support, table column-count validation, localized calendar
  month/year labels, input-group button variants, and reduced-motion spinner
  control.
- Expanded menu composition with inset, disabled, close-on-select, alignment,
  side, and offset parameters across Menubar, Context Menu, and Dropdown Menu.
- Added stronger controlled/uncontrolled state coverage, exact dynamic Razor
  source synchronization, and reviewed desktop/mobile visual proof across all
  64 documented component families.

### Changed

- Reworked documentation controls to showcase package components consistently,
  including compact `ShadcnSelect` controls instead of native selects outside
  the Native Select dossier.
- Completed parity and realistic interactive dossiers for Button, Button Group,
  Checkbox, Slider, Switch, Toggle, Toggle Group, Radio Group, and Select.
- Refined Calendar, Date Picker, Combobox, Input, Input Group, Input OTP,
  Native Select, Textarea, Field, and Label behavior and documentation.
- Refined Alert Dialog, Dialog, Command, Menubar, Popover, Sheet, Drawer,
  Tooltip, Context Menu, Dropdown Menu, and Hover Card positioning, dismissal,
  submenu, focus, and keyboard behavior.
- Refined Accordion, Collapsible, Breadcrumb, Navigation Menu, Pagination,
  Resizable, Scroll Area, Sidebar, and Tabs responsive navigation behavior.
- Upgraded Avatar, Badge, Card, Carousel, Progress, Skeleton, Spinner, and Toast
  examples and states, plus Table, Data Table, and Message workflows.
- Improved Aspect Ratio, Direction, Empty, Item, Kbd, Separator, and Typography
  semantics, layout, readable rhythm, RTL behavior, and documentation shell
  contracts.

### Fixed

- Corrected selection and action state, slider dragging, connected button and
  toggle geometry, table composition validation, progress synchronization,
  carousel axis locking, avatar fallback, and responsive aspect-ratio behavior.
- Corrected overlay collision geometry, pointer boundaries, repeatable trigger
  attachment, nested-menu navigation, focus restoration, and non-modal hover
  behavior in LTR and RTL layouts.
- Preserved editor-style Razor/C# syntax colors, repeatable copy feedback,
  square chart bars, responsive documentation rails, and release-scoped
  stylesheet cache revisions.
- Hardened browser interaction assertions, reduced-motion drawer readiness,
  and hosted browser timeout budgeting for stable CI execution (#167-#169).

## [1.0.11] - 2026-08-17

- Keep prose inside Razor/HTML markup in the code block foreground color while preserving syntax highlighting for actual code.
- Add regression coverage for the light/dark editor palette and markup text classification.

## [1.0.10] - 2026-08-17

- Restore the full light/dark Razor and C# editor palette in documentation code blocks.
- Make copy feedback discoverable, transient, and repeatable without leaving a persistent outline.
- Polish conversation, forms, data-display, feedback, and action examples with interactive state, responsive sizing, and source that matches each preview.
- Refresh reviewed visual proof for the centered date picker, compact input/OTP cards, radio/select controls, table layout, and related annotation fixes.

## [1.0.9] - 2026-08-17

### Fixed

- Restored the editor-style C# and Razor syntax palette across package and
  Showcase code blocks.
- Triggered the GitHub Pages Showcase deployment from published releases and
  pinned release builds to the exact published tag so the docs stay in sync
  with the NuGet package.

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

[Unreleased]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/compare/v1.0.11...v1.1.0
[1.0.11]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.11
[1.0.10]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.10
[1.0.9]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.9
[1.0.8]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.8
[1.0.7]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.7
[1.0.6]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.6
[1.0.5]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.5
[1.0.4]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.4
[1.0.2]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.2
[1.0.1]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.1
[1.0.0]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/v1.0.0
