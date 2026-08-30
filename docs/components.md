# Component catalog

The library contains 66 reviewed component families. The machine-readable
catalog is available in [`component-catalog.json`](component-catalog.json), and
the Showcase provides a live dossier for each family.

## Foundations and layout

Direction, Aspect Ratio, Typography, Label, Field, Item, Empty, Kbd, Separator,
Resizable, and Scroll Area.

## Actions and selection

Button, Button Group, Toggle, Toggle Group, Checkbox, Radio Group, Switch,
Slider, and Pagination.

## Forms and date selection

Input, Textarea, Input Group, Input OTP, Native Select, Select, Combobox,
Calendar, and Date Picker.

## Disclosure and navigation

Accordion, Collapsible, Tabs, Navigation Menu, Breadcrumb, and Sidebar.

## Overlays and menus

Dialog, Alert Dialog, Sheet, Drawer, Popover, Hover Card, Tooltip, Dropdown
Menu, Context Menu, Menubar, and Command.

## Feedback and content

Alert, Progress, Skeleton, Spinner, Toast, Avatar, Badge, Card, and Carousel.

## Data display

Table, Data Table, and Chart.

## Conversation workflows

Attachment, Bubble, Marker, Message, Message Scroller, and Questionnaire.

Public API compatibility is checked against the committed API snapshot. A
breaking API change requires an intentional major-version decision and an
updated migration note.

## Interactive demo and visual proof

The [public Showcase](https://maliev-co-ltd.github.io/Maliev.ShadcnBlazor/)
contains a searchable, interactive dossier for every component. Each dossier
uses the package's public API and exposes live controls, source examples,
accessibility notes, and reviewed evidence.

Run the complete desktop-light and mobile-dark-RTL proof locally with:

```powershell
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --filter ComponentCatalogVisualProofTests
```

Normal local and CI runs compare against the committed images under
`docs/evidence/component-catalog-baselines` and do not update baselines.
Screenshots and diffs are written to `artifacts/visual-proof` for inspection.

An intentional baseline update must be explicit:

```powershell
$env:SHADCN_UPDATE_VISUAL_BASELINES='1' # SHADCN_UPDATE_VISUAL_BASELINES=1
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --filter ComponentCatalogVisualProofTests
Remove-Item Env:SHADCN_UPDATE_VISUAL_BASELINES
```

During local component work, set `SHADCN_VISUAL_PROOF_SLUGS` to a comma-separated
list of catalog slugs to capture and compare only those dossiers. CI leaves this
unset and always reviews the complete catalog.

```powershell
$env:SHADCN_VISUAL_PROOF_SLUGS='date-picker,data-table'
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --filter ComponentCatalogVisualProofTests
Remove-Item Env:SHADCN_VISUAL_PROOF_SLUGS
```

Inspect every changed image at its original resolution, rerun without the
environment variable, and commit the reviewed images separately. Pull-request
automation never enables the update variable and cannot approve visual changes.

## Programmatic focus

Public controls with one stable native focus target and audited composite widgets
implement `IShadcnFocusable`. Capture the component with `@ref` after it renders
and call `FocusAsync(bool preventScroll = false)`. This keeps focus restoration
independent of package DOM IDs and selectors. Dual-mode controls focus their
rendered anchor or button without changing disabled, click, keyboard, or ARIA
behavior.

Composite entry targets are deterministic:

- Radio and toggle groups focus the selected enabled item, the current roving
  item, or the first enabled item.
- Tabs, accordion, navigation menu, and menubar focus the selected or open
  enabled trigger, the current roving trigger, or the first enabled trigger.
- Select and date picker focus their trigger; combobox and command focus their
  text input; calendar focuses its current or nearest enabled day.
- Slider focuses its first thumb by default and exposes `FocusThumbAsync(index,
  preventScroll)` for an explicit thumb.
- Empty or fully disabled composites perform no focus movement.

The audit intentionally excludes roots that do not own one semantic target:
`ShadcnSidebarRail` is deliberately removed from the tab order,
`ShadcnAvatarGroupCount` can render non-interactive text, and multi-action
containers such as toaster, code block, reaction overflow, and reply quote must
be focused through their specific child action instead.
