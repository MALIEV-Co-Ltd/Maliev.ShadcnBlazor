# Getting started

## Requirements

- .NET 10 SDK and a .NET 10 Blazor application.

## Install and register

```bash
dotnet add package Maliev.ShadcnBlazor --version 2.1.2
```

```csharp
using Maliev.ShadcnBlazor;
using Maliev.ShadcnBlazor.Theming;

builder.Services.AddMalievShadcn(options =>
{
    options.DefaultDarkMode = false;
    options.DefaultDirection = ShadcnDirection.LeftToRight;
});
```

Add the namespaces needed by your components to `_Imports.razor`:

```razor
@using Maliev.ShadcnBlazor.Components
@using Maliev.ShadcnBlazor.Components.Actions
@using Maliev.ShadcnBlazor.Components.Selection
@using Maliev.ShadcnBlazor.Theming
```

Wrap the rendered application content once:

```razor
<ShadcnThemeProvider>
    @Body
</ShadcnThemeProvider>
```

The provider renders only package-owned markup. Dialogs, menus, popovers, tooltips,
and toasts need no external root provider.

## Styles and scripts

Load the Shadcn base and semantic styles, followed by the
component-family styles you use. Loading all family files is supported:

```html
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-base.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-semantic-foundations.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-actions.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-data-display.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-disclosure-navigation.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-forms.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-feedback-content.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-overlays-menus.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-conversation.css" rel="stylesheet" />
```

Component-specific JavaScript modules are imported on demand by the library.

## First component

```razor
<ShadcnButton Variant="ShadcnButtonVariant.Default" @onclick="SaveAsync">
    Save
</ShadcnButton>
```

Prefer native Blazor binding and callbacks. Components forward unmatched HTML
attributes so applications can add `data-*`, `aria-*`, and testing attributes
without wrapper elements.

## Next steps

- Browse the [component catalog](components.md).
- Configure [themes and direction](theming.md).
- Run the Showcase with
  `dotnet run --project samples/Maliev.ShadcnBlazor.Showcase`.
