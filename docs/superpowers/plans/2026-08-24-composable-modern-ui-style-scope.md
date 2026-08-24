# Composable Modern UI Style Scope Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an application-independent `ShadcnVisualStyleScope` that composes modern visual treatments around existing Maliev.ShadcnBlazor components, document it with three realistic dossiers, and let Theme Studio preview and export curated combinations without restyling the Studio shell.

**Architecture:** A neutral wrapper emits typed, stable data attributes for visual style, color, depth, motion, and intensity. A dedicated package stylesheet translates those attributes into bounded semantic tokens and applies them to existing component slots; Theme Studio stores the five typed values beside each curated preset and wraps only its preview subtree. Bento remains a separate layout primitive composed inside the style scope.

**Tech Stack:** .NET 10, Blazor components, bUnit/xUnit, CSS custom properties and feature queries, Playwright browser tests, JSON-backed Theme Studio presets.

**Spec:** `docs/superpowers/specs/2026-08-24-composable-modern-ui-style-scope-design.md`

## Global Constraints

- The public namespace is `Maliev.ShadcnBlazor.Components.Styling`; the feature must remain reusable and contain no Showcase dependency.
- The wrapper owns presentation only: it must not change child layout, portal placement, focus order, pointer behavior, component state, or semantic roles.
- `ShadcnBentoGrid` remains the layout primitive; modern visual styling composes around it rather than replacing it.
- Nested scopes inherit unspecified layers and override only explicit layers through normal CSS custom-property inheritance.
- Theme Studio applies style values only below `.theme-preview-scope`; the MALIEV Studio shell must remain unchanged.
- Reduced motion, forced colors, increased contrast, unsupported backdrop filters, LTR, RTL, desktop, tablet, and mobile must have explicit verified behavior.
- No new runtime dependency and no `ShadcnThemeDocument` schema-version change are allowed in this slice.
- Preserve `samples/Maliev.ShadcnBlazor.Showcase/packages.lock.json` and `.impeccable/live/` as unrelated user work.

---

### Task 1: Typed style-scope component contract

**Files:**
- Create: `src/Maliev.ShadcnBlazor/Components/Styling/ShadcnVisualStyle.cs`
- Create: `src/Maliev.ShadcnBlazor/Components/Styling/ShadcnColorTreatment.cs`
- Create: `src/Maliev.ShadcnBlazor/Components/Styling/ShadcnDepthTreatment.cs`
- Create: `src/Maliev.ShadcnBlazor/Components/Styling/ShadcnMotionTreatment.cs`
- Create: `src/Maliev.ShadcnBlazor/Components/Styling/ShadcnStyleIntensity.cs`
- Create: `src/Maliev.ShadcnBlazor/Components/Styling/ShadcnVisualStyleScope.razor`
- Create: `tests/Maliev.ShadcnBlazor.Tests/Components/Styling/VisualStyleScopeTests.cs`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Contracts/PublicApiSnapshotTests.cs`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt`

**Interfaces:**
- Consumes: `ShadcnComponentBase.MergeClass`, `MergeStyle`, and `AttributesExcept`.
- Produces: `ShadcnVisualStyleScope` parameters `VisualStyle`, `ColorTreatment`, `Depth`, `Motion`, `Intensity`, and `ChildContent`; enums exactly matching the approved spec.

- [ ] **Step 1: Write failing bUnit tests for defaults, explicit values, nesting, and attribute forwarding**

```csharp
[Fact]
public void DefaultsEmitStableInheritanceAttributes()
{
    var cut = Render<ShadcnVisualStyleScope>(parameters => parameters
        .AddChildContent("Fixture"));
    var root = cut.Find("[data-slot='visual-style-scope']");
    Assert.Equal("inherit", root.GetAttribute("data-visual-style"));
    Assert.Equal("inherit", root.GetAttribute("data-color-treatment"));
    Assert.Equal("inherit", root.GetAttribute("data-depth"));
    Assert.Equal("inherit", root.GetAttribute("data-motion"));
    Assert.Equal("default", root.GetAttribute("data-intensity"));
}

[Fact]
public void ExplicitLayersUseKebabCaseAndPreserveCallerAttributes()
{
    var cut = Render<ShadcnVisualStyleScope>(parameters => parameters
        .Add(x => x.VisualStyle, ShadcnVisualStyle.LiquidGlass)
        .Add(x => x.ColorTreatment, ShadcnColorTreatment.VibrantDark)
        .Add(x => x.Depth, ShadcnDepthTreatment.Spatial)
        .Add(x => x.Motion, ShadcnMotionTreatment.Expressive)
        .Add(x => x.Intensity, ShadcnStyleIntensity.Strong)
        .AddUnmatched("aria-label", "Styled production workspace"));
    var root = cut.Find("[data-slot='visual-style-scope']");
    Assert.Equal("liquid-glass", root.GetAttribute("data-visual-style"));
    Assert.Equal("vibrant-dark", root.GetAttribute("data-color-treatment"));
    Assert.Equal("Styled production workspace", root.GetAttribute("aria-label"));
}
```

- [ ] **Step 2: Run the focused test and confirm it fails because the styling API does not exist**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter FullyQualifiedName~VisualStyleScopeTests`

Expected: FAIL with missing `Maliev.ShadcnBlazor.Components.Styling` types.

- [ ] **Step 3: Implement the five enums and neutral wrapper**

```razor
@inherits Maliev.ShadcnBlazor.Components.Primitives.ShadcnComponentBase

<div @attributes="ForwardedAttributes"
     class="@MergeClass("shadcn-visual-style-scope")"
     style="@MergeStyle(null)"
     data-slot="visual-style-scope"
     data-visual-style="@VisualStyle.ToAttributeValue()"
     data-color-treatment="@ColorTreatment.ToAttributeValue()"
     data-depth="@Depth.ToAttributeValue()"
     data-motion="@Motion.ToAttributeValue()"
     data-intensity="@Intensity.ToAttributeValue()">
    @ChildContent
</div>
```

Implement attribute conversion as an internal exhaustive switch in the component file so public enum names stay strongly typed while emitted values remain stable kebab-case.

- [ ] **Step 4: Add the Styling namespace to the public snapshot contract and refresh the approved snapshot**

Add `"Maliev.ShadcnBlazor.Components.Styling"` to `OwnedNamespaces`, then run:

`$env:SHADCN_UPDATE_PUBLIC_API='1'; dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter FullyQualifiedName~PublicApiSnapshotTests; Remove-Item Env:SHADCN_UPDATE_PUBLIC_API`

Expected: PASS and the snapshot lists all five enums plus the wrapper parameters.

- [ ] **Step 5: Run focused component and public contract tests**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~VisualStyleScopeTests|FullyQualifiedName~PublicApiSnapshotTests"`

Expected: PASS.

- [ ] **Step 6: Commit the public API slice**

```powershell
git add src/Maliev.ShadcnBlazor/Components/Styling tests/Maliev.ShadcnBlazor.Tests/Components/Styling tests/Maliev.ShadcnBlazor.Tests/Contracts/PublicApiSnapshotTests.cs tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt
git commit -m "feat(styling): add composable visual style scope"
```

### Task 2: Bounded visual-treatment stylesheet

**Files:**
- Create: `src/Maliev.ShadcnBlazor/wwwroot/css/shadcn-visual-styles.css`
- Create: `tests/Maliev.ShadcnBlazor.Tests/Contracts/VisualStyleContractTests.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/index.html`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Export/ThemeBundleTemplates.cs`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Contracts/PackageContractTests.cs`

**Interfaces:**
- Consumes: the five `data-*` attributes from Task 1 and existing semantic tokens such as `--shadcn-background`, `--shadcn-card`, `--shadcn-border`, `--shadcn-primary`, and `--shadcn-ring`.
- Produces: the eleven approved `--shadcn-style-*` local variables and presentation rules for existing component slot/class families.

- [ ] **Step 1: Write failing CSS contract tests**

```csharp
[Theory]
[InlineData("minimal")]
[InlineData("glass")]
[InlineData("neo-brutalist")]
[InlineData("liquid-glass")]
public void VisualTreatmentsHaveScopedRules(string style)
{
    var css = Read("shadcn-visual-styles.css");
    Assert.Contains($"[data-visual-style=\"{style}\"]", css, StringComparison.Ordinal);
}

[Fact]
public void AccessibilityFallbacksAreExplicit()
{
    var css = Read("shadcn-visual-styles.css");
    Assert.Contains("@media (forced-colors: active)", css, StringComparison.Ordinal);
    Assert.Contains("@media (prefers-contrast: more)", css, StringComparison.Ordinal);
    Assert.Contains("@media (prefers-reduced-motion: reduce)", css, StringComparison.Ordinal);
    Assert.Contains("@supports not ((backdrop-filter:", css, StringComparison.Ordinal);
}
```

Also assert the stylesheet contains no `body`, `:root`, `position: fixed`, or `display: contents` selector/declaration and that PackageContract includes one `staticwebassets/css/shadcn-visual-styles.css` entry.

- [ ] **Step 2: Run focused contracts and confirm failure due to the missing stylesheet**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~VisualStyleContractTests|FullyQualifiedName~PackageContractTests"`

Expected: FAIL because `shadcn-visual-styles.css` is absent.

- [ ] **Step 3: Implement scoped tokens and the five visual/color treatments**

Start the stylesheet with wrapper-local defaults:

```css
.shadcn-visual-style-scope {
  --shadcn-style-surface: var(--shadcn-card);
  --shadcn-style-surface-strong: var(--shadcn-popover);
  --shadcn-style-border: var(--shadcn-border);
  --shadcn-style-border-width: 1px;
  --shadcn-style-shadow: none;
  --shadcn-style-shadow-hover: none;
  --shadcn-style-blur: 0px;
  --shadcn-style-saturation: 100%;
  --shadcn-style-radius-factor: 1;
  --shadcn-style-control-offset: 0px;
  --shadcn-style-transition-duration: var(--shadcn-motion-duration, 160ms);
}
```

Define style selectors only below `[data-slot="visual-style-scope"]`, use `:where(...)` semantic target lists to keep specificity bounded, and limit Neo-Brutalist interaction offsets to `transform` so layout never shifts. Add solid fallbacks before any `color-mix`/backdrop declarations and forced-color rules that remove blur, translucency, glow, and decorative shadows.

- [ ] **Step 4: Wire the static asset into Showcase and generated integration HTML**

Add `<link href="_content/Maliev.ShadcnBlazor/css/shadcn-visual-styles.css" rel="stylesheet" />` after the package component styles in both `index.html` and `ThemeBundleTemplates.cs`.

- [ ] **Step 5: Run CSS and package contracts**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~VisualStyleContractTests|FullyQualifiedName~PackageContractTests"`

Expected: PASS.

- [ ] **Step 6: Commit the stylesheet slice**

```powershell
git add src/Maliev.ShadcnBlazor/wwwroot/css/shadcn-visual-styles.css samples/Maliev.ShadcnBlazor.Showcase/wwwroot/index.html samples/Maliev.ShadcnBlazor.Showcase/Export/ThemeBundleTemplates.cs tests/Maliev.ShadcnBlazor.Tests/Contracts/VisualStyleContractTests.cs tests/Maliev.ShadcnBlazor.Tests/Contracts/PackageContractTests.cs
git commit -m "feat(styling): add modern visual treatment tokens"
```

### Task 3: Dedicated Visual Style Scope documentation dossier

**Files:**
- Modify: `docs/component-catalog.json`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Documentation/ComponentDocumentationCatalog.json`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Documentation/Api/ComponentApiCatalog.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Documentation/Examples/SemanticFoundationExamples.cs`
- Create: `tests/Maliev.ShadcnBlazor.Tests/Showcase/VisualStyleScopeShowcaseTests.cs`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ComponentDossierTests.cs`

**Interfaces:**
- Consumes: the Task 1 component/enums and Task 2 stylesheet.
- Produces: Foundation catalog slug `visual-style-scope` with authoritative API metadata and exactly three interactive, realistic examples.

- [ ] **Step 1: Write failing catalog and dossier tests**

```csharp
[Fact]
public void VisualStyleScopeIsACompleteFoundationComponent()
{
    var entry = new ComponentDocumentationCatalog().FindBySlug("visual-style-scope");
    Assert.NotNull(entry);
    Assert.Equal("Foundation", entry!.Category);
    Assert.Equal("ShadcnVisualStyleScope", entry.PrimaryType);
    Assert.Equal(3, new ComponentExampleRegistry().GetExamples(entry).Count);
}
```

Assert the three examples expose all style enums, include one Bento composition, include overlays and form controls, and contain no Theme Studio classes.

- [ ] **Step 2: Run the focused dossier tests and confirm the missing catalog entry fails**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~VisualStyleScopeShowcaseTests|FullyQualifiedName~ComponentDossierTests"`

Expected: FAIL because `visual-style-scope` is not registered.

- [ ] **Step 3: Register authoritative catalog and API metadata**

Add a complete Foundation ledger entry named `Visual Style Scope`, metadata namespace `Maliev.ShadcnBlazor.Components.Styling`, primary type `ShadcnVisualStyleScope`, and API types for the wrapper plus all five enums.

- [ ] **Step 4: Add three dedicated interactive examples**

Extend `SemanticFoundationExamples.Create` with `"visual-style-scope" => VisualStyleScope()` and implement:

1. `minimal-and-brutalist-approval`: switch a production approval surface between Minimal and Neo-Brutalist.
2. `glass-scheduling-analytics`: compare Glass and Liquid Glass around a real scheduling form plus chart and compose it with `ShadcnBentoGrid`.
3. `vibrant-dark-operations`: exercise controls, disabled/focus states, data feedback, and an interactive overlay under Vibrant Dark.

Each example must use the package components directly, expose typed controls through `ComponentParameterControl`, and supply exact Razor source through its source provider.

- [ ] **Step 5: Run catalog, dossier, and route tests**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~VisualStyleScopeShowcaseTests|FullyQualifiedName~ComponentDossierTests|FullyQualifiedName~DocumentationRouteTests"`

Expected: PASS.

- [ ] **Step 6: Commit the documentation slice**

```powershell
git add docs/component-catalog.json samples/Maliev.ShadcnBlazor.Showcase/Documentation tests/Maliev.ShadcnBlazor.Tests/Showcase/VisualStyleScopeShowcaseTests.cs tests/Maliev.ShadcnBlazor.Tests/Showcase/ComponentDossierTests.cs
git commit -m "docs(styling): add visual style scope dossier"
```

### Task 4: Theme Studio state, presets, and history integration

**Files:**
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Theming/Presets/ThemeStudioPresetDefinition.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Theming/Presets/ThemeStudioPresetCatalog.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Theming/Presets/ThemeStudioPresetCatalog.json`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioSnapshot.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioState.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeInspector.razor`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioCuratedPresetTests.cs`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioStateTests.cs`

**Interfaces:**
- Consumes: Task 1 enums.
- Produces: Theme Studio properties and setters `VisualStyle`, `ColorTreatment`, `DepthTreatment`, `MotionTreatment`, `StyleIntensity`, `SetVisualStyle`, `SetColorTreatment`, `SetDepthTreatment`, `SetMotionTreatment`, and `SetStyleIntensity`; preset and snapshot persistence for all five.

- [ ] **Step 1: Write failing state tests for presets, undo/redo, and shuffle stability**

```csharp
[Fact]
public void StyleLayersParticipateInUndoAndRedo()
{
    var state = CreateState();
    state.SetVisualStyle(ShadcnVisualStyle.Glass);
    state.SetDepthTreatment(ShadcnDepthTreatment.Floating);
    state.Undo();
    Assert.NotEqual(ShadcnDepthTreatment.Floating, state.DepthTreatment);
    state.Redo();
    Assert.Equal(ShadcnDepthTreatment.Floating, state.DepthTreatment);
}

[Fact]
public void ShuffleChangesAReviewedPresetWithoutChangingWorkbenchPosition()
{
    var state = CreateState();
    state.Workbench.SetScrollPosition(640);
    state.ShufflePreset();
    Assert.Equal(640, state.Workbench.ScrollPosition);
}
```

Also assert every JSON preset materializes valid enum values and covers every visual style across the curated catalog.

- [ ] **Step 2: Run focused state tests and confirm the new properties are missing**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~ThemeStudioStateTests|FullyQualifiedName~ThemeStudioCuratedPresetTests"`

Expected: FAIL with missing style-layer state.

- [ ] **Step 3: Extend preset materialization and state snapshots**

Add five typed fields to `ThemeStudioPresetDefinition`, five string fields to JSON entries that parse case-insensitively with a precise invalid-value exception, and all five values to `ThemeStudioSnapshot`. Include them in capture, restore, baseline reset, curated preset application, and history equality.

- [ ] **Step 4: Add compact typed controls to Theme Inspector**

Create a `Visual treatment` section with five `ShadcnSelect` controls bound to the typed setters. Options must use MALIEV-facing labels (`Clean`, `Frosted`, `Bold frame`, `Spatial glass`, `Vibrant night`, `Flat`, `Raised`, `Floating`, `Spatial`, `Calm`, `Expressive`, `None`, `Subtle`, `Standard`, `Strong`) and stable accessible labels.

- [ ] **Step 5: Run Theme Studio state and inspector contracts**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~ThemeStudioStateTests|FullyQualifiedName~ThemeStudioCuratedPresetTests|FullyQualifiedName~ThemeStudioWorkbenchContractTests"`

Expected: PASS.

- [ ] **Step 6: Commit the Theme Studio state slice**

```powershell
git add samples/Maliev.ShadcnBlazor.Showcase/Theming samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeInspector.razor tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioCuratedPresetTests.cs tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioStateTests.cs
git commit -m "feat(theme-studio): configure composable visual treatments"
```

### Task 5: Preview-only wrapper and export snippet

**Files:**
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Pages/ThemeStudio.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Export/ThemeStudioCodeGenerator.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeCodeDialog.razor`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioWorkbenchContractTests.cs`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeBundleTests.cs`

**Interfaces:**
- Consumes: Task 4 state properties and Task 1 wrapper.
- Produces: a preview-only wrapper and `WriteVisualStyleSnippet(...)` output that consumers can copy without altering the v2 theme document.

- [ ] **Step 1: Write failing preview-boundary and export tests**

```csharp
[Fact]
public void ThemeStudioWrapsOnlyThePreviewCanvas()
{
    var page = ReadThemeStudio();
    Assert.Contains("<ShadcnVisualStyleScope", page, StringComparison.Ordinal);
    Assert.True(page.IndexOf("<ShadcnVisualStyleScope", StringComparison.Ordinal) >
                page.IndexOf("class=\"shadcn-scope theme-preview-scope\"", StringComparison.Ordinal));
    Assert.DoesNotContain("<ThemeStudioSidebar State=\"State\" />\n        <ShadcnVisualStyleScope", page, StringComparison.Ordinal);
}
```

Test exact generated markup includes typed enum values and a `ShadcnBentoGrid` child placeholder while JSON output remains byte-for-byte the existing schema-v2 shape.

- [ ] **Step 2: Run the focused tests and confirm the wrapper/snippet are absent**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~ThemeStudioWorkbenchContractTests|FullyQualifiedName~ThemeBundleTests"`

Expected: FAIL on missing preview wrapper and snippet.

- [ ] **Step 3: Wrap only the preview content**

Inside the existing `.shadcn-scope.theme-preview-scope`, render:

```razor
<ShadcnVisualStyleScope VisualStyle="State.VisualStyle"
                        ColorTreatment="State.ColorTreatment"
                        Depth="State.DepthTreatment"
                        Motion="State.MotionTreatment"
                        Intensity="State.StyleIntensity"
                        data-testid="theme-visual-style-scope">
    <CascadingValue Value="PreviewContext">
        <ThemeBento Locale="State.Locale" Paused="State.Workbench.RunwayPaused" ReducedMotion="State.Workbench.ReducedMotion" />
    </CascadingValue>
</ShadcnVisualStyleScope>
```

Do not add any style data attributes to `ThemeStudioLayout`, `.theme-studio`, sidebar, or app bar.

- [ ] **Step 4: Generate a separate integration snippet**

Add `ThemeStudioCodeGenerator.WriteVisualStyleSnippet(...)` that emits the exact wrapper and enum names. Expose it in the code dialog as an additional copyable section; do not add fields to `ShadcnThemeDocument` and do not alter `WriteJson`.

- [ ] **Step 5: Run preview-boundary and export tests**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~ThemeStudioWorkbenchContractTests|FullyQualifiedName~ThemeBundleTests"`

Expected: PASS.

- [ ] **Step 6: Commit the preview/export slice**

```powershell
git add samples/Maliev.ShadcnBlazor.Showcase/Pages/ThemeStudio.razor samples/Maliev.ShadcnBlazor.Showcase/Export/ThemeStudioCodeGenerator.cs samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeCodeDialog.razor tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioWorkbenchContractTests.cs tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeBundleTests.cs
git commit -m "feat(theme-studio): preview composable visual styles"
```

### Task 6: Cross-boundary build, test, and browser verification

**Files:**
- Modify only if a verified defect requires it: files owned by Tasks 1-5
- Test: `tests/Maliev.ShadcnBlazor.Tests/Browser/ThemeStudioBrowserTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Browser/DocumentationBrowserTests.cs`

**Interfaces:**
- Consumes: the completed package, documentation, and Theme Studio slices.
- Produces: release-build, suite, accessibility, interaction, responsive, and packaging evidence.

- [ ] **Step 1: Build the solution before any broader tests**

Run: `dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore`

Expected: exit 0 with zero warnings and zero errors.

- [ ] **Step 2: Run the full affected test suite**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build`

Expected: every test passes.

- [ ] **Step 3: Pack and inspect static assets**

Run: `dotnet pack src/Maliev.ShadcnBlazor/Maliev.ShadcnBlazor.csproj -c Release --no-build -o artifacts/style-scope-pack`

Open the generated `.nupkg` as a zip and verify exactly one `staticwebassets/css/shadcn-visual-styles.css` plus the compiled Styling API.

- [ ] **Step 4: Run focused Playwright checks at desktop, tablet, and mobile widths**

Start the Showcase on port 5080, then run the repository browser projects/filters for Theme Studio and component docs. Verify:

- the Studio header/sidebar computed styles do not change when style selectors change;
- preview cards, forms, charts, Bento spans, dialog, drawer, dropdown, context menu, popover, hover card, sheet, tooltip, and toast remain interactive;
- Minimal, Glass, Neo-Brutalist, Liquid Glass, and Vibrant Dark are visually distinct;
- unsupported backdrop fallback stays opaque and legible;
- reduced motion disables decorative movement;
- forced colors retains visible boundaries and focus;
- LTR/RTL and all three viewport controls preserve source order and do not overflow.

Expected: all Playwright checks pass with no console or page errors.

- [ ] **Step 5: Inspect the working tree and verify unrelated work remains untouched**

Run: `git status --short` and `git diff -- samples/Maliev.ShadcnBlazor.Showcase/packages.lock.json`.

Expected: only the pre-existing lockfile newline change and `.impeccable/live/` remain outside committed work.

- [ ] **Step 6: Commit any test-only browser assertions if they were added**

```powershell
git add tests/Maliev.ShadcnBlazor.Tests/Browser/ThemeStudioBrowserTests.cs tests/Maliev.ShadcnBlazor.Tests/Browser/DocumentationBrowserTests.cs
git commit -m "test(styling): verify visual style scope boundaries"
```

Skip this commit if the existing browser suite required no source changes.
