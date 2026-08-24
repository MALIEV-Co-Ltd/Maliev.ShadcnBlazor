# Composable Modern UI Style Scope Design

## Status

Approved in conversation on 2026-08-24. This document defines the public styling architecture to implement before producing the detailed implementation plan.

## Problem

`ShadcnBentoGrid` provides a responsive composition system, but a layout primitive cannot express the broader visual languages consumers need. Minimal, glass, neo-brutalist, vibrant-dark, and liquid-glass interfaces change surfaces, controls, depth, contrast, and motion independently of whether the page uses Bento, an ordinary grid, a form, or an overlay.

Consumers currently have to fork component CSS or add page-specific selectors to create those styles. That approach is difficult to reuse, easy to leak into the application shell, and cannot be configured safely in Theme Studio.

## Goals

1. Ship a reusable public wrapper that applies a modern visual treatment to an arbitrary component subtree.
2. Keep visual treatment independent from layout, theme palette, and application data.
3. Allow style layers to compose without an enum for every possible combination.
4. Preserve component behavior, accessibility, RTL behavior, responsive layout, and caller-provided classes and attributes.
5. Keep all style changes scoped to the wrapper and prevent Theme Studio settings from restyling the Theme Studio application shell.
6. Demonstrate each supported treatment through realistic, interactive workflows in Theme Studio and dedicated component documentation.

## Non-goals

- Replacing `ShadcnThemeProvider` or the semantic theme-document format.
- Making Bento a visual treatment; Bento remains a separate layout primitive.
- Recreating full application themes inside every component.
- Applying styles globally to `body`, `:root`, or unrelated MudBlazor content.
- Using continuously running JavaScript pointer tracking for decorative effects.
- Guaranteeing blur or transparency when the browser, operating system, forced-colors mode, or user preference disables those effects.

## Public API

### `ShadcnVisualStyleScope`

The component lives in `Maliev.ShadcnBlazor.Components.Styling`, inherits `ShadcnComponentBase`, renders one neutral `div`, and forwards unmatched attributes. It owns only its slot and style data attributes.

```razor
<ShadcnVisualStyleScope VisualStyle="ShadcnVisualStyle.Glass"
                        ColorTreatment="ShadcnColorTreatment.VibrantDark"
                        Depth="ShadcnDepthTreatment.Spatial"
                        Motion="ShadcnMotionTreatment.Expressive">
    <ShadcnBentoGrid>
        ...
    </ShadcnBentoGrid>
</ShadcnVisualStyleScope>
```

Public parameters:

| Parameter | Type | Default | Purpose |
| --- | --- | --- | --- |
| `VisualStyle` | `ShadcnVisualStyle` | `Inherit` | Surface and control language: `Inherit`, `Minimal`, `Glass`, `NeoBrutalist`, or `LiquidGlass`. |
| `ColorTreatment` | `ShadcnColorTreatment` | `Inherit` | Color treatment independent of surface style: `Inherit` or `VibrantDark`. |
| `Depth` | `ShadcnDepthTreatment` | `Inherit` | Elevation treatment: `Inherit`, `Flat`, `Raised`, `Floating`, or `Spatial`. |
| `Motion` | `ShadcnMotionTreatment` | `Inherit` | Local motion personality: `Inherit`, `Calm`, `Expressive`, or `None`. |
| `Intensity` | `ShadcnStyleIntensity` | `Default` | `Subtle`, `Default`, or `Strong` tuning without changing style identity. |
| `ChildContent` | `RenderFragment?` | `null` | Arbitrary Shadcn components, layouts, or consumer markup. |

Every enum value is validated. The rendered root exposes stable kebab-case attributes such as `data-visual-style="glass"`, `data-color-treatment="vibrant-dark"`, and `data-slot="visual-style-scope"`. Nested scopes are supported: an inner explicit value replaces that layer while inherited layers continue from the outer scope.

The existing `Class`, `Style`, and unmatched-attribute contract remains unchanged. The API does not accept raw CSS fragments beyond the existing caller-owned `Style` parameter.

## Layer model

The layers solve different concerns and therefore remain independently configurable:

- **Visual style** changes surface material, control geometry, border character, and decorative treatment.
- **Color treatment** changes the scoped semantic palette. `VibrantDark` uses deep neutral surfaces and the active theme's primary/chart colors as controlled accents; it does not toggle the host application's dark mode.
- **Depth** changes elevation and separation without changing palette or layout.
- **Motion** changes local transition timing and hover/focus movement while still respecting reduced-motion policy.
- **Intensity** scales effects such as blur, border weight, shadow, and accent glow through bounded tokens.

This permits combinations such as Minimal + Flat, Glass + VibrantDark + Floating, or LiquidGlass + Spatial. Neo-brutalism can use the normal light palette or the same vibrant-dark color treatment without another public preset type.

## Token and selector contract

The wrapper emits a small set of local CSS custom properties. These tokens are implementation details but use stable names so advanced consumers can inspect and override them:

- `--shadcn-style-surface`
- `--shadcn-style-surface-strong`
- `--shadcn-style-border`
- `--shadcn-style-border-width`
- `--shadcn-style-shadow`
- `--shadcn-style-shadow-hover`
- `--shadcn-style-blur`
- `--shadcn-style-saturation`
- `--shadcn-style-radius-factor`
- `--shadcn-style-control-offset`
- `--shadcn-style-transition-duration`

The package stylesheet owns a documented set of semantic targets rather than styling every descendant element. Targets are selected by existing `data-slot` values and grouped as:

1. **Surfaces:** cards, alerts, tables, command palettes, calendars, questionnaires, navigation panels, and message surfaces.
2. **Floating surfaces:** dialogs, drawers, sheets, popovers, menus, hover cards, tooltips, and toasts.
3. **Controls:** buttons, inputs, selects, toggles, sliders, tabs, checkboxes, radio items, and dropzones.
4. **Data and feedback:** chart surfaces, progress tracks, badges, skeletons, spinners, and empty states.

The stylesheet changes presentation only. It must not change display mode, grid placement, intrinsic width, focus order, disabled state, pointer behavior, or component-owned positioning.

## Supported visual treatments

### Minimal

- Transparent or quiet solid surfaces.
- Hairline boundaries only where grouping requires them.
- Flat depth, restrained transitions, generous but bounded spacing, and no decorative glow.
- Clear type hierarchy and strong focus states remain visible.

### Glass

- Translucent semantic surfaces with bounded blur and saturation.
- Thin light/dark-aware borders and a subtle elevation layer.
- A solid semantic fallback is declared before `backdrop-filter`.
- Transparency never lowers text or control contrast below the package's validation thresholds.

### Neo-brutalist

- Opaque, high-contrast surfaces with heavier borders and short offset shadows.
- Deliberate square or lightly rounded geometry.
- Hover and pressed movement uses small physical offsets without changing layout dimensions.
- Focus rings remain distinct from decorative outlines.

### Liquid glass

- Layered translucent surfaces, soft spectral highlights, deeper spatial shadows, and squircle-like radius treatment.
- Motion reacts to hover and focus-within using CSS transforms and highlight-position transitions rather than continuous pointer tracking.
- `@supports` fallbacks reduce the treatment to ordinary glass or a solid raised surface when blur, color mixing, or advanced masking is unavailable.

### Vibrant dark

- A color treatment rather than a surface style.
- Uses deep neutral semantic surfaces, readable foreground values, and controlled theme-primary/chart accents.
- Accent glow is limited to selected, focused, active, or data-emphasis states; ordinary body text never glows.
- It composes with Minimal, Glass, Neo-brutalist, or Liquid glass.

## Layout composition

The wrapper does not know about Bento. Consumers compose the two explicitly:

```razor
<ShadcnVisualStyleScope VisualStyle="ShadcnVisualStyle.LiquidGlass"
                        Depth="ShadcnDepthTreatment.Spatial">
    <ShadcnBentoGrid Columns="4" MediumColumns="2">
        <ShadcnBentoItem ColumnSpan="2">...</ShadcnBentoItem>
        <ShadcnBentoItem>...</ShadcnBentoItem>
    </ShadcnBentoGrid>
</ShadcnVisualStyleScope>
```

The same wrapper works around a form, application shell section, table, dialog composition, or ordinary `div`. Removing the wrapper restores normal package styling without changing markup structure.

## Overlay behavior

Current Shadcn overlay portal components remain in the Blazor render subtree, so they inherit the wrapper's CSS variables and selectors. Tests cover dialog, drawer, dropdown, context-menu, popover, hover-card, sheet, tooltip, and toast content.

MudBlazor services rendered by the application-level `ShadcnThemeProvider` are not silently restyled by a nested visual scope. Supporting external portal roots would require an explicit future bridge rather than leaking a nested style onto the global overlay container.

## Theme Studio integration

Theme Studio wraps only `.theme-preview-scope` content in `ShadcnVisualStyleScope`. The documentation header, settings sidebar, and application background remain MALIEV's company theme.

The sidebar gains a compact **Visual treatment** group:

- Style: Minimal, Glass, Neo Brutalist, or Liquid Glass.
- Color: Theme palette or Vibrant Dark.
- Depth: Flat, Raised, Floating, or Spatial.
- Motion: Calm or Expressive, with existing reduced-motion controls retaining priority.
- Intensity: Subtle, Default, or Strong.

Curated theme presets store these typed values. Shuffle changes them only as part of a reviewed preset, preserves Bento content and scroll position, and never changes the Theme Studio shell. The existing theme export includes the visual-style configuration as an optional integration snippet without changing the versioned theme-document schema in this slice.

## Documentation

Register **Visual Style Scope** as a Foundation component. Its dossier contains three dedicated interactive examples:

1. Minimal and neo-brutalist production approval flows.
2. Glass and liquid-glass scheduling/analytics surfaces with fallback status.
3. Vibrant-dark composition showing controls, chart data, overlays, focus, disabled state, and reduced motion.

The examples explain composability and do not reuse Theme Studio workflow cards. Usage documentation shows wrapping Bento and non-Bento content, nesting scopes, fallbacks, and accessibility constraints.

## Accessibility and resilience

- `prefers-reduced-motion: reduce`, `ShadcnThemeProvider`'s always-reduce policy, or `Motion=None` removes decorative transforms and extended transitions.
- Forced-colors mode removes blur, transparency, glow, and decorative shadow while retaining system borders and focus indicators.
- `prefers-contrast: more` strengthens semantic boundaries and disables low-opacity borders.
- Transparent styles always provide opaque fallback backgrounds.
- Visual style never conveys state without text, icon, or semantic state already owned by the component.
- Logical properties and inherited direction preserve LTR and RTL behavior.
- The wrapper adds no landmark or interaction semantics by default.

## Testing strategy

1. Component tests validate enum values, data attributes, class/style merging, unmatched attributes, nesting, and child rendering.
2. CSS contract tests verify all layers, semantic target groups, fallbacks, reduced motion, forced colors, and contrast media rules.
3. Public API and catalog tests register the component and prevent accidental enum or parameter drift.
4. Theme Studio state tests prove style changes are preview-scoped and preserved by history/import/export behavior.
5. Browser tests cover each treatment at desktop, tablet, and mobile sizes; representative forms, charts, menus, dialogs, and Bento spans remain interactive.
6. Accessibility scans run for every treatment, including Vibrant Dark and forced-colors-compatible markup.
7. Package build, full unit suite, repository suite, and focused browser suite must pass with zero build warnings.

## Migration and compatibility

The feature is additive. Existing themes and components render identically when no `ShadcnVisualStyleScope` is present or when all layers are `Inherit`. Existing Bento APIs do not change.

No theme-document migration is required. A future schema revision may represent the style layers directly only after consumer demand proves that configuration belongs in the portable theme document rather than the composition layer.

## Acceptance criteria

- One public wrapper applies typed, composable visual treatments to arbitrary Shadcn content.
- Minimal, Glass, Neo Brutalist, Liquid Glass, and Vibrant Dark are visibly distinct and can be combined with Bento or non-Bento layouts.
- The wrapper does not mutate `:root`, `body`, the Theme Studio shell, or unrelated sibling content.
- Existing behavior is unchanged when all layers inherit.
- Overlays inside the wrapper receive the same treatment without losing focus management, dismissal, positioning, or pointer interaction.
- Every treatment has solid fallbacks and passes reduced-motion, forced-colors, RTL, mobile overflow, and accessibility checks.
- Theme Studio exposes the style layers in its sidebar, includes them in curated preset/shuffle behavior, and preserves preview scroll and content.
- Dedicated documentation includes at least three interactive examples and copyable source.
- The package build and affected automated suites complete with zero warnings and zero failures.
