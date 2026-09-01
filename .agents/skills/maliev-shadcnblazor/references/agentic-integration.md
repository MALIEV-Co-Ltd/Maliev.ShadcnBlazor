# Agentic application integration

Use this reference when a consuming application wants repository-level agent
guidance or asks an agent to compose a multi-component workflow. The installed
package version and its official component dossiers remain authoritative.

## Application AGENTS.md contract

Add or adapt a focused section like this in the consuming repository's
`AGENTS.md`. Preserve any stricter repository rules already present.

```markdown
## Maliev.ShadcnBlazor UI work

- Use `$maliev-shadcnblazor` for package installation, component selection,
  composition, theming, and consumer-side diagnosis.
- Inspect the installed package version, existing application shell, theme
  provider, assets, and validation approach before editing.
- Confirm component parameters from the installed assembly or the matching
  official dossier. Do not infer a React shadcn/ui or MudBlazor API.
- Prefer public package components and semantic theme tokens over copied
  Showcase markup, private selectors, or replacement JavaScript.
- Keep application data, routes, services, and state ownership outside the
  package components.
- Build before tests. Verify the affected keyboard, focus, validation,
  responsive, theme, RTL, reduced-motion, and forced-color states.
```

This contract routes the work; it does not authorize package upgrades, pushes,
deployments, or other external changes.

## Task-oriented composition

Translate the user's product goal into public components only after inspecting
the installed package version and the existing application structure.

### Responsive sidebar

- Start with `ShadcnSidebar` and its documented composition primitives.
- Use `ShadcnSidebarTrigger` only when the chosen collapse mode exposes a
  collapse action, and keep that control within the sidebar composition.
- Add `ShadcnTooltip` when an icon rail needs accessible text beyond its visible
  labels.
- Verify expanded, icon, off-canvas, no-collapse, mobile, focus-return, LTR, and
  RTL behavior supported by the installed version.

### Financial charts

- Start with `ShadcnChart`, then add documented `ShadcnChartTooltipContent` and
  `ShadcnChartLegendContent` composition only when the data benefits from them.
- Choose a chart type from the analytical question. Do not combine mutually
  exclusive plot modes, and do not apply Cartesian axes or grids to pie or
  donut charts.
- Give every series or slice an accessible name and use semantic theme-aware
  colors. Verify resizing, loading, empty data, keyboard access, and narrow
  layouts.

### Validated quotation form

- Compose documented form primitives such as `ShadcnInput`, `ShadcnSelect`, and
  `ShadcnDatePicker`; confirm the exact names and parameters in the installed
  version before using them.
- Keep the form model, validation messages, submission state, and business
  rules caller-owned. Preserve native labels, required state, error
  relationships, and disabled/read-only semantics.
- Verify keyboard submission, invalid and valid states, loading, error recovery,
  responsive layout, and both light and dark themes.

## Completion evidence

Report the installed package version, chosen public components, files changed,
and the exact build, test, browser, and accessibility checks performed. If the
installed API cannot support the requested behavior, explain the version
boundary instead of copying Showcase internals or inventing a parameter.
