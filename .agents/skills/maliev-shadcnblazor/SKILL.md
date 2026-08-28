---
name: maliev-shadcnblazor
description: Integrate Maliev.ShadcnBlazor into a .NET Blazor application. Use when installing or configuring the package, selecting and composing components, applying themes, wiring forms or overlays, or diagnosing consumer-side rendering, accessibility, asset, and interop issues. Do not use for changing the library repository itself; use maliev-shadcnblazor-maintainer instead.
---

# Maliev ShadcnBlazor

Build with the public package contract. Do not copy Showcase internals or
invent parameters that are not present in the installed version.

## Workflow

1. Inspect the consuming application before editing:
   - target framework and Blazor hosting model;
   - installed `Maliev.ShadcnBlazor` and `MudBlazor` versions;
   - `Program.cs`, `_Imports.razor`, the root layout, and loaded static assets;
   - existing theme provider, direction, validation, and overlay setup.
2. Read [references/setup-and-selection.md](references/setup-and-selection.md)
   when installing the package, selecting a component, or checking asset order.
3. Confirm the component API from the installed assembly, current source, or
   official component dossier. Never infer a React shadcn/ui API or a MudBlazor
   API from a similar name.
4. Make the smallest composition that solves the application workflow:
   - prefer typed parameters, `EventCallback`, and native form semantics;
   - keep state ownership explicit;
   - pass supported attributes without replacing component-owned ARIA state;
   - use semantic theme tokens instead of styling package internals;
   - keep application DTOs, routes, and services outside the UI component.
5. Verify the states users can reach: default, loading, empty, success, error,
   disabled, read-only, and validation where applicable.
6. Build the consuming project before tests. Exercise keyboard behavior, focus,
   accessible names, light/dark themes, narrow layouts, and any overlay or JS
   interaction changed by the task.

## Guardrails

- Treat the installed package version as authoritative. If online documentation
  differs, explain the version mismatch and adapt to the installed API.
- Do not add every package stylesheet blindly when the app intentionally uses a
  documented subset; preserve the required order for the selected assets.
- Do not target generated element IDs or private `shadcn-*` implementation
  structure from application CSS or JavaScript.
- Do not add raw JavaScript for behavior the component already owns.
- Do not claim accessibility or responsive behavior passes without exercising
  the relevant states.
- A skill provides guidance only. It does not grant tools, credentials, publish
  rights, or permission to change external systems.

## Deliverable

Report the package version used, registration and asset changes, components
composed, verification performed, and any version-specific limitation.
