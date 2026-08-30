# Changelog

All notable changes to this project are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and releases follow
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.1.2] - 2026-08-30

### Changed

- Reorganized Theme Studio palette customization into an inline collapsible
  sidebar workflow with a taller palette preview, one authoritative main color,
  derived harmony colors, and optional advanced anchor controls.

### Fixed

- Coalesced repeated palette-generation requests after each completed
  derivation to prevent expensive validation and preview updates from queuing
  until the browser becomes unresponsive.

## [2.1.1] - 2026-08-30

### Fixed

- Made the version-one theme recipe serialization regression check independent
  of checkout line endings so immutable release validation is consistent across
  local, pull-request, and release runners.

## [2.1.0] - 2026-08-30

### Added

- Added the Theme Studio palette workbench with deterministic harmony recipes,
  lockable semantic anchors, contrast-aware generation, shareable state, and
  portable versioned theme documents.

### Changed

- Hardened Theme Studio palette history, diagnostics, import/export, responsive
  editing, modal isolation, startup performance, and recipe compatibility while
  preserving legacy documents.

### Fixed

- Composed the selected Thai fallback into the effective portable document font
  stack, preserving the chosen body face while placing Thai families before
  generic system fallbacks across preview, export, and restored state (#249).

## [2.0.0] - 2026-08-30

### Added

- Added target-aware programmatic focus APIs across buttons, form controls,
  action and trigger controls, and composite widgets (#237-#241).

### Changed

- Removed the core package's MudBlazor dependency, providers, adapters, assets,
  and license. Applications that still use MudBlazor must reference and
  configure it directly; see the
  [MudBlazor decoupling migration](docs/migration-mudblazor.md) (#244).
- Scoped Tabs arrow-key guards to the tab list so controls rendered inside tab
  panels retain their native keyboard behavior (#243).

### Fixed

- Restored alert-dialog focus to the current trigger after Blazor replaces the
  original trigger during a close render (#246).
- Preserved responsive Showcase reflow at 200% zoom after removing MudBlazor's
  global visual defaults.

## [1.2.2] - 2026-08-28

### Added

- Added composable visual-style scopes, Bento Grid and Reveal primitives, and
  agent-ready consumer and maintainer skill packages.
- Added a complete five-minute documentation quickstart and separated the
  component catalog into a focused, navigable reference experience.

### Changed

- Refined Theme Studio navigation with a compact desktop settings rail,
  temporary hover expansion, a mobile off-canvas drawer, a compact Ko-fi
  action, and responsive category navigation.
- Extended Theme Studio workflow icons to follow the selected Lucide, Tabler,
  Phosphor, or Hugeicons companion package.

### Fixed

- Corrected GitHub Pages module resolution, Theme Studio form and overlay
  interactions, chart and carousel rendering, sequential OTP entry, and
  responsive text wrapping.
- Hardened sidebar overlay cleanup, glass-material coverage and flicker,
  table-caption alignment, transfer-action spacing, and reviewed visual
  behavior across desktop, mobile, dark, and RTL modes.

## [1.2.1] - 2026-08-23

### Added

- Published the complete licensed Lucide, Tabler, Phosphor, and Hugeicons
  companion catalogs as separately consumable NuGet packages.

### Changed

- Reworked Theme Studio into a centered, curated use-case runway with stable
  realistic cards, responsive device controls, controlled preset shuffling,
  scoped typography, and interaction-aware opposing column motion (#227).

### Fixed

- Kept Message Scroller content constrained above its composer and limited the
  bottom fade to the conversation content instead of the scrollbar.
- Stabilized Code Block language and copy controls, unified the Theme Studio
  shell with documentation navigation, and versioned mutable Showcase boot
  assets so mobile clients do not mix pre- and post-deployment application
  shells.

## [1.2.0] - 2026-08-23

### Added

- Added the responsive Theme Studio workbench with a canonical portable theme
  document schema, strict loading and migration, deterministic palette
  generation, typography roles, 198 bilingual component scenarios, and
  package-supported build-time validation for clean consumers (#171).
- Added the accessible Dropzone component and catalog dossier, custom-answer
  questionnaire choices, primary and secondary chart axes with major and minor
  grid controls, and meaningful expanded table content (#194-#196, #208).
- Added reusable state transitions, controlled and uncontrolled secret-input
  reveal with partial masking, and card presentation for radio groups
  (#199, #206, #209).

### Changed

- Expanded the reviewed catalog to 66 component families, including complete
  Code Block and Dropzone documentation, synchronized Razor source, and
  desktop/mobile Theme scenario evidence (#207-#208).
- Refined Calendar week numbers, package Select month/year navigation,
  invalid-date feedback, Combobox chip spacing, Date Picker width, semantic
  Toast queues, vertical Carousel sizing, and the Toggle Group use case
  (#198, #200-#206, #210).
- Made the documentation header span the viewport while retaining centered
  readable content and responsive navigation (#170).

### Fixed

- Stabilized Accordion, Sidebar, Alert Dialog, Context Menu, Drawer, Dropdown
  Menu, Hover Card, Menubar, and Sheet interaction and responsive presentation
  without trigger or layout shifts (#211-#219).
- Kept Message Scroller auto-follow continuous and resumable, aligned avatars
  to message bodies, and kept the composer and fade inside the transcript safe
  area (#192-#193).
- Finished Code Block copy, toolbar, language selection, Razor/C# highlighting,
  generated imports, API-table wrapping, prose-list alignment, and mobile
  documentation navigation (#184-#191).
- Corrected Chart, Table, Badge, and Carousel rendering states and preserved
  Marker text beneath its left-to-right streaming shimmer (#195-#198).

## [1.1.1] - 2026-08-22

### Fixed

- Read slider read-only state from the live DOM after controlled rerenders so
  changing orientation or read-only state does not leave stale interaction
  behavior.
- Recover NuGet publication with a new patch version. This release supersedes
  the unshipped 1.1.0 NuGet package; the public v1.1.0 tag and GitHub release
  remain unchanged after their validation run stopped before package assets or
  NuGet publication.

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

[Unreleased]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/compare/v2.1.2...HEAD
[2.1.2]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/compare/v2.1.1...v2.1.2
[2.1.1]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/compare/v2.1.0...v2.1.1
[2.1.0]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/compare/v2.0.0...v2.1.0
[2.0.0]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/compare/v1.2.2...v2.0.0
[1.2.2]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/compare/v1.2.1...v1.2.2
[1.2.1]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/compare/v1.2.0...v1.2.1
[1.2.0]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/compare/v1.1.1...v1.2.0
[1.1.1]: https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/compare/v1.1.0...v1.1.1
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
