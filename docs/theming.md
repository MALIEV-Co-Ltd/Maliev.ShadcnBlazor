# Theming

`ShadcnThemeProvider` scopes semantic color, typography, radius, spacing, and
motion tokens to its rendered subtree. It also coordinates the compatible
MudBlazor theme and overlay providers.

## Configure defaults

```csharp
builder.Services.AddMalievShadcn(options =>
{
    options.DefaultDarkMode = true;
    options.DefaultDirection = ShadcnDirection.RightToLeft;
    options.FontFamily = "system-ui, sans-serif";
});
```

Provider parameters can change `IsDarkMode`, `Direction`, and the theme at
runtime. Use logical CSS properties in application extensions so LTR and RTL
remain equivalent.

## Theme presets and export

The theming namespace exposes built-in presets, validation, JSON
serialization, and CSS/C# writers. The Showcase Theme Studio can edit a theme,
preview it in multiple layouts, and export an integration bundle.

Validate imported or generated themes before use. The validator rejects
invalid semantic values rather than emitting unsafe CSS. Applications should
still treat user-provided theme files as untrusted input and enforce their own
upload size and storage policies.

## Accessibility preferences

Component styles include forced-colors and reduced-motion behavior. Avoid
overriding these media queries with application rules. Test custom themes in
normal light and dark modes, RTL, forced colors, reduced motion, and at 200%
zoom before release.
