# Theme Studio Curated Runway Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace Theme Studio's dashboard/catalog preview with the approved responsive counter-scrolling use-case runway, add real optional icon catalogs, and close the supporting CodeBlock and Message Scroller defects tracked by issue #227.

**Architecture:** Keep the core component package lean by defining one icon rendering contract and shipping each full free upstream catalog in an optional companion package. Theme Studio uses one fixed registry of independently stateful use-case cards, a finite catalog of materialized theme presets, a central deterministic demonstration clock, and a JS/CSS runway controller that counter-scrolls inert mirrors only where the viewport fits. Documentation and Theme Studio share the same app bar; preview-specific settings live in the package Sidebar and apply only below the preview theme scope.

**Tech Stack:** .NET 10, Blazor WebAssembly, Razor Class Library, bUnit/xUnit, System.Text.Json, PowerShell maintainer tooling, JavaScript modules, CSS logical properties, Playwright, Axe, NuGet package validation.

**Spec:** `docs/superpowers/specs/2026-08-23-theme-studio-curated-runway-design.md`

## Global Constraints

- Work from issue #227 and the approved comps in `.impeccable/mocks/theme-studio-approved-direction-{desktop,mobile}.png`.
- Preserve the existing `Maliev.ShadcnBlazor` public API unless a member is explicitly introduced by Task 1.
- Card identity, ordering, copy, and local state never change when Shuffle selects a preset.
- Desktop and fitting landscape tablet use two equal tracks; portrait tablet and mobile use one natural-scroll column.
- Mobile and reduced-motion contexts never auto-scroll the runway.
- Loop mirrors are inert, `aria-hidden`, pointer-inert, timer-free, network-free, and absent from live announcements.
- Typography and icon selection apply only beneath the preview scope, never the documentation app bar or Sidebar.
- Builds, tests, and package restore never fetch icon catalogs or fonts from the network.
- Only Hugeicons Free MIT assets are permitted; no Hugeicons Pro path, package, file, or notice may enter the repository or nupkg.
- Every new public member has XML documentation and an intentional `tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt` entry.
- Run `node C:\Users\natth\.agents\skills\impeccable\scripts\detect.mjs --json` exactly once after the final UI implementation, not during intermediate tasks.

---

## File map

### Core icon boundary

- Create `src/Maliev.ShadcnBlazor/Components/Icons/ShadcnIcon.razor`: accessible renderer for sanitized `ShadcnIconData`.
- Create `src/Maliev.ShadcnBlazor/Components/Icons/ShadcnIconData.cs`: immutable icon definition and catalog interface.
- Modify `src/Maliev.ShadcnBlazor/wwwroot/css/shadcn-base.css`: size/current-color/forced-color icon rules.
- Modify `tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt`: exact renderer/data/catalog surface.

### Optional icon packages

- Create `src/Maliev.ShadcnBlazor.Icons.{Lucide,Tabler,Phosphor,Hugeicons}/`: one packable project per free catalog.
- Create `eng/Refresh-IconCatalogs.ps1`: maintainer-only pinned downloader, sanitizer, deterministic catalog/name generator, and license copier.
- Create `eng/icon-sources.json`: immutable upstream coordinates, paths, versions, commits, hashes, and license metadata.
- Modify `Maliev.ShadcnBlazor.slnx`, `THIRD-PARTY-NOTICES.md`, package/repository tests, and Showcase project references.

### Shared UI corrections

- Modify `src/Maliev.ShadcnBlazor/Components/Typography/ShadcnCodeBlock.razor` and `wwwroot/css/shadcn-base.css`.
- Modify `samples/Maliev.ShadcnBlazor.Showcase/Layout/DocumentationHeader.razor`.
- Modify `samples/Maliev.ShadcnBlazor.Showcase/Components/Documentation/ConversationScrollerDossierPreview.razor` and `wwwroot/css/showcase.css`.
- Extend existing CodeBlock, documentation shell, conversation unit, and browser tests.

### Theme Studio model and UI

- Create `samples/Maliev.ShadcnBlazor.Showcase/Theming/Presets/ThemeStudioPresetDefinition.cs` and `ThemeStudioPresetCatalog.cs`.
- Create `samples/Maliev.ShadcnBlazor.Showcase/Theming/Runway/ThemeUseCaseDefinition.cs`, `ThemeUseCaseRegistry.cs`, `ThemeRunwayState.cs`, and `ThemeDemonstrationClock.cs`.
- Create focused card components under `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/Runway/Cards/`.
- Create `ThemeRunway.razor`, `ThemeRunwayTrack.razor`, and `ThemePresetDock.razor`.
- Create `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/js/theme-studio-runway.js`.
- Modify `ThemeStudio.razor`, `ThemeStudioLayout.razor`, `ThemeStudioSidebar.razor`, `ThemeInspector.razor`, `ThemeStudioState.cs`, `ThemeStudioWorkbenchState.cs`, `ThemeStudioGeneratorCatalog.cs`, `theme-studio.js`, and `showcase.css`.
- Remove primary-route use of `PreviewToolbar.razor`, `MockSiteHost.razor`, and `ThemeScenarioBrowser.razor`; retain the latter as an independent QA route.

---

### Task 1: Add the core accessible icon contract

**Files:**
- Create: `src/Maliev.ShadcnBlazor/Components/Icons/ShadcnIconData.cs`
- Create: `src/Maliev.ShadcnBlazor/Components/Icons/ShadcnIcon.razor`
- Modify: `src/Maliev.ShadcnBlazor/wwwroot/css/shadcn-base.css`
- Create: `tests/Maliev.ShadcnBlazor.Tests/Components/Icons/IconTests.cs`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt`

**Interfaces:**
- Produces: `ShadcnIconData`, `IShadcnIconCatalog`, and `<ShadcnIcon Icon="..." Label="..." Size="..." />`.
- `ShadcnIconData` accepts only already-sanitized inner SVG markup; companion packages own ingestion and validation.

- [ ] **Step 1: Write failing renderer and API tests**

```csharp
[Fact]
public void DecorativeIconIsHiddenAndUsesCurrentColor()
{
    var cut = RenderComponent<ShadcnIcon>(p => p
        .Add(x => x.Icon, new ShadcnIconData("test", "arrow", "0 0 24 24", "<path d=\"M4 12h16\" />")));
    cut.Find("svg").GetAttribute("aria-hidden").ShouldBe("true");
    cut.Find("svg").GetAttribute("data-library").ShouldBe("test");
}

[Fact]
public void NamedIconUsesImageSemantics()
{
    var cut = RenderComponent<ShadcnIcon>(p => p
        .Add(x => x.Icon, TestIcon)
        .Add(x => x.Label, "Change direction"));
    cut.Find("svg").GetAttribute("role").ShouldBe("img");
    cut.Find("svg").GetAttribute("aria-label").ShouldBe("Change direction");
}
```

- [ ] **Step 2: Run the focused tests and verify RED**

Run:

```powershell
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~IconTests"
```

Expected: compile failure because `ShadcnIcon` and `ShadcnIconData` do not exist.

- [ ] **Step 3: Implement the minimal immutable contract**

```csharp
namespace Maliev.ShadcnBlazor.Components.Icons;

public sealed record ShadcnIconData(string Library, string Name, string ViewBox, string SvgContent);

public interface IShadcnIconCatalog
{
    string Library { get; }
    IReadOnlyList<string> Names { get; }
    bool TryGet(string name, out ShadcnIconData? icon);
    ShadcnIconData Get(string name);
}
```

Render `SvgContent` only from `ShadcnIconData`, forward safe unmatched attributes through `ShadcnComponentBase`, default to decorative semantics, use `role=img` only when `Label` is nonempty, and validate positive finite `Size`.

- [ ] **Step 4: Build before tests**

Run:

```powershell
dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore -p:NuGetAudit=false
```

Expected: 0 warnings, 0 errors.

- [ ] **Step 5: Run focused tests and public API verification**

Run the `IconTests|PublicApiSnapshotTests` filter and `eng/Verify-PublicSurface.ps1 -Root .`.

- [ ] **Step 6: Commit the green core boundary**

```powershell
git add src/Maliev.ShadcnBlazor/Components/Icons src/Maliev.ShadcnBlazor/wwwroot/css/shadcn-base.css tests/Maliev.ShadcnBlazor.Tests/Components/Icons tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt
git commit -m "feat(icons): add accessible icon rendering contract"
```

### Task 2: Import and package the four free icon catalogs

**Files:**
- Create: `eng/icon-sources.json`
- Create: `eng/Refresh-IconCatalogs.ps1`
- Create: `src/Maliev.ShadcnBlazor.Icons.Lucide/*`
- Create: `src/Maliev.ShadcnBlazor.Icons.Tabler/*`
- Create: `src/Maliev.ShadcnBlazor.Icons.Phosphor/*`
- Create: `src/Maliev.ShadcnBlazor.Icons.Hugeicons/*`
- Modify: `Maliev.ShadcnBlazor.slnx`
- Modify: `THIRD-PARTY-NOTICES.md`
- Create: `tests/Maliev.ShadcnBlazor.Tests/Components/Icons/IconCatalogTests.cs`
- Create: `tests/Maliev.ShadcnBlazor.RepositoryTests/IconCatalogRepositoryTests.cs`
- Modify: `tests/Maliev.ShadcnBlazor.RepositoryTests/PackageArchiveTests.cs`

**Interfaces:**
- Consumes: `IShadcnIconCatalog` and `ShadcnIconData` from Task 1.
- Produces: `LucideIconCatalog.Instance`, `TablerIconCatalog.Instance`, `PhosphorIconCatalog.Instance`, `HugeiconsIconCatalog.Instance`, plus generated `*IconNames` constants.

- [ ] **Step 1: Add RED source, sanitizer, license, and archive tests**

Tests must assert these immutable sources:

```json
{
  "lucide": { "version": "1.33.0", "commit": "59978cecf84986af59f1f9f503bcebdc89c6d166", "license": "ISC" },
  "tabler": { "version": "3.46.0", "commit": "8ac7d81b72ece11072ef25ea9fd92e80c6f3c9fc", "license": "MIT" },
  "phosphor": { "version": "2.0.8", "commit": "d42782b2abe747d904b971ccab48b182a1455f86", "license": "MIT" },
  "hugeicons": { "version": "free-3365154", "commit": "3365154e0ae2461fbfb6249b89649127207a4f9e", "license": "MIT" }
}
```

For every generated icon, reject `script`, `foreignObject`, `style`, `iframe`, `object`, `embed`, `on*`, `href`, `xlink:href`, external URL text, declarations, malformed XML, missing viewBox, duplicate normalized name, and path data above the documented size cap. Assert deterministic ordinal names and LF/no-BOM output.

- [ ] **Step 2: Run repository tests and verify RED**

Expected failures: missing source manifest, refresh tool, companion projects, catalogs, license files, and nupkg entries.

- [ ] **Step 3: Implement the maintainer-only importer**

`Refresh-IconCatalogs.ps1` must:

1. accept `-Library lucide|tabler|phosphor|hugeicons|all` and `-DestinationRoot`;
2. download only immutable GitHub archive URLs declared in `icon-sources.json`;
3. verify the archive SHA-256 before extraction;
4. parse SVG with secure `XmlReaderSettings` (`DtdProcessing=Prohibit`, `XmlResolver=$null`);
5. allow only `svg`, `g`, `path`, `circle`, `ellipse`, `line`, `polyline`, `polygon`, `rect` and the exact presentation attributes required by the source;
6. normalize markup, names, and view boxes deterministically;
7. write checked-in `Catalog/icons.json`, `Generated/*IconNames.g.cs`, and `licenses/<upstream>-LICENSE.*` atomically;
8. never run from build, restore, test, CI, package initialization, or Showcase startup.

- [ ] **Step 4: Create four packable projects**

Each project targets `net10.0`, references the core project, embeds `Catalog/icons.json`, exposes one sealed lazy catalog, and packs its upstream license under `licenses/`. Package IDs exactly match the project names. Package dependencies contain only the matching version of `Maliev.ShadcnBlazor`.

- [ ] **Step 5: Generate the catalogs once and inspect the diff**

Run the refresh command with `-Library all`, then scan generated outputs for private paths, credentials, Pro identifiers, external URLs, scripts, and non-free license strings.

- [ ] **Step 6: Build and run focused catalog/archive tests**

Expected: solution build 0 warnings/0 errors; every catalog resolves representative icons including a bidirectional-text icon or its closest library-specific semantic equivalent; every companion nupkg contains only its assembly, XML docs, core dependency, license, readme, and metadata.

- [ ] **Step 7: Commit the four optional packages**

```powershell
git add eng/icon-sources.json eng/Refresh-IconCatalogs.ps1 src/Maliev.ShadcnBlazor.Icons.* Maliev.ShadcnBlazor.slnx THIRD-PARTY-NOTICES.md tests/Maliev.ShadcnBlazor.Tests/Components/Icons tests/Maliev.ShadcnBlazor.RepositoryTests
git commit -m "feat(icons): ship licensed optional icon catalogs"
```

### Task 3: Correct CodeBlock, universal app bar, and Message Scroller geometry

**Files:**
- Modify: `src/Maliev.ShadcnBlazor/Components/Typography/ShadcnCodeBlock.razor`
- Modify: `src/Maliev.ShadcnBlazor/wwwroot/css/shadcn-base.css`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Layout/DocumentationHeader.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Documentation/ConversationScrollerDossierPreview.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css`
- Modify: existing CodeBlock, DocumentationWorkbench, ConversationWorkflow, and visual tests.

**Interfaces:**
- Consumes: icon data from Task 2 for the direction action.
- Produces: stable CodeBlock toolbar, shared app bar behavior, and bounded transcript/composer DOM used by Theme Studio cards.

- [ ] **Step 1: Add RED CodeBlock geometry tests**

Assert a multi-language toolbar has one visible language control, no redundant static label, selector left edge before copy left edge, copy right edge stable within one pixel through idle→copied→idle, toolbar height stable, and compact selector width based on its label rather than one-third of the row.

- [ ] **Step 2: Add RED Message Scroller DOM and browser geometry tests**

The test must prove:

```csharp
Assert.True(lastMessage.Bottom <= composer.Top);
Assert.True(fade.Bottom <= composer.Top);
Assert.True(fade.Right <= scrollbar.Left || fade.Left >= scrollbar.Right);
Assert.Equal(viewport.Top, scrollbar.Top, tolerance: 2);
Assert.Equal(viewport.Bottom, scrollbar.Bottom, tolerance: 2);
```

Repeat after streamed growth, wheel-away, return-to-end, mobile, and RTL.

- [ ] **Step 3: Verify both focused groups fail for the reported causes**

Do not accept failures from server startup, selectors, or timeouts.

- [ ] **Step 4: Implement stable CodeBlock layout**

Use a three-column grid: compact selector or static label, flexible spacer, fixed-size copy action. Keep copied status in the accessible announcement and swap only equal-size icons inside the action.

- [ ] **Step 5: Implement the shared direction icon**

Use the selected real icon definition inside `DocumentationHeader`; preserve the current state-dependent label and title. Theme Studio will consume this same header in Task 7.

- [ ] **Step 6: Implement transcript/composer siblings**

Create a positioned transcript wrapper containing the viewport plus an inset, pointer-inert fade. Place the composer as the second row of the scroller grid. Offset the jump button above the composer. Remove the root pseudo-element and bottom padding that allowed messages and the scrollbar to extend behind the composer.

- [ ] **Step 7: Build and run unit/browser regressions**

Run focused CodeBlock, documentation header, and conversation tests. Inspect the affected CodeBlock and Message Scroller desktop/mobile visual proofs before committing.

- [ ] **Step 8: Commit the shared corrections**

```powershell
git add src/Maliev.ShadcnBlazor/Components/Typography src/Maliev.ShadcnBlazor/wwwroot/css/shadcn-base.css samples/Maliev.ShadcnBlazor.Showcase/Layout/DocumentationHeader.razor samples/Maliev.ShadcnBlazor.Showcase/Components/Documentation/ConversationScrollerDossierPreview.razor samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css tests
git commit -m "fix(showcase): stabilize shared code and conversation surfaces"
```

### Task 4: Replace random palette generation with curated materialized presets

**Files:**
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Theming/Presets/ThemeStudioPresetDefinition.cs`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Theming/Presets/ThemeStudioPresetCatalog.cs`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Theming/Presets/ThemeStudioPresetCatalog.json`
- Modify: `ThemeStudioState.cs`, `ThemeStudioSnapshot.cs`, `ThemeStudioStorage.cs`, generator UI, export tests, and state tests.

**Interfaces:**
- Produces: `ThemeStudioPresetDefinition`, `IThemeStudioPresetCatalog`, `ThemeStudioState.ApplyPreset(string)`, and `ThemeStudioState.ShufflePreset()`.
- Preset documents use existing canonical `ShadcnThemeDocument` v2; no schema bump is needed.

- [ ] **Step 1: Write RED preset-catalog and Shuffle tests**

Assert at least twelve unique reviewed presets. Each has a stable ID, display name, style, base, accent, radius, density, border, surface, control, motion, and icon-library choice plus one fully materialized valid document. Assert `ShufflePreset()` always returns a catalog member other than the current item when more than one exists, creates one undo entry, and does not alter card registry order or card-state snapshots.

- [ ] **Step 2: Verify RED against the current seed generator**

Expected: missing types/methods and current `GenerateNewPalette()` can produce values outside a finite preset catalog.

- [ ] **Step 3: Implement immutable catalog loading**

Embed the JSON catalog, deserialize once, reject unknown members/duplicate IDs/invalid documents/unsupported icon libraries, and expose ordinal snapshots. Materialize all theme tokens in the file so runtime Shuffle does not run `ShadcnPaletteGenerator`.

- [ ] **Step 4: Implement transactional Apply and Shuffle**

`ApplyPreset` validates before capturing history and leaves the prior state untouched on error. `ShufflePreset` uses `RandomNumberGenerator.GetInt32` only to choose an index from the finite candidates excluding the current preset. Keep the advanced package palette APIs intact but remove seed generation and raw token editors from the primary Theme Studio UI.

- [ ] **Step 5: Preserve import/export fidelity**

Existing canonical document import remains accepted. If the imported document matches a preset byte-for-byte, select it; otherwise label it `Custom imported` without inventing preset provenance. Export emits the exact applied document and selected icon package guidance.

- [ ] **Step 6: Build, run state/bundle/storage tests, and commit**

```powershell
git add samples/Maliev.ShadcnBlazor.Showcase/Theming/Presets samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudio*.cs samples/Maliev.ShadcnBlazor.Showcase/Components/Theming tests/Maliev.ShadcnBlazor.Tests/Showcase
git commit -m "feat(theme): add curated reproducible presets"
```

### Task 5: Build the fixed realistic use-case registry and demonstration clock

**Files:**
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Theming/Runway/*.cs`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/Runway/Cards/*.razor`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/Runway/ThemeUseCaseCardHost.razor`
- Create: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeRunwayStateTests.cs`
- Create: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeUseCaseRegistryTests.cs`

**Interfaces:**
- Produces: `IThemeUseCaseRegistry.All`, `ThemeRunwayState`, `ThemeDemonstrationClock`, `ThemeDemonstrationFrame`, and card renderers accepting `State`, `Frame`, and `IsMirror`.

- [ ] **Step 1: Write RED registry invariants**

Assert exactly twelve IDs in approved order, six assigned left and six right, unique copy in English/Thai, at least three package component types per card, independent state factories, and no dependency on `IComponentDocumentationCatalog` or `IThemeScenarioRegistry`.

- [ ] **Step 2: Write RED clock and state tests**

Use a fake time provider. Assert phases advance deterministically, pause freezes both card and track time, resume continues without a jump, reset returns the same frame, reduced motion returns stable representative frames, and mirrors never register timers or live announcements.

- [ ] **Step 3: Implement registry and centralized clock**

Use one periodic loop owned by `ThemeRunwayState`, not timers in cards. Expose immutable frame values such as `CapacityPercent`, `UploadPercent`, `FormStep`, `ChatCharacters`, `ToastVisible`, and `ApprovalState`. Dispose and cancellation must be idempotent.

- [ ] **Step 4: Implement twelve authentic card compositions**

Each card uses real package components, controlled values, actual callbacks on the logical copy, and disabled/inert rendering on the mirror. Use fictional Thai names and organizations only. Do not import dossier preview components.

- [ ] **Step 5: Add component tests for each state transition**

Test neutral, active, completed/error, reduced-motion, and mirror states. Verify no mirror form is submittable and no mirror live region exists.

- [ ] **Step 6: Build, run focused/full package tests, and commit**

```powershell
git add samples/Maliev.ShadcnBlazor.Showcase/Theming/Runway samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/Runway tests/Maliev.ShadcnBlazor.Tests/Showcase
git commit -m "feat(theme): add realistic animated use-case deck"
```

### Task 6: Implement the responsive counter-scrolling runway

**Files:**
- Create: `ThemeRunway.razor`, `ThemeRunwayTrack.razor`, `ThemePresetDock.razor`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/js/theme-studio-runway.js`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css`
- Create: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeRunwayContractTests.cs`
- Extend: `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeStudioBrowserTests.cs`

**Interfaces:**
- Consumes: fixed registry/state from Task 5 and preset actions from Task 4.
- Produces: JS module `attachRunway(root)` returning `{ setPersistentPaused, refresh, dispose }`.

- [ ] **Step 1: Add RED source and browser contracts**

Assert two tracks at desktop/landscape tablet, one logical column at portrait/mobile, left and right transform directions oppose, mirrors are inert/aria-hidden, and mobile has no mirror or automatic animation. Browser tests measure at least 20px movement in the expected direction over a bounded interval.

- [ ] **Step 2: Add RED pause lifecycle tests**

For pointer enter, focus, wheel, touch, keydown, visibility hidden, and persistent Pause, record transforms before and after a bounded interval and assert no movement. For temporary pointer/wheel pauses, assert movement resumes only after the inactivity interval. Focus must prevent resume until focus leaves.

- [ ] **Step 3: Implement CSS track topology**

Use logical sizes, equal `minmax(0,1fr)` tracks, clipped stage edges, stable card gaps, and a preview-only bottom dock. Do not animate layout properties. Mobile switches to normal document flow and safe-area padding.

- [ ] **Step 4: Implement JS animation ownership**

Use Web Animations or `requestAnimationFrame` transforms at a constant pixels-per-second rate. Measure the logical sequence with `ResizeObserver`, preserve normalized progress across resize, suspend while hidden, and dispose every observer/listener/animation. Never use `setInterval` for layout motion.

- [ ] **Step 5: Integrate the persistent Pause control**

The Sidebar control writes `ThemeStudioWorkbenchState.RunwayPaused`; the preview reflects `data-runway-paused`. Reduced motion always wins over requested resume.

- [ ] **Step 6: Build, run unit/source/browser tests, and commit**

```powershell
git add samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/Runway samples/Maliev.ShadcnBlazor.Showcase/wwwroot/js/theme-studio-runway.js samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css tests
git commit -m "feat(theme): add pausable counter-scrolling runway"
```

### Task 7: Unify the app bar and rebuild Theme Studio Sidebar/responsive scope

**Files:**
- Modify: `ThemeStudio.razor`, `ThemeStudioLayout.razor`, `ThemeStudioSidebar.razor`, `ThemeInspector.razor`, `PreviewToolbar.razor`, `DocumentationHeader.razor`, `DocumentationLayout.razor`, `ThemeStudioWorkbenchState.cs`, and `showcase.css`.
- Modify: Showcase `Program.cs` and project references for all icon companion catalogs.
- Extend: Theme Studio unit/browser tests.

**Interfaces:**
- Consumes: runway, presets, and icon catalogs from Tasks 2, 4, and 6.
- Produces: one universal app bar and preview-scoped Theme Studio surface.

- [ ] **Step 1: Add RED shared-header and Sidebar navigation tests**

Assert documentation and Theme Studio render the same `documentation-header` component. Theme Studio Sidebar actions must preserve `/theme` and current query/fragment; clicking Typography, Accessibility, or Import/export must not navigate to `/docs`.

- [ ] **Step 2: Add RED responsive device-choice tests**

At a desktop host width, expect Desktop/Tablet/Mobile. At tablet host width, expect Tablet/Mobile. At mobile host width, expect no device selector and a 390px/mobile state. Verify at runtime after resizing, not only initial markup.

- [ ] **Step 3: Replace the custom Theme Studio app bar**

Extend `DocumentationHeader` with explicit optional settings-trigger parameters rather than duplicating markup. The existing documentation mode remains byte/behavior compatible. Theme Studio consumes global `ShowcaseState` for mode/direction and maps those values into the preview provider.

- [ ] **Step 4: Recompose controls with package Sidebar components**

Use `ShadcnSidebarProvider`, `ShadcnSidebar`, header/content/group/footer/menu primitives, and the package trigger. Move device, typography, icon library, accessibility, pause, and import/export controls into the Sidebar. Remove the rendered `PreviewToolbar` from the Theme Studio route.

- [ ] **Step 5: Scope preview CSS variables**

Apply `ShadcnThemeCssWriter.WriteProperties(State.Document, ...)` and typography/icon attributes only to `.theme-preview-scope`. Browser tests compare computed app-bar font/icon geometry before and after changing preview typography/library and require equality within one pixel.

- [ ] **Step 6: Build and run focused browser accessibility tests**

Cover 320/390/768/1280px, LTR/RTL, Thai/English, dark/system, forced colors, reduced motion, 200% zoom, drawer focus trap/restore, and Axe serious/critical zero.

- [ ] **Step 7: Commit the unified workbench**

```powershell
git add samples/Maliev.ShadcnBlazor.Showcase/Pages/ThemeStudio.razor samples/Maliev.ShadcnBlazor.Showcase/Layout samples/Maliev.ShadcnBlazor.Showcase/Components/Theming samples/Maliev.ShadcnBlazor.Showcase/Theming samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css samples/Maliev.ShadcnBlazor.Showcase/Maliev.ShadcnBlazor.Showcase.csproj tests
git commit -m "feat(theme): unify the responsive Theme Studio workbench"
```

### Task 8: Complete documentation, package consumption, and reviewed evidence

**Files:**
- Modify: `README.md`, `docs/theming.md`, `docs/components.md`, `THIRD-PARTY-NOTICES.md`.
- Modify: package archive/public surface/clean-consumer/workflow tests.
- Modify: Theme Studio and affected CodeBlock/Message Scroller visual baselines only after review.

**Interfaces:**
- Consumes: all prior tasks.
- Produces: exact install guidance for the selected optional icon package and evidence for issue #227.

- [ ] **Step 1: Add RED documentation contracts**

Assert docs show installation and usage for each companion package, explain free-license boundaries, name the selected package in Theme Studio export output, document preview-only typography, curated Shuffle, pause behavior, and mobile static flow.

- [ ] **Step 2: Extend clean installed-consumer validation**

Pack core plus four companions to a unique local version. Create physical consumers that install core plus exactly one companion each, restore locked, render one icon, and build Release with 0 warnings/0 errors. Assert no project references or network icon fetches.

- [ ] **Step 3: Run the one-time Impeccable detector**

Run exactly once against the final changed Razor/CSS/JS targets:

```powershell
node C:\Users\natth\.agents\skills\impeccable\scripts\detect.mjs --json samples/Maliev.ShadcnBlazor.Showcase/Pages/ThemeStudio.razor samples/Maliev.ShadcnBlazor.Showcase/Components/Theming samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css src/Maliev.ShadcnBlazor/Components/Typography/ShadcnCodeBlock.razor
```

Fix task-owned mechanical findings in one batch; record inherited findings without broadening scope. Do not run the detector again.

- [ ] **Step 4: Run focused browser journeys**

Run Theme Studio, CodeBlock, documentation header, and Conversation Workflow filters. Exercise real clicks, focus, wheel, touch, keyboard, Shuffle, preset import/export, icon selection, and streaming.

- [ ] **Step 5: Capture and inspect visual proofs**

Generate only the declared Theme Studio desktop-light, tablet-dark-RTL, and mobile-forced-colors proofs plus causal CodeBlock/Message Scroller proofs. Inspect every changed PNG at original resolution, revert incidental rewrites, and rerun strict update-disabled visual tests.

- [ ] **Step 6: Run complete validation**

In order:

```powershell
dotnet restore Maliev.ShadcnBlazor.slnx --locked-mode -p:NuGetAudit=false
dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore -p:NuGetAudit=false
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --no-build
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --no-build
dotnet format Maliev.ShadcnBlazor.slnx --verify-no-changes --no-restore
powershell -NoProfile -File eng/Verify-PublicSurface.ps1 -Root .
```

Also run `node --check` for every changed JS module, package all five packages, inspect their archives, and build the four physical installed consumers.

- [ ] **Step 7: Run the Impeccable finish review and document the shipped system**

Pass the original request, approved comps, desktop/mobile final screenshots, direction contract, detector output, and `C:\Users\natth\.agents\skills\impeccable\reference\craft-floor.md` to the fresh finish reviewer. Apply material fixes within the allowed review rounds, obtain the verdict, then run the documenter to record `DESIGN.md` and the final surface brief.

- [ ] **Step 8: Commit the validated documentation and evidence**

```powershell
git add README.md docs THIRD-PARTY-NOTICES.md tests .github samples/Maliev.ShadcnBlazor.Showcase src
git commit -m "docs: document the curated Theme Studio and icon packages"
```

## Final issue handoff

Before requesting review, verify the branch is clean, every commit is independently buildable, issue #227 contains links to the approved spec and validation evidence, and no version, tag, GitHub release, package publication, push, PR, or deployment occurs without the owner's explicit authorization.
