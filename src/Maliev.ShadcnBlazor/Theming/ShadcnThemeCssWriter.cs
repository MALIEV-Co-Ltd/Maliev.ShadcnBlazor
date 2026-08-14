using System.Globalization;
using System.Text;

namespace Maliev.ShadcnBlazor.Theming;

public static class ShadcnThemeCssWriter
{
    public static string Write(ShadcnTheme theme)
    {
        EnsureValid(theme);
        var builder = new StringBuilder();
        AppendBlock(builder, "light", theme.Light, theme.Metrics);
        builder.Append('\n');
        AppendBlock(builder, "dark", theme.Dark, theme.Metrics);
        return builder.ToString();
    }

    internal static string WriteProperties(ShadcnTheme theme, bool darkMode)
    {
        EnsureValid(theme);
        var scheme = darkMode ? theme.Dark : theme.Light;
        return string.Join("; ", GetDeclarations(scheme, theme.Metrics)
            .Select(declaration => $"{declaration.Name}: {declaration.Value}"));
    }

    internal static void EnsureValid(ShadcnTheme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        var validation = ShadcnThemeValidator.Validate(theme);
        if (!validation.IsValid)
        {
            throw new ArgumentException(
                "Theme is invalid: " + string.Join("; ", validation.Errors.Select(error => $"{error.Path}: {error.Message}")),
                nameof(theme));
        }
    }

    private static void AppendBlock(
        StringBuilder builder,
        string schemeName,
        ShadcnColorScheme scheme,
        ShadcnThemeMetrics metrics)
    {
        builder.Append(".shadcn-scope[data-shadcn-theme=\"")
            .Append(schemeName)
            .Append("\"],\n.shadcn-overlay-scope[data-shadcn-theme=\"")
            .Append(schemeName)
            .Append("\"] {\n");
        foreach (var declaration in GetDeclarations(scheme, metrics))
            builder.Append("  ").Append(declaration.Name).Append(": ").Append(declaration.Value).Append(";\n");
        builder.Append("}\n");
    }

    private static IReadOnlyList<(string Name, string Value)> GetDeclarations(
        ShadcnColorScheme scheme,
        ShadcnThemeMetrics metrics) =>
    [
        ("--shadcn-font-sans", metrics.FontFamily),
        ("--shadcn-font-mono", metrics.MonospaceFontFamily),
        ("--shadcn-typeset-font-mono", metrics.MonospaceFontFamily),
        ("--shadcn-background", scheme.Background),
        ("--shadcn-foreground", scheme.Foreground),
        ("--shadcn-card", scheme.Card),
        ("--shadcn-card-foreground", scheme.CardForeground),
        ("--shadcn-popover", scheme.Popover),
        ("--shadcn-popover-foreground", scheme.PopoverForeground),
        ("--shadcn-primary", scheme.Primary),
        ("--shadcn-primary-foreground", scheme.PrimaryForeground),
        ("--shadcn-secondary", scheme.Secondary),
        ("--shadcn-secondary-foreground", scheme.SecondaryForeground),
        ("--shadcn-muted", scheme.Muted),
        ("--shadcn-muted-foreground", scheme.MutedForeground),
        ("--shadcn-accent", scheme.Accent),
        ("--shadcn-accent-foreground", scheme.AccentForeground),
        ("--shadcn-destructive", scheme.Destructive),
        ("--shadcn-destructive-foreground", scheme.DestructiveForeground),
        ("--shadcn-border", scheme.Border),
        ("--shadcn-input", scheme.Input),
        ("--shadcn-ring", scheme.Ring),
        ("--shadcn-chart-1", scheme.Chart1),
        ("--shadcn-chart-2", scheme.Chart2),
        ("--shadcn-chart-3", scheme.Chart3),
        ("--shadcn-chart-4", scheme.Chart4),
        ("--shadcn-chart-5", scheme.Chart5),
        ("--shadcn-sidebar", scheme.Sidebar),
        ("--shadcn-sidebar-foreground", scheme.SidebarForeground),
        ("--shadcn-sidebar-primary", scheme.SidebarPrimary),
        ("--shadcn-sidebar-primary-foreground", scheme.SidebarPrimaryForeground),
        ("--shadcn-sidebar-accent", scheme.SidebarAccent),
        ("--shadcn-sidebar-accent-foreground", scheme.SidebarAccentForeground),
        ("--shadcn-sidebar-border", scheme.SidebarBorder),
        ("--shadcn-sidebar-ring", scheme.SidebarRing),
        ("--shadcn-radius", $"{Format(metrics.RadiusRem)}rem"),
        ("--shadcn-radius-sm", Scale(metrics.RadiusSmallScale)),
        ("--shadcn-radius-md", Scale(metrics.RadiusMediumScale)),
        ("--shadcn-radius-lg", Scale(metrics.RadiusLargeScale)),
        ("--shadcn-radius-xl", Scale(metrics.RadiusExtraLargeScale)),
        ("--shadcn-radius-2xl", Scale(metrics.Radius2ExtraLargeScale)),
        ("--shadcn-radius-3xl", Scale(metrics.Radius3ExtraLargeScale)),
        ("--shadcn-radius-4xl", Scale(metrics.Radius4ExtraLargeScale)),
        ("--shadcn-control-height", $"{Format(metrics.ControlHeightRem)}rem"),
        ("--shadcn-control-height-sm", $"{Format(metrics.ControlHeightSmallRem)}rem"),
        ("--shadcn-control-height-lg", $"{Format(metrics.ControlHeightLargeRem)}rem"),
        ("--shadcn-spacing-multiplier", Format(metrics.SpacingScaleMultiplier)),
        ("--shadcn-focus-ring-width", $"{Format(metrics.FocusRingWidthPx)}px"),
        ("--shadcn-focus-ring-offset", $"{Format(metrics.FocusRingOffsetPx)}px"),
        ("--shadcn-motion-duration", MotionDuration(metrics)),
        ("--shadcn-motion-duration-fast", FastMotionDuration(metrics)),
        ("--shadcn-motion-duration-slow", SlowMotionDuration(metrics)),
        ("--shadcn-motion-easing", metrics.MotionEasing),
        ("--shadcn-motion-easing-standard", StandardMotionEasing(metrics)),
        ("--shadcn-motion-easing-enter", metrics.MotionEasing),
        ("--shadcn-reduced-motion-duration", "0.01ms"),
        ("--shadcn-shadow-xs", scheme.ShadowExtraSmall),
        ("--shadcn-shadow-sm", scheme.ShadowSmall),
        ("--shadcn-shadow-md", scheme.ShadowMedium)
    ];

    private static string Scale(double scale) => scale == 1
        ? "var(--shadcn-radius)"
        : $"calc(var(--shadcn-radius) * {Format(scale)})";

    private static string Format(double value) => value.ToString("R", CultureInfo.InvariantCulture);

    private static string MotionDuration(ShadcnThemeMetrics metrics) =>
        $"{metrics.MotionDurationMilliseconds.ToString(CultureInfo.InvariantCulture)}ms";

    private static string FastMotionDuration(ShadcnThemeMetrics metrics) =>
        $"{Format(metrics.MotionDurationMilliseconds * (2d / 3d))}ms";

    private static string SlowMotionDuration(ShadcnThemeMetrics metrics) =>
        $"{Format(metrics.MotionDurationMilliseconds * (28d / 3d))}ms";

    private static string StandardMotionEasing(ShadcnThemeMetrics metrics) =>
        metrics.MotionEasing == "ease-out" ? "ease" : metrics.MotionEasing;
}
