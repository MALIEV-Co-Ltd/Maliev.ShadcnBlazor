# Reusable Motion Reveal Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add reusable, accessible reveal primitives to the component library and migrate Theme Studio’s curated Bento examples to them without resetting component state.

**Architecture:** `ShadcnRevealGroup` owns one IntersectionObserver and mutation registration boundary; `ShadcnReveal` supplies stable item metadata and CSS variables. The DOM is visible by default for SSR and becomes reveal-enabled only after JavaScript registration. Theme Studio composes these primitives around its existing keyed Bento items and removes its private reveal engine.

**Tech Stack:** .NET 9, Blazor, bUnit/xUnit, JavaScript IntersectionObserver and MutationObserver, CSS custom properties, Playwright browser tests.

**Spec:** `docs/superpowers/specs/2026-08-25-reusable-motion-reveal-design.md`

## Global Constraints

- Preserve unrelated dirty files and stage only files owned by this plan.
- Follow red-green-refactor for every behavior change.
- Keep reveal DOM identity stable across Shuffle, locale, and visual-style updates.
- Respect `prefers-reduced-motion` and explicit reduced-motion state.
- Keep server-rendered content visible when JavaScript is unavailable.
- Use component-aware choreography rather than applying one identical entrance to every nested element.

---

### Task 1: Define and test the public reveal API

**Files:**
- Create: `tests/Maliev.ShadcnBlazor.Tests/Components/Layout/RevealTests.cs`
- Create: `src/Maliev.ShadcnBlazor/Components/Layout/ShadcnRevealEffect.cs`
- Create: `src/Maliev.ShadcnBlazor/Components/Layout/ShadcnRevealGroup.razor`
- Create: `src/Maliev.ShadcnBlazor/Components/Layout/ShadcnReveal.razor`

- [ ] Write bUnit tests for semantic tag rendering, default attributes, typed effects, delays, disabled state, and explicit reduced motion.
- [ ] Run the focused test and confirm it fails because the reveal types do not exist.
- [ ] Implement the minimal public components with validated tags and CSS-variable metadata.
- [ ] Run the library build first, then the focused test, and confirm both pass with zero warnings.

### Task 2: Implement the shared reveal runtime and motion styles

**Files:**
- Create: `src/Maliev.ShadcnBlazor/wwwroot/js/shadcn-reveal.js`
- Modify: `src/Maliev.ShadcnBlazor/wwwroot/css/shadcn-layout.css`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Components/Layout/RevealTests.cs`

- [ ] Add failing tests for module import, one group attachment, option forwarding, and disposal.
- [ ] Run the focused test and confirm the expected JS-interoperability failure.
- [ ] Implement one IntersectionObserver plus MutationObserver per group, one-time reveal semantics, capped stagger, pause/resume, and reduced-motion bypass.
- [ ] Add CSS for fade, rise, scale, and clip effects with visible SSR defaults and exponential ease-out.
- [ ] Add cascade selectors for charts, progress, metrics, messages, and form sections while avoiding layout-affecting animation.
- [ ] Build first, then run focused tests until green.

### Task 3: Freeze the public contract and document the component

**Files:**
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Documentation/ComponentDocumentationCatalog.json`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Documentation/Examples/SemanticFoundationExamples.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Documentation/Api/ComponentApiCatalog.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/ThemeScenarios/ThemeScenarioCatalog.json`
- Modify: relevant documentation contract tests discovered by `rg`.

- [ ] Add failing contract tests for the public API and a discoverable Motion Reveal documentation entry with three examples.
- [ ] Run the focused contract tests and confirm the missing-contract failures.
- [ ] Add the public API snapshot entries and documentation metadata/examples for standard stagger, component cascade, and reduced-motion behavior.
- [ ] Build first, then run the contract and documentation tests until green.

### Task 4: Migrate Theme Studio from the private reveal engine

**Files:**
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/Runway/ThemeBento.razor`
- Delete: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/js/theme-bento.js`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeRunwayContractTests.cs`

- [ ] Replace private-engine assertions with failing tests that require `ShadcnRevealGroup`, stable keyed `ShadcnReveal` items, and no Theme Studio JS ownership.
- [ ] Run the focused test and confirm it fails for the expected old integration.
- [ ] Wrap the existing keyed Bento items with public reveal primitives, map selected cards to varied effects/cascades, and forward pause/reduced-motion state.
- [ ] Remove the private JavaScript module and Theme Studio-specific reveal CSS without changing card content or order.
- [ ] Build first, then run focused Theme Studio contract tests until green.

### Task 5: Add browser regression coverage

**Files:**
- Modify: `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeStudioBrowserTests.cs`

- [ ] Add browser tests for below-fold reveal, scroll activation, reduced-motion visibility, and preservation of an interacted card’s DOM/state across Shuffle.
- [ ] Run the focused browser test and confirm it fails against the incomplete integration.
- [ ] Make only the minimal implementation corrections required by the browser evidence.
- [ ] Rebuild and rerun the focused browser tests until green.

### Task 6: Validate, inspect, and commit

**Files:**
- Inspect all files changed by Tasks 1–5.

- [ ] Build the library, Showcase, unit-test project, browser-test project, and repository-test project with zero warnings and zero errors.
- [ ] Run focused reveal, contract, documentation, and Theme Studio tests.
- [ ] Run the full affected unit and browser suites.
- [ ] Run the Impeccable detector once against all changed UI targets.
- [ ] Inspect Theme Studio in the in-app browser at desktop and mobile widths with normal motion and explicit reduced motion.
- [ ] Confirm unrelated dirty files remain unstaged.
- [ ] Commit the coherent validated implementation with an outcome-focused message.
