# Consumer setup and component selection

Use this reference only for application integration. The installed package and
its XML documentation remain authoritative for the exact version in use.

## Install and register

```bash
dotnet add package Maliev.ShadcnBlazor
```

```csharp
using Maliev.ShadcnBlazor;

builder.Services.AddMalievShadcn();
```

Add the component and theming namespaces to `_Imports.razor`, then place one
provider around the application body:

```razor
@using Maliev.ShadcnBlazor.Components
@using Maliev.ShadcnBlazor.Theming

<ShadcnThemeProvider>
    @Body
</ShadcnThemeProvider>
```

## Static assets

Load MudBlazor first, then the Maliev.ShadcnBlazor layers in this order. An app
may omit documented component-family layers it never uses, but must not reorder
the layers it keeps.

```html
<link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
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
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-mudblazor.css" rel="stylesheet" />
<script src="_content/MudBlazor/MudBlazor.min.js"></script>
```

## Choose from evidence, not name similarity

1. Start with the [live component documentation](https://maliev-co-ltd.github.io/Maliev.ShadcnBlazor/docs/components).
2. Check the repository [component catalog](https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/blob/main/docs/component-catalog.json) for the canonical component name and category.
3. Use the dossier's Razor example and API table for the installed version.
4. Prefer a native semantic primitive when it already covers the need. Compose
   multiple package components only when the workflow requires it.

## Consumer verification

At minimum:

```bash
dotnet build <path-to-app-project> -c Release
dotnet test <path-to-app-test-project> -c Release
```

For interactive work, also test real keyboard and pointer behavior, focus
return for overlays, validation relationships, a narrow viewport, and both
light and dark themes. Include RTL, forced colors, reduced motion, and zoom
when the affected UI changes under those conditions.
