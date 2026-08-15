namespace Maliev.ShadcnBlazor.Theming;

public static class ShadcnThemePresets
{
    private static readonly ShadcnThemePreset BaseVegaNeutralTemplate = new(
        "base-vega-neutral",
        "Base / Vega / Neutral",
        new ShadcnTheme
        {
            Name = "Base / Vega / Neutral",
            Light = CreateLight(),
            Dark = CreateDark(),
            Metrics = CreateMetrics()
        });

    public static ShadcnThemePreset BaseVegaNeutral => Clone(BaseVegaNeutralTemplate);

    public static IReadOnlyList<ShadcnThemePreset> All => [BaseVegaNeutral];

    private static ShadcnThemePreset Clone(ShadcnThemePreset preset) => preset with
    {
        Theme = preset.Theme.DeepClone()
    };

    private static ShadcnColorScheme CreateLight() => new()
    {
        Background = "oklch(1 0 0)",
        Foreground = "oklch(0.145 0 0)",
        Card = "oklch(1 0 0)",
        CardForeground = "oklch(0.145 0 0)",
        Popover = "oklch(1 0 0)",
        PopoverForeground = "oklch(0.145 0 0)",
        Primary = "oklch(0.205 0 0)",
        PrimaryForeground = "oklch(0.985 0 0)",
        Secondary = "oklch(0.97 0 0)",
        SecondaryForeground = "oklch(0.205 0 0)",
        Muted = "oklch(0.97 0 0)",
        MutedForeground = "oklch(0.556 0 0)",
        Accent = "oklch(0.97 0 0)",
        AccentForeground = "oklch(0.205 0 0)",
        Destructive = "oklch(0.577 0.245 27.325)",
        DestructiveForeground = "oklch(0.985 0 0)",
        Border = "oklch(0.922 0 0)",
        Input = "oklch(0.922 0 0)",
        Ring = "oklch(0.708 0 0)",
        Chart1 = "oklch(0.646 0.222 41.116)",
        Chart2 = "oklch(0.6 0.118 184.704)",
        Chart3 = "oklch(0.398 0.07 227.392)",
        Chart4 = "oklch(0.828 0.189 84.429)",
        Chart5 = "oklch(0.769 0.188 70.08)",
        Sidebar = "oklch(0.985 0 0)",
        SidebarForeground = "oklch(0.145 0 0)",
        SidebarPrimary = "oklch(0.205 0 0)",
        SidebarPrimaryForeground = "oklch(0.985 0 0)",
        SidebarAccent = "oklch(0.97 0 0)",
        SidebarAccentForeground = "oklch(0.205 0 0)",
        SidebarBorder = "oklch(0.922 0 0)",
        SidebarRing = "oklch(0.708 0 0)",
        ShadowExtraSmall = "0 1px 2px rgb(0 0 0 / 0.05)",
        ShadowSmall = "0 1px 3px rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1)",
        ShadowMedium = "0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)"
    };

    private static ShadcnColorScheme CreateDark() => new()
    {
        Background = "oklch(0.145 0 0)",
        Foreground = "oklch(0.985 0 0)",
        Card = "oklch(0.205 0 0)",
        CardForeground = "oklch(0.985 0 0)",
        Popover = "oklch(0.205 0 0)",
        PopoverForeground = "oklch(0.985 0 0)",
        Primary = "oklch(0.922 0 0)",
        PrimaryForeground = "oklch(0.205 0 0)",
        Secondary = "oklch(0.269 0 0)",
        SecondaryForeground = "oklch(0.985 0 0)",
        Muted = "oklch(0.269 0 0)",
        MutedForeground = "oklch(0.708 0 0)",
        Accent = "oklch(0.269 0 0)",
        AccentForeground = "oklch(0.985 0 0)",
        Destructive = "oklch(0.704 0.191 22.216)",
        DestructiveForeground = "oklch(0.985 0 0)",
        Border = "oklch(1 0 0 / 10%)",
        Input = "oklch(1 0 0 / 15%)",
        Ring = "oklch(0.556 0 0)",
        Chart1 = "oklch(0.488 0.243 264.376)",
        Chart2 = "oklch(0.696 0.17 162.48)",
        Chart3 = "oklch(0.769 0.188 70.08)",
        Chart4 = "oklch(0.627 0.265 303.9)",
        Chart5 = "oklch(0.645 0.246 16.439)",
        Sidebar = "oklch(0.205 0 0)",
        SidebarForeground = "oklch(0.985 0 0)",
        SidebarPrimary = "oklch(0.488 0.243 264.376)",
        SidebarPrimaryForeground = "oklch(0.985 0 0)",
        SidebarAccent = "oklch(0.269 0 0)",
        SidebarAccentForeground = "oklch(0.985 0 0)",
        SidebarBorder = "oklch(1 0 0 / 10%)",
        SidebarRing = "oklch(0.556 0 0)",
        ShadowExtraSmall = "0 1px 2px rgb(0 0 0 / 0.24)",
        ShadowSmall = "0 1px 2px rgb(0 0 0 / 0.28), 0 0 0 1px rgb(255 255 255 / 0.06)",
        ShadowMedium = "0 8px 24px rgb(0 0 0 / 0.36), 0 0 0 1px rgb(255 255 255 / 0.08)"
    };

    private static ShadcnThemeMetrics CreateMetrics() => new()
    {
        FontFamily = "'IBM Plex Sans', 'IBM Plex Sans Thai', ui-sans-serif, system-ui, sans-serif",
        MonospaceFontFamily = "'IBM Plex Mono', ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace",
        RadiusRem = 0.625,
        RadiusSmallScale = 0.6,
        RadiusMediumScale = 0.8,
        RadiusLargeScale = 1,
        RadiusExtraLargeScale = 1.4,
        Radius2ExtraLargeScale = 1.8,
        Radius3ExtraLargeScale = 2.2,
        Radius4ExtraLargeScale = 2.6,
        ControlHeightRem = 2.25,
        ControlHeightSmallRem = 2,
        ControlHeightLargeRem = 2.5,
        SpacingScaleMultiplier = 1,
        FocusRingWidthPx = 3,
        FocusRingOffsetPx = 0,
        MotionDurationMilliseconds = 150,
        MotionEasing = "ease-out",
        ReducedMotionBehavior = ShadcnReducedMotionBehavior.RespectSystemPreference
    };
}
