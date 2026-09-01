# DataTable Compact Toolbar and v2.1.4 Documentation Identity

## Status

Approved in chat on 2026-09-01 for implementation, pull-request merge, and
release as `v2.1.4`.

## Problem

`ShadcnDataTable<TItem>` currently renders global search, every configured
column filter, and every column-visibility checkbox directly in one toolbar.
Operational tables therefore expose too many permanent controls and degrade
into a crowded stack at narrow widths. Consumers cannot replace this with a
compact accessible hierarchy without duplicating table state behavior.

The public documentation also does not identify the package version from which
it was built, so a visitor cannot reliably connect a deployed example to a
specific release.

## Goals

- Add an opt-in compact DataTable toolbar without changing existing consumers.
- Keep global search immediately available.
- Place column filters and visibility controls in labelled disclosures.
- Preserve the current typed state and manual-request contract.
- Add caller-owned actions at the logical start and end of the toolbar.
- Keep the toolbar usable at 320 CSS pixels in LTR and RTL.
- Display the exact package version represented by the documentation build.
- Release the complete reviewed hardening branch as `v2.1.4`.

## Public API

Add a public `ShadcnDataTableToolbarMode` enum:

```csharp
public enum ShadcnDataTableToolbarMode
{
    Default,
    Compact
}
```

Add these parameters to `ShadcnDataTable<TItem>`:

```csharp
[Parameter] public ShadcnDataTableToolbarMode ToolbarMode { get; set; }
[Parameter] public RenderFragment? ToolbarStartTemplate { get; set; }
[Parameter] public RenderFragment? ToolbarEndTemplate { get; set; }
[Parameter] public string FiltersLabel { get; set; } = "Filters";
```

`ColumnsLabel`, `FilterPlaceholder`, and each column's existing filter label
remain the localization inputs for column visibility, global search, and
individual filters. The new enum and parameters are additive and intentional
public-surface changes.

## Rendering and behavior

`Default` mode preserves the existing toolbar markup and behavior.

`Compact` mode renders, in logical order:

1. `ToolbarStartTemplate`, when supplied.
2. The existing global search field.
3. A Filters trigger when at least one column is filterable.
4. A Columns trigger when at least one column is hideable.
5. `ToolbarEndTemplate`, when supplied.

The Filters and Columns disclosures compose the package's existing Popover
primitives. Their triggers therefore expose `aria-haspopup`, `aria-controls`,
and `aria-expanded`; Escape and outside press close the active disclosure; and
focus behavior stays owned by the overlay family. Filter inputs and visibility
checkboxes continue to call the existing DataTable state transitions, so
manual mode still emits the same complete `ShadcnDataTableRequest`.

The DataTable owns the two disclosure-open states. Changing toolbar mode closes
both disclosures. Hiding the last visible column remains prohibited by the
existing state invariant.

## Layout and accessibility

Package CSS owns the compact hierarchy. The global search may grow while the
action cluster remains content-sized on wider layouts. At narrow widths the
toolbar becomes a single-column layout and the action cluster wraps within the
component rather than the page. Controls use logical properties and semantic
theme tokens.

All compact triggers have a minimum 44-by-44 CSS-pixel target. Popover content
has a bounded inline size and viewport-safe maximum size. Filter labels remain
programmatically associated with their inputs. The Columns group keeps its
fieldset and legend semantics. Focus-visible, forced-colors, dark theme,
reduced-motion, zoom, and RTL behavior must remain supported by the underlying
primitives and focused browser coverage.

## Showcase documentation

The DataTable dossier will opt into compact mode and use at least four
filterable and four hideable columns so the acceptance scenario is visible.
An interactive control will switch between `Compact` and `Default`, proving
backward compatibility. The copyable example will include the new enum,
localized labels, and both toolbar template slots.

The documentation header will show a compact version link beside the product
name. Its value comes from the built `Maliev.ShadcnBlazor` assembly's
informational version, normalized to `v<semver>`, rather than duplicated text.
The link targets the matching GitHub release tag. This keeps main and
release-tag deployments truthful to the source that produced them.

## Version and release

Prepare `v2.1.4` by updating all package `VersionPrefix` values, checked package
consumer metadata, locked dependencies, getting-started commands, and
`CHANGELOG.md`. The branch will first incorporate current `origin/main`, which
contains #256 and #260, without dropping any of the existing hardening commits.

After local validation, push the feature branch, create a PR linked to #259,
wait for required checks, and merge through the protected branch. Publish the
GitHub release from the resulting main commit. The existing release workflow
owns NuGet packing, provenance, and publication; the Pages workflow deploys
the exact release tag.

## Validation

Required evidence:

- A focused component test that fails before the compact API exists and proves
  the number and semantics of always-visible controls.
- State tests proving compact filters and visibility controls publish the same
  typed state and manual request as default mode.
- Public API snapshot coverage for the enum and parameters.
- Browser tests for keyboard operation, `aria-expanded`, Escape, 320px no-page-
  overflow behavior, touch target dimensions, LTR/RTL, dark theme, and both
  toolbar modes.
- Documentation tests proving the version is assembly-derived and linked to
  the matching release.
- Release build with zero warnings and errors; full component and repository
  suites; relevant browser suite; formatting; public-surface verification; and
  package-consumer validation.
- Required PR checks, release workflow, NuGet indexes, GitHub release assets,
  and the deployed version badge verified after publication.

## Non-goals

- Replacing the entire toolbar with an untyped caller template.
- Creating a separate public family of DataTable toolbar primitives.
- Changing DataTable filtering, sorting, selection, or paging semantics.
- Making compact mode the default in `v2.1.4`.
- Manually publishing NuGet packages outside the release workflow.
