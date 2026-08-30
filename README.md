# Maliev.ShadcnBlazor

Accessible, themeable Shadcn-inspired components for .NET 10 Blazor
applications. The package includes semantic foundations, forms, actions,
selection, overlays, navigation, feedback, data display, and conversation
workflow components, plus static web assets delivered by the Razor Class
Library.

[Explore every component in the live interactive demo](https://maliev-co-ltd.github.io/Maliev.ShadcnBlazor/).

## Install

```bash
dotnet add package Maliev.ShadcnBlazor --version 2.1.3
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
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-base.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-semantic-foundations.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-layout.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-actions.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-data-display.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-disclosure-navigation.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-forms.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-feedback-content.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-overlays-menus.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-conversation.css" rel="stylesheet" />
```

See the [getting started guide](docs/getting-started.md) for provider setup,
asset selection, and a first component. The repository Showcase demonstrates
every component family.

## Agent-ready integration

This repository includes portable Agent Skills for both package consumers and
library maintainers. Install them with the open `skills` CLI:

```bash
npx skills add MALIEV-Co-Ltd/Maliev.ShadcnBlazor \
  --skill maliev-shadcnblazor \
  --skill maliev-shadcnblazor-maintainer
```

Use `$maliev-shadcnblazor` to integrate the released package into an
application. Use `$maliev-shadcnblazor-maintainer` only when changing this
repository. Agents that support repository-local discovery can use the skills
directly from `.agents/skills/` without a global install.

See [Agent Skills](docs/agent-skills.md) for Codex installation, project versus
global scope, prompts, package contents, and safety boundaries.

## Highlights

- Native Blazor components with strongly typed parameters and callbacks.
- Light and dark themes, LTR and RTL layout, forced-colors support, and reduced
  motion behavior.
- Keyboard and screen-reader behavior covered by unit and real-browser tests.
- Theme tokens and package-owned providers with no transitive UI-framework dependency.
- Bundled Geist, Noto Sans Thai, and JetBrains Mono fonts for a
  deterministic offline default; see [theming](docs/theming.md).
- A Blazor-first Theme Studio with live generator options plus portable JSON
  and ready-to-paste C# output.
- A checked [package-only theme consumer](samples/Maliev.ShadcnBlazor.ThemeConsumer/README.md)
  that demonstrates the exact export, add, register, load, build, and verify
  journey for canonical `theme.json` and `theme.css` artifacts.
- A standalone Showcase project with all component dossiers.

## Compatibility

The package targets .NET 10 and has no transitive component-framework dependency.
See the [MudBlazor decoupling migration](docs/migration-mudblazor.md) when upgrading
from the former Mud-backed provider contract.

## Repository

- Source: <https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor>
- Live interactive demo: <https://maliev-co-ltd.github.io/Maliev.ShadcnBlazor/>
- Getting started: [docs/getting-started.md](docs/getting-started.md)
- Component catalog: [docs/components.md](docs/components.md)
- Theming: [docs/theming.md](docs/theming.md)
- Agent Skills: [docs/agent-skills.md](docs/agent-skills.md)
- Contributing: [CONTRIBUTING.md](CONTRIBUTING.md)
- Support: [SUPPORT.md](SUPPORT.md)
- Security policy: [SECURITY.md](SECURITY.md)
Maliev.ShadcnBlazor is licensed under the [MIT License](LICENSE). Third-party
attribution is recorded in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
