# Theme Studio Workbench Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Turn Theme Studio into an accessible full-width workbench that can generate, inspect, export, import, load, and build-validate one canonical package-owned theme document while demonstrating every supported component in realistic responsive scenarios.

**Architecture:** `Maliev.ShadcnBlazor` owns a versioned `ShadcnThemeDocument`; the Showcase edits that document directly, and the same JSON is exported, loaded by consumers through a dependency-free BCL loader, and validated during builds by a packaged dependency-free MSBuild task. The workbench is split into six mergeable slices: shell, portable document, deterministic palette, typography catalog, 64-component scenario matrix, and consumer/build delivery. No source generator is introduced because issue #171 requires portable loading and actionable validation, not generated source.

**Tech Stack:** .NET 10, Blazor WebAssembly, Razor components, `System.Text.Json`, package-owned semantic CSS, bUnit/xUnit, Microsoft Playwright, Axe, MSBuild tasks, GitHub Actions.

**Spec:** [GitHub issue #171](https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/issues/171) and the existing [theming guide](../../theming.md).

## Global Constraints

- Work from exact `origin/main` commit `6b3643e5e40eaa8b2b43ae895f8970488e397826`; rebase each implementation branch before its final gate.
- Preserve the current `ShadcnTheme` schema-version-1 serializer and public API as a compatibility input; new portable exports use `ShadcnThemeDocument` schema version 2.
- Maintain exactly one canonical portable document. Do not retain `ThemeStudioGeneratorConfig` as a second persisted/exported schema after its migration path ships.
- Add no runtime, build-time, font-catalog, or generator NuGet dependency beyond the repository's current framework/package boundary.
- Package a compiled MSBuild task plus `buildTransitive` props/targets; do not add a source generator unless a separately approved acceptance criterion requires generated code.
- Generate palettes deterministically from algorithm version, seed, base color, and locks; materialize every token in the document so old documents remain reproducible if future algorithms change.
- Keep Google Fonts network credentials out of the browser and package. Refresh a checked-in catalog from the official API only through a maintainer script; the workbench must remain usable offline with bundled fonts and its checked-in snapshot.
- The scenario registry must contain at least `64 * 3 = 192` records and use real package components rather than look-alike HTML.
- Support light/dark, LTR/RTL, Thai/English locale, desktop/tablet/mobile, keyboard-only use, 200% zoom, reduced motion, and forced colors.
- Use logical CSS properties and semantic tokens. Never put customer data, private URLs, API keys, or machine-specific absolute paths in source, fixtures, exports, screenshots, or documentation.
- Follow test-driven development: write each focused regression first, observe the intended failure, implement only that slice, build with zero warnings/errors, run focused then affected suites, and commit only a green coherent slice.

## Acceptance-Criteria Traceability

| Issue #171 criterion | Owning slice | Required proof |
| --- | --- | --- |
| Full-width app bar with official responsive MALIEV branding | 1 | Shell unit contract, desktop/tablet/mobile browser geometry, visual proof |
| Collapsible color/typography/generation/import-export settings | 1 | Keyboard, focus restoration, landmarks, drawer state browser tests |
| Coherent reproducible palette with locks | 3 | Golden vectors, lock invariants, contrast diagnostics, export/import replay |
| Searchable Google Fonts catalog, Thai fallback, code and semantic roles | 4 | Offline catalog tests, keyboard search, font-load/fallback browser tests |
| Desktop/tablet/mobile preview controls | 1 | Exact 1280/768/390 viewport contracts and overflow assertions |
| Three realistic scenarios for every supported component | 5 | Registry count/coverage contract: 64 slugs, three unique scenarios each |
| Live theme/direction/locale/motion/accessibility changes | 1, 3, 4, 5 | Cross-state browser matrix and component computed-style assertions |
| Export reproduces current preview in a clean sample | 2, 6 | Canonical round-trip test and physical clean-consumer build/browser smoke |
| Lossless import/export | 2 | Byte-stable canonical serialization and semantic equality tests |
| Invalid themes produce actionable build errors/warnings | 6 | MSBuild fixtures asserting stable codes, paths, line/column, severity |
| Documentation and CI-validated end-to-end sample | 6 | Guide, sample app, package archive, repository and workflow contracts |
| Browser tests for shell, generation, fonts, scenarios, transfer | 1-6 | Focused Playwright classes plus full unfiltered browser suite |
| Keyboard/focus/landmarks/labels/contrast/forced/reduced accessibility | 1-6 | Axe, keyboard journeys, contrast tests, forced-colors and reduced-motion proof |

## Public API and Compatibility Contract

The following names and shapes are fixed for all six slices. XML documentation is required for every public member, and `tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt` changes only when the owning slice is reviewed.

```csharp
public sealed record ShadcnThemeDocument
{
    public const int CurrentSchemaVersion = 2;
    public int SchemaVersion { get; init; } = CurrentSchemaVersion;
    public required string Name { get; init; }
    public required ShadcnTheme Theme { get; init; }
    public required ShadcnThemeApplication Application { get; init; }
    public required ShadcnPaletteRecipe Palette { get; init; }
    public required ShadcnTypographyScale Typography { get; init; }
}

public sealed record ShadcnThemeApplication(
    string Preset,
    string Style,
    string BaseColor,
    string IconLibrary,
    string MenuAccent,
    string MenuColor,
    bool DefaultDarkMode,
    ShadcnDirection DefaultDirection,
    string DefaultLocale,
    ShadcnReducedMotionBehavior ReducedMotionBehavior);

public sealed record ShadcnPaletteRecipe(
    int AlgorithmVersion,
    ulong Seed,
    string BaseColor,
    IReadOnlyList<string> LockedTokens);

public sealed record ShadcnTypographyScale(
    ShadcnFontSelection Body,
    ShadcnFontSelection ThaiFallback,
    ShadcnFontSelection Code,
    IReadOnlyDictionary<ShadcnTypographyRole, ShadcnTypographyRoleStyle> Roles);

public sealed record ShadcnFontSelection(string Family, string Fallback, string? GoogleFontsId);
public sealed record ShadcnTypographyRoleStyle(int Weight, double Scale, double LineHeight, double LetterSpacingEm);
public enum ShadcnTypographyRole { Body, Heading1, Heading2, Heading3, Heading4To6, Label, Button, Caption, Code }

public static class ShadcnThemeDocumentSerializer
{
    public static string Serialize(ShadcnThemeDocument document);
    public static ShadcnThemeDocument Deserialize(string json);
    public static ShadcnThemeDocument Deserialize(ReadOnlySpan<byte> utf8Json);
}

public static class ShadcnThemeDocumentValidator
{
    public static ShadcnThemeValidationResult Validate(ShadcnThemeDocument document);
}

public static class ShadcnThemeDocumentLoader
{
    public static ShadcnThemeDocument Load(Stream stream);
    public static ValueTask<ShadcnThemeDocument> LoadAsync(Stream stream, CancellationToken cancellationToken = default);
}
```

`ShadcnThemeSerializer.Deserialize` continues accepting raw schema-version-0/1 `ShadcnTheme` JSON. The new document serializer accepts canonical schema version 2 and delegates older raw-theme and Showcase-generator migration to internal `ShadcnThemeDocumentMigrator` methods. Syntax and schema failures throw `JsonException` or `NotSupportedException` with stable code/path text; semantic failures come from `ShadcnThemeDocumentValidator` as stable diagnostics. Unknown future schema versions, unknown members, duplicate members, invalid enums, invalid token values, and contrast failures are never silently ignored. The validator also enforces that body/code selections equal the legacy `Theme.Metrics` font fields, so compatibility fields cannot drift from the richer typography model.

Runtime consumption is additive:

```csharp
await using var stream = await httpClient.GetStreamAsync("theme.json", cancellationToken);
var document = await ShadcnThemeDocumentLoader.LoadAsync(stream, cancellationToken);
builder.Services.AddMalievShadcn(options => options.Theme = document.Theme);
```

`ShadcnOptions.Theme` is nullable and `ShadcnThemeProvider.Theme` remains the higher-precedence per-provider override. Existing applications that only configure `FontFamily`, mode, direction, or toast timing behave exactly as before.

---

### Task 1: Full-width workbench shell, navigation, and live preview state

**Files:**
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Pages/ThemeStudio.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeStudioLayout.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/PreviewToolbar.razor`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeStudioAppBar.razor`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeStudioSidebar.razor`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioWorkbenchState.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioStateTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioWorkbenchContractTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeStudioBrowserTests.cs`
- Visual: `tests/Maliev.ShadcnBlazor.BrowserTests/ComponentCatalogVisualProofTests.cs`

**Interfaces:**
- Produces: `ThemeStudioWorkbenchState` with `SidebarOpen`, `ActiveSection`, `Viewport`, `Mode`, `Direction`, `Locale`, `ReducedMotion`, `HighContrastPreview`, `OpenSidebar()`, `CloseSidebar()`, and `SetViewport(ThemeStudioViewport)`.
- Produces: landmarks named `Theme Studio`, `Theme settings`, and `Theme preview`; the sidebar trigger uses `aria-controls="theme-studio-sidebar"` and restores focus when the modal drawer closes.
- Preserves: existing 1280, 768, and 390 CSS-pixel preview choices and current theme-history semantics.

- [ ] **Step 1: Write failing shell/state contracts**

Add tests that render `ThemeStudio.razor` and assert the official `/images/brand/MALIEV_BLACK.svg`, one full-width app bar, the three named landmarks, four sidebar section links (`colors`, `typography`, `generation`, `transfer`), 1280/768/390 viewport values, no placeholder star logo, and accessible names for every control. In `ThemeStudioStateTests`, assert all live preview dimensions are independent from the browser viewport and all state transitions raise exactly one change notification.

- [ ] **Step 2: Run the focused tests and observe the intended failure**

```powershell
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~ThemeStudioStateTests|FullyQualifiedName~ThemeStudioWorkbenchContractTests"
```

Expected: failures for the missing workbench state/app bar/sidebar and placeholder branding.

- [ ] **Step 3: Implement the shell with package components**

Create `ThemeStudioAppBar` with the official logo, document title, sidebar trigger, viewport controls, theme, direction, locale, reduced-motion, and high-contrast-preview controls. Create `ThemeStudioSidebar` as a persistent complementary landmark at desktop and an accessible modal drawer below 64rem; use existing package Button, Toggle, Select, Tabs, Scroll Area, Separator, and Tooltip components. Keep settings content mounted when collapsed so edits and validation state are not lost.

- [ ] **Step 4: Connect a single live preview state**

Move shell-only properties out of `ThemeStudioState` into `ThemeStudioWorkbenchState`. Cascade both states through `ThemeStudioLayout`; apply `dir`, `lang`, theme mode, reduced-motion data attribute, and preview width to the preview host rather than the entire docs page. Make controls controlled, deterministic, and source-independent; no checkbox may stand in for an interactive preview behavior.

- [ ] **Step 5: Add responsive, accessibility, and visual browser tests**

In `ThemeStudioBrowserTests`, cover persistent desktop settings, tablet/mobile drawer, Escape/backdrop close, focus restoration, keyboard section navigation, 200% zoom, zero document horizontal overflow, and exact preview widths. Add light LTR desktop, dark RTL tablet, and forced-colors mobile captures; assert reduced motion disables shell transition duration before capture.

- [ ] **Step 6: Build and validate slice 1**

```powershell
dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore -p:NuGetAudit=false
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~ThemeStudioStateTests|FullyQualifiedName~ThemeStudioWorkbenchContractTests"
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --no-build --filter FullyQualifiedName~ThemeStudioBrowserTests
```

Expected: zero build warnings/errors; focused unit and browser tests pass; Axe reports no serious/critical violations.

- [ ] **Step 7: Commit slice 1**

```powershell
git add samples/Maliev.ShadcnBlazor.Showcase/Pages/ThemeStudio.razor samples/Maliev.ShadcnBlazor.Showcase/Components/Theming samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioWorkbenchState.cs samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioStateTests.cs tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioWorkbenchContractTests.cs tests/Maliev.ShadcnBlazor.BrowserTests/ThemeStudioBrowserTests.cs tests/Maliev.ShadcnBlazor.BrowserTests/ComponentCatalogVisualProofTests.cs
git commit -m "feat(showcase): build the Theme Studio workbench shell"
```

### Task 2: Canonical package-owned document and lossless transfer

**Files:**
- Create: `src/Maliev.ShadcnBlazor/Theming/ShadcnThemeDocument.cs`
- Create: `src/Maliev.ShadcnBlazor/Theming/ShadcnThemeApplication.cs`
- Create: `src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteRecipe.cs`
- Create: `src/Maliev.ShadcnBlazor/Theming/ShadcnTypographyScale.cs`
- Create: `src/Maliev.ShadcnBlazor/Theming/ShadcnThemeDocumentSerializer.cs`
- Create: `src/Maliev.ShadcnBlazor/Theming/ShadcnThemeDocumentValidator.cs`
- Create: `src/Maliev.ShadcnBlazor/Theming/Internal/ShadcnThemeDocumentMigrator.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioState.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioStorage.cs`
- Delete after migration tests pass: `samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioGeneratorConfig.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Export/ThemeBundleBuilder.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Export/ThemeImportService.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Export/ThemeStudioCodeGenerator.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeImportDialog.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeExportDialog.razor`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Theming/ShadcnThemeDocumentTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioStateTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeBundleTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeImportExportBrowserTests.cs`
- API: `tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt`

**Interfaces:**
- Produces: the public document, application, palette, typography, font, role-style, role enum, serializer, and validator signatures declared in “Public API and Compatibility Contract”. The loader lands in Task 6.
- Produces: internal `ShadcnThemeDocumentMigrator.FromTheme(ShadcnTheme)` and `FromGeneratorConfigV1(JsonElement)`.
- Replaces: local-storage key `maliev.shadcn.theme-studio.v1` with `maliev.shadcn.theme-studio.document.v2`, deleting the old key only after the v2 write succeeds.

- [ ] **Step 1: Write failing canonical serialization and migration tests**

Cover deterministic property order and LF termination; serialize-deserialize-serialize byte equality; deep semantic equality; unknown/duplicate/future-member rejection; raw schema-0 and schema-1 theme migration; generator-config-v1 migration; mismatched duplicated font/radius fields preferring the nested theme with a warning; and the storage sequence read-v2, read-v1, validate, write-v2, remove-v1.

- [ ] **Step 2: Run the focused tests and observe missing types/migrations**

```powershell
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~ShadcnThemeDocumentTests|FullyQualifiedName~ThemeBundleTests|FullyQualifiedName~ThemeStudioStateTests"
```

Expected: compilation failures for the new package document and failing v1 storage/export assertions.

- [ ] **Step 3: Implement schema version 2 without changing schema version 1**

Keep `ShadcnTheme.CurrentSchemaVersion == 1`. Implement the new records with immutable collection snapshots, strict camel-case JSON, string enums, bounded input size, stable validation paths, and defensive copies. Make the document's nested `Theme` the only materialized visual token source; `Application`, `Palette`, and `Typography` describe reproducibility and consumer wiring without duplicating mutable color values.

- [ ] **Step 4: Migrate Theme Studio state, storage, import, and export**

Make `ThemeStudioState.Document` authoritative and derive `CurrentTheme` from `Document.Theme`. Replace generator-config export/import with canonical document JSON. The bundle's `theme.json`, generated C# theme, CSS, README, and Razor sample must all derive from the same captured document instance. Import is transactional: parse, migrate, validate, then replace state/history; a failed import must not mutate state or local storage.

- [ ] **Step 5: Prove transfer behavior in a browser**

Test download/upload, clipboard fallback, invalid JSON, future schema, legacy generator-config migration, and lossless state restoration after page reload. Assert imported theme, application defaults (mode, direction, locale, and motion), typography, and palette seed/locks match the exported snapshot.

- [ ] **Step 6: Build, validate public API, and run transfer suites**

```powershell
dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore -p:NuGetAudit=false
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~ShadcnThemeDocumentTests|FullyQualifiedName~ThemeBundleTests|FullyQualifiedName~ThemeStudioStateTests"
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --no-build --filter FullyQualifiedName~ThemeImportExportBrowserTests
pwsh -NoProfile -File eng/Verify-PublicSurface.ps1 -Root .
```

- [ ] **Step 7: Commit slice 2**

```powershell
git add src/Maliev.ShadcnBlazor/Theming samples/Maliev.ShadcnBlazor.Showcase/Theming samples/Maliev.ShadcnBlazor.Showcase/Export samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeImportDialog.razor samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeExportDialog.razor tests/Maliev.ShadcnBlazor.Tests/Theming/ShadcnThemeDocumentTests.cs tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioStateTests.cs tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeBundleTests.cs tests/Maliev.ShadcnBlazor.BrowserTests/ThemeImportExportBrowserTests.cs tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt
git commit -m "feat(theming): add a canonical portable theme document"
```

### Task 3: Deterministic accessible palette generator with locks

**Files:**
- Create: `src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteGenerator.cs`
- Create: `src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteGenerationResult.cs`
- Create: `src/Maliev.ShadcnBlazor/Theming/Internal/OklchColor.cs`
- Create: `src/Maliev.ShadcnBlazor/Theming/Internal/SplitMix64.cs`
- Modify: `src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteRecipe.cs`
- Modify: `src/Maliev.ShadcnBlazor/Theming/ShadcnThemeValidator.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeGeneratorOptions.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeColorGroup.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioState.cs`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Theming/ShadcnPaletteGeneratorTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Theming/ShadcnThemeDomainTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioStateTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeControlsBrowserTests.cs`
- API: `tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt`

**Interfaces:**
- Produces: `ShadcnPaletteGenerator.Generate(ShadcnTheme source, ShadcnPaletteRecipe recipe) : ShadcnPaletteGenerationResult`.
- Produces: `ShadcnPaletteGenerationResult(ShadcnTheme Theme, IReadOnlyList<ShadcnThemeValidationMessage> Errors, IReadOnlyList<ShadcnThemeValidationMessage> Warnings)` with `IsValid`.
- Consumes: all 32 existing light/dark semantic color tokens; shadows remain copied from the source theme.

- [ ] **Step 1: Write failing golden-vector and invariant tests**

Add fixed vectors for three seeds and four base colors. Assert identical inputs produce byte-identical documents; different seeds alter at least accent hue/chroma; all unlocked tokens are materialized; every locked token remains byte-identical; text pairs meet 4.5:1; large/control boundaries meet 3:1; light/dark ordering remains coherent; and impossible locked combinations return path-specific errors without mutating the source.

- [ ] **Step 2: Run focused tests and verify the generator is absent**

```powershell
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~ShadcnPaletteGeneratorTests|FullyQualifiedName~ShadcnThemeDomainTests"
```

- [ ] **Step 3: Implement deterministic OKLCH generation**

Use dependency-free OKLCH parsing/conversion and SplitMix64. `AlgorithmVersion = 1` fixes hue offsets, chroma clamps, tone steps, gamut reduction, and rounding. Derive background/foreground/card/popover, primary/secondary/muted/accent/destructive, borders/input/ring, chart-1..5, and sidebar variants for both schemes. Apply locks after derivation, rerun contrast validation, and return errors rather than silently changing locked values.

- [ ] **Step 4: Add seed, regenerate, randomize, and per-token lock controls**

Make seed input explicit; “Regenerate” preserves the seed, while “New seed” generates a cryptographically random `ulong` in the Showcase and records it in the document. Each token exposes a keyboard-operable lock toggle with its token name in the accessible label. One undo step must restore the complete pre-generation document.

- [ ] **Step 5: Add live and accessibility browser proof**

Assert regeneration changes computed preview variables immediately, locked tokens do not move, undo/redo are exact, import replays the palette, contrast failures are announced through a polite summary linked to the failing control, and forced-colors/reduced-motion modes do not hide status or focus.

- [ ] **Step 6: Build and validate slice 3**

```powershell
dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore -p:NuGetAudit=false
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~ShadcnPaletteGeneratorTests|FullyQualifiedName~ShadcnThemeDomainTests|FullyQualifiedName~ThemeStudioStateTests"
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --no-build --filter FullyQualifiedName~ThemeControlsBrowserTests
pwsh -NoProfile -File eng/Verify-PublicSurface.ps1 -Root .
```

- [ ] **Step 7: Commit slice 3**

```powershell
git add src/Maliev.ShadcnBlazor/Theming samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeGeneratorOptions.razor samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeColorGroup.razor samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioState.cs tests/Maliev.ShadcnBlazor.Tests/Theming tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioStateTests.cs tests/Maliev.ShadcnBlazor.BrowserTests/ThemeControlsBrowserTests.cs tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt
git commit -m "feat(theming): generate deterministic accessible palettes"
```

### Task 4: Searchable offline-first typography workbench

**Files:**
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Theming/Fonts/GoogleFontCatalogEntry.cs`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Theming/Fonts/GoogleFontCatalog.cs`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeTypographyEditor.razor`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/data/google-fonts-catalog.json`
- Create: `eng/Refresh-GoogleFontsCatalog.ps1`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Pages/ThemeStudio.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioState.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css`
- Modify: `docs/theming.md`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Showcase/GoogleFontCatalogTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioStateTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.RepositoryTests/PublicDocumentationTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeControlsBrowserTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeStudioBrowserTests.cs`

**Interfaces:**
- Produces: internal `GoogleFontCatalog.LoadAsync(HttpClient, CancellationToken)` and `Search(string query, string? subset, bool variableOnly)`.
- Consumes: `ShadcnTypographyScale` and its nine required role entries.
- Preserves: bundled Geist, Noto Sans Thai, and JetBrains Mono as offline defaults; remote font failure never blocks editing or export.

- [ ] **Step 1: Write failing catalog, role, and offline tests**

Assert the checked-in catalog has a source timestamp, upstream family identifier, display name, category, subsets, axes/weights, and CSS2 family query for every entry; contains a broad set of Latin and Thai-capable families; has no API key or remote CSS payload; searches case/diacritic-insensitively; and always injects the three bundled defaults. Assert all nine semantic roles validate weight 100..900, scale, line height, and letter spacing bounds, and that body/code selections stay equal to `Theme.Metrics.FontFamily` and `Theme.Metrics.MonospaceFontFamily` after every edit and migration.

- [ ] **Step 2: Run the focused tests and verify missing catalog/editor failures**

```powershell
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~GoogleFontCatalogTests|FullyQualifiedName~ThemeStudioStateTests"
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --filter FullyQualifiedName~PublicDocumentationTests
```

- [ ] **Step 3: Implement the maintainer-only catalog refresh path**

`eng/Refresh-GoogleFontsCatalog.ps1` reads `GOOGLE_FONTS_API_KEY` from the process environment, calls the official Web Fonts Developer API, projects only public font metadata, sorts deterministically, validates required fields, and writes the checked-in snapshot. It must never print the key or run from the client. Document the source (`developers.google.com/fonts/docs/developer_api`), CSS2 use (`developers.google.com/fonts/docs/css2`), refresh command, offline behavior, and licensing review boundary.

- [ ] **Step 4: Implement accessible font search and semantic role editing**

Use package Combobox/Select/Input/Slider components. Provide body, Thai fallback, and code-family pickers plus role-specific weight, scale, line-height, and letter-spacing controls. Search must support keyboard navigation, announce result counts, virtualize only if semantics remain intact, and show bundled/remote/subset badges. Build the remote CSS2 URL from checked-in metadata with `display=swap`; never interpolate arbitrary raw URLs.

- [ ] **Step 5: Apply font loading without layout deadlock**

Keep bundled fallbacks active until `document.fonts.load` resolves or a bounded timeout elapses. Expose loading, loaded, fallback, and failed states in a polite status. Persist only family identifiers and typography values in the canonical document; do not persist network responses. Recompute the preview and export from the same state.

- [ ] **Step 6: Add browser and documentation proof**

Cover keyboard search, Thai subset filtering, offline fallback via aborted font requests, body/heading/button/code computed families, role slider source synchronization, dark/RTL, reduced motion, forced colors, and no cumulative shell shift during load. Validate external documentation URLs with the repository's link contract.

- [ ] **Step 7: Build and validate slice 4**

```powershell
dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore -p:NuGetAudit=false
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~GoogleFontCatalogTests|FullyQualifiedName~ThemeStudioStateTests"
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --no-build --filter FullyQualifiedName~PublicDocumentationTests
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --no-build --filter "FullyQualifiedName~ThemeControlsBrowserTests|FullyQualifiedName~ThemeStudioBrowserTests"
```

- [ ] **Step 8: Commit slice 4**

```powershell
git add samples/Maliev.ShadcnBlazor.Showcase/Theming/Fonts samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeTypographyEditor.razor samples/Maliev.ShadcnBlazor.Showcase/wwwroot/data/google-fonts-catalog.json samples/Maliev.ShadcnBlazor.Showcase/Pages/ThemeStudio.razor samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioState.cs samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css eng/Refresh-GoogleFontsCatalog.ps1 docs/theming.md tests/Maliev.ShadcnBlazor.Tests/Showcase/GoogleFontCatalogTests.cs tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioStateTests.cs tests/Maliev.ShadcnBlazor.RepositoryTests/PublicDocumentationTests.cs tests/Maliev.ShadcnBlazor.BrowserTests/ThemeControlsBrowserTests.cs tests/Maliev.ShadcnBlazor.BrowserTests/ThemeStudioBrowserTests.cs
git commit -m "feat(showcase): add offline-first typography controls"
```

### Task 5: Data-driven 64-component scenario matrix

**Files:**
- Create: `samples/Maliev.ShadcnBlazor.Showcase/ThemeScenarios/ThemeScenarioDefinition.cs`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/ThemeScenarios/IThemeScenarioRegistry.cs`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/ThemeScenarios/ThemeScenarioRegistry.cs`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/ThemeScenarios/ThemeScenarioCatalog.json`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeScenarioBrowser.razor`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeScenarioHost.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Pages/ThemeStudio.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Program.cs`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeScenarioCatalogTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeScenarioRenderTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeScenarioBrowserTests.cs`
- Visual: `tests/Maliev.ShadcnBlazor.BrowserTests/ComponentCatalogVisualProofTests.cs`

**Interfaces:**
- Produces: `ThemeScenarioDefinition(string Id, string ComponentSlug, string Title, string Description, IReadOnlyList<string> Tags, RenderFragment Preview)`.
- Produces: `IThemeScenarioRegistry.All`, `ForComponent(string slug)`, and `Find(string query)`.
- Consumes: all 64 slugs from `Documentation/ComponentDocumentationCatalog.json`; each slug owns exactly three stable scenario IDs: `<slug>-default`, `<slug>-stress`, and `<slug>-accessible`.

- [ ] **Step 1: Write failing completeness and authenticity contracts**

Assert catalog/documentation slug sets are equal; exactly 192 unique scenario records exist; every component has default, stress, and accessible records; every record has a non-empty realistic description and package-component factory; no scenario renders placeholder text alone; every control has an accessible name; and all scenario IDs remain stable/sorted. Add render smoke tests for each record in light/dark and LTR/RTL.

- [ ] **Step 2: Run focused tests and observe the empty registry failure**

```powershell
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~ThemeScenarioCatalogTests|FullyQualifiedName~ThemeScenarioRenderTests"
```

- [ ] **Step 3: Build the registry by existing component family**

Reuse package components and neutral fictional Thai/English product content. “Default” shows the normal workflow, “stress” covers long text/loading/empty/error/dense content as appropriate, and “accessible” exposes keyboard, disabled/read-only/invalid, live-region, or focus behavior appropriate to that component. Reuse existing dossier factories only through shared helper methods; do not couple stateful Theme Studio scenarios to dossier control state.

- [ ] **Step 4: Implement searchable scenario navigation and preview**

Add component/category search, scenario tabs, previous/next navigation, and a direct link containing component/scenario query values. Mount one active scenario at a time; dispose timers, streams, JS handles, and event subscriptions on navigation. Preserve the active scenario while theme, viewport, locale, direction, and accessibility settings change.

- [ ] **Step 5: Add parameterized browser coverage**

For all 192 records, assert the selected package component renders without console/page errors at desktop light LTR and mobile dark RTL. For a representative component from every catalog category, exercise keyboard interaction, reduced motion, forced colors, Thai locale, 200% zoom, and theme mutation. Visual proof captures one matrix page per category plus all three states for high-risk overlay, form, data, and conversation components.

- [ ] **Step 6: Build and validate slice 5**

```powershell
dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore -p:NuGetAudit=false
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~ThemeScenarioCatalogTests|FullyQualifiedName~ThemeScenarioRenderTests"
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --no-build --filter FullyQualifiedName~ThemeScenarioBrowserTests
```

- [ ] **Step 7: Commit slice 5**

```powershell
git add samples/Maliev.ShadcnBlazor.Showcase/ThemeScenarios samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeScenarioBrowser.razor samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeScenarioHost.razor samples/Maliev.ShadcnBlazor.Showcase/Pages/ThemeStudio.razor samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css samples/Maliev.ShadcnBlazor.Showcase/Program.cs tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeScenarioCatalogTests.cs tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeScenarioRenderTests.cs tests/Maliev.ShadcnBlazor.BrowserTests/ThemeScenarioBrowserTests.cs tests/Maliev.ShadcnBlazor.BrowserTests/ComponentCatalogVisualProofTests.cs
git commit -m "feat(showcase): cover every component with theme scenarios"
```

### Task 6: Dependency-free consumer loader, buildTransitive validation, sample, and release gates

**Files:**
- Create: `src/Maliev.ShadcnBlazor/Theming/ShadcnThemeDocumentLoader.cs`
- Modify: `src/Maliev.ShadcnBlazor/Theming/ShadcnOptions.cs`
- Modify: `src/Maliev.ShadcnBlazor/ServiceCollectionExtensions.cs`
- Modify: `src/Maliev.ShadcnBlazor/Components/ShadcnThemeProvider.razor`
- Create: `src/Maliev.ShadcnBlazor.Build/Maliev.ShadcnBlazor.Build.csproj`
- Create: `src/Maliev.ShadcnBlazor.Build/ValidateShadcnThemeTask.cs`
- Create: `src/Maliev.ShadcnBlazor.Build/ThemeDocumentBuildValidator.cs`
- Create: `src/Maliev.ShadcnBlazor/buildTransitive/Maliev.ShadcnBlazor.props`
- Create: `src/Maliev.ShadcnBlazor/buildTransitive/Maliev.ShadcnBlazor.targets`
- Modify: `src/Maliev.ShadcnBlazor/Maliev.ShadcnBlazor.csproj`
- Modify: `Maliev.ShadcnBlazor.slnx`
- Create: `samples/Maliev.ShadcnBlazor.ThemeConsumer/Maliev.ShadcnBlazor.ThemeConsumer.csproj`
- Create: `samples/Maliev.ShadcnBlazor.ThemeConsumer/Program.cs`
- Create: `samples/Maliev.ShadcnBlazor.ThemeConsumer/App.razor`
- Create: `samples/Maliev.ShadcnBlazor.ThemeConsumer/Layout/MainLayout.razor`
- Create: `samples/Maliev.ShadcnBlazor.ThemeConsumer/Pages/Home.razor`
- Create: `samples/Maliev.ShadcnBlazor.ThemeConsumer/wwwroot/theme.json`
- Create: `tests/Maliev.ShadcnBlazor.Tests/Theming/ShadcnThemeDocumentLoaderTests.cs`
- Create: `tests/Maliev.ShadcnBlazor.RepositoryTests/ThemeBuildValidationTests.cs`
- Create: `tests/Maliev.ShadcnBlazor.RepositoryTests/Fixtures/Themes/valid-theme.json`
- Create: `tests/Maliev.ShadcnBlazor.RepositoryTests/Fixtures/Themes/invalid-token.json`
- Create: `tests/Maliev.ShadcnBlazor.RepositoryTests/Fixtures/Themes/contrast-warning.json`
- Modify: `tests/Maliev.ShadcnBlazor.RepositoryTests/PackageArchiveTests.cs`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Components/ShadcnThemeProviderTests.cs`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt`
- Modify: `docs/theming.md`
- Modify: `README.md`
- Modify: `.github/workflows/ci.yml`
- Test: `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeStudioBrowserTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeImportExportBrowserTests.cs`
- Test: `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeScenarioBrowserTests.cs`

**Interfaces:**
- Produces: `ShadcnThemeDocumentLoader` declared above, implemented only with BCL stream/JSON APIs and bounded reads.
- Produces: nullable `ShadcnOptions.Theme`; provider precedence is parameter `Theme`, configured `ShadcnOptions.Theme`, then current factory defaults.
- Produces: MSBuild item `@(MalievShadcnTheme)` and properties `MalievShadcnValidateThemes` (default `true`) and `MalievShadcnThemeWarningsAsErrors` (default `false`).
- Produces: target `ValidateMalievShadcnThemes` before `CoreCompile` and diagnostic family `MSHCN001` schema/JSON, `MSHCN002` required/token, `MSHCN003` contrast error, `MSHCN101` contrast warning.

- [ ] **Step 1: Write failing loader, provider, archive, and build-diagnostic tests**

Test sync/async stream loading, cancellation, disposed/non-readable stream, maximum document size, invalid UTF-8, canonical validation, and no stream ownership transfer. Test provider precedence and unchanged legacy defaults. Package tests must assert `tools/net10.0/Maliev.ShadcnBlazor.Build.dll`, `buildTransitive/Maliev.ShadcnBlazor.props`, and `.targets` exist and that the consumer dependency group has no new package dependency. Physical-project tests must build the valid fixture and fail invalid fixtures with the exact code, normalized JSON path, source file, line, and column.

- [ ] **Step 2: Run focused tests and observe missing loader/task failures**

```powershell
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~ShadcnThemeDocumentLoaderTests|FullyQualifiedName~ShadcnThemeProviderTests"
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --filter "FullyQualifiedName~ThemeBuildValidationTests|FullyQualifiedName~PackageArchiveTests"
```

- [ ] **Step 3: Implement the dependency-free runtime loader and provider fallback**

Use `System.Text.Json` and pooled BCL buffers only. Bound input at 1 MiB, honor cancellation between reads, leave caller streams open, and return the strict canonical serializer's errors. Add `ShadcnOptions.Theme`; update `ShadcnThemeProvider` so an explicit parameter wins, configured theme is next, and current factory defaults remain last.

- [ ] **Step 4: Implement the packaged MSBuild task without a source generator**

Compile `Maliev.ShadcnBlazor.Build` for net10.0 using SDK-provided MSBuild framework/utilities references marked private. Link or share the dependency-free JSON validation core; do not reference the Razor Class Library at task runtime. The props include `wwwroot/theme.json` only when present unless the consumer supplies explicit `@(MalievShadcnTheme)` items. The target runs before `CoreCompile`, validates offline, maps UTF-8 token offsets to line/column, logs stable codes, and respects warning escalation. Do not generate C# or CSS during the build.

- [ ] **Step 5: Pack and test clean consumers**

Pack to a temporary artifact directory, inspect the `.nupkg`, create a fresh physical Blazor app outside the repository, add only the local `Maliev.ShadcnBlazor` package, copy a valid exported `theme.json`, restore from a local feed plus NuGet.org, and build. Repeat with each invalid fixture and assert the exact diagnostic. Build the checked-in consumer sample and launch it for the final browser smoke.

```powershell
dotnet pack src/Maliev.ShadcnBlazor/Maliev.ShadcnBlazor.csproj -c Release --no-build -o artifacts/theme-studio-package
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --no-build --filter "FullyQualifiedName~ThemeBuildValidationTests|FullyQualifiedName~PackageArchiveTests"
```

- [ ] **Step 6: Document and CI-enforce the end-to-end path**

Update the theming guide and README with export, runtime load, explicit MSBuild item, disabling validation, warning escalation, migration, offline font behavior, and failure examples. Add CI steps that pack once, run the physical clean-consumer matrix against that artifact, build the checked-in sample, and retain no generated consumer directories.

- [ ] **Step 7: Run the final package, repository, browser, accessibility, and visual gates**

```powershell
dotnet restore Maliev.ShadcnBlazor.slnx --locked-mode -p:NuGetAudit=false
dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore -p:NuGetAudit=false
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --no-build
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --no-build
dotnet format Maliev.ShadcnBlazor.slnx --verify-no-changes --no-restore
pwsh -NoProfile -File eng/Verify-PublicSurface.ps1 -Root .
git diff --check
```

Run the opt-in full catalog visual proof once, inspect every changed Theme Studio image at original resolution, then rerun strict comparison without the update variable. Expected: zero build warnings/errors, all package/repository/browser tests pass, no serious/critical Axe violations, no unexpected baseline changes, and no conflict markers.

- [ ] **Step 8: Commit slice 6**

```powershell
git add src/Maliev.ShadcnBlazor src/Maliev.ShadcnBlazor.Build Maliev.ShadcnBlazor.slnx samples/Maliev.ShadcnBlazor.ThemeConsumer tests/Maliev.ShadcnBlazor.Tests tests/Maliev.ShadcnBlazor.RepositoryTests tests/Maliev.ShadcnBlazor.BrowserTests docs/theming.md README.md .github/workflows/ci.yml
git commit -m "feat(theming): validate portable themes in consumer builds"
```

## Branch, Commit, and PR Sequence

Implement each slice on a branch cut from the preceding merged slice; do not stack all six commits in one review. Each PR must include only its listed production/test/docs files and must be rebased onto the then-current `origin/main` before browser/visual validation.

1. `feat/theme-studio-shell` — commit `feat(showcase): build the Theme Studio workbench shell`.
2. `feat/theme-document-v2` — commit `feat(theming): add a canonical portable theme document`; includes public API snapshot and migrations.
3. `feat/theme-palette-generator` — commit `feat(theming): generate deterministic accessible palettes`; depends on document v2.
4. `feat/theme-typography-catalog` — commit `feat(showcase): add offline-first typography controls`; depends on document v2 roles.
5. `feat/theme-scenario-matrix` — commit `feat(showcase): cover every component with theme scenarios`; rebase after component-affecting PRs to keep all 64 factories current.
6. `feat/theme-consumer-validation` — commit `feat(theming): validate portable themes in consumer builds`; lands last because it locks package/archive/CI contracts.

For every PR: run the Impeccable detector once on changed UI targets, review applicable findings, obtain serialized Playwright-lane ownership, run only focused browser/visual proof during development, then run the full gates listed in Task 6 on the final stacked result. Never update a baseline to hide a functional, accessibility, responsive, or contrast regression.

## Final Self-Review Checklist

- [ ] All 13 issue acceptance criteria map to an owning slice and automated proof.
- [ ] `ShadcnThemeDocument` is the only exported/persisted portable document after migration.
- [ ] Existing raw `ShadcnTheme` schema-version-0/1 consumers continue to deserialize.
- [ ] Runtime loading and build validation add no consumer dependency and make no network request.
- [ ] The package contains a compiled task and buildTransitive wiring, not a source generator.
- [ ] Palette algorithm version, seed, locks, materialized tokens, and diagnostics reproduce exactly after import.
- [ ] Google Fonts search works from the checked-in snapshot and bundled defaults when offline.
- [ ] Registry coverage is exactly 64 component slugs with at least three realistic scenarios each.
- [ ] Desktop/tablet/mobile, dark, RTL, Thai, reduced motion, forced colors, keyboard, focus, labels, contrast, and 200% zoom are tested.
- [ ] Exported JSON builds and renders in a physical clean consumer using the packed artifact.
- [ ] Public API, package archive, documentation, CI, format, diff, and visual evidence gates are green before merge.
