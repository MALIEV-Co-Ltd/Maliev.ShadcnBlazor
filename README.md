# Maliev.ShadcnBlazor

Accessible, themeable Shadcn-inspired components for .NET 10 Blazor
applications. The package includes semantic foundations, forms, actions,
selection, overlays, navigation, feedback, data display, and conversation
workflow components, plus static web assets delivered by the Razor Class
Library.

## Install

```bash
dotnet add package Maliev.ShadcnBlazor --version 1.0.0
```

Register the services in `Program.cs`:

```csharp
using Maliev.ShadcnBlazor;

builder.Services.AddMalievShadcn();
```

Add the namespaces you use to `_Imports.razor` and wrap the application root
with `ShadcnThemeProvider`:

```razor
@using Maliev.ShadcnBlazor.Components
@using Maliev.ShadcnBlazor.Theming

<ShadcnThemeProvider>
    @Body
</ShadcnThemeProvider>
```

Load MudBlazor followed by the component stylesheets distributed under
`_content/Maliev.ShadcnBlazor`. The repository Showcase demonstrates every
component family and can be started with `dotnet run` from its sample project.

## Highlights

- Native Blazor components with strongly typed parameters and callbacks.
- Light and dark themes, LTR and RTL layout, forced-colors support, and reduced
  motion behavior.
- Keyboard and screen-reader behavior covered by unit and real-browser tests.
- Theme tokens and MudBlazor adapters for applications that need both systems.
- A standalone Showcase project with all component dossiers.

## Compatibility

The 1.x line targets .NET 10 and MudBlazor 9.7.x. MudBlazor is pinned to 9.7.0
by the package. Revalidate adapter behavior before moving to another MudBlazor
minor line.

## Repository

- Source: <https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor>
Maliev.ShadcnBlazor is licensed under the [MIT License](LICENSE). Third-party
attribution is recorded in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
