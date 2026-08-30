# MudBlazor decoupling migration

The core `Maliev.ShadcnBlazor` package no longer depends on MudBlazor. This is a
breaking public and runtime dependency change and therefore requires the next
package release containing it to use a new major version.

`AddMalievShadcn()` now registers only package-owned services, and
`ShadcnThemeProvider` renders only its scoped theme/direction context. Existing
applications keep the same root provider for Maliev components.

Applications that still use MudBlazor must reference MudBlazor directly, call
its service registration, render the Mud providers required by that application,
and load MudBlazor CSS/JavaScript themselves. Those providers should be composed
beside or inside `ShadcnThemeProvider` according to the application's needs.

The Mud-specific `ShadcnThemeFactory`, `ShadcnMudChartOptions`,
`shadcn-mudblazor.css`, and packaged MudBlazor license were removed rather than
moved to a compatibility package. Replace theme mapping and chart palette setup
in the consuming application. Remove these core-package asset references:

- `_content/MudBlazor/MudBlazor.min.css`
- `_content/MudBlazor/MudBlazor.min.js`
- `_content/Maliev.ShadcnBlazor/css/shadcn-mudblazor.css`

Keep the package's `shadcn-*.css` files and allow its JavaScript modules to load
on demand.
