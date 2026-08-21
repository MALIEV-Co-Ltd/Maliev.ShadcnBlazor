# Maliev.ShadcnBlazor

Accessible, themeable Shadcn-inspired components for .NET 10 Blazor
applications. The package includes semantic foundations, forms, actions,
selection, overlays, navigation, feedback, data display, and conversation
workflow components, plus static web assets delivered by the Razor Class
Library.

[Explore every component in the live interactive demo](https://maliev-co-ltd.github.io/Maliev.ShadcnBlazor/).

## Install

```bash
dotnet add package Maliev.ShadcnBlazor --version 1.1.1
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

Load the stylesheets in this order:

```html
<link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-base.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-semantic-foundations.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-actions.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-data-display.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-disclosure-navigation.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-forms.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-feedback-content.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-overlays-menus.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-conversation.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-mudblazor.css" rel="stylesheet" />
<script src="_content/MudBlazor/MudBlazor.min.js"></script>
```

See the [getting started guide](docs/getting-started.md) for provider setup,
asset selection, and a first component. The repository Showcase demonstrates
every component family.

## Highlights

- Native Blazor components with strongly typed parameters and callbacks.
- Light and dark themes, LTR and RTL layout, forced-colors support, and reduced
  motion behavior.
- Keyboard and screen-reader behavior covered by unit and real-browser tests.
- Theme tokens and MudBlazor adapters for applications that need both systems.
- Bundled Geist, Noto Sans Thai, and JetBrains Mono fonts for a
  deterministic offline default; see [theming](docs/theming.md).
- A Blazor-first Theme Studio with live generator options plus portable JSON
  and ready-to-paste C# output.
- A standalone Showcase project with all component dossiers.

## Compatibility

The 1.x line targets .NET 10 and MudBlazor 9.7.x. MudBlazor is pinned to 9.7.0
by the package. Revalidate adapter behavior before moving to another MudBlazor
minor line.

## Repository

- Source: <https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor>
- Live interactive demo: <https://maliev-co-ltd.github.io/Maliev.ShadcnBlazor/>
- Getting started: [docs/getting-started.md](docs/getting-started.md)
- Component catalog: [docs/components.md](docs/components.md)
- Theming: [docs/theming.md](docs/theming.md)
- Contributing: [CONTRIBUTING.md](CONTRIBUTING.md)
- Support: [SUPPORT.md](SUPPORT.md)
- Security policy: [SECURITY.md](SECURITY.md)
Maliev.ShadcnBlazor is licensed under the [MIT License](LICENSE). Third-party
attribution is recorded in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
