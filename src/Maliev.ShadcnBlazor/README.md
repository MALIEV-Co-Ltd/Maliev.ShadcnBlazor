# Maliev.ShadcnBlazor

Reusable Shadcn Base/Vega/Neutral components for .NET 10 Blazor, backed by MudBlazor 9.7.0.

## Register

```csharp
using Maliev.ShadcnBlazor;
using Maliev.ShadcnBlazor.Theming;

builder.Services.AddMalievShadcn(options =>
{
    options.FontFamily = "'IBM Plex Sans', 'IBM Plex Sans Thai', ui-sans-serif, system-ui, sans-serif";
    options.DefaultDarkMode = false;
    options.DefaultDirection = ShadcnDirection.LeftToRight;
});
```

The configured font family is applied to both MudBlazor typography and the scoped
`--shadcn-font-sans` semantic token. Provider parameters override the configured defaults.

## Load assets in this order

```html
<link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
<link rel="preconnect" href="https://fonts.googleapis.com" />
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
<link href="https://fonts.googleapis.com/css2?family=IBM+Plex+Mono:wght@400;500;600;700&family=IBM+Plex+Sans+Thai:wght@400;500;600;700&family=IBM+Plex+Sans:wght@400;500;600;700&display=swap" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-base.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-semantic-foundations.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-actions.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-data-display.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-forms.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-feedback-content.css" rel="stylesheet" />
<link href="_content/Maliev.ShadcnBlazor/css/shadcn-mudblazor.css" rel="stylesheet" />
<script src="_content/MudBlazor/MudBlazor.min.js"></script>
```

## Provide theme and portals

Add the component and theme namespaces to the consuming application's `_Imports.razor`:

```razor
@using Maliev.ShadcnBlazor.Components
@using Maliev.ShadcnBlazor.Theming
```

Then wrap the application content at its root:

```razor
<ShadcnThemeProvider>
    @Body
</ShadcnThemeProvider>
```

Set `IsDarkMode` or `Direction` on the provider when an application needs to override either
configured default dynamically.

Do not also render `MudThemeProvider`, `MudPopoverProvider`, `MudDialogProvider`, or `MudSnackbarProvider` in the same application root.

## Semantic foundations

The first reusable component family includes `ShadcnDirectionProvider`, `ShadcnAspectRatio`,
`ShadcnTypeset`, `ShadcnTypography`, `ShadcnLabel`, the complete `ShadcnField*` composition,
the complete `ShadcnItem*` composition, `ShadcnKbd`, `ShadcnKbdGroup`, `ShadcnSeparator`, and
the complete `ShadcnEmpty*` composition. Every visual root supports `Class`, `Style`, and
unmatched HTML attributes through the common component contract.

```razor
<ShadcnField DescriptionId="project-help" ErrorId="project-error" Invalid="@hasError">
    <ShadcnFieldLabel For="project-name">Project name</ShadcnFieldLabel>
    <input id="project-name" aria-describedby="project-help project-error" />
    <ShadcnFieldDescription>Use the customer-facing project name.</ShadcnFieldDescription>
    <ShadcnFieldError Id="project-error" Errors="@errors" />
</ShadcnField>
```

## Actions and selection

Import `Maliev.ShadcnBlazor.Components.Actions` and
`Maliev.ShadcnBlazor.Components.Selection` for the Button, Button Group, Toggle, Toggle Group,
Checkbox, Radio Group, Switch, and Slider families. Value controls are controlled Blazor
components: bind the value and provide `ValueExpression` automatically with `@bind-Value`
inside an `EditForm` when validation tracking is required.

```razor
<EditForm Model="@settings">
    <ShadcnCheckbox @bind-Value="settings.Accepted" Name="accepted" />

    <ShadcnRadioGroup TValue="string" @bind-Value="settings.Density" Name="density">
        <ShadcnRadioGroupItem TValue="string" Value="@("default")">Default</ShadcnRadioGroupItem>
        <ShadcnRadioGroupItem TValue="string" Value="@("compact")">Compact</ShadcnRadioGroupItem>
    </ShadcnRadioGroup>

    <ShadcnSlider @bind-Values="settings.Budget" Minimum="0" Maximum="100" Step="5" />
    <ShadcnSwitch @bind-Value="settings.Notifications" />
    <ShadcnButton ButtonType="ShadcnButtonType.Submit">Save</ShadcnButton>
</EditForm>
```

`ReadOnly` keeps Checkbox, Radio Group, Switch, and Slider inspectable while suppressing DOM
and model mutation. Toggle Group and Radio Group use orientation-aware roving focus, including
Home/End, disabled-item skipping, and RTL horizontal arrows. Slider supports one or more ordered
thumbs, nearest-thumb pointer input and drag, vertical orientation, RTL, and coarse-pointer
targets. Use `Name`, `Form`, and `Required` for shared native form behavior or pass one
`ShadcnSliderThumbAttributes` entry per value for stable ID, name, external form owner,
required state, accessible name, and additional per-thumb input attributes.

Button and Toggle intentionally use the platform cursor by default. Set `PointerCursor="true"`
when the product's interaction language calls for an explicit pointer cursor.

Review the live family fixture at `/components/actions-and-selection` and each copyable dossier
at `/docs/components/{slug}`. Use `/theme` to customize semantic tokens and export the complete
CSS/C# integration bundle.

## MudBlazor version boundary

`Maliev.ShadcnBlazor` is built against MudBlazor **9.7.0** and its adapter selectors, state classes, and portal markup are supported only within the MudBlazor 9.7 line. Keep the consuming application on MudBlazor 9.7.x (the package pins 9.7.0); upgrading MudBlazor requires revalidating the adapter contracts and browser inventory before adoption.
