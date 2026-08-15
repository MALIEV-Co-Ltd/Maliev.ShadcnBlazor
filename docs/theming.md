# Theming

`ShadcnThemeProvider` scopes semantic color, typography, radius, spacing, and
motion tokens to its rendered subtree. It also coordinates the compatible
MudBlazor theme and overlay providers.

The default typography stack uses the bundled Geist, Noto Sans Thai, and
JetBrains Mono web fonts. Latin text uses Geist, Thai text switches to Noto
Sans Thai, and code/keyboard/monospace content uses JetBrains Mono.
Because the font files ship as static package assets, this default remains
deterministic in offline and self-hosted deployments. Applications can replace
either stack with a validated theme metric or `ShadcnOptions.FontFamily` value.

## Configure defaults

```csharp
builder.Services.AddMalievShadcn(options =>
{
    options.DefaultDarkMode = true;
    options.DefaultDirection = ShadcnDirection.RightToLeft;
    options.FontFamily = "'Geist', 'Noto Sans Thai', ui-sans-serif, system-ui, sans-serif";
});
```

Provider parameters can change `IsDarkMode`, `Direction`, and the theme at
runtime. Use logical CSS properties in application extensions so LTR and RTL
remain equivalent.

## Theme presets and export

The theming namespace exposes built-in presets, validation, JSON
serialization, and CSS/C# writers. The Showcase Theme Studio can edit a theme,
preview it in multiple layouts, and export an integration bundle.

## Generate a portable Blazor configuration

Open `/theme` and use **Generator options** to choose the component style, base
color, icon library, radius, menu accent, menu surface, and typography. **Get
code** produces two equivalent artifacts:

- `maliev-shadcn-theme.json` is a versioned, portable source of truth. Keep it
  in your design-system repository, review it like any other configuration, and
  import it back into the Studio when you need to continue editing.
- `MalievShadcnTheme.cs` is a ready-to-paste typed factory. It includes the
  semantic `ShadcnTheme`, the generated `ShadcnOptions` defaults, and the
  application metadata (style, base color, icon library, radius, and menu
  treatment) that a Blazor app must wire into its own icon/menu adapters.

The generated C# is intentionally application-owned: the library does not
silently install an icon package or impose a navigation implementation. Register
the package and provider once, then use the generated theme factory:

```csharp
builder.Services.AddMalievShadcn(MalievShadcnTheme.Configure);
```

The JSON schema is strict and versioned. Unknown options, invalid semantic
tokens, and unsupported future schema versions are rejected transactionally;
the current preview remains unchanged when an import fails.

Validate imported or generated themes before use. The validator rejects
invalid semantic values rather than emitting unsafe CSS. Applications should
still treat user-provided theme files as untrusted input and enforce their own
upload size and storage policies.

## Accessibility preferences

Component styles include forced-colors and reduced-motion behavior. Avoid
overriding these media queries with application rules. Test custom themes in
normal light and dark modes, RTL, forced colors, reduced motion, and at 200%
zoom before release.
