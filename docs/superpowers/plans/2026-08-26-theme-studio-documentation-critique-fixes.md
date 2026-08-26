# Theme Studio and Documentation Critique Fixes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make Theme Studio immediately understandable and navigable, clarify export safety, and make component dossiers easier to scan without weakening their evidence or accessibility contracts.

**Architecture:** Keep the current single ordered masonry canvas as the canonical preview model. Add an evaluation toolbar and category anchors around the existing registry-driven cards, centralize validation wording in a small presentation helper, and progressively disclose full example source inside each live preview while leaving package installation and concise usage guidance in the dossier.

**Tech Stack:** .NET 10, Blazor Razor components, bUnit, Microsoft Playwright, semantic HTML, scoped showcase CSS.

**Spec:** `.impeccable/critique/2026-08-26T08-20-25Z__liev-shadcnblazor-showcase-pages-themestudio-razor.md`

## Global Constraints

- Preserve the existing single ordered masonry canvas, its 37 stable card IDs, card state, and deterministic order.
- Keep reusable library behavior unchanged; these changes belong to the showcase and documentation experience.
- Preserve unrelated changes in both `packages.lock.json` files, `src/Maliev.ShadcnBlazor/wwwroot/js/shadcn-message-scroller.js`, and `.impeccable/live/`.
- Use native headings, links, buttons, and disclosure semantics with keyboard-visible focus.
- Do not hide device or locale controls at supported mobile widths.
- Treat validation errors as blocking and warnings as advisory; export remains available for advisory-only themes and the export dialog keeps its acknowledgement gate.
- Use `Maliev Shadcn Blazor` for the public product name and `Maliev.ShadcnBlazor` only for package or namespace identifiers.
- Build the affected solution in Release with zero warnings and zero errors before counting green tests.

---

### Task 1: Orient and navigate the Theme Studio runway

**Files:**
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Pages/ThemeStudio.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/Runway/ThemeBento.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Theming/Runway/ThemeUseCaseDefinition.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Theming/Runway/ThemeUseCaseRegistry.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css`
- Test: `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeStudioBrowserTests.cs`

**Interfaces:**
- Consumes: `IThemeUseCaseRegistry.All`, the existing stable card IDs, `ThemeStudioState.SelectedPresetId`, and validation counts.
- Produces: `ThemeUseCaseCategory`, `ThemeUseCaseDefinition.Category`, a visible `theme-preview-intro`, category anchor navigation, and an h2 runway heading before card h3 headings.

- [ ] **Step 1: Write the failing browser test**

Add a test that opens `/theme` at desktop and mobile widths and asserts observable behavior:

```csharp
[Theory]
[InlineData(1440, 900)]
[InlineData(390, 844)]
public async Task ThemeStudioExplainsAndNavigatesTheEvaluationRunway(int width, int height)
{
    await using var context = await NewContextAsync(width, height, ReducedMotion.Reduce);
    var page = await OpenAsync(context);

    await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Theme Studio", Level = 1 })).ToBeVisibleAsync();
    await Assertions.Expect(page.GetByRole(AriaRole.Navigation, new() { Name = "Preview categories" })).ToBeVisibleAsync();
    await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Workflow examples", Level = 2 })).ToBeVisibleAsync();
    Assert.Equal(37, await page.Locator("[data-use-case-id]").CountAsync());

    var forms = page.GetByRole(AriaRole.Link, new() { Name = "Forms and input" });
    await forms.ClickAsync();
    await Assertions.Expect(page.Locator("#theme-category-forms")).ToBeVisibleAsync();
}
```

Also assert that `theme-device-controls`, English, and Thai controls remain visible at 390px and that `.theme-preset-status` computes to at least 12px.

- [ ] **Step 2: Build and run the focused test to verify RED**

Run:

```powershell
dotnet build tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --no-build --filter "FullyQualifiedName~ThemeStudioExplainsAndNavigatesTheEvaluationRunway"
```

Expected: the test builds, then fails because the preview-category navigation and runway h2 do not exist and the desktop introduction is hidden.

- [ ] **Step 3: Implement the minimal orientation and navigation model**

Add a stable category to each registry definition, using these public labels and anchor IDs:

```csharp
public enum ThemeUseCaseCategory
{
    Overview,
    Forms,
    Data,
    Communication,
    Overlays,
    Security,
    Media
}
```

Render a compact visible introduction in `ThemeStudio.razor` with the active preset, advisory/error summary, and the existing explanatory sentence. In `ThemeBento.razor`, render one `h2` named `Workflow examples`, a `nav aria-label="Preview categories"`, and anchors targeting the first stable card in each category. Do not split the masonry grid or reorder cards.

Use logical CSS properties, visible `:focus-visible` treatment, horizontal overflow for the category links on narrow widths, a minimum 12px preset-status font, and keep device/locale controls visible.

- [ ] **Step 4: Build and run the focused test to verify GREEN**

Run the two commands from Step 2. Expected: build succeeds with zero warnings/errors and the focused browser test passes at both widths.

- [ ] **Step 5: Commit**

```powershell
git add -- samples/Maliev.ShadcnBlazor.Showcase/Pages/ThemeStudio.razor samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/Runway/ThemeBento.razor samples/Maliev.ShadcnBlazor.Showcase/Theming/Runway/ThemeUseCaseDefinition.cs samples/Maliev.ShadcnBlazor.Showcase/Theming/Runway/ThemeUseCaseRegistry.cs samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css tests/Maliev.ShadcnBlazor.BrowserTests/ThemeStudioBrowserTests.cs docs/superpowers/plans/2026-08-26-theme-studio-documentation-critique-fixes.md
git commit -m "fix(theme-studio): orient and navigate the preview runway"
```

### Task 2: Make validation and export consequences explicit

**Files:**
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeValidationPresentation.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeInspector.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeValidationSummary.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeExportDialog.razor`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeValidationPresentationTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeImportExportBrowserTests.cs`

**Interfaces:**
- Consumes: validation error/warning counts and the existing export-dialog warning acknowledgement.
- Produces: `ThemeValidationPresentation.StatusLabel(int errors, int advisories)` and consistent blocking/advisory wording across the inspector, summary, and export dialog.

- [ ] **Step 1: Write failing unit tests**

```csharp
[Theory]
[InlineData(0, 0, "Ready to export")]
[InlineData(0, 16, "Ready to export · 16 advisories")]
[InlineData(2, 16, "Export blocked · 2 errors")]
public void StatusLabelExplainsExportConsequence(int errors, int advisories, string expected)
{
    Assert.Equal(expected, ThemeValidationPresentation.StatusLabel(errors, advisories));
}
```

Add a browser assertion that advisory-only state keeps Export enabled and that opening Validation exposes `Advisories do not block export`; retain the existing export-dialog acknowledgement behavior.

- [ ] **Step 2: Build and verify RED**

```powershell
dotnet build tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~ThemeValidationPresentationTests"
```

Expected: compilation fails because `ThemeValidationPresentation` does not exist; after adding only the type shell if needed for compilation, the assertions fail against the old wording.

- [ ] **Step 3: Implement centralized semantic wording**

Implement the helper exactly as:

```csharp
public static string StatusLabel(int errors, int advisories) => errors switch
{
    > 0 => $"Export blocked · {errors} {(errors == 1 ? "error" : "errors")}",
    _ when advisories > 0 => $"Ready to export · {advisories} {(advisories == 1 ? "advisory" : "advisories")}",
    _ => "Ready to export"
};
```

Use `advisory/advisories` in the inspector, validation summary, and export dialog. Do not remove acknowledgement of advisory contrast findings from the export dialog.

- [ ] **Step 4: Build and verify GREEN**

Run the focused unit test, then the focused export browser test. Expected: zero warnings/errors and all focused cases pass.

- [ ] **Step 5: Commit**

```powershell
git add -- samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeValidationPresentation.cs samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeInspector.razor samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeValidationSummary.razor samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeExportDialog.razor tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeValidationPresentationTests.cs tests/Maliev.ShadcnBlazor.BrowserTests/ThemeImportExportBrowserTests.cs
git commit -m "fix(theme-studio): clarify export readiness"
```

### Task 3: Progressively disclose dossier source and improve reading flow

**Files:**
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Pages/Docs/ComponentDocumentation.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Documentation/ComponentPreview.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Documentation/ComponentCodeExample.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Documentation/ComponentConsumptionGuide.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Documentation/ComponentApiTable.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ComponentDossierTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.BrowserTests/DocumentationWorkbenchBrowserTests.cs`

**Interfaces:**
- Consumes: `ComponentExampleDefinition.RazorSource`, its live source updates, existing `ShadcnCodeBlock`, and dossier outline IDs.
- Produces: one collapsed full-source disclosure inside each preview, concise Usage decision guidance, and API/code regions that only scroll horizontally when their contents actually overflow.

- [ ] **Step 1: Write failing component and browser tests**

Add bUnit coverage proving `ComponentCodeExample` renders a native closed `details` element when `Collapsible="true"`, with summary text `View complete source`, and renders its existing always-visible section when false.

Add a browser test for `/docs/components/button`:

```csharp
var preview = page.GetByTestId("component-preview").First;
var source = preview.Locator("details[data-testid='example-source']");
await Assertions.Expect(source).Not.ToHaveAttributeAsync("open", "");
await source.Locator("summary").ClickAsync();
await Assertions.Expect(source).ToHaveAttributeAsync("open", "");
Assert.Equal(1, await page.Locator("text=Example source").CountAsync());
await Assertions.Expect(page.Locator("#usage")).ToContainTextAsync("Use when");
await Assertions.Expect(page.Locator("#usage")).ToContainTextAsync("Avoid when");
```

Also assert paragraph measure does not exceed 75 characters and API header contrast is at least 4.5:1 in the light theme.

- [ ] **Step 2: Build and verify RED**

Build the package and browser-test projects, then run the named component and browser tests. Expected: tests fail because the example source is a separate always-visible section and Usage lacks decision guidance.

- [ ] **Step 3: Implement disclosure and readable defaults**

Add `Collapsible`, `Summary`, and optional `TestId` parameters to `ComponentCodeExample`. For collapsible mode, use native markup:

```razor
<details class="component-code-disclosure" data-testid="@TestId">
    <summary>@Summary</summary>
    <div class="component-code-disclosure__content">
        <ShadcnCodeBlock Class="component-code__surface" Source="@Source" Language="@Language" Sources="@Sources" />
    </div>
</details>
```

Move the complete example source into `ComponentPreview` with `Collapsible="true"`; remove the duplicate sibling code block from `ComponentDocumentation.razor`. Replace the Usage code repetition with concise `Use when` and `Avoid when` guidance plus a link back to the preview source. Keep the package installation code copyable.

Set prose measure to `min(75ch, 100%)`, allow page-level vertical scrolling rather than fixed-height code scrolling, retain horizontal overflow for genuinely wide code/API content, add adequate API scroller padding, and strengthen the muted table-header/code token enough to meet 4.5:1.

- [ ] **Step 4: Build and verify GREEN**

Run the focused bUnit and Playwright tests. Expected: all pass with zero warnings/errors.

- [ ] **Step 5: Commit**

```powershell
git add -- samples/Maliev.ShadcnBlazor.Showcase/Pages/Docs/ComponentDocumentation.razor samples/Maliev.ShadcnBlazor.Showcase/Components/Documentation/ComponentPreview.razor samples/Maliev.ShadcnBlazor.Showcase/Components/Documentation/ComponentCodeExample.razor samples/Maliev.ShadcnBlazor.Showcase/Components/Documentation/ComponentConsumptionGuide.razor samples/Maliev.ShadcnBlazor.Showcase/Components/Documentation/ComponentApiTable.razor samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css tests/Maliev.ShadcnBlazor.Tests/Showcase/ComponentDossierTests.cs tests/Maliev.ShadcnBlazor.BrowserTests/DocumentationWorkbenchBrowserTests.cs
git commit -m "fix(docs): progressively disclose component source"
```

### Task 4: Normalize public copy and validate the complete experience

**Files:**
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Documentation/ComponentStatusEvidence.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Documentation/Api/ComponentApiCatalog.cs`
- Modify: public-facing showcase Razor files returned by the scoped product-name search
- Test: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ComponentDossierTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Showcase/DocumentationCatalogTests.cs`

**Interfaces:**
- Consumes: public documentation catalog entries and reflected API descriptors.
- Produces: consistent brand copy, `Integration` evidence label, outcome-oriented fallback API descriptions, and user-facing certification explanation.

- [ ] **Step 1: Write failing tests**

Add tests that assert the Integration evidence row renders exactly `Integration`, fallback parameter descriptions do not begin with `Configures the`, enum constraints still enumerate allowed values, and the Button entry summary explains when to use a Button rather than restating its name.

- [ ] **Step 2: Build and verify RED**

```powershell
dotnet build tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~ComponentDossierTests|FullyQualifiedName~DocumentationCatalogTests"
```

Expected: focused assertions fail against `Integration integration`, generic fallback descriptions, or tautological public copy.

- [ ] **Step 3: Implement scoped copy corrections**

Change the evidence label to `Integration`. Change the fallback parameter sentence to `Sets the {split parameter name}.` and the absence of constraints to `No additional constraints.` Preserve specific curated descriptions and enum-value constraints. Update the Button summary with decision-oriented copy and define certification as the repository’s reviewed evidence status in the visible evidence section.

Normalize public branding to `Maliev Shadcn Blazor`; do not alter package names, namespaces, commands, URLs, or code samples containing `Maliev.ShadcnBlazor`.

- [ ] **Step 4: Build, run the relevant suites, and run repository gates**

```powershell
dotnet build Maliev.ShadcnBlazor.slnx -c Release
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --no-build --filter "FullyQualifiedName~ThemeStudioBrowserTests|FullyQualifiedName~ThemeImportExportBrowserTests|FullyQualifiedName~DocumentationWorkbenchBrowserTests"
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --no-build
dotnet format Maliev.ShadcnBlazor.slnx --verify-no-changes --no-restore
pwsh -NoProfile -File eng/Verify-PublicSurface.ps1 -Root .
```

Expected: zero build warnings/errors; all focused and affected suites pass; formatting and public-surface checks exit zero.

- [ ] **Step 5: Commit**

```powershell
git add -- samples/Maliev.ShadcnBlazor.Showcase/Components/Documentation/ComponentStatusEvidence.razor samples/Maliev.ShadcnBlazor.Showcase/Documentation/Api/ComponentApiCatalog.cs samples/Maliev.ShadcnBlazor.Showcase/Documentation/ComponentDocumentationCatalog.cs tests/Maliev.ShadcnBlazor.Tests/Showcase/ComponentDossierTests.cs tests/Maliev.ShadcnBlazor.Tests/Showcase/DocumentationCatalogTests.cs
git commit -m "fix(docs): clarify public component guidance"
```

## Self-review

- Spec coverage: all five priority issues, the dependable detector findings, product naming, and the visible copy defect are assigned to Tasks 1–4.
- Explicit non-defects: screen-reader chart occlusion, loaded-avatar fallback contrast, camera overlay contrast, Geist use, kickers, and bounded icon tiles are not changed because the critique classified them as false positives or advisory taste signals.
- Type consistency: Task 1 owns the category contract; Task 2 owns validation presentation; Task 3 owns code disclosure; Task 4 consumes only existing documentation contracts.
- Shared-file sequence: Tasks 1 and 3 both modify `showcase.css`, and Tasks 3 and 4 both modify dossier tests; execution is sequential and each task starts from the prior committed result.
- Placeholder scan: no deferred implementation steps or unspecified test commands remain.
