# Theme Studio Bento Layout Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship reusable responsive Bento layout components and rebuild Theme Studio as an animated, interactive collection of dedicated realistic workflows with complete package-component coverage.

**Architecture:** `ShadcnBentoGrid` owns a named inline-size query container and an inner CSS Grid; `ShadcnBentoItem` owns validated column and row spans. Theme Studio composes only curated workflow cards inside these primitives, tracks component coverage separately from documentation scenarios, and uses a preview-scoped reveal coordinator for one-time card and content animation.

**Tech Stack:** .NET 10, Blazor Razor Class Library, CSS Grid, CSS container queries, JavaScript `IntersectionObserver`, bUnit/xUnit, Playwright.

**Spec:** `docs/superpowers/specs/2026-08-24-theme-studio-bento-layout-design.md`

## Global Constraints

- Theme Studio must not render documentation scenario cards or reuse documentation preview content.
- Every public catalog component must map to at least three realistic curated workflow usages or meaningful states.
- DOM, reading, and keyboard focus order must remain aligned; do not use `grid-auto-flow: dense`, CSS `order`, or visual-only reordering.
- Theme configuration and animation selectors remain scoped to `.theme-preview-scope`.
- Theme shuffling preserves preview scroll position, card order, content, and interaction state where component semantics allow.
- All package components remain interactive; reduced motion reveals all content immediately.
- Preserve unrelated `.impeccable/live/` files and do not stage them.

---

### Task 1: Public Bento layout primitives

**Files:**
- Create: `src/Maliev.ShadcnBlazor/Components/Layout/ShadcnBentoGrid.razor`
- Create: `src/Maliev.ShadcnBlazor/Components/Layout/ShadcnBentoItem.razor`
- Create: `src/Maliev.ShadcnBlazor/wwwroot/css/shadcn-layout.css`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/index.html`
- Modify: `README.md`
- Modify: `src/Maliev.ShadcnBlazor/README.md`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt`
- Create: `tests/Maliev.ShadcnBlazor.Tests/Components/Layout/BentoGridTests.cs`

**Interfaces:**
- Produces: `ShadcnBentoGrid.Columns:int`, `MediumColumns:int`, `Gap:string?`, `ChildContent:RenderFragment?`.
- Produces: `ShadcnBentoItem.ColumnSpan:int`, `RowSpan:int`, `ChildContent:RenderFragment?`.
- Produces slots: `bento-grid`, `bento-grid-layout`, and `bento-item`.

- [ ] **Step 1: Write failing bUnit tests**

```csharp
[Fact]
public void GridRendersQueryContainerAndConfiguredTracks()
{
    var cut = Render<ShadcnBentoGrid>(p => p
        .Add(x => x.Columns, 4)
        .Add(x => x.MediumColumns, 2)
        .Add(x => x.Gap, "1.25rem")
        .AddChildContent<ShadcnBentoItem>(item => item
            .Add(x => x.ColumnSpan, 2)
            .Add(x => x.RowSpan, 1)
            .AddChildContent("workflow")));

    Assert.Contains("--shadcn-bento-columns: 4", cut.Find("[data-slot='bento-grid']").GetAttribute("style"));
    Assert.Equal("2", cut.Find("[data-slot='bento-item']").GetAttribute("data-column-span"));
}

[Theory]
[InlineData(0, 2)]
[InlineData(4, 0)]
public void GridRejectsNonPositiveColumnCounts(int columns, int mediumColumns) =>
    Assert.ThrowsAny<Exception>(() => Render<ShadcnBentoGrid>(p => p
        .Add(x => x.Columns, columns)
        .Add(x => x.MediumColumns, mediumColumns)));
```

- [ ] **Step 2: Run the focused tests and confirm failure because the components do not exist**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter FullyQualifiedName~BentoGridTests`

- [ ] **Step 3: Implement the Razor components with validated parameters, owned slots, forwarded attributes, and CSS custom properties**

```razor
<div @attributes="ForwardedAttributes" class="@MergeClass("shadcn-bento-grid")"
     style="@MergeStyle(GridStyle)" data-slot="bento-grid">
    <div class="shadcn-bento-grid__layout" data-slot="bento-grid-layout">@ChildContent</div>
</div>
```

```css
.shadcn-bento-grid { container: shadcn-bento / inline-size; min-inline-size: 0; }
.shadcn-bento-grid__layout { display: grid; grid-template-columns: minmax(0, 1fr); gap: var(--shadcn-bento-gap, 1rem); align-items: start; }
.shadcn-bento-item { min-inline-size: 0; grid-column: span 1; }
@container shadcn-bento (min-width: 40rem) {
  .shadcn-bento-grid__layout { grid-template-columns: repeat(var(--shadcn-bento-medium-columns), minmax(0, 1fr)); }
  .shadcn-bento-item { grid-column: span min(var(--shadcn-bento-column-span), var(--shadcn-bento-medium-columns)); }
}
@container shadcn-bento (min-width: 72rem) {
  .shadcn-bento-grid__layout { grid-template-columns: repeat(var(--shadcn-bento-columns), minmax(0, 1fr)); }
  .shadcn-bento-item { grid-column: span min(var(--shadcn-bento-column-span), var(--shadcn-bento-columns)); }
}
```

- [ ] **Step 4: Add the stylesheet references and intentional public API snapshot entries**
- [ ] **Step 5: Build the solution with zero warnings and errors**

Run: `dotnet build Maliev.ShadcnBlazor.slnx -c Release`

- [ ] **Step 6: Run the focused tests and commit**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build --filter FullyQualifiedName~BentoGridTests`

Commit: `feat(layout): add responsive bento grid primitives`

### Task 2: Bento component dossier and three examples

**Files:**
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Documentation/ComponentDocumentationCatalog.json`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Documentation/Examples/SemanticFoundationExamples.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Documentation/Api/ComponentApiCatalog.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/ThemeScenarios/ThemeScenarioCatalog.json`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ComponentDossierTests.cs`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Showcase/DocumentationCatalogTests.cs`

**Interfaces:**
- Consumes: public `ShadcnBentoGrid` and `ShadcnBentoItem` from Task 1.
- Produces: `/docs/components/bento-grid`, one dossier with three switchable examples, and three theme-scenario state definitions used only by documentation QA.

- [ ] **Step 1: Add failing catalog and example tests**

```csharp
[Fact]
public void BentoGridHasThreeDedicatedDocumentationExamples()
{
    var catalog = new ComponentDocumentationCatalog();
    var entry = Assert.IsType<ComponentDocumentationEntry>(catalog.FindBySlug("bento-grid"));
    var examples = new ComponentExampleRegistry(catalog).GetBySlug(entry.Slug);
    Assert.Equal(3, examples.Count);
    Assert.All(examples, example => Assert.Contains("<ShadcnBentoGrid", example.RazorSource));
}
```

- [ ] **Step 2: Run the focused tests and confirm the missing catalog entry failure**
- [ ] **Step 3: Register the Layout component and create three dedicated previews: featured summary, mixed spans, and narrow reflow**
- [ ] **Step 4: Add API descriptions, accessibility notes, theming tokens, and documentation-only default/stress/accessible scenarios**
- [ ] **Step 5: Build and run the focused dossier/catalog tests**
- [ ] **Step 6: Commit**

Commit: `docs(layout): add interactive bento grid dossier`

### Task 3: Migrate Theme Studio to the public Bento components

**Files:**
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/Runway/ThemeBento.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/Runway/ThemeUseCaseCardHost.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/Runway/ThemeScenarioBentoCard.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeRunwayContractTests.cs`
- Modify: `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeStudioBrowserTests.cs`

**Interfaces:**
- Consumes: `<ShadcnBentoGrid Columns="4" MediumColumns="2">` and `<ShadcnBentoItem ColumnSpan="...">`.
- Removes: `IThemeScenarioRegistry` from `ThemeBento` and all rendered `ThemeScenarioBentoCard` entries.

- [ ] **Step 1: Replace the old column-count contract test with failing assertions for public Bento primitives and no scenario loop**

```csharp
Assert.Contains("<ShadcnBentoGrid", bento);
Assert.Contains("<ShadcnBentoItem", bento);
Assert.DoesNotContain("IThemeScenarioRegistry", bento);
Assert.DoesNotContain("<ThemeScenarioBentoCard", bento);
Assert.Contains("display: grid", packageCss);
Assert.DoesNotContain("column-count", themeCss);
Assert.DoesNotContain("grid-auto-flow: dense", packageCss);
```

- [ ] **Step 2: Add failing Playwright assertions that a wide card spans two tracks at desktop, the full two-column row at tablet, and one track at mobile**
- [ ] **Step 3: Wrap curated cards in `ShadcnBentoItem`, map `ThemeBentoSize`, remove the scenario-card loop, and delete obsolete multi-column CSS**
- [ ] **Step 4: Apply `padding-block: 1rem` and `padding-inline: 0` to the Theme Studio Bento region without clipping card borders**
- [ ] **Step 5: Build, run focused contract tests, run the Bento browser test, and commit**

Commit: `refactor(theme-studio): use package bento layout`

### Task 4: Curated workflow coverage and composition quality

**Files:**
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Theming/Runway/ThemeCuratedCoverageRegistry.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Theming/Runway/ThemeUseCaseRegistry.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/Runway/ThemeUseCaseCardHost.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css`
- Create: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeCuratedCoverageTests.cs`

**Interfaces:**
- Produces: `IThemeCuratedCoverageRegistry.All:IReadOnlyList<ThemeCuratedCoverage>`.
- Produces: `ThemeCuratedCoverage(string ComponentSlug, IReadOnlyList<string> WorkflowIds)` with at least three distinct IDs per catalog component.
- Consumes: `IComponentDocumentationCatalog` and `IThemeUseCaseRegistry` in validation tests.

- [ ] **Step 1: Write a failing coverage test that requires every catalog slug to have three existing workflow references**

```csharp
[Fact]
public void EveryPublicComponentHasThreeRealCuratedUsages()
{
    var workflows = new ThemeUseCaseRegistry().All.ToDictionary(x => x.Id);
    var coverage = new ThemeCuratedCoverageRegistry().All.ToDictionary(x => x.ComponentSlug);
    foreach (var component in new ComponentDocumentationCatalog().All)
    {
        var entry = Assert.Contains(component.Slug, coverage);
        Assert.True(entry.WorkflowIds.Distinct(StringComparer.OrdinalIgnoreCase).Count() >= 3);
        Assert.All(entry.WorkflowIds, id => Assert.True(workflows.ContainsKey(id), $"Missing workflow {id}"));
    }
}
```

- [ ] **Step 2: Run the test and record the uncovered component slugs**
- [ ] **Step 3: Group missing coverage into coherent production workflows rather than one card per component**
- [ ] **Step 4: Implement each workflow with real package components and fictional Thai manufacturing content**
- [ ] **Step 5: Replace the static drawing workspace with attachment/revision/reviewer/context-action behavior**
- [ ] **Step 6: Normalize avatar identity composition and audit card header/content spacing against isolated package cards**
- [ ] **Step 7: Complete the coverage registry only after rendered component usage exists**
- [ ] **Step 8: Build, run coverage and Theme Studio state tests, inspect all workflow cards, and commit**

Commit: `feat(theme-studio): complete realistic curated workflows`

### Task 5: Scroll reveal and safe typing demonstrations

**Files:**
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/Runway/ThemeAnimatedInput.razor`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/js/theme-bento.js`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/Runway/ThemeBento.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/Runway/ThemeUseCaseCardHost.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeRunwayContractTests.cs`
- Modify: `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeStudioBrowserTests.cs`

**Interfaces:**
- Produces JS: `attachBentoReveal(root):number` and `detachBentoReveal(handle):void`.
- Produces attributes: `data-reveal-state="pending|visible"` and `data-reveal-kind="card|chart|progress|text"`.
- Produces `ThemeAnimatedInput` with `Value`, `ValueChanged`, `Multiline`, and immediate reveal cancellation on interaction.

- [ ] **Step 1: Add failing contract and browser tests for one-time viewport reveal, reduced-motion completion, editable animated inputs, and intact Thai text**
- [ ] **Step 2: Implement an intersection observer rooted at the actual Theme Studio preview scroller**
- [ ] **Step 3: Add preview-scoped card, chart-bar, progress, and whole-text reveal keyframes**
- [ ] **Step 4: Replace per-grapheme inline typing spans with whole laid-out text clipping so Thai shaping and wrapping remain intact**
- [ ] **Step 5: Wrap every prefilled curated text control with `ThemeAnimatedInput`; focus and input complete the reveal immediately**
- [ ] **Step 6: Verify direct interaction pauses motion and Shuffle preserves scroll/reveal state**
- [ ] **Step 7: Build, run focused tests and Playwright checks, and commit**

Commit: `feat(theme-studio): animate workflows on reveal`

### Task 6: Full validation and handoff

**Files:**
- Modify only if validation exposes an in-scope defect.

**Interfaces:**
- Verifies all public and preview boundaries introduced by Tasks 1-5.

- [ ] **Step 1: Build the full solution first**

Run: `dotnet restore Maliev.ShadcnBlazor.slnx`

Run: `dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore`

Expected: zero warnings, zero errors.

- [ ] **Step 2: Run the full package and repository suites**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build`

Run: `dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --no-build`

- [ ] **Step 3: Run the full browser suite with Chromium installed**

Run: `pwsh tests/Maliev.ShadcnBlazor.BrowserTests/bin/Release/net10.0/playwright.ps1 install chromium`

Run: `dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --no-build`

- [ ] **Step 4: Run formatting and public-surface verification**

Run: `dotnet format Maliev.ShadcnBlazor.slnx --verify-no-changes --no-restore`

Run: `pwsh -NoProfile -File eng/Verify-PublicSurface.ps1 -Root .`

- [ ] **Step 5: Run the Impeccable layout detector once across final changed UI targets**

Run: `node C:\Users\natth\.agents\skills\impeccable\scripts\detect.mjs --json --scope layout src/Maliev.ShadcnBlazor/Components/Layout samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/Runway samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css`

- [ ] **Step 6: Inspect desktop, tablet, mobile, dark, RTL, reduced-motion, forced-colors, zoom, long Thai text, and every interactive overlay in one bounded browser pass**
- [ ] **Step 7: Confirm only `.impeccable/live/` remains unrelated and untracked, then commit any validation-only fixes as one coherent commit**

Commit when needed: `fix(theme-studio): close bento validation gaps`
