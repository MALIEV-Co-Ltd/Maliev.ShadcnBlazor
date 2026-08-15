# Shadcn-Style Documentation Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a responsive Shadcn-style documentation site with categorized component navigation, complete Blazor consumption guidance, a contextual section outline, and safe public contributor instructions.

**Architecture:** Keep the component catalog, examples, API descriptors, and evidence as the authoritative data sources. A scoped `DocumentationPageState` connects dossier pages to the layout's “On This Page” rail, while the existing `DocumentationNavigationState` owns mutually exclusive component and outline drawers. The Showcase remains a static Blazor WebAssembly site deployable under a repository base path.

**Tech Stack:** .NET 10, Blazor WebAssembly, Razor components, semantic CSS tokens, bUnit/xUnit, Microsoft Playwright, Axe, GitHub Pages.

## Global Constraints

- Preserve all existing public `Maliev.ShadcnBlazor` component APIs and executable example behavior.
- All internal Showcase URLs must remain relative to the configured Blazor base path.
- Every dossier must derive examples, API, namespace, token, evidence, and source links from checked-in authoritative metadata.
- Support light/dark, LTR/RTL, keyboard, reduced motion, forced colors, 200% zoom, desktop, tablet, and mobile.
- Do not add secrets, private URLs, customer data, private package feeds, private application dependencies, or application-specific DTOs/routes.
- Build before tests; run focused tests before relevant full suites; commit each coherent green slice.

---

### Task 1: Categorized documentation navigation and section state

**Files:**
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Documentation/DocumentationNavigationState.cs`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Documentation/DocumentationPageState.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Program.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Layout/DocumentationCatalogRail.razor`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Layout/DocumentationOnThisPage.razor`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Showcase/DocumentationNavigationTests.cs`

**Interfaces:**
- Produces: `DocumentationSection(string Id, string Label)`.
- Produces: scoped `DocumentationPageState` with `IReadOnlyList<DocumentationSection> Sections`, `SetSections(IEnumerable<DocumentationSection>)`, `Clear()`, and `Changed`.
- Extends: `DocumentationNavigationState` with `OutlineOpen`; `CatalogOpen` and `OutlineOpen` remain mutually exclusive.

- [ ] **Step 1: Write failing navigation and state tests**

```csharp
[Fact]
public void CatalogRail_GroupsEveryComponentByCategoryAndShowsEmptySearch()
{
    var state = new DocumentationNavigationState();
    var cut = Render<DocumentationCatalogRail>(p => p.Add(x => x.State, state));
    Assert.Equal(64, cut.FindAll(".documentation-component-list a").Count);
    Assert.NotEmpty(cut.FindAll(".documentation-category"));
    state.Query = "no-such-component";
    cut.Render();
    Assert.Equal("No components found", cut.Find("[role='status']").TextContent.Trim());
}

[Fact]
public void PageState_NormalizesUniqueSectionsAndClears()
{
    var state = new DocumentationPageState();
    state.SetSections([new("usage", "Usage"), new("usage", "Duplicate")]);
    Assert.Equal([new DocumentationSection("usage", "Usage")], state.Sections);
    state.Clear();
    Assert.Empty(state.Sections);
}
```

- [ ] **Step 2: Run the focused tests and verify the new types/behavior fail**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter FullyQualifiedName~DocumentationNavigationTests`

Expected: compilation failure for `DocumentationPageState` and assertion failures for the ungrouped rail.

- [ ] **Step 3: Implement state and categorized navigation**

```csharp
public sealed record DocumentationSection(string Id, string Label);

public sealed class DocumentationPageState
{
    private IReadOnlyList<DocumentationSection> _sections = [];
    public event EventHandler? Changed;
    public IReadOnlyList<DocumentationSection> Sections => _sections;
    public void SetSections(IEnumerable<DocumentationSection> sections)
    {
        ArgumentNullException.ThrowIfNull(sections);
        var next = sections
            .Select(section => new DocumentationSection(
                string.IsNullOrWhiteSpace(section.Id) ? throw new ArgumentException("Section IDs are required.", nameof(sections)) : section.Id.Trim(),
                string.IsNullOrWhiteSpace(section.Label) ? throw new ArgumentException("Section labels are required.", nameof(sections)) : section.Label.Trim()))
            .DistinctBy(section => section.Id, StringComparer.Ordinal)
            .ToArray();
        if (_sections.SequenceEqual(next)) return;
        _sections = next;
        Changed?.Invoke(this, EventArgs.Empty);
    }
    public void Clear()
    {
        if (_sections.Count == 0) return;
        _sections = [];
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
```

Render one `.documentation-category` section per non-empty category, use category headings plus nested lists, retain normalized search and `aria-current`, and render a clear-search action when zero results remain. Register `DocumentationPageState` as scoped in `Program.cs`.

- [ ] **Step 4: Implement `DocumentationOnThisPage`**

Render a named `nav` containing links `href="#@section.Id"` for `PageState.Sections`. Subscribe/unsubscribe to `Changed`; render nothing when no dossier sections exist.

- [ ] **Step 5: Build and rerun focused tests**

Run:

```powershell
dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore -p:NuGetAudit=false
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build --filter FullyQualifiedName~DocumentationNavigationTests
```

Expected: build with zero warnings/errors and all focused tests passing.

- [ ] **Step 6: Commit the state/navigation slice**

```powershell
git add samples/Maliev.ShadcnBlazor.Showcase/Documentation samples/Maliev.ShadcnBlazor.Showcase/Layout/DocumentationCatalogRail.razor samples/Maliev.ShadcnBlazor.Showcase/Layout/DocumentationOnThisPage.razor samples/Maliev.ShadcnBlazor.Showcase/Program.cs tests/Maliev.ShadcnBlazor.Tests/Showcase/DocumentationNavigationTests.cs
git commit -m "feat(showcase): add categorized documentation navigation"
```

### Task 2: Responsive three-column documentation shell

**Files:**
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Layout/DocumentationLayout.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Layout/DocumentationHeader.razor`
- Delete: `samples/Maliev.ShadcnBlazor.Showcase/Layout/DocumentationThemeDock.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Showcase/DocumentationNavigationTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.BrowserTests/DocumentationWorkbenchBrowserTests.cs`

**Interfaces:**
- Consumes: `DocumentationNavigationState.CatalogOpen` and `.OutlineOpen`.
- Consumes: `DocumentationOnThisPage` and `ShowcaseState`.
- Produces: landmarks `#documentation-catalog`, `#documentation-content`, and `#documentation-outline`.

- [ ] **Step 1: Write failing shell tests**

Assert the layout has three landmarks, no `#documentation-theme`, header theme/direction controls, component and outline triggers with correct `aria-controls`, mutual exclusion, Escape closing, and focus restoration for each drawer.

- [ ] **Step 2: Run tests and verify failure against the current theme-dock shell**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter FullyQualifiedName~DocumentationNavigationTests`

- [ ] **Step 3: Implement header and layout semantics**

Move theme/direction actions into `DocumentationHeader`, replace the theme aside with `DocumentationOnThisPage`, and give catalog/outline drawers symmetric close/focus behavior. Keep the skip links and use logical inset properties for RTL.

- [ ] **Step 4: Replace the documentation CSS world**

Implement:

```css
.documentation-workbench {
  display: grid;
  grid-template-columns: minmax(15rem,17rem) minmax(0,48rem) minmax(12rem,14rem);
  justify-content: center;
}
.documentation-catalog,
.documentation-outline { position: sticky; inset-block-start: 3.5rem; block-size: calc(100vh - 3.5rem); overflow-y: auto; }
```

At `80rem`, collapse the outline to a drawer; at `48rem`, collapse both rails. Preserve forced-color focus, reduced-motion transitions, RTL mirroring, and local overflow containers for code/API.

- [ ] **Step 5: Update and run focused real-browser layout tests**

Assert simultaneous sidebar/outline visibility at 1440px, outline collapse at 1024/768px, both mobile drawers at 390/320px, Escape/backdrop/focus restoration, sticky geometry, theme/direction mutation, and no horizontal overflow.

Run: `dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --no-build --filter FullyQualifiedName~DocumentationWorkbenchBrowserTests`

- [ ] **Step 6: Commit the shell slice**

```powershell
git add samples/Maliev.ShadcnBlazor.Showcase/Layout samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css tests/Maliev.ShadcnBlazor.Tests/Showcase/DocumentationNavigationTests.cs tests/Maliev.ShadcnBlazor.BrowserTests/DocumentationWorkbenchBrowserTests.cs
git commit -m "feat(showcase): redesign the documentation shell"
```

### Task 3: Complete component consumption articles

**Files:**
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Components/Documentation/ComponentConsumptionGuide.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Documentation/ComponentPreview.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Documentation/ComponentCodeExample.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Pages/Docs/ComponentDocumentation.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Showcase/DocumentationRouteTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ComponentDossierContractTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.BrowserTests/ComponentDossierBrowserTests.cs`

**Interfaces:**
- `ComponentConsumptionGuide` parameters: `ComponentDocumentationEntry Entry`, `IReadOnlyList<ComponentExampleDefinition> Examples`, `IReadOnlyList<ComponentApiDescriptor> ApiDescriptors`.
- `ComponentDocumentation` publishes a deterministic `DocumentationSection[]` to `DocumentationPageState` and clears it on disposal.

- [ ] **Step 1: Write failing dossier structure tests**

For representative `accordion`, `button`, `dialog`, `chart`, and `message`, assert stable IDs `overview`, `preview`, `installation`, `usage`, `composition`, `accessibility`, `api-reference`, `theming`, `evidence`, and `references`; assert the installation command, namespace, executable first example source, and matching outline links.

- [ ] **Step 2: Run focused tests and verify missing sections fail**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~DocumentationRouteTests|FullyQualifiedName~ComponentDossierContractTests"`

- [ ] **Step 3: Implement `ComponentConsumptionGuide`**

Render the exact package command, `@using Entry.Namespace`, first example source, API-type composition tree, and token groups. Reuse `ComponentCodeExample` for every copyable block so clipboard behavior and fallback remain one implementation.

- [ ] **Step 4: Recompose `ComponentDocumentation`**

Add previous/next navigation from catalog order; group preview and code for each example; render the consumption guide before accessibility/API/evidence; attach stable IDs; publish only rendered sections; preserve unknown-slug Empty behavior and public source links.

- [ ] **Step 5: Refine preview/code visual structure**

Attach preview controls, canvas, and source into one example block with restrained borders, compact headings, readable code, and no dashboard cards or marketing hero treatment.

- [ ] **Step 6: Run unit and browser dossier tests**

Run:

```powershell
dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore -p:NuGetAudit=false
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~DocumentationRouteTests|FullyQualifiedName~ComponentDossierContractTests"
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --no-build --filter FullyQualifiedName~ComponentDossierBrowserTests
```

- [ ] **Step 7: Commit the dossier slice**

```powershell
git add samples/Maliev.ShadcnBlazor.Showcase/Components/Documentation samples/Maliev.ShadcnBlazor.Showcase/Pages/Docs/ComponentDocumentation.razor samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css tests/Maliev.ShadcnBlazor.Tests/Showcase tests/Maliev.ShadcnBlazor.BrowserTests/ComponentDossierBrowserTests.cs
git commit -m "feat(showcase): document component consumption"
```

### Task 4: Public-safe contributor agent contract

**Files:**
- Create: `AGENTS.md`
- Modify: `CONTRIBUTING.md`
- Test: `tests/Maliev.ShadcnBlazor.RepositoryTests/PublicRepositoryTests.cs`

**Interfaces:**
- Produces: repository-wide contributor-agent instructions consistent with `CONTRIBUTING.md` and `SECURITY.md`.

- [ ] **Step 1: Write failing repository contract tests**

Assert `AGENTS.md` names the six repository boundaries, build-first validation, focused/relevant/browser tests, formatting, `Verify-PublicSurface.ps1`, accessibility states, dossier/evidence updates, secret/private-dependency prohibitions, coherent commits, and non-implicit release/deploy authority. Assert it contains no private product identifiers or local absolute paths.

- [ ] **Step 2: Run the repository test and verify missing-file failure**

Run: `dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --filter FullyQualifiedName~PublicRepositoryTests`

- [ ] **Step 3: Write `AGENTS.md` and align contributing guidance**

Use concise imperative sections: scope, repository map, implementation boundaries, TDD, validation commands, accessibility/evidence, public safety, Git/PR discipline, and release/deployment authorization.

- [ ] **Step 4: Build and run the full repository suite**

Run:

```powershell
dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore -p:NuGetAudit=false
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --no-build
pwsh -NoProfile -File eng/Verify-PublicSurface.ps1 -Root .
```

- [ ] **Step 5: Commit the public contract slice**

```powershell
git add AGENTS.md CONTRIBUTING.md tests/Maliev.ShadcnBlazor.RepositoryTests/PublicRepositoryTests.cs
git commit -m "docs: add safe contributor agent guidance"
```

### Task 5: Visual proof, complete validation, and delivery

**Files:**
- Modify: `tests/Maliev.ShadcnBlazor.BrowserTests/ComponentCatalogVisualProofTests.cs`
- Modify: `docs/visual-proof/README.md`
- Modify: `README.md`
- Modify: screenshots under `docs/visual-proof/components/` only through the opt-in generator

**Interfaces:**
- Consumes: finalized shell and dossier routes.
- Produces: representative regenerated docs proof plus full validation evidence.

- [ ] **Step 1: Extend visual/browser contracts for the docs shell**

Capture representative foundation, forms, overlays, data, and conversation dossiers at desktop light and mobile dark/RTL. Assert sidebar/content/outline geometry and actual section headings before capturing.

- [ ] **Step 2: Run the opt-in visual generator once**

Run the existing documented visual-proof workflow locally with its update environment variable. Confirm only intended documentation screenshots change.

- [ ] **Step 3: Perform one batched visual inspection and fix findings**

Inspect desktop and mobile together for hierarchy, clipping, sticky rails, code overflow, focus, drawer placement, dark contrast, and RTL. Apply one coherent correction batch, then run one confirmation capture pass.

- [ ] **Step 4: Run final validation**

```powershell
dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore -p:NuGetAudit=false
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --no-build
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --no-build
dotnet format Maliev.ShadcnBlazor.slnx --verify-no-changes --no-restore
pwsh -NoProfile -File eng/Verify-PublicSurface.ps1 -Root .
git diff --check
```

Expected: all builds with zero warnings/errors; all tests pass with zero skipped unless already documented; formatting, public-surface, and diff checks clean.

- [ ] **Step 5: Run the Impeccable detector once over changed UI targets**

Run: `node C:\Users\natth\.agents\skills\impeccable\scripts\detect.mjs --json samples/Maliev.ShadcnBlazor.Showcase/Layout samples/Maliev.ShadcnBlazor.Showcase/Pages/Docs samples/Maliev.ShadcnBlazor.Showcase/Components/Documentation samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css`

Resolve every applicable finding or document why it is a false positive.

- [ ] **Step 6: Commit final proof/docs**

```powershell
git add README.md docs/visual-proof tests/Maliev.ShadcnBlazor.BrowserTests
git commit -m "test(showcase): verify redesigned component documentation"
```

- [ ] **Step 7: Push, open the pull request, wait for all checks, merge with approved administrator workflow, and verify Pages**

Verify the public root, direct `/docs/components/accordion`, component sidebar navigation, outline anchors, theme/direction controls, preview mutation, source copy, and browser console. Confirm GitHub Pages deployed the exact merge commit and leave branch protection restored.
