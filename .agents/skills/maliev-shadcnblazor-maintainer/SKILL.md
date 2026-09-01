---
name: maliev-shadcnblazor-maintainer
description: Extend and verify the Maliev.ShadcnBlazor repository. Use when adding or changing a public component, parameter, theme token, CSS layer, JavaScript module, icon adapter, Showcase dossier, component catalog entry, API snapshot, browser behavior, packaging contract, or release metadata. Do not use for ordinary application integration; use maliev-shadcnblazor instead.
---

# Maliev ShadcnBlazor Maintainer

Follow the repository's `AGENTS.md` first. This skill adds component-specific
routing; it does not replace repository authority or validation requirements.

## Workflow

1. Read `AGENTS.md` and `CONTRIBUTING.md`, inspect `git status`, and identify
   unrelated work before editing.
2. Read [references/component-change-checklist.md](references/component-change-checklist.md)
   and select the smallest validation lane that fully covers the boundary.
3. Trace the complete component slice before changing it:
   - Razor/C# implementation and component-family CSS;
   - JavaScript module or MudBlazor adapter, if used;
   - public API snapshot and XML documentation;
   - `docs/component-catalog.json`;
   - Showcase catalog entry, dossier, examples, and route;
   - agent guidance in `AGENTS.md`, `docs/agent-skills.md`, and both repository
     skills when the change affects setup, component selection, public APIs,
     repository paths, or validation commands;
   - unit, contract, repository, and Playwright coverage.
4. Add a focused failing test for a feature or bug fix. Confirm that it fails
   for the intended reason before changing production code.
5. Implement the smallest application-independent API. Preserve controlled and
   uncontrolled state ownership, SSR/hydration, rerender, and disposal.
6. Verify semantic HTML, accessible names and relationships, keyboard and focus
   behavior, disabled/read-only states, LTR/RTL, light/dark, reduced motion,
   forced colors, zoom, and responsive behavior as applicable.
7. Build first, then run focused tests, the affected suite, browser checks, and
   repository/public-surface validation. Inspect visual evidence at original
   resolution before accepting a baseline change.

## Public boundary rules

- Never couple the package to application DTOs, routes, services, private URLs,
  or proprietary fixtures.
- Do not invent React shadcn/ui parity where Blazor semantics require a
  different typed contract. Document intentional differences.
- Do not update `PublicApi.approved.txt` or visual baselines to hide accidental
  drift. Every public surface change must be intentional, documented, and
  covered by a matching test.
- Keep overlay, focus, ID, ARIA, and state ownership inside the component.
  Forward only supported caller attributes.
- Keep CSS in the correct family layer and use semantic tokens plus logical
  properties. Avoid selectors that depend on Showcase-only markup.
- Keep examples neutral, fictional, portable, and compilable.
- Keep the documentation home's agentic workflow truthful. When it names a
  component or promises a verification behavior, ensure the consumer skill
  routes to the current installed-package contract and add a repository test
  for any synchronization invariant that should not drift.

## Deliverable

Report the observable component outcome, public contracts inspected, exact
build/test/browser/static-check results, evidence or snapshots changed, commit
hash, and any deliberately unrun gate with its residual risk.
