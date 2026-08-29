# Theme Studio Palette Workbench Design

**Status:** Approved design

**Date:** 2026-08-29

**Mode:** Operate

**Selected direction:** Preview plus palette flyout (Option A)

## Purpose

Theme Studio currently exposes curated presets and a visual color-treatment
choice, while its deterministic palette generator, token locks, share codec,
and individual token color editors are not available in the primary editor.
Users need Coolors-style freedom to generate and tune an unlimited sequence of
palettes without learning the complete Shadcn semantic token model.

The palette workbench will expose five editable anchor colors, deterministic
generation, color locking, live semantic mapping, accessibility validation,
and exact export. It will keep the component preview visible so users judge a
palette as an interface rather than as isolated swatches.

## Scope

This slice includes:

- a compact active-palette summary inside Visual treatment;
- a non-modal desktop palette workbench beside the live preview;
- a full-height responsive workbench on constrained viewports;
- five editable and lockable anchor colors;
- Free, Analogous, Complementary, and Triadic harmony modes;
- deterministic generation from a portable recipe;
- automatic light/dark semantic-token mapping and contrast repair;
- v1 recipe compatibility and deliberate v2 upgrade behavior;
- undo, redo, persistence, sharing, import, and export integration;
- English and Thai user-facing generator copy;
- component, contract, state, serialization, migration, and browser coverage.

This slice does not add a hosted palette service, Coolors integration, user
accounts, cloud synchronization, image extraction, AI generation, or an
unbounded saved-palette collection. Generation itself has no quota; users keep
palettes through ordinary theme documents, share values, and exports.

The code-block selector polish and overlay-use-case redesign remain a separate
validated commit so their regression boundary is not obscured by the palette
subsystem.

## Interaction design

### Compact sidebar state

Visual treatment will retain its existing surface, depth, motion, and intensity
controls. The color-treatment row will be followed by a compact palette summary
containing:

- the current five-color strip;
- the palette name or deterministic seed label;
- a concise contrast state;
- a single **Customize palette** action.

The summary is not a second token editor. It communicates the current palette
and opens the workbench.

### Desktop workbench

At wide desktop sizes, the workbench becomes a dedicated layout column between
the settings sidebar and preview. It does not trap focus or disable the preview.
Users can continue interacting with representative components while editing.

The workbench contains:

1. A heading, close action, palette identity, and contrast summary.
2. Five anchor swatches: Brand, Support, Highlight, Data A, and Data B.
3. For each swatch: color picker, hexadecimal/OKLCH value editor, copy action,
   and lock toggle.
4. Harmony selection: Free, Analogous, Complementary, or Triadic.
5. A primary **Generate palette** action and the optional Spacebar shortcut
   while focus is within the workbench and not inside an editable control.
6. Precise validation details when the generated semantic theme needs review.

Directly editing a swatch locks that anchor. Unlocking it allows later
generation to replace it. Generation changes only unlocked anchors and creates
one undoable history entry.

Closing the workbench preserves the active palette and returns the sidebar to
its compact summary.

### Responsive workbench

When adding the workbench column would reduce the preview below its useful
minimum width, the same workbench becomes a full-height sheet. Its palette strip
and Generate action remain sticky, and **Return to preview** closes the sheet.
The sheet owns focus while open and restores focus to Customize palette.

No horizontal page scrollbar is introduced by opening the workbench.

## Palette semantics

The five anchors express visual taste; they are not raw semantic tokens.

| Anchor | Primary semantic use |
| --- | --- |
| Brand | Primary actions, focus ring, sidebar primary |
| Support | Secondary controls and supporting selections |
| Highlight | Accent surfaces and selected states |
| Data A | Fourth chart-series identity and supporting data color |
| Data B | Fifth chart-series identity and supporting data color |

The five chart colors map directly from Brand, Support, Highlight, Data A, and
Data B so charts visibly belong to the selected palette.

Backgrounds, cards, popovers, muted surfaces, borders, readable foregrounds,
sidebar surfaces, and dark-mode variants are derived from the anchors and the
existing neutral-family selection. Destructive colors remain a dedicated red
semantic family so arbitrary palette generation does not change the meaning of
dangerous actions.

An anchor lock preserves the exact anchor supplied to future generations. The
mapper may derive different lightness and chroma for light and dark semantic
roles while preserving that anchor's hue identity. Existing semantic-token
locks remain expert overrides and take precedence after mapping.

## Public library architecture

The reusable Razor Class Library owns palette recipes, deterministic
generation, semantic mapping, validation, migration, and serialization. The
Showcase owns the Theme Studio workbench composition and application state.
Consuming applications do not ship the editor to use an exported theme.

### Recipe v2

`ShadcnPaletteRecipe` keeps its existing four-argument constructor so current
consumers remain source compatible. Version 2 adds typed properties for:

- a five-value `ShadcnPaletteAnchors` snapshot;
- `ShadcnPaletteHarmony`;
- an immutable set of locked `ShadcnPaletteAnchorRole` values.

The existing seed, neutral base color, algorithm version, and locked semantic
token paths remain part of the recipe. New collections take defensive immutable
snapshots, matching the current locked-token contract.

`ShadcnPaletteGenerator.Generate` dispatches by algorithm version:

- v1 remains byte-identical for existing recipes;
- v2 generates only unlocked anchors, maps the five anchors into a complete
  theme, applies semantic-token locks, and validates the result.

Unsupported versions continue to fail closed with a diagnostic. Public API
additions require XML documentation, API snapshot review, and focused tests.

### Migration behavior

Opening a v1 document derives a read-only five-swatch workbench preview from its
materialized semantic theme: Primary, Secondary, Accent, Chart4, and Chart5.
The stored recipe and exported bytes remain v1 until the user edits a swatch,
changes harmony, or generates a new palette.

The first palette mutation explicitly upgrades the recipe to v2 and captures a
history entry. Import never silently upgrades or rewrites a valid v1 document.

Materialized theme values remain embedded in JSON and generated CSS. Runtime
consumers therefore receive exact values and do not need to execute the palette
generator.

## Theme Studio state and data flow

1. The workbench reads the active recipe and materialized theme from
   `ThemeStudioState`.
2. A picker interaction starts a coalesced pointer transaction.
3. Valid color input creates a v2 candidate recipe and requests generation.
4. The generator returns a candidate theme plus errors and advisories without
   mutating the source theme.
5. A valid candidate becomes Draft and Applied, updates the document template,
   creates one history entry, persists through the existing storage boundary,
   and refreshes the live preview.
6. An invalid candidate leaves the last valid preview and document unchanged.
7. Undo and redo restore the theme, recipe, locks, diagnostics, and workbench
   summary together.

Generation is local, synchronous, deterministic, and network-independent. A
seed plus identical v2 recipe produces byte-identical materialized output.

## Validation and failure behavior

- Invalid or unsupported color syntax produces an inline field error and does
  not change the active theme.
- Out-of-gamut input is normalized through the library color parser and the
  normalized value is shown back to the user.
- Contrast repair adjusts unlocked derived foregrounds or surfaces, never an
  explicitly locked anchor.
- If locked semantic tokens make a required contrast pair impossible, the
  generation is rejected with the exact paths, measured ratio, and required
  ratio.
- Errors block application; advisories do not. The workbench and existing
  validation summary use the same diagnostic source.
- Persistence, share decoding, or import failures keep the previous valid state
  and expose a user-readable diagnostic.

## Accessibility and localization

- Swatches have semantic labels and textual values; meaning never relies only
  on color.
- All controls have visible focus, logical order, accessible names, and at
  least the repository's supported target size.
- Generate, lock, copy, close, and Return to preview are keyboard operable.
- Escape closes the responsive sheet; desktop Escape closes the workbench only
  when focus is inside it.
- Spacebar generation is active only within the workbench and never intercepts
  typing, listbox operation, or native color-input behavior.
- Live regions announce successful generation, lock changes, blocking errors,
  and application state without announcing each pointer movement.
- Reduced-motion mode removes swatch and preview color transitions.
- Forced-colors mode retains boundaries, lock states, selection, and focus.
- User-facing workbench labels, statuses, and diagnostics have complete English
  and Thai copy. Palette values, identifiers, and exported code remain
  language-neutral.

## Verification strategy

Implementation begins test-first and must cover:

### Library and contract tests

- v1 generation remains byte-identical;
- v2 generation is deterministic across repeated and parallel calls;
- each harmony produces valid anchor relationships;
- locked anchors survive regeneration;
- direct anchor edits, semantic-token locks, and their precedence;
- light/dark semantic mapping and five-color chart mapping;
- contrast repair and impossible locked-constraint failures;
- constructor compatibility, defensive snapshots, JSON round trips, schema
  migration, and unsupported-version rejection;
- public API and XML documentation changes.

### Theme Studio state and component tests

- compact summary state and workbench open/close behavior;
- automatic lock after direct editing and explicit unlock;
- one history entry per generation or coalesced picker gesture;
- undo, redo, persistence, sharing, import, export, and v1 upgrade timing;
- invalid input preserves the last valid preview;
- English and Thai copy completeness.

### Browser tests

- wide desktop workbench and live preview remain simultaneously usable;
- tablet/mobile sheet layout, focus containment, close, and restoration;
- no viewport overflow at supported widths and zoom levels;
- mouse, keyboard, native color input, and safe Spacebar generation;
- focus visibility, live announcements, reduced motion, forced colors, LTR,
  RTL, light, and dark behavior;
- rapid picker input produces stable preview updates without excessive history
  or persistence writes;
- exported theme values match the visible active palette.

Validation follows repository order: Release build with zero warnings and
errors, focused tests, the complete affected test projects, relevant
Playwright filters, formatting, public-surface verification, and diff checks.

## Implementation boundaries

The work should land as coherent validated commits:

1. Library recipe v2, generator mapping, migration, and contract tests.
2. Theme Studio state integration and palette workbench UI with component and
   browser tests.
3. The separately approved code-block and overlay refinements with their own
   focused regression tests.

No version release, push, deployment, or package publication is authorized by
this design approval.
