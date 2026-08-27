# Theme Studio Bento Layout Design

## Status

Approved in conversation on 2026-08-24. This document defines the architecture to implement before producing the detailed implementation plan.

## Problem

Theme Studio currently uses CSS multi-column flow while presenting itself as a Bento layout. Multi-column flow creates independent vertical columns, so a wide card cannot span adjacent columns and the resulting composition behaves like four fixed feeds rather than a responsive Bento grid.

The preview also appends one scenario card for every documentation component. Those cards reuse documentation-oriented examples and states, making Theme Studio feel like a duplicate component catalog instead of a coherent set of realistic application workflows.

## Goals

1. Ship reusable public Bento layout primitives in `Maliev.ShadcnBlazor.Components.Layout`.
2. Make the layout respond to its own available inline size, including Theme Studio's simulated device width.
3. Allow individual items to span multiple columns and rows without page-specific positioning rules.
4. Preserve DOM, reading, and keyboard focus order across responsive layouts.
5. Make Theme Studio contain only purpose-built realistic workflow compositions.
6. Demonstrate every public component in at least three meaningful curated workflow contexts or states.
7. Keep documentation examples and Theme Studio use cases as separate content systems.

## Non-goals

- Native CSS masonry or JavaScript masonry positioning.
- Reordering cards to fill every visual gap.
- Copying component dossier previews into Theme Studio.
- Adding application-specific data models or business services to the component package.
- Changing card order or content when a theme preset is shuffled.

## Public component architecture

### `ShadcnBentoGrid`

`ShadcnBentoGrid` is the responsive layout owner. It renders a query container around an inner CSS Grid so the grid can adapt to the component's actual allocated width rather than the browser viewport.

Proposed public parameters:

| Parameter | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Columns` | `int` | `4` | Maximum column count at the widest supported container size. |
| `MediumColumns` | `int` | `2` | Column count at the intermediate container size. |
| `Gap` | `string?` | `null` | Optional CSS length overriding the semantic layout gap token. |
| `ChildContent` | `RenderFragment?` | `null` | Bento items rendered in source order. |

The component validates column counts and CSS-length input. It forwards unmatched attributes without allowing callers to replace owned slot attributes.

DOM structure:

```html
<div class="shadcn-bento-grid" data-slot="bento-grid">
  <div class="shadcn-bento-grid__layout" data-slot="bento-grid-layout">
    ...items...
  </div>
</div>
```

The outer element declares `container-type: inline-size`. The inner element owns `display: grid`, gap, and column templates.

### `ShadcnBentoItem`

`ShadcnBentoItem` owns item placement without knowing the content rendered inside it.

Proposed public parameters:

| Parameter | Type | Default | Purpose |
| --- | --- | --- | --- |
| `ColumnSpan` | `int` | `1` | Requested maximum number of columns occupied. |
| `RowSpan` | `int` | `1` | Requested maximum number of grid rows occupied. |
| `ChildContent` | `RenderFragment?` | `null` | Card or workflow content. |

Spans are emitted as component-owned CSS custom properties and data attributes. Values are validated as positive and are capped by the active column count in CSS. At a one-column container, every item occupies one column regardless of its requested maximum span.

## Responsive layout behavior

The grid uses mobile-first container queries:

- Narrow container: one column; all items span one column.
- Medium container: `MediumColumns`, normally two; wide items may span two.
- Wide container: up to `Columns`, normally four; standard items span one, wide items span two, and explicitly featured workflows may use larger spans.

Theme Studio keeps `1rem 0` block/inline padding on the Bento grid region. Card borders remain entirely visible because the layout does not clip or position cards against the container edge.

The component does not use `grid-auto-flow: dense`, CSS `order`, or explicit placement that changes logical order. Small unused cells are acceptable when preserving reading and focus sequence requires them.

## Theme Studio composition

`ThemeBento` will use `ShadcnBentoGrid` and wrap every workflow in `ShadcnBentoItem`. `ThemeBentoSize` maps to public spans:

- `Standard`: one column, one row.
- `Wide`: two columns, one row.
- `Tall`: one column, two rows when the content benefits from a taller composition.

The existing CSS `column-count` implementation and page-specific child flow rules will be removed.

## Dedicated realistic use cases

Theme Studio will stop rendering `ThemeScenarioBentoCard` entries from `IThemeScenarioRegistry`. Documentation scenarios remain available only through the component documentation routes and their testing infrastructure.

Every Theme Studio card must represent a recognizable task, decision, or operational state. Initial workflow families include:

- Production capacity, scheduling, machine state, and progress.
- Quotation review, pricing approval, actions, and status.
- Drawing attachment, upload, revision, and file actions.
- Inspection results, nonconformance reporting, alerts, and evidence tables.
- Shipping address, handoff, calendar scheduling, and dispatch confirmation.
- Staff profiles, assignments, permissions, and reviewer collaboration.
- Assistant conversation, message navigation, reactions, markers, and questionnaires.
- Production analytics, chart comparisons, data tables, filtering, and pagination.
- Settings, disclosure, navigation, menus, overlays, and contextual help embedded in appropriate workflows.
- Loading, empty, success, warning, error, disabled, and recovery states within those workflows.

Cards may share fictional production entities when doing so creates continuity, but each card must remain understandable on its own. Content must use neutral fictional Thai people and MALIEV manufacturing scenarios without private customer or employee data.

## Curated coverage contract

A checked-in curated coverage registry maps each public catalog component to at least three distinct realistic workflow usages or meaningful states. A usage counts only when the real package component is rendered; class-name imitation or static markup does not count.

Coverage validation will enforce:

1. Every registered public component appears in the curated coverage registry.
2. Every component has at least three distinct workflow or state references.
3. Every referenced workflow exists in `ThemeUseCaseRegistry`.
4. Every workflow declares the package components it actually renders.
5. Theme Studio contains no documentation scenario-card loop.

The coverage registry is a test and maintenance boundary, not visible Theme Studio UI.

## Interaction and motion

All components remain fully interactive inside their workflow cards, including file selection, forms, menus, dialogs, drawers, sheets, tooltips, hover cards, message submission, questionnaire progression, table operations, and chart controls.

### Scroll-triggered reveal sequence

The Bento canvas remains manually scrollable. A preview-scoped intersection observer marks each workflow card as it enters the visible preview region for the first time. The card then runs a short entrance sequence rather than appearing abruptly:

1. The card surface fades and translates into place.
2. Its heading and primary content reveal in reading order.
3. Data visualizations animate from their semantic origin: bars grow from the baseline, progress indicators fill from logical start, and chart paths reveal without changing the underlying values.
4. Conversation turns and form demonstrations begin only after their parent card is visible.

Cards already revealed do not replay merely because the user scrolls away and back. Newly inserted or remounted workflow content receives its own reveal sequence. The observer and animation state live inside Theme Studio and do not change the behavior of package components in consuming applications.

### Text-entry demonstrations

Every prefilled text input or textarea in the curated preview uses a Theme Studio composition wrapper around the real `ShadcnInput` or `ShadcnTextarea`. The wrapper presents a CSS-driven text reveal while leaving the real control available for interaction. Focusing, clicking, or typing immediately completes the reveal and hands control to the native input without losing its value or selection.

Typing effects reveal the complete laid-out string using clipping or masking rather than wrapping every character in an independently sized inline box. This preserves Thai grapheme shaping, word wrapping, punctuation, and bubble geometry. Conversation messages use the same whole-text reveal and never animate backwards.

Purposeful demonstrations may animate progress, streaming text, or loading state. Motion must pause during direct user interaction, honor the preview's reduced-motion setting, and never reset card order or scroll position when a theme is shuffled.

Overlays may temporarily cover neighboring cards because they demonstrate the real package behavior. Their focus management, dismissal, portal placement, and responsive geometry remain component-owned.

Reduced-motion mode bypasses reveal delays and displays every value and control immediately. It must never leave content transparent, clipped, or visually incomplete.

## Workflow composition quality

### Drawing workspace

The drawing workspace is a real file-review task rather than a passive rectangle. It includes the package attachment or dropzone surface, file identity and revision metadata, preview status, reviewer ownership, and contextual file actions such as open, compare revision, download, and archive. Pointer and keyboard users can open the real context menu from a clearly named target.

### Avatar and identity alignment

Every avatar-and-copy composition uses a consistent inline layout: avatar first, then a left-aligned identity block whose name and supporting text share the same logical start edge. The composition must remain correct in RTL by using logical alignment rather than physical left/right offsets. Avatar groups use the package group component rather than hand-built overlapping circles.

### Card spacing ownership

Implementation begins with an isolated comparison between `ShadcnCardHeader`/`ShadcnCardContent` defaults and the Theme Studio overrides. A defect reproduced in an ordinary card is fixed in the package; a mismatch caused only by the curated preview is fixed in the preview composition. Theme Studio then applies one documented card-density contract across every workflow: consistent outer padding, deliberate header-to-content separation, and no collapsed, doubled, or negative margins.

## Theming boundary

Theme configuration changes apply only inside `.theme-preview-scope`. The Theme Studio application shell, documentation header, and settings sidebar retain the MALIEV company theme. Typography, radius, palette, icon library, contrast, and motion settings must not leak outside the preview scope.

## Documentation

The new Bento primitives are registered as a Layout component with a full dossier. Its live preview contains three dedicated examples:

1. Responsive product summary with one featured wide item.
2. Mixed standard, wide, and tall operational cards.
3. Narrow-container reflow demonstrating that spans safely collapse to one column.

These dossier examples explain the layout API; Theme Studio does not reuse their content.

## Accessibility

- DOM order matches visual reading and keyboard focus order.
- The layout adds no landmark or list semantics unless supplied by the consumer.
- Cards remain operable at 200% zoom and at a 320 CSS-pixel viewport without horizontal page scrolling.
- Logical properties support LTR and RTL.
- Forced-colors mode preserves visible card and focus boundaries.
- Container reflow does not hide content or create unreachable interactive controls.

## Migration sequence

1. Add failing component tests for the public Bento API, validation, attributes, and responsive CSS contract.
2. Implement and style `ShadcnBentoGrid` and `ShadcnBentoItem`.
3. Add the public API snapshot, Layout catalog entry, dossier, and three documentation examples.
4. Add failing Theme Studio contract and browser tests for spans, reflow, source order, and documentation-content separation.
5. Migrate curated workflows to the package Bento components and remove multi-column CSS.
6. Replace documentation scenario cards with dedicated realistic workflows and the nonvisual coverage registry.
7. Verify build, focused tests, full package tests, browser behavior, public-surface checks, formatting, and the Impeccable layout detector.

## Acceptance criteria

- A wide Theme Studio card is approximately two grid tracks plus one gap at the four-column desktop size.
- The same card spans both tracks at the two-column size and one track at mobile size.
- Standard cards retain one-track width and all card borders are visible.
- No `column-count`, multi-column card flow, `grid-auto-flow: dense`, or documentation scenario-card loop remains in Theme Studio.
- Every public package component has at least three validated realistic curated usages or states.
- All curated cards use actual `Maliev.ShadcnBlazor` components and remain interactive.
- Manually scrolling the preview reveals newly visible cards and starts their internal chart, progress, conversation, and form sequences once.
- Prefilled text controls use a CSS-driven reveal, become immediately editable on interaction, and preserve Thai grapheme shaping and normal wrapping.
- The assistant response reveals from logical start to end without fragmented glyphs, reversed completion, or layout collapse.
- The drawing workspace represents a complete interactive file-review workflow rather than a static context-menu target.
- Avatar identity blocks share one logical start edge and remain aligned in LTR and RTL.
- Card headers and bodies use a consistent verified spacing contract, with package changes made only when the defect reproduces outside Theme Studio.
- Theme shuffling changes only preview-scoped styling and preserves scroll position, card order, content, and interaction state where component semantics allow.
- The new Bento component is packaged, documented, keyboard-order-safe, responsive, RTL-safe, forced-colors-safe, and covered by automated tests.
