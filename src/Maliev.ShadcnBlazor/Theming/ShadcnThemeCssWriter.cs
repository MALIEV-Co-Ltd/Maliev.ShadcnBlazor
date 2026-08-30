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

    /// <summary>Writes deterministic CSS variables for a complete portable theme document.</summary>
    public static string Write(ShadcnThemeDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var validation = ShadcnThemeDocumentValidator.Validate(document);
        if (!validation.IsValid)
            throw new ArgumentException(
                "Theme document is invalid: " + string.Join("; ", validation.Errors.Select(error => $"{error.Path}: {error.Message}")),
                nameof(document));

        var builder = new StringBuilder();
        AppendDocumentBlock(builder, "light", document.Theme.Light, document.Theme.Metrics, document.Typography);
        builder.Append('\n');
        AppendDocumentBlock(builder, "dark", document.Theme.Dark, document.Theme.Metrics, document.Typography);
        return builder.ToString();
    }

    internal static string WriteProperties(ShadcnTheme theme, bool darkMode)
    {
        EnsureValid(theme);
        var scheme = darkMode ? theme.Dark : theme.Light;
        return string.Join("; ", GetDeclarations(scheme, theme.Metrics)
            .Select(declaration => $"{declaration.Name}: {declaration.Value}"));
    }

    /// <summary>Writes inline-safe CSS custom properties for a complete portable theme document.</summary>
    public static string WriteProperties(ShadcnThemeDocument document, bool darkMode)
    {
        ArgumentNullException.ThrowIfNull(document);
        var validation = ShadcnThemeDocumentValidator.Validate(document);
        if (!validation.IsValid)
            throw new ArgumentException("A valid theme document is required.", nameof(document));
        var scheme = darkMode ? document.Theme.Dark : document.Theme.Light;
        return string.Join("; ", GetDocumentDeclarations(scheme, document.Theme.Metrics, document.Typography)
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

    private static void AppendDocumentBlock(
        StringBuilder builder,
        string schemeName,
        ShadcnColorScheme scheme,
        ShadcnThemeMetrics metrics,
        ShadcnTypographyScale typography)
    {
        builder.Append(".shadcn-scope[data-shadcn-theme=\"")
            .Append(schemeName)
            .Append("\"],\n.shadcn-overlay-scope[data-shadcn-theme=\"")
            .Append(schemeName)
            .Append("\"] {\n");
        foreach (var declaration in GetDocumentDeclarations(scheme, metrics, typography))
            builder.Append("  ").Append(declaration.Name).Append(": ").Append(declaration.Value).Append(";\n");
        builder.Append("}\n");
    }

    private static IEnumerable<(string Name, string Value)> GetDocumentDeclarations(
        ShadcnColorScheme scheme,
        ShadcnThemeMetrics metrics,
        ShadcnTypographyScale typography)
    {
        var effectiveSansFamily = ComposeSansFamily(typography);
        foreach (var declaration in GetDeclarations(scheme, metrics))
            yield return declaration.Name == "--shadcn-font-sans"
                ? (declaration.Name, effectiveSansFamily)
                : declaration;
        yield return ("--shadcn-font-thai", typography.ThaiFallback.Family);
        foreach (var role in Enum.GetValues<ShadcnTypographyRole>())
        {
            var style = typography.Roles[role];
            var name = RoleName(role);
            yield return ($"--shadcn-typography-{name}-weight", style.Weight.ToString(CultureInfo.InvariantCulture));
            yield return ($"--shadcn-typography-{name}-scale", Format(style.Scale));
            yield return ($"--shadcn-typography-{name}-line-height", Format(style.LineHeight));
            yield return ($"--shadcn-typography-{name}-letter-spacing", $"{Format(style.LetterSpacingEm)}em");
        }
    }

    private static string ComposeSansFamily(ShadcnTypographyScale typography)
    {
        var (bodyPrimary, bodyFallback) = SeparateFontFamily(
            typography.Body.Family,
            typography.Body.Fallback);
        var (thaiPrimary, thaiFallback) = SeparateFontFamily(
            typography.ThaiFallback.Family,
            typography.ThaiFallback.Fallback);
        var thaiNames = thaiPrimary.Concat(thaiFallback)
            .Select(NormalizeFamily)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var ordered = new List<string>(
            bodyPrimary.Count + bodyFallback.Count + thaiPrimary.Count + thaiFallback.Count);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        Add(bodyPrimary.Where(token => !IsGenericFamily(token) && !thaiNames.Contains(NormalizeFamily(token))));
        Add(thaiPrimary.Where(token => !IsGenericFamily(token)));
        Add(thaiFallback.Where(token => !IsGenericFamily(token)));
        Add(bodyFallback.Where(token => !IsGenericFamily(token) && !thaiNames.Contains(NormalizeFamily(token))));
        Add(bodyPrimary.Where(IsGenericFamily));
        Add(bodyFallback.Where(IsGenericFamily));
        Add(thaiPrimary.Where(IsGenericFamily));
        Add(thaiFallback.Where(IsGenericFamily));
        return string.Join(", ", ordered);

        void Add(IEnumerable<string> tokens)
        {
            foreach (var token in tokens)
            {
                if (seen.Add(NormalizeFamily(token)))
                    ordered.Add(token);
            }
        }
    }

    private static (IReadOnlyList<string> Primary, IReadOnlyList<string> Fallback) SeparateFontFamily(
        string family,
        string fallback)
    {
        var familyTokens = ParseFontFamily(family).ToArray();
        var fallbackTokens = ParseFontFamily(fallback).ToArray();
        var hasFallbackSuffix = fallbackTokens.Length > 0 &&
            familyTokens.Length >= fallbackTokens.Length &&
            familyTokens[^fallbackTokens.Length..]
                .Select(NormalizeFamily)
                .SequenceEqual(fallbackTokens.Select(NormalizeFamily), StringComparer.OrdinalIgnoreCase);
        return hasFallbackSuffix
            ? (familyTokens[..^fallbackTokens.Length], familyTokens[^fallbackTokens.Length..])
            : (familyTokens, fallbackTokens);
    }

    private static IEnumerable<string> ParseFontFamily(string value)
    {
        var start = 0;
        var quote = '\0';
        var escaped = false;
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (escaped)
            {
                escaped = false;
                continue;
            }
            if (character == '\\' && quote != '\0')
            {
                escaped = true;
                continue;
            }
            if (quote == '\0' && character is '\'' or '"')
            {
                quote = character;
                continue;
            }
            if (character == quote)
            {
                quote = '\0';
                continue;
            }
            if (quote == '\0' && character == ',')
            {
                var token = value[start..index].Trim();
                if (token.Length > 0)
                    yield return token;
                start = index + 1;
            }
        }

        var finalToken = value[start..].Trim();
        if (finalToken.Length > 0)
            yield return finalToken;
    }

    private static bool IsGenericFamily(string value) => NormalizeFamily(value) is
        "serif" or "sans-serif" or "monospace" or "cursive" or "fantasy" or
        "system-ui" or "ui-serif" or "ui-sans-serif" or "ui-monospace" or
        "ui-rounded" or "math" or "emoji" or "fangsong";

    private static string NormalizeFamily(string value)
    {
        var normalized = value.Trim();
        if (normalized.Length >= 2 &&
            normalized[0] is '\'' or '"' &&
            normalized[^1] == normalized[0])
            normalized = normalized[1..^1];
        return normalized.Replace("\\'", "'", StringComparison.Ordinal)
            .Replace("\\\"", "\"", StringComparison.Ordinal)
            .Trim()
            .ToLowerInvariant();
    }

    private static string RoleName(ShadcnTypographyRole role) => role switch
    {
        ShadcnTypographyRole.Body => "body",
        ShadcnTypographyRole.Heading1 => "heading-1",
        ShadcnTypographyRole.Heading2 => "heading-2",
        ShadcnTypographyRole.Heading3 => "heading-3",
        ShadcnTypographyRole.Heading4To6 => "heading-4-to-6",
        ShadcnTypographyRole.Label => "label",
        ShadcnTypographyRole.Button => "button",
        ShadcnTypographyRole.Caption => "caption",
        ShadcnTypographyRole.Code => "code",
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown typography role.")
    };

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
