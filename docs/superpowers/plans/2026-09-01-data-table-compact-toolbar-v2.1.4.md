# DataTable Compact Toolbar and v2.1.4 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deliver an accessible opt-in compact DataTable toolbar, show the documentation package version, and publish the complete hardening branch as v2.1.4.

**Architecture:** `ShadcnDataTable<TItem>` keeps ownership of its existing typed state and renders either the byte-compatible default toolbar or a compact composition built from existing Popover primitives. The showcase exposes both modes and derives its version badge from the referenced package assembly. Release metadata is updated only after component and documentation behavior is proven.

**Tech Stack:** .NET 10, Blazor WebAssembly, Razor components, bUnit, xUnit, Microsoft Playwright, GitHub Actions, NuGet trusted publishing.

**Spec:** `docs/superpowers/specs/2026-09-01-data-table-compact-toolbar-v2.1.4-design.md`

## Global Constraints

- `ShadcnDataTableToolbarMode.Default` must preserve the existing rendered controls and behavior.
- `ShadcnDataTableToolbarMode.Compact` is opt-in and keeps global search visible.
- Compact filters and columns use existing Popover components and existing DataTable state transitions.
- The public API is additive: `ToolbarMode`, `ToolbarStartTemplate`, `ToolbarEndTemplate`, and `FiltersLabel`.
- All built-in labels remain caller-localizable.
- The toolbar must not create page overflow at 320 CSS pixels and must support LTR, RTL, dark, forced-colors, reduced-motion, and zoom.
- Compact action targets are at least 44 by 44 CSS pixels.
- Documentation displays an assembly-derived `v2.1.4` link to the matching GitHub release.
- No manual NuGet publication; the existing release workflow remains authoritative.

---

### Task 1: Compact DataTable public API and rendering

**Files:**
- Modify: `src/Maliev.ShadcnBlazor/Components/DataDisplay/ShadcnDataTableModels.cs`
- Modify: `src/Maliev.ShadcnBlazor/Components/DataDisplay/ShadcnDataTable.razor`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Components/DataDisplay/DataTableTests.cs`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt`

**Interfaces:**
- Consumes: existing `ShadcnPopover`, `ShadcnPopoverTrigger`, `ShadcnPopoverContent`, `ShadcnDataTableState`, and `PublishAsync(ShadcnDataTableState)`.
- Produces: public `ShadcnDataTableToolbarMode { Default, Compact }`; parameters `ToolbarMode`, `ToolbarStartTemplate`, `ToolbarEndTemplate`, and `FiltersLabel`.

- [ ] **Step 1: Write failing bUnit tests for the compact contract**

Add tests that render four filterable/hideable columns in compact mode and assert one visible global search, no inline column filters or visibility fieldset, labelled Filters and Columns triggers with `aria-expanded="false"`, and both start/end fragments. Add a default-mode regression asserting the inline inputs and fieldset remain.

- [ ] **Step 2: Run the focused tests and verify the red state**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj --configuration Release --filter "FullyQualifiedName~DataTableTests"`

Expected: compilation failure because `ShadcnDataTableToolbarMode` and the four new parameters do not exist.

- [ ] **Step 3: Add the enum, parameters, and compact markup**

Add the enum to `ShadcnDataTableModels.cs`. In `ShadcnDataTable.razor`, branch only the toolbar body: retain current default markup verbatim, and render start fragment, global search, Popover-based Filters disclosure, Popover-based Columns disclosure, and end fragment in compact mode. Use the current column-filter and visibility handlers so controlled, uncontrolled, and manual requests remain identical.

- [ ] **Step 4: Add state and mode-transition handling**

Track `_filtersOpen`, `_columnsOpen`, and the previous toolbar mode. Close both disclosures when the mode changes. Do not change filtering, paging, selection, sorting, or last-visible-column invariants.

- [ ] **Step 5: Update and verify the public API snapshot**

Add the enum and exact parameter signatures to `tests/Maliev.ShadcnBlazor.Tests/Contracts/public-api.txt`, then run:

`dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj --configuration Release --filter "FullyQualifiedName~PublicSurfaceTests"`

Expected: PASS.

- [ ] **Step 6: Run focused component tests**

Run the DataTable test command from Step 2.

Expected: PASS, including default-mode compatibility and compact state/request behavior.

- [ ] **Step 7: Commit the validated component slice**

Stage only the four Task 1 files and commit with `Add compact DataTable toolbar composition`.

---

### Task 2: Responsive and accessible compact styling

**Files:**
- Modify: `src/Maliev.ShadcnBlazor/wwwroot/css/shadcn-data-display.css`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Components/DataDisplay/DataTableTests.cs`

**Interfaces:**
- Consumes: Task 1 data slots and Popover markup.
- Produces: package-owned responsive compact toolbar layout and 44px triggers.

- [ ] **Step 1: Add failing style-contract assertions**

Assert compact mode emits stable slots/classes for its search, action cluster, disclosures, and panels. Assert package CSS contains logical sizing, `min-block-size: 2.75rem`, viewport-bounded popover widths, and the existing narrow breakpoint.

- [ ] **Step 2: Run focused tests and verify failure**

Run the Task 1 DataTable test command.

Expected: FAIL on absent compact style contract.

- [ ] **Step 3: Implement package CSS**

Make the wide toolbar a growing search plus content-sized action cluster. At the existing `30rem` breakpoint, stack the search and wrap actions without document overflow. Use logical properties, semantic tokens, forced-colors-compatible borders, and no motion beyond Popover primitives.

- [ ] **Step 4: Run focused tests**

Run the Task 1 DataTable test command.

Expected: PASS.

- [ ] **Step 5: Commit the styling slice**

Stage the CSS and focused test changes and commit with `Harden compact DataTable toolbar layout`.

---

### Task 3: Showcase compact mode and browser behavior

**Files:**
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Documentation/Examples/DataDisplayExamples.cs`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Showcase/DataDisplayShowcaseContractTests.cs`
- Modify: `tests/Maliev.ShadcnBlazor.BrowserTests/DataDisplayBrowserTests.cs`

**Interfaces:**
- Consumes: Task 1 public API and Task 2 CSS contract.
- Produces: a four-column compact dossier with a compact/default selector, localized labels, both template slots, and copyable source.

- [ ] **Step 1: Write failing showcase contract tests**

Require the DataTable example to default to Compact, expose a `Toolbar mode` select, define at least four filterable/hideable columns, pass `FiltersLabel` and `ColumnsLabel`, render start/end templates, and serialize the chosen mode into source.

- [ ] **Step 2: Run the showcase contract tests and verify failure**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj --configuration Release --filter "FullyQualifiedName~DataDisplayShowcaseContractTests"`

Expected: FAIL because the dossier still renders the default inline toolbar.

- [ ] **Step 3: Implement the showcase dossier**

Extend `DataDisplayExamples.DataTable()` with four meaningful payment fields and mode state. Add controls using the existing dossier control primitives. Pass localized labels and start/end fragments to the package component; update source generation to mirror selected state.

- [ ] **Step 4: Add Playwright coverage**

Update the DataTable browser test to open Filters and Columns via keyboard, assert `aria-expanded`, edit a compact filter, toggle visibility, close with Escape, switch back to Default, and confirm inline controls. Add a 320px forced-colors/reduced-motion check for 44px targets, no page overflow, then repeat direction and dark-theme assertions.

- [ ] **Step 5: Run component and browser tests**

Run sequentially:

1. `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj --configuration Release --filter "FullyQualifiedName~DataDisplayShowcaseContractTests"`
2. `dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj --configuration Release --filter "FullyQualifiedName~DataDisplayBrowserTests.DataTable"`

Expected: PASS.

- [ ] **Step 6: Commit the showcase slice**

Stage the four Task 3 files and commit with `Showcase the compact DataTable workflow`.

---

### Task 4: Assembly-derived documentation version

**Files:**
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/Layout/DocumentationHeader.razor`
- Modify: `samples/Maliev.ShadcnBlazor.Showcase/wwwroot/css/showcase.css`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Showcase/DocumentationNavigationTests.cs`
- Modify: `tests/Maliev.ShadcnBlazor.Tests/Showcase/DocumentationShellStyleContractTests.cs`

**Interfaces:**
- Consumes: `typeof(ShadcnDataTable<>).Assembly` informational version metadata.
- Produces: normalized `DocumentationVersion` and `DocumentationVersionUrl`, rendered as a compact release link beside the brand.

- [ ] **Step 1: Write failing documentation tests**

Render `DocumentationHeader` and assert a `documentation-version-link` whose text begins with `v`, whose URL ends with that exact tag, and whose value equals the referenced package assembly informational version with build metadata removed.

- [ ] **Step 2: Run focused tests and verify failure**

Run: `dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj --configuration Release --filter "FullyQualifiedName~DocumentationNavigationTests|FullyQualifiedName~DocumentationShellStyleContractTests"`

Expected: FAIL because no version link exists.

- [ ] **Step 3: Implement version derivation and styling**

Read `AssemblyInformationalVersionAttribute`, fall back to `AssemblyName.Version`, discard `+metadata`, normalize to `v<semver>`, and construct `https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/releases/tag/<tag>`. Render the link next to the brand name with compact, focus-visible, narrow-layout-safe styles.

- [ ] **Step 4: Run focused tests**

Run the Step 2 command.

Expected: PASS.

- [ ] **Step 5: Commit the documentation identity slice**

Stage the four Task 4 files and commit with `Show package version in documentation`.

---

### Task 5: Integrate main and prepare v2.1.4 metadata

**Files:**
- Modify: `src/Maliev.ShadcnBlazor/Maliev.ShadcnBlazor.csproj`
- Modify: `src/Maliev.ShadcnBlazor.Icons.Hugeicons/Maliev.ShadcnBlazor.Icons.Hugeicons.csproj`
- Modify: `src/Maliev.ShadcnBlazor.Icons.Lucide/Maliev.ShadcnBlazor.Icons.Lucide.csproj`
- Modify: `src/Maliev.ShadcnBlazor.Icons.Phosphor/Maliev.ShadcnBlazor.Icons.Phosphor.csproj`
- Modify: `src/Maliev.ShadcnBlazor.Icons.Tabler/Maliev.ShadcnBlazor.Icons.Tabler.csproj`
- Modify: `samples/Maliev.ShadcnBlazor.ThemeConsumer/Maliev.ShadcnBlazor.ThemeConsumer.csproj`
- Modify: `docs/getting-started.md`
- Modify: `CHANGELOG.md`
- Modify: repository tests and checked lock files containing `2.1.3`
- Modify: showcase cache-busting URLs containing `2.1.3`

**Interfaces:**
- Consumes: current `origin/main` commits #256 and #260 and validated Tasks 1-4.
- Produces: a clean branch based on current main with all package/release metadata consistently set to `2.1.4`.

- [ ] **Step 1: Incorporate current main without dropping hardening commits**

Run `git fetch origin --prune`, inspect `git log --left-right --cherry-pick origin/main...HEAD`, then rebase the feature branch onto `origin/main`. Resolve only genuine overlaps and preserve both upstream fixes and all branch commits.

- [ ] **Step 2: Add failing release-metadata expectations**

Change package metadata/archive tests to expect `2.1.4`, then run the repository metadata tests and verify they fail against unchanged projects.

- [ ] **Step 3: Update versioned files consistently**

Set all five package `VersionPrefix` values and ThemeConsumer fallback version to `2.1.4`; update lock files with locked restore; update cache-busting URLs, getting-started installation command, changelog heading/link, archive tests, package metadata tests, icon catalog expectations, and any remaining intentional `2.1.3` release assertions.

- [ ] **Step 4: Verify no stale release metadata remains**

Run `rg -n "2\.1\.3|v2\.1\.3" src samples tests docs CHANGELOG.md` and classify every remaining hit as historical changelog content or fix it.

- [ ] **Step 5: Commit release metadata**

After focused metadata tests pass, stage only version/release files and commit with `Prepare v2.1.4 release`.

---

### Task 6: Full validation, PR, merge, release, and production verification

**Files:**
- Verify: `.github/workflows/ci.yml`
- Verify: `.github/workflows/release.yml`
- Verify: `.github/workflows/pages.yml`
- Verify: `docs/releasing.md`

**Interfaces:**
- Consumes: the complete rebased v2.1.4 branch.
- Produces: merged PR closing #259, GitHub release `v2.1.4`, NuGet packages, and Pages documentation showing `v2.1.4`.

- [ ] **Step 1: Run the MALIEV full cleanup and pre-commit/release gate**

Confirm only intentional changes, no generated artifacts, no credentials, and no unrelated dirty work. Audit workflows for least privilege, pinned actions, tag/version validation, trusted publishing, and exact-tag Pages deployment.

- [ ] **Step 2: Build first**

Run: `dotnet build Maliev.ShadcnBlazor.slnx --configuration Release --locked-mode`

Expected: 0 warnings, 0 errors.

- [ ] **Step 3: Run the full repository suites sequentially**

Run all test projects referenced by `Maliev.ShadcnBlazor.slnx` in Release configuration, followed by the complete browser project. Record totals and any skips. A flaky failure is not accepted without rerunning the failing test independently and then rerunning the affected suite.

- [ ] **Step 4: Run static/package validation**

Run the repository formatting check, `eng/Verify-PublicSurface.ps1`, package archive/metadata tests, and ThemeConsumer package validation described by `docs/releasing.md` and `.github/workflows/release.yml`.

- [ ] **Step 5: Perform local visual verification**

Start the showcase server, inspect `/docs/components/data-table` at desktop and 320px, exercise both disclosures and both modes, toggle dark and RTL, and confirm the header version link. Capture screenshots only in an ignored verification location and remove them before final status.

- [ ] **Step 6: Push and monitor branch CI**

Push the feature branch, identify runs by head SHA, and use `gh run watch --exit-status` until all relevant runs succeed. Fix and repeat if any required check fails.

- [ ] **Step 7: Create and merge the PR**

Create a PR whose body includes `Closes #259`, scope, accessibility behavior, release impact, and exact validation. Wait for required checks and reviews, then merge through the protected branch. Verify the resulting main SHA contains every branch commit.

- [ ] **Step 8: Publish v2.1.4 and monitor release workflows**

Create the GitHub release/tag `v2.1.4` from the merged main SHA with changelog-backed notes. Monitor the release and Pages workflows to successful completion.

- [ ] **Step 9: Verify public artifacts**

Verify the GitHub release assets, NuGet registration/index entries for all five packages at `2.1.4`, and the deployed documentation header link/text. Report URLs, merged PR number, main SHA, tag SHA, workflow conclusions, and any residual risk.
