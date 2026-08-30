# Theme Studio Palette Workbench Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a Coolors-style, deterministic five-anchor palette workbench to Theme Studio while preserving algorithm-v1 output, portable theme documents, and accessible live preview behavior.

**Architecture:** The Razor Class Library owns recipe versioning, immutable palette anchors, harmony generation, semantic mapping, validation, serialization, and migration. The Showcase owns palette editing state, localized presentation, responsive workbench composition, focus behavior, persistence orchestration, and browser verification; exported documents remain fully materialized so consumers do not execute the generator.

**Tech Stack:** .NET 10, C# records and System.Text.Json, Blazor/Razor components, Shadcn Blazor primitives, bUnit, xUnit, Playwright, CSS logical properties, JavaScript modules.

**Spec:** `docs/superpowers/specs/2026-08-29-theme-studio-palette-workbench-design.md`

## Global Constraints

- Preserve the existing four-argument `ShadcnPaletteRecipe` constructor and algorithm-v1 byte output.
- Do not rewrite or upgrade a valid v1 palette recipe until the first swatch edit, harmony change, or palette generation.
- Keep five anchors: Brand, Support, Highlight, Data A, and Data B.
- Support Free, Analogous, Complementary, and Triadic harmony modes.
- Keep destructive colors in the dedicated red semantic family.
- Semantic-token locks override generated semantic mapping; anchor locks protect exact anchor values during regeneration.
- A failed candidate must leave the last valid preview, document, undo history, and persisted value unchanged.
- Persist one history entry and one storage write for a coalesced picker gesture.
- Provide complete English and Thai workbench copy without translating identifiers, color values, or exported code.
- Keep the live preview usable beside the workbench on wide desktop; use a focus-contained full-height sheet at constrained widths.
- Respect reduced motion, forced colors, LTR, RTL, light mode, dark mode, keyboard input, and native color-input behavior.
- Do not add hosted services, Coolors integration, accounts, cloud synchronization, image extraction, AI generation, or an unbounded palette library.
- Do not include the separately approved code-block selector or overlay-use-case changes in these commits; they require their own implementation plan and validation boundary.
- Do not push, deploy, publish a package, or release a version without separate user authorization.

## File Structure

### Reusable library

- Create `src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteAnchorRole.cs` for the five stable anchor identities.
- Create `src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteHarmony.cs` for the four public harmony choices.
- Create `src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteAnchors.cs` for an immutable, validated five-color snapshot and role lookup.
- Create `src/Maliev.ShadcnBlazor/Theming/Internal/ShadcnPaletteColorParser.cs` for strict hex/OKLCH normalization into the existing `OklchColor` model.
- Create `src/Maliev.ShadcnBlazor/Theming/Internal/ShadcnPaletteHarmonyGenerator.cs` for deterministic unlocked-anchor generation.
- Create `src/Maliev.ShadcnBlazor/Theming/Internal/ShadcnPaletteSemanticMapper.cs` for light/dark token derivation and fixed destructive semantics.
- Modify `src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteRecipe.cs` to expose optional v2 fields without changing serialized v1 documents.
- Modify `src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteGenerator.cs` to dispatch v1 and v2 explicitly.
- Modify `src/Maliev.ShadcnBlazor/Theming/ShadcnThemeDocumentValidator.cs` and `src/Maliev.ShadcnBlazor/Schemas/shadcn-theme-document-v2.schema.json` to validate the version-dependent recipe shape.
- Modify `tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt` only through the reviewed snapshot update command.

### Theme Studio

- Create `samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioPaletteCopy.cs` for complete English/Thai labels and announcements.
- Create `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemePaletteSummary.razor` for the compact sidebar strip and Customize action.
- Create `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemePaletteWorkbench.razor` for the single responsive editor DOM.
- Create `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemePaletteAnchorEditor.razor` for one swatch row with picker, text value, copy, and lock.
- Modify `samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioState.cs` for v1 projection, v2 mutation, coalesced history, and transactional candidate application.
- Modify `samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioWorkbenchState.cs` to own workbench open state separately from the settings sidebar.
- Modify `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeInspector.razor` to mount the compact summary.
- Modify `samples/Maliev.ShadcnBlazor.Showcase/Pages/ThemeStudio.razor` to place the editor between sidebar and preview and coordinate persistence.
- Modify `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/js/theme-studio.js` for constrained-viewport focus containment and focus restoration.
- Modify `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css` for three-column desktop layout, responsive sheet layout, swatches, forced colors, and reduced motion.

### Tests

- Modify `tests/Maliev.ShadcnBlazor.Tests/Theming/ShadcnPaletteGeneratorTests.cs` for version dispatch, harmonies, anchors, locks, semantic mapping, contrast, and determinism.
- Modify `tests/Maliev.ShadcnBlazor.Tests/Theming/ShadcnThemeDocumentTests.cs` for JSON round trips, v1 byte preservation, v2 validation, defensive snapshots, and unsupported versions.
- Modify `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioStateTests.cs` for mutation, upgrade timing, undo/redo, persistence readiness, copy completeness, and component behavior.
- Modify `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioWorkbenchContractTests.cs` for required landmarks, hooks, classes, and no duplicate editor DOM.
- Modify `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeStudioBrowserTests.cs` for desktop, mobile, focus, keyboard, localization, overflow, reduced-motion, and exported-value behavior.
- Modify `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeImportExportBrowserTests.cs` for v1 preservation and v2 exact-value round trips.

---

### Task 1: Freeze version-one behavior and add the version-two recipe contract

**Files:**
- Create: `src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteAnchorRole.cs`
- Create: `src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteHarmony.cs`
- Create: `src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteAnchors.cs`
- Modify: `src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteRecipe.cs:5-40`
- Modify: `src/Maliev.ShadcnBlazor/Theming/ShadcnThemeDocumentValidator.cs:61-87`
- Modify: `src/Maliev.ShadcnBlazor/Schemas/shadcn-theme-document-v2.schema.json:31-42`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Theming/ShadcnThemeDocumentTests.cs`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Theming/ShadcnPaletteGeneratorTests.cs:8-65`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt`

**Interfaces:**
- Consumes: existing `ShadcnPaletteRecipe(int algorithmVersion, ulong seed, string baseColor, IReadOnlyList<string> lockedTokens)` and canonical document serializer.
- Produces: `ShadcnPaletteAnchorRole`, `ShadcnPaletteHarmony`, `ShadcnPaletteAnchors`, `ShadcnPaletteRecipe.CreateV2(...)`, and `ShadcnPaletteRecipe.IsVersion2`.

- [ ] **Step 1: Add failing contract and compatibility tests**

Add focused tests with these exact assertions:

```csharp
[Fact]
public void VersionOneRecipeSerializationRemainsByteIdentical()
{
    var recipe = new ShadcnPaletteRecipe(1, 42, "neutral", ["light.primary"]);
    var document = CreateDocument() with { Palette = recipe };

    var json = ShadcnThemeDocumentSerializer.Serialize(document);

    Assert.DoesNotContain("\"anchors\"", json, StringComparison.Ordinal);
    Assert.DoesNotContain("\"harmony\"", json, StringComparison.Ordinal);
    Assert.DoesNotContain("\"lockedAnchors\"", json, StringComparison.Ordinal);
    Assert.Equal(json, ShadcnThemeDocumentSerializer.Serialize(
        ShadcnThemeDocumentSerializer.Deserialize(json)));
}

[Fact]
public void VersionTwoRecipeTakesDefensiveAnchorLockSnapshotAndRoundTrips()
{
    var locks = new[] { ShadcnPaletteAnchorRole.Brand };
    var anchors = new ShadcnPaletteAnchors("#2563eb", "#14b8a6", "#f59e0b", "#8b5cf6", "#ec4899");
    var recipe = ShadcnPaletteRecipe.CreateV2(42, "neutral", [], anchors,
        ShadcnPaletteHarmony.Triadic, locks);
    locks[0] = ShadcnPaletteAnchorRole.DataB;

    var restored = ShadcnThemeDocumentSerializer.Deserialize(
        ShadcnThemeDocumentSerializer.Serialize(CreateDocument() with { Palette = recipe })).Palette;

    Assert.Equal(2, restored.AlgorithmVersion);
    Assert.Equal(anchors, restored.Anchors);
    Assert.Equal(ShadcnPaletteHarmony.Triadic, restored.Harmony);
    Assert.Equal([ShadcnPaletteAnchorRole.Brand], restored.LockedAnchors);
}
```

Also extend the existing algorithm-version test to assert materialized `0`, legacy deterministic `1`, and current deterministic `2`.

- [ ] **Step 2: Run the focused tests and verify the expected failure**

Run:

```powershell
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~ShadcnThemeDocumentTests|FullyQualifiedName~ShadcnPaletteGeneratorTests"
```

Expected: FAIL because the anchor types, v2 factory, and version-two properties do not exist.

- [ ] **Step 3: Add the public v2 types and a serialization-compatible recipe**

Create the enums exactly as stable public wire values:

```csharp
namespace Maliev.ShadcnBlazor.Theming;

public enum ShadcnPaletteAnchorRole { Brand, Support, Highlight, DataA, DataB }
public enum ShadcnPaletteHarmony { Free, Analogous, Complementary, Triadic }
```

Create `ShadcnPaletteAnchors` as a sealed record with a five-argument constructor, non-null string properties, and:

```csharp
public string Get(ShadcnPaletteAnchorRole role) => role switch
{
    ShadcnPaletteAnchorRole.Brand => Brand,
    ShadcnPaletteAnchorRole.Support => Support,
    ShadcnPaletteAnchorRole.Highlight => Highlight,
    ShadcnPaletteAnchorRole.DataA => DataA,
    ShadcnPaletteAnchorRole.DataB => DataB,
    _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown palette anchor role.")
};

public ShadcnPaletteAnchors Set(ShadcnPaletteAnchorRole role, string value) => role switch
{
    ShadcnPaletteAnchorRole.Brand => this with { Brand = value },
    ShadcnPaletteAnchorRole.Support => this with { Support = value },
    ShadcnPaletteAnchorRole.Highlight => this with { Highlight = value },
    ShadcnPaletteAnchorRole.DataA => this with { DataA = value },
    ShadcnPaletteAnchorRole.DataB => this with { DataB = value },
    _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown palette anchor role.")
};
```

Update `ShadcnPaletteRecipe` so the existing constructor delegates to a JSON constructor with null v2 members. Mark nullable `Anchors`, `Harmony`, and `LockedAnchors` with `JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)` so v1 JSON does not gain members. For v2, `LockedAnchors` must be a unique, sorted, defensive read-only snapshot. Add:

```csharp
public const int LegacyAlgorithmVersion = 1;
public const int CurrentAlgorithmVersion = 2;
public bool IsVersion2 => AlgorithmVersion == CurrentAlgorithmVersion;

public static ShadcnPaletteRecipe CreateV2(
    ulong seed,
    string baseColor,
    IReadOnlyList<string> lockedTokens,
    ShadcnPaletteAnchors anchors,
    ShadcnPaletteHarmony harmony,
    IEnumerable<ShadcnPaletteAnchorRole> lockedAnchors)
```

The factory must reject undefined roles, remove duplicates, sort by enum value, and defensively snapshot both lock collections.

- [ ] **Step 4: Make document validation and JSON Schema version-dependent**

Keep algorithm `0` valid only for materialized documents, allow `1` and `2`, and require all v2 fields only when `algorithmVersion == 2`. Add exact errors:

```csharp
new("required-palette-anchors", "palette.anchors", "Palette anchors are required for algorithm version 2.")
new("required-palette-harmony", "palette.harmony", "Palette harmony is required for algorithm version 2.")
new("invalid-locked-anchor", "palette.lockedAnchors", "Locked anchors must be unique supported roles.")
new("unexpected-palette-v2-field", "palette", "Version-two palette fields are not allowed on materialized or version-one recipes.")
```

In the packaged JSON Schema, allow only algorithm versions `0`, `1`, and `2`. Use `allOf` with an `if` on `algorithmVersion: { "const": 2 }`, then require `anchors`, `harmony`, and `lockedAnchors`; define five required string anchor properties and enum values `free`, `analogous`, `complementary`, and `triadic`. The `else` branch must reject those three v2-only members.

- [ ] **Step 5: Refresh and review the public API snapshot**

Run:

```powershell
$env:SHADCN_UPDATE_PUBLIC_API='1'
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter FullyQualifiedName~PublicApiSnapshotTests
Remove-Item Env:SHADCN_UPDATE_PUBLIC_API
git diff -- tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt
```

Expected: the diff contains only the three new public types and the reviewed new `ShadcnPaletteRecipe` members.

- [ ] **Step 6: Run focused contract tests and commit**

Run:

```powershell
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~ShadcnThemeDocumentTests|FullyQualifiedName~PublicApiSnapshotTests|FullyQualifiedName~PackageContractTests"
pwsh -NoProfile -File eng/Verify-PublicSurface.ps1 -Root .
git diff --check
```

Expected: all selected tests pass, public surface verification succeeds, and the diff check is empty.

Commit:

```powershell
git add src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteAnchorRole.cs src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteHarmony.cs src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteAnchors.cs src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteRecipe.cs src/Maliev.ShadcnBlazor/Theming/ShadcnThemeDocumentValidator.cs src/Maliev.ShadcnBlazor/Schemas/shadcn-theme-document-v2.schema.json tests/Maliev.ShadcnBlazor.Tests/Theming/ShadcnThemeDocumentTests.cs tests/Maliev.ShadcnBlazor.Tests/Theming/ShadcnPaletteGeneratorTests.cs tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt
git commit -m "Add portable palette recipe v2"
```

### Task 2: Implement deterministic harmony generation and semantic mapping

**Files:**
- Create: `src/Maliev.ShadcnBlazor/Theming/Internal/ShadcnPaletteColorParser.cs`
- Create: `src/Maliev.ShadcnBlazor/Theming/Internal/ShadcnPaletteHarmonyGenerator.cs`
- Create: `src/Maliev.ShadcnBlazor/Theming/Internal/ShadcnPaletteSemanticMapper.cs`
- Modify: `src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteGenerator.cs:8-196`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Theming/ShadcnPaletteGeneratorTests.cs`

**Interfaces:**
- Consumes: `ShadcnPaletteRecipe.IsVersion2`, `Anchors`, `Harmony`, `LockedAnchors`, existing `SplitMix64`, `OklchColor`, token catalog, validator, and contrast repair.
- Produces: unchanged public `ShadcnPaletteGenerator.Generate(ShadcnTheme, ShadcnPaletteRecipe)` with explicit v1/v2 dispatch and deterministic materialized output.

- [ ] **Step 1: Add failing v2 behavior tests**

Add theories covering all harmonies and these invariants:

```csharp
[Theory]
[InlineData(ShadcnPaletteHarmony.Free)]
[InlineData(ShadcnPaletteHarmony.Analogous)]
[InlineData(ShadcnPaletteHarmony.Complementary)]
[InlineData(ShadcnPaletteHarmony.Triadic)]
public void VersionTwoIsDeterministicAndMapsAllFiveAnchors(ShadcnPaletteHarmony harmony)
{
    var source = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
    var recipe = ShadcnPaletteRecipe.CreateV2(117, "neutral", [],
        new("#2563eb", "#14b8a6", "#f59e0b", "#8b5cf6", "#ec4899"), harmony, []);

    var first = ShadcnPaletteGenerator.Generate(source, recipe);
    var second = ShadcnPaletteGenerator.Generate(source, recipe);

    Assert.True(first.IsValid, string.Join(Environment.NewLine, first.Errors));
    Assert.Equal(ShadcnThemeSerializer.Serialize(first.Theme), ShadcnThemeSerializer.Serialize(second.Theme));
    Assert.Equal(Hue(first.Theme.Light.Primary), Hue(first.Theme.Light.Chart1), precision: 2);
    Assert.Equal(Hue(first.Theme.Light.Secondary), Hue(first.Theme.Light.Chart2), precision: 2);
    Assert.Equal(Hue(first.Theme.Light.Accent), Hue(first.Theme.Light.Chart3), precision: 2);
    Assert.Equal(5, new[] { first.Theme.Light.Chart1, first.Theme.Light.Chart2,
        first.Theme.Light.Chart3, first.Theme.Light.Chart4, first.Theme.Light.Chart5 }.Distinct().Count());
}
```

Add tests that a locked Brand remains byte-identical while unlocked anchors change, semantic-token locks win after mapping, invalid color syntax returns `palette-invalid-anchor` at `palette.anchors.brand`, out-of-gamut input is normalized, destructive hue stays in the red family, and algorithm-v1 golden vectors remain unchanged.

- [ ] **Step 2: Run the tests and verify the v2 failures**

Run:

```powershell
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter FullyQualifiedName~ShadcnPaletteGeneratorTests
```

Expected: existing v1 tests pass; new v2 tests fail because version two is not dispatched.

- [ ] **Step 3: Implement strict anchor normalization**

`ShadcnPaletteColorParser.TryNormalize` must accept `#rgb`, `#rrggbb`, and the repository's canonical `oklch(L C H)` form, reject alpha and CSS functions outside that set, convert hex through linear sRGB to OKLCH, clamp lightness, fit chroma with `OklchColor.FitToSrgb()`, and return canonical `OklchColor.ToCss()` output. Do not use current culture for parsing or formatting.

Use this exact contract:

```csharp
internal static bool TryNormalize(string? value, out OklchColor color, out string normalized)
```

- [ ] **Step 4: Implement deterministic unlocked-anchor harmonies**

Use the recipe seed through `SplitMix64`; never call `Random`. Preserve normalized locked anchors exactly. Generate unlocked roles from Brand using these base hue offsets, then apply at most plus-or-minus six deterministic degrees of jitter:

```csharp
private static ReadOnlySpan<double> Offsets(ShadcnPaletteHarmony harmony) => harmony switch
{
    ShadcnPaletteHarmony.Free => [0d, 71d, 143d, 214d, 286d],
    ShadcnPaletteHarmony.Analogous => [0d, 30d, -30d, 60d, -60d],
    ShadcnPaletteHarmony.Complementary => [0d, 180d, 30d, 210d, -30d],
    ShadcnPaletteHarmony.Triadic => [0d, 120d, 240d, 60d, 300d],
    _ => throw new ArgumentOutOfRangeException(nameof(harmony), harmony, "Unknown palette harmony.")
};
```

Free mode may vary chroma and lightness more widely; the named harmonies must retain their stated hue relationships within the tested jitter tolerance.

- [ ] **Step 5: Map anchors to complete light and dark semantic schemes**

`ShadcnPaletteSemanticMapper.Map` must assign Brand to Primary/Ring/SidebarPrimary/Chart1, Support to Secondary/Chart2, Highlight to Accent/SidebarAccent/Chart3, Data A to Chart4, and Data B to Chart5. Derive neutral surfaces from the existing base-color family. Keep Destructive centered at OKLCH hue 25 in both schemes. Preserve source shadows and metrics.

After mapping, apply semantic token locks from the source, call the existing contrast repair, then return path-specific errors for any remaining required contrast failure. Do not repair an explicitly locked endpoint.

- [ ] **Step 6: Dispatch versions without modifying v1 code paths**

Refactor the current generator body into `GenerateV1` without changing its statements or constants. Dispatch explicitly:

```csharp
return recipe.AlgorithmVersion switch
{
    ShadcnPaletteRecipe.LegacyAlgorithmVersion => GenerateV1(source, recipe),
    ShadcnPaletteRecipe.CurrentAlgorithmVersion => GenerateV2(source, recipe),
    _ => Result(source.DeepClone(),
        [new("palette-unsupported-algorithm", "palette.algorithmVersion",
            $"Palette algorithm version {recipe.AlgorithmVersion} is not supported.")], [])
};
```

- [ ] **Step 7: Run generation, domain, and compatibility tests and commit**

Run:

```powershell
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~ShadcnPaletteGeneratorTests|FullyQualifiedName~ShadcnThemeDomainTests|FullyQualifiedName~ShadcnThemeDocumentTests"
dotnet format Maliev.ShadcnBlazor.slnx --verify-no-changes
git diff --check
```

Expected: all focused tests pass, including existing v1 golden vectors and parallel byte-stability tests.

Commit:

```powershell
git add src/Maliev.ShadcnBlazor/Theming/Internal/ShadcnPaletteColorParser.cs src/Maliev.ShadcnBlazor/Theming/Internal/ShadcnPaletteHarmonyGenerator.cs src/Maliev.ShadcnBlazor/Theming/Internal/ShadcnPaletteSemanticMapper.cs src/Maliev.ShadcnBlazor/Theming/ShadcnPaletteGenerator.cs tests/Maliev.ShadcnBlazor.Tests/Theming/ShadcnPaletteGeneratorTests.cs
git commit -m "Generate deterministic five-anchor palettes"
```

### Task 3: Integrate transactional v2 editing into Theme Studio state

**Files:**
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioPaletteCopy.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioState.cs:138-222,480-635,743-754,978-1045`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioWorkbenchState.cs:17-40`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioStateTests.cs:785-847`

**Interfaces:**
- Consumes: `ShadcnPaletteRecipe.CreateV2`, v2 generator results, document serialization, existing history snapshots, and Workbench change notifications.
- Produces: `PaletteAnchors`, `PaletteHarmony`, `PaletteWorkbenchOpen`, `IsPointerInteractionActive`, `SetPaletteAnchor`, `SetPaletteAnchorLock`, `SetPaletteHarmony`, `OpenPaletteWorkbench`, and `ClosePaletteWorkbench`.

- [ ] **Step 1: Add failing state tests for projection, upgrade, and transactions**

Add exact tests for these behaviors:

```csharp
[Fact]
public void MaterializedOrV1DocumentProjectsAnchorsWithoutUpgradingUntilMutation()
{
    var state = new ThemeStudioState(new NoOpStorage());
    var v1 = state.Document with
    {
        Palette = new ShadcnPaletteRecipe(1, 42, "neutral", [])
    };
    Assert.True(state.ImportDocument(v1));
    var before = state.SerializeDocument();

    Assert.Equal(state.Applied.Light.Primary, state.PaletteAnchors.Brand);
    Assert.Equal(ShadcnPaletteRecipe.LegacyAlgorithmVersion, state.Document.Palette.AlgorithmVersion);
    Assert.Equal(before, state.SerializeDocument());

    Assert.True(state.SetPaletteAnchor(ShadcnPaletteAnchorRole.Brand, "#2563eb"));
    Assert.Equal(2, state.Document.Palette.AlgorithmVersion);
    Assert.True(state.Document.Palette.LockedAnchors.Contains(ShadcnPaletteAnchorRole.Brand));
}

[Fact]
public void PickerGestureCreatesOneHistoryEntryAndSignalsPersistenceOnlyAtEnd()
{
    var state = new ThemeStudioState(new NoOpStorage());
    state.BeginPointerInteraction("palette.brand");
    Assert.True(state.SetPaletteAnchor(ShadcnPaletteAnchorRole.Brand, "#1d4ed8"));
    Assert.True(state.SetPaletteAnchor(ShadcnPaletteAnchorRole.Brand, "#2563eb"));
    Assert.True(state.IsPointerInteractionActive);
    state.EndPointerInteraction();

    Assert.False(state.IsPointerInteractionActive);
    Assert.True(state.Undo());
    Assert.False(state.CanUndo);
}
```

Also test: direct edit locks the role, unlock allows generation to replace it, changing harmony upgrades and is undoable, invalid input preserves document/history, impossible semantic locks preserve the previous preview, undo/redo restores anchors/harmony/locks/diagnostics, and share/import round trips all v2 values.

- [ ] **Step 2: Run state tests and verify the expected failures**

Run:

```powershell
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter FullyQualifiedName~ThemeStudioStateTests
```

Expected: FAIL because palette projection and v2 editing methods do not exist.

- [ ] **Step 3: Add localized copy as a complete typed record**

Create a `ThemeStudioPaletteCopy` record containing every workbench label, anchor name, status, error prefix, and announcement. Expose exactly two static instances and select without partial fallback:

```csharp
public static ThemeStudioPaletteCopy For(ThemeStudioLocale locale) => locale switch
{
    ThemeStudioLocale.English => English,
    ThemeStudioLocale.Thai => Thai,
    _ => throw new ArgumentOutOfRangeException(nameof(locale), locale, "Unknown Theme Studio locale.")
};
```

Include English and Thai values for Customize palette, Active palette, Contrast ready, Needs review, Generate palette, Return to preview, Close palette editor, all four harmony names, all five anchor names, Lock, Unlock, Copy, copied status, generated status, and validation summary.

- [ ] **Step 4: Implement read-only v1 projection and explicit v2 mutation**

Add:

```csharp
public ShadcnPaletteAnchors PaletteAnchors => _documentTemplate.Palette.Anchors ?? new(
    Applied.Light.Primary,
    Applied.Light.Secondary,
    Applied.Light.Accent,
    Applied.Light.Chart4,
    Applied.Light.Chart5);

public ShadcnPaletteHarmony PaletteHarmony =>
    _documentTemplate.Palette.Harmony ?? ShadcnPaletteHarmony.Free;
```

Implement one private `TryApplyPaletteRecipe(ShadcnPaletteRecipe recipe, string mutationKey)` that generates a candidate first, updates diagnostics, and mutates history/document/Draft/Applied only when valid. `SetPaletteAnchor` must normalize through generation, automatically add that role to locked anchors, and call this method. `SetPaletteHarmony` and `SetPaletteAnchorLock` use the same path. `GeneratePalette` preserves current anchors, harmony, anchor locks, and semantic locks.

- [ ] **Step 5: Expose persistence readiness for coalesced gestures**

Add `public bool IsPointerInteractionActive => _pointerMutationKey is not null;`. Track whether a valid mutation occurred during the gesture. `EndPointerInteraction` must raise one final `Changed` event only when a mutation occurred, allowing the page to persist the final value while intermediate events render only the preview.

Add `PaletteWorkbenchOpen` plus open/close methods to `ThemeStudioWorkbenchState`; changing palette visibility must raise Workbench `Changed` but must not create a theme history entry.

- [ ] **Step 6: Run state, share, and import/export unit tests and commit**

Run:

```powershell
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --filter "FullyQualifiedName~ThemeStudioStateTests|FullyQualifiedName~ThemeBundleTests|FullyQualifiedName~ShadcnThemeDocumentTests"
git diff --check
```

Expected: all selected tests pass and failed candidates leave the serialized document unchanged.

Commit:

```powershell
git add samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioPaletteCopy.cs samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioState.cs samples/Maliev.ShadcnBlazor.Showcase/Theming/ThemeStudioWorkbenchState.cs tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioStateTests.cs
git commit -m "Integrate transactional palette editing"
```

### Task 4: Build the accessible responsive palette workbench

**Files:**
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemePaletteSummary.razor`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemePaletteAnchorEditor.razor`
- Create: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemePaletteWorkbench.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeInspector.razor:50-57`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Pages/ThemeStudio.razor:15-48,79-138`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/js/theme-studio.js`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css:8601-8743,9132-9249`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioStateTests.cs:572-784`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioWorkbenchContractTests.cs`
- Modify: `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeStudioBrowserTests.cs`
- Modify: `tests/Maliev.ShadcnBlazor.BrowserTests/ThemeImportExportBrowserTests.cs`

**Interfaces:**
- Consumes: Task 3 state and copy APIs, existing Shadcn Button/Input/Select primitives, icon resolver conventions, page persistence loop, and theme-studio JS module.
- Produces: one `data-testid="theme-palette-workbench"` DOM instance, summary trigger `theme-palette-customize`, stable per-role hooks, and responsive focus behavior.

- [ ] **Step 1: Add failing bUnit and structural contract tests**

Render `ThemeInspector` and assert one summary strip with five swatches, localized contrast state, and Customize action. Render `ThemePaletteWorkbench` and assert:

```csharp
Assert.Equal(1, cut.FindAll("[data-testid='theme-palette-workbench']").Count);
Assert.Equal(5, cut.FindAll("[data-palette-anchor-role]").Count);
Assert.NotEmpty(cut.FindAll("[data-testid='theme-palette-generate']"));
Assert.NotEmpty(cut.FindAll("[role='status'][aria-live='polite']"));
Assert.All(Enum.GetValues<ShadcnPaletteAnchorRole>(), role =>
    Assert.NotEmpty(cut.FindAll($"[data-testid='theme-palette-anchor-{role.ToString().ToLowerInvariant()}']")));
```

Switch locale to Thai and assert no English control label remains. Update `ThemeStudioWorkbenchContractTests` to require `data-palette-open`, the workbench between sidebar and preview, mobile dialog attributes, reduced-motion rules, forced-color rules, and exactly one workbench component reference in `ThemeStudio.razor`.

- [ ] **Step 2: Add failing Playwright tests for desktop and constrained viewports**

Add tests at 1440x900, 1024x768, 390x844, and 320x568. Verify:

```csharp
await page.GetByTestId("theme-palette-customize").ClickAsync();
await Assertions.Expect(page.GetByTestId("theme-palette-workbench")).ToBeVisibleAsync();
await Assertions.Expect(page.Locator(".theme-preview-region")).ToBeVisibleAsync();
Assert.Equal(1, await page.GetByTestId("theme-palette-workbench").CountAsync());
Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > document.documentElement.clientWidth"));
```

On desktop, edit Brand and assert the preview primary CSS variable changes while preview buttons remain clickable. On constrained viewports, assert `role="dialog"`, `aria-modal="true"`, Tab remains inside, Escape closes, and focus returns to `theme-palette-customize`. Test Spacebar generates only when workbench chrome has focus and does nothing inside the value input or open harmony listbox.

- [ ] **Step 3: Implement the compact summary and anchor row**

`ThemePaletteSummary` reads `State.PaletteAnchors`, emits five decorative swatches plus a text contrast status, and opens `State.Workbench.OpenPaletteWorkbench()`. Give the trigger id `theme-palette-customize` for deterministic focus restoration.

`ThemePaletteAnchorEditor` accepts `State`, `Role`, and localized `Copy`; emit a native color input, a text `ShadcnInput`, Copy button, and pressed Lock button. Wire pointer down/up/cancel to `BeginPointerInteraction($"palette.{Role}")` and `EndPointerInteraction()`. The text field commits on change; the native picker updates on input. Never announce every pointer movement.

- [ ] **Step 4: Implement one responsive workbench DOM**

Place `ThemePaletteWorkbench` once between `ThemeStudioSidebar` and `theme-preview-region`. Render it only when open. Its root is an `aside` on wide screens and receives `role="dialog" aria-modal="true"` only from JS at constrained widths. Include heading/close, five anchor editors, harmony `ShadcnSelect`, Generate action, diagnostics, and one polite live region.

The component's close action must call the JS binding's `restoreFocus()` before changing `PaletteWorkbenchOpen`. Keyboard filtering stays in the JS binding because Blazor `KeyboardEventArgs` does not expose the originating element. Space activates `[data-testid='theme-palette-generate']` only when the event target is not an `input`, `textarea`, `select`, `button`, contenteditable element, or active listbox. Escape clicks the workbench close action so the same restoration path is used.

- [ ] **Step 5: Add responsive focus containment and restoration**

Export this JS contract:

```javascript
export function bindPaletteWorkbench(root, returnFocusId) {
    const media = window.matchMedia("(max-width: 64rem)");
    // Set/remove role=dialog and aria-modal based on media.matches.
    // On constrained viewports, focus the first enabled control and cycle Tab.
    // Space clicks Generate only from non-editable chrome; Escape clicks Close.
    // dispose removes key and media listeners.
    // restoreFocus() focuses document.getElementById(returnFocusId).
    return { restoreFocus, dispose };
}
```

The Razor component stores the module/object references, calls `restoreFocus` after closing, and disposes both references. Do not install a desktop focus trap.

- [ ] **Step 6: Implement three-column desktop and full-height sheet CSS**

At widths above 64rem, use:

```css
.theme-studio-workbench[data-palette-open="true"] {
    grid-template-columns: minmax(18rem, 21rem) minmax(18rem, 24rem) minmax(30rem, 1fr);
}
.theme-palette-workbench { min-inline-size: 0; overflow-y: auto; border-inline-end: 1px solid var(--shadcn-border); }
```

Account for the existing 3.5rem collapsed sidebar. At 64rem and below, position the same workbench fixed at `inset: 0`, `z-index: 70`, `block-size: 100dvh`, and keep its header, palette strip, and Generate action sticky. Use logical properties for RTL. Provide visible focus, 44px minimum targets, forced-color borders and lock state, and remove color transitions under both the preview reduced-motion attribute and `prefers-reduced-motion`.

- [ ] **Step 7: Prevent intermediate picker persistence**

In `ThemeStudio.razor`, always request re-render on `Changed`, but set `_persistRequested` only when `!State.IsPointerInteractionActive`. The final `EndPointerInteraction` event persists once. Keep the existing drain loop and exception reporting unchanged.

- [ ] **Step 8: Complete browser coverage for localization, export, and accessibility**

Add assertions that English mode contains only English workbench labels and Thai mode contains the complete Thai set. Generate/edit a palette, export JSON, parse its five anchors, and compare each with the displayed normalized value and live CSS token. Import that JSON in a fresh context and assert the same five values and harmony.

Run axe against the open desktop editor and constrained dialog; reject serious or critical violations. Emulate reduced motion and forced colors, verify no swatch transition, and verify lock/selection/focus remain distinguishable without relying on fill color.

- [ ] **Step 9: Run the complete affected validation lane and commit**

Run in repository order:

```powershell
dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~ShadcnPaletteGeneratorTests|FullyQualifiedName~ShadcnThemeDocumentTests|FullyQualifiedName~ThemeStudioStateTests|FullyQualifiedName~ThemeStudioWorkbenchContractTests|FullyQualifiedName~ThemeBundleTests|FullyQualifiedName~PublicApiSnapshotTests"
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --no-build
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --no-build --filter "FullyQualifiedName~ThemeStudioBrowserTests|FullyQualifiedName~ThemeImportExportBrowserTests"
dotnet format Maliev.ShadcnBlazor.slnx --verify-no-changes --no-restore
pwsh -NoProfile -File eng/Verify-PublicSurface.ps1 -Root .
git diff --check
```

Expected: Release build has zero warnings and errors; all focused, affected unit, repository, and browser tests pass; formatting, public surface, and diff checks pass.

Commit:

```powershell
git add samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemePaletteSummary.razor samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemePaletteAnchorEditor.razor samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemePaletteWorkbench.razor samples/Maliev.ShadcnBlazor.Showcase/Components/Theming/ThemeInspector.razor samples/Maliev.ShadcnBlazor.Showcase/Pages/ThemeStudio.razor samples/Maliev.ShadcnBlazor.Showcase/wwwroot/js/theme-studio.js samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioStateTests.cs tests/Maliev.ShadcnBlazor.Tests/Showcase/ThemeStudioWorkbenchContractTests.cs tests/Maliev.ShadcnBlazor.BrowserTests/ThemeStudioBrowserTests.cs tests/Maliev.ShadcnBlazor.BrowserTests/ThemeImportExportBrowserTests.cs
git commit -m "Add responsive Theme Studio palette workbench"
```

### Task 5: Audit the completed slice against the approved specification

**Files:**
- Modify only if evidence exposes a defect in a file already owned by Tasks 1-4.
- Review: `docs/superpowers/specs/2026-08-29-theme-studio-palette-workbench-design.md`
- Review: `docs/superpowers/plans/2026-08-29-theme-studio-palette-workbench.md`

**Interfaces:**
- Consumes: all outputs and validation evidence from Tasks 1-4.
- Produces: a clean, reviewable branch whose commits contain only the approved palette subsystem.

- [ ] **Step 1: Verify every specification requirement has direct evidence**

Create a local checklist from the specification headings and map each item to a named test or direct browser assertion. Required mappings include: v1 byte identity, v2 determinism, all harmony modes, anchor locks, semantic-lock precedence, exact export, invalid-candidate rollback, desktop coexistence, responsive focus containment, localization, reduced motion, forced colors, RTL, light/dark, and persistence coalescing.

- [ ] **Step 2: Inspect commit scope and working-tree ownership**

Run:

```powershell
git status --short
git log --oneline --decorate -6
git diff d91add3..HEAD --stat
git diff d91add3..HEAD --name-only
```

Expected: only the approved design/plan and palette implementation files appear; `.superpowers/` and unrelated user changes remain untracked or unstaged.

- [ ] **Step 3: Re-run final verification from a clean build boundary**

Run:

```powershell
dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --no-build
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --no-build --filter "FullyQualifiedName~ThemeStudioBrowserTests|FullyQualifiedName~ThemeImportExportBrowserTests"
dotnet format Maliev.ShadcnBlazor.slnx --verify-no-changes --no-restore
pwsh -NoProfile -File eng/Verify-PublicSurface.ps1 -Root .
git diff --check d91add3..HEAD
```

Expected: every command succeeds, build reports zero warnings/errors, and no known requirement remains unverified.

- [ ] **Step 4: Stop without pushing or releasing**

Report changed boundaries, exact commands and pass counts, commit hashes, excluded work, and any residual risk. Do not push, deploy, publish a package, or release a version unless the user separately authorizes it.
