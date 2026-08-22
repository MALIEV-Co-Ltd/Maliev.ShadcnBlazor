using System.Globalization;
using System.Text.RegularExpressions;

namespace Maliev.ShadcnBlazor.Theming;

public static partial class ShadcnThemeValidator
{
    /// <summary>WCAG AA minimum contrast for normal text.</summary>
    public const double TextContrastRatio = 4.5;

    /// <summary>WCAG minimum contrast for focus indicators and visual boundaries.</summary>
    public const double NonTextContrastRatio = 3;

    private static readonly (string Foreground, string Background)[] ContrastPairs =
    [
        ("foreground", "background"),
        ("cardForeground", "card"),
        ("popoverForeground", "popover"),
        ("primaryForeground", "primary"),
        ("secondaryForeground", "secondary"),
        ("mutedForeground", "muted"),
        ("accentForeground", "accent"),
        ("destructiveForeground", "destructive"),
        ("sidebarForeground", "sidebar"),
        ("sidebarPrimaryForeground", "sidebarPrimary"),
        ("sidebarAccentForeground", "sidebarAccent")
    ];

    public static ShadcnThemeValidationResult Validate(ShadcnTheme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);

        var errors = new List<ShadcnThemeValidationMessage>();
        var warnings = new List<ShadcnThemeValidationMessage>();
        var contrastResults = new List<ShadcnContrastResult>();

        ValidateSchema(theme, errors);
        ValidateName(theme.Name, errors);
        ValidateScheme("light", theme.Light, errors);
        ValidateScheme("dark", theme.Dark, errors);
        ValidateMetrics(theme.Metrics, errors);

        MeasureContrast("light", theme.Light, errors, warnings, contrastResults);
        MeasureContrast("dark", theme.Dark, errors, warnings, contrastResults);

        return new ShadcnThemeValidationResult(errors.AsReadOnly(), warnings.AsReadOnly(), contrastResults.AsReadOnly());
    }

    internal static string ToHexColor(string value)
    {
        if (!TryParseColor(value, out var color))
            throw new ArgumentException("The value is not a supported Shadcn color.", nameof(value));

        var valueWithoutAlpha = $"#{ToByte(color.Red):x2}{ToByte(color.Green):x2}{ToByte(color.Blue):x2}";
        return color.Alpha >= 1
            ? valueWithoutAlpha
            : $"{valueWithoutAlpha}{ToAlphaByte(color.Alpha):x2}";
    }

    private static int ToAlphaByte(double alpha) =>
        (int)Math.Round(Math.Clamp(alpha, 0, 1) * 255, MidpointRounding.AwayFromZero);

    private static int ToByte(double linear)
    {
        var srgb = ToSrgb(linear);
        return (int)Math.Round(Math.Clamp(srgb, 0, 1) * 255, MidpointRounding.AwayFromZero);
    }

    private static double ToSrgb(double linear) => linear <= 0.0031308
        ? 12.92 * linear
        : (1.055 * Math.Pow(linear, 1d / 2.4d)) - 0.055;

    private static void ValidateSchema(ShadcnTheme theme, ICollection<ShadcnThemeValidationMessage> errors)
    {
        if (theme.SchemaVersion != ShadcnTheme.CurrentSchemaVersion)
        {
            errors.Add(new ShadcnThemeValidationMessage(
                "unsupported-schema",
                "schemaVersion",
                $"Theme schema version must be {ShadcnTheme.CurrentSchemaVersion}."));
        }
    }

    private static void ValidateName(string? name, ICollection<ShadcnThemeValidationMessage> errors)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100 || name.Any(char.IsControl) ||
            name.IndexOfAny([';', '{', '}', '<', '>']) >= 0)
        {
            errors.Add(new ShadcnThemeValidationMessage(
                "invalid-name",
                "name",
                "Theme name must be 1 to 100 characters and cannot contain control, markup, brace, or semicolon characters."));
        }
    }

    private static void ValidateScheme(
        string schemeName,
        ShadcnColorScheme? scheme,
        ICollection<ShadcnThemeValidationMessage> errors)
    {
        if (scheme is null)
        {
            errors.Add(new ShadcnThemeValidationMessage("required-scheme", schemeName, "Color scheme is required."));
            return;
        }

        foreach (var (name, value) in GetColorTokens(scheme))
        {
            if (!TryParseColor(value, out _))
            {
                errors.Add(new ShadcnThemeValidationMessage(
                    "invalid-color",
                    $"{schemeName}.{name}",
                    "Color must be a hexadecimal or oklch() value in the supported canonical syntax."));
            }
        }

        foreach (var (name, value) in GetShadowTokens(scheme))
        {
            if (!TryParseShadow(value))
            {
                errors.Add(new ShadcnThemeValidationMessage(
                    "invalid-shadow",
                    $"{schemeName}.{name}",
                    "Shadow must contain three or four px lengths followed by an rgb() color per layer."));
            }
        }
    }

    private static void ValidateMetrics(
        ShadcnThemeMetrics? metrics,
        ICollection<ShadcnThemeValidationMessage> errors)
    {
        if (metrics is null)
        {
            errors.Add(new ShadcnThemeValidationMessage("required-metrics", "metrics", "Theme metrics are required."));
            return;
        }

        if (!IsSafeFontFamily(metrics.FontFamily))
        {
            errors.Add(new ShadcnThemeValidationMessage(
                "unsafe-font-family",
                "metrics.fontFamily",
                "Font family contains unsupported or declaration-breaking text."));
        }

        if (!IsSafeFontFamily(metrics.MonospaceFontFamily))
        {
            errors.Add(new ShadcnThemeValidationMessage(
                "unsafe-font-family",
                "metrics.monospaceFontFamily",
                "Monospace font family contains unsupported or declaration-breaking text."));
        }

        if (!MotionEasings.Contains(metrics.MotionEasing))
        {
            errors.Add(new ShadcnThemeValidationMessage(
                "invalid-motion-easing",
                "metrics.motionEasing",
                "Motion easing must be linear, ease, ease-in, ease-out, or ease-in-out."));
        }

        if (!Enum.IsDefined(metrics.ReducedMotionBehavior))
        {
            errors.Add(new ShadcnThemeValidationMessage(
                "invalid-reduced-motion-behavior",
                "metrics.reducedMotionBehavior",
                "Reduced-motion behavior must respect the system preference or always reduce motion."));
        }

        foreach (var (name, value) in GetMetrics(metrics))
        {
            if (!double.IsFinite(value) || value <= 0 || value > 100)
            {
                errors.Add(new ShadcnThemeValidationMessage(
                    "invalid-metric",
                    $"metrics.{name}",
                    "Metric must be a finite invariant number greater than zero and no greater than 100."));
            }
        }
        ValidateRange("spacingScaleMultiplier", metrics.SpacingScaleMultiplier, 0.25, 4, errors);
        ValidateRange("focusRingWidthPx", metrics.FocusRingWidthPx, 1, 8, errors);
        ValidateRange("focusRingOffsetPx", metrics.FocusRingOffsetPx, 0, 8, errors);
        ValidateRange("motionDurationMilliseconds", metrics.MotionDurationMilliseconds, 50, 2000, errors);
    }

    private static void ValidateRange(
        string name,
        double value,
        double minimum,
        double maximum,
        ICollection<ShadcnThemeValidationMessage> errors)
    {
        if (!double.IsFinite(value) || value < minimum || value > maximum)
        {
            errors.Add(new ShadcnThemeValidationMessage(
                "invalid-metric",
                $"metrics.{name}",
                $"Metric must be between {minimum.ToString(CultureInfo.InvariantCulture)} and {maximum.ToString(CultureInfo.InvariantCulture)}."));
        }
    }

    private static bool IsSafeFontFamily(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Length <= 256 &&
        !value.Any(char.IsControl) &&
        !value.Contains("url(", StringComparison.OrdinalIgnoreCase) &&
        FontFamilyPattern().IsMatch(value);

    private static void MeasureContrast(
        string schemeName,
        ShadcnColorScheme? scheme,
        IReadOnlyCollection<ShadcnThemeValidationMessage> errors,
        ICollection<ShadcnThemeValidationMessage> warnings,
        ICollection<ShadcnContrastResult> results)
    {
        if (scheme is null)
            return;

        var tokens = GetColorTokens(scheme).ToDictionary(token => token.Name, token => token.Value, StringComparer.Ordinal);
        foreach (var (foregroundName, backgroundName) in ContrastPairs)
        {
            MeasurePair(
                ShadcnContrastKind.Text,
                schemeName,
                foregroundName,
                backgroundName,
                TextContrastRatio,
                "low-contrast",
                tokens,
                errors,
                warnings,
                results);
        }

        MeasureFocusIndicator(schemeName, tokens, errors, warnings, results);
        MeasurePair(ShadcnContrastKind.Boundary, schemeName, "border", "background",
            NonTextContrastRatio, "low-boundary-contrast", tokens, errors, warnings, results);
        MeasurePair(ShadcnContrastKind.Boundary, schemeName, "input", "background",
            NonTextContrastRatio, "low-boundary-contrast", tokens, errors, warnings, results);
        MeasurePair(ShadcnContrastKind.DestructiveAdjacency, schemeName, "destructive", "background",
            NonTextContrastRatio, "low-destructive-adjacency-contrast", tokens, errors, warnings, results);
        foreach (var chart in new[] { "chart1", "chart2", "chart3", "chart4", "chart5" })
        {
            MeasurePair(ShadcnContrastKind.Chart, schemeName, chart, "background",
                NonTextContrastRatio, "low-chart-contrast", tokens, errors, warnings, results);
        }
        MeasurePair(ShadcnContrastKind.Boundary, schemeName, "sidebarBorder", "sidebar",
            NonTextContrastRatio, "low-boundary-contrast", tokens, errors, warnings, results);
        MeasurePair(ShadcnContrastKind.FocusRing, schemeName, "sidebarRing", "sidebar",
            NonTextContrastRatio, "low-focus-ring-contrast", tokens, errors, warnings, results);
        MeasureDisabledControl(schemeName, tokens, errors, warnings, results);
    }

    private static void MeasureFocusIndicator(
        string schemeName,
        IReadOnlyDictionary<string, string> tokens,
        IReadOnlyCollection<ShadcnThemeValidationMessage> errors,
        ICollection<ShadcnThemeValidationMessage> warnings,
        ICollection<ShadcnContrastResult> results)
    {
        if (HasInvalidToken(errors, schemeName, "ring", "background"))
            return;

        _ = TryParseColor(tokens["ring"], out var ring);
        _ = TryParseColor(tokens["background"], out var background);
        var opaqueBackground = background.Over(Rgba.White);
        var renderedRing = ring.WithOpacity(0.5).Over(opaqueBackground);
        AddMeasurement(ShadcnContrastKind.FocusRing, schemeName, "ring", "background",
            renderedRing, opaqueBackground, NonTextContrastRatio, "low-focus-ring-contrast", warnings, results);
    }

    private static void MeasureDisabledControl(
        string schemeName,
        IReadOnlyDictionary<string, string> tokens,
        IReadOnlyCollection<ShadcnThemeValidationMessage> errors,
        ICollection<ShadcnThemeValidationMessage> warnings,
        ICollection<ShadcnContrastResult> results)
    {
        if (HasInvalidToken(errors, schemeName, "primaryForeground", "primary", "background"))
            return;

        _ = TryParseColor(tokens["background"], out var page);
        _ = TryParseColor(tokens["primary"], out var surface);
        _ = TryParseColor(tokens["primaryForeground"], out var foreground);
        var opaquePage = page.Over(Rgba.White);
        var opaqueSurface = surface.Over(opaquePage);
        var opaqueForeground = foreground.Over(opaqueSurface);
        var renderedSurface = opaqueSurface.WithOpacity(0.5).Over(opaquePage);
        var renderedForeground = opaqueForeground.WithOpacity(0.5).Over(opaquePage);
        AddMeasurement(ShadcnContrastKind.DisabledState, schemeName, "primaryForeground", "primary",
            renderedForeground, renderedSurface, TextContrastRatio, "low-disabled-state-contrast", warnings, results);
    }

    private static void MeasurePair(
        ShadcnContrastKind kind,
        string schemeName,
        string foregroundName,
        string backgroundName,
        double requiredRatio,
        string warningCode,
        IReadOnlyDictionary<string, string> tokens,
        IReadOnlyCollection<ShadcnThemeValidationMessage> errors,
        ICollection<ShadcnThemeValidationMessage> warnings,
        ICollection<ShadcnContrastResult> results)
    {
        if (HasInvalidToken(errors, schemeName, foregroundName, backgroundName))
            return;

        _ = TryParseColor(tokens[foregroundName], out var foreground);
        _ = TryParseColor(tokens[backgroundName], out var background);
        var opaqueBackground = background.Over(Rgba.White);
        var opaqueForeground = foreground.Over(opaqueBackground);
        AddMeasurement(kind, schemeName, foregroundName, backgroundName,
            opaqueForeground, opaqueBackground, requiredRatio, warningCode, warnings, results);
    }

    private static bool HasInvalidToken(
        IReadOnlyCollection<ShadcnThemeValidationMessage> errors,
        string schemeName,
        params string[] tokenNames) =>
        tokenNames.Any(token => errors.Any(error => error.Path == $"{schemeName}.{token}"));

    private static void AddMeasurement(
        ShadcnContrastKind kind,
        string schemeName,
        string foregroundName,
        string backgroundName,
        Rgba renderedForeground,
        Rgba renderedBackground,
        double requiredRatio,
        string warningCode,
        ICollection<ShadcnThemeValidationMessage> warnings,
        ICollection<ShadcnContrastResult> results)
    {
        var ratio = Contrast(renderedForeground, renderedBackground);
        var passes = ratio >= requiredRatio;

        results.Add(new ShadcnContrastResult(
            kind,
            schemeName,
            foregroundName,
            backgroundName,
            ratio,
            requiredRatio,
            passes));

        if (!passes)
        {
            warnings.Add(new ShadcnThemeValidationMessage(
                warningCode,
                $"{schemeName}.{foregroundName}",
                $"{kind} contrast against {backgroundName} is {ratio.ToString("0.###", CultureInfo.InvariantCulture)}:1; {requiredRatio.ToString("0.###", CultureInfo.InvariantCulture)}:1 is required."));
        }
    }

    private static double Contrast(Rgba first, Rgba second)
    {
        var firstLuminance = Luminance(first);
        var secondLuminance = Luminance(second);
        return (Math.Max(firstLuminance, secondLuminance) + 0.05) /
               (Math.Min(firstLuminance, secondLuminance) + 0.05);
    }

    private static double Luminance(Rgba color) =>
        (0.2126 * color.Red) + (0.7152 * color.Green) + (0.0722 * color.Blue);

    private static bool TryParseColor(string? value, out Rgba color)
    {
        color = default;
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128 ||
            value.IndexOfAny([';', '{', '}', '<', '>']) >= 0 ||
            value.Contains("url(", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return TryParseHex(value, out color) || TryParseOklch(value, out color);
    }

    private static bool TryParseHex(string value, out Rgba color)
    {
        color = default;
        var match = HexPattern().Match(value);
        if (!match.Success)
            return false;

        var hex = match.Groups["value"].Value;
        if (hex.Length is 3 or 4)
            hex = string.Concat(hex.Select(character => new string(character, 2)));

        var red = byte.Parse(hex.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var green = byte.Parse(hex.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var blue = byte.Parse(hex.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var alpha = hex.Length == 8
            ? byte.Parse(hex.AsSpan(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d
            : 1d;
        color = new Rgba(ToLinear(red / 255d), ToLinear(green / 255d), ToLinear(blue / 255d), alpha);
        return true;
    }

    private static bool TryParseRgb(string value, out Rgba color)
    {
        color = default;
        var match = RgbPattern().Match(value);
        if (!match.Success ||
            !TryNumber(match.Groups["red"].Value, out var red) ||
            !TryNumber(match.Groups["green"].Value, out var green) ||
            !TryNumber(match.Groups["blue"].Value, out var blue) ||
            red is < 0 or > 255 || green is < 0 or > 255 || blue is < 0 or > 255 ||
            !TryAlpha(match.Groups["alpha"].Value, out var alpha))
        {
            return false;
        }

        color = new Rgba(ToLinear(red / 255d), ToLinear(green / 255d), ToLinear(blue / 255d), alpha);
        return true;
    }

    private static bool TryParseOklch(string value, out Rgba color)
    {
        color = default;
        var match = OklchPattern().Match(value);
        if (!match.Success ||
            !TryNumber(match.Groups["lightness"].Value, out var lightness) ||
            !TryNumber(match.Groups["chroma"].Value, out var chroma) ||
            !TryNumber(match.Groups["hue"].Value, out var hue) ||
            lightness is < 0 or > 1 || chroma is < 0 or > 0.4 || hue is < 0 or > 360 ||
            !TryAlpha(match.Groups["alpha"].Value, out var alpha))
        {
            return false;
        }

        var radians = hue * Math.PI / 180d;
        var a = chroma * Math.Cos(radians);
        var b = chroma * Math.Sin(radians);
        var l = lightness + (0.3963377774 * a) + (0.2158037573 * b);
        var m = lightness - (0.1055613458 * a) - (0.0638541728 * b);
        var s = lightness - (0.0894841775 * a) - (1.291485548 * b);
        l *= l * l;
        m *= m * m;
        s *= s * s;

        color = new Rgba(
            Math.Clamp((4.0767416621 * l) - (3.3077115913 * m) + (0.2309699292 * s), 0, 1),
            Math.Clamp((-1.2684380046 * l) + (2.6097574011 * m) - (0.3413193965 * s), 0, 1),
            Math.Clamp((-0.0041960863 * l) - (0.7034186147 * m) + (1.707614701 * s), 0, 1),
            alpha);
        return true;
    }

    private static bool TryParseShadow(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 512 ||
            value.IndexOfAny([';', '{', '}', '<', '>']) >= 0 ||
            value.Contains("url(", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        foreach (var layer in value.Split(',', StringSplitOptions.TrimEntries))
        {
            var match = ShadowLayerPattern().Match(layer);
            if (!match.Success || !TryParseRgb(match.Groups["color"].Value, out _))
                return false;
        }

        return true;
    }

    private static bool TryNumber(string value, out double result) =>
        double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result) &&
        double.IsFinite(result);

    private static bool TryAlpha(string value, out double alpha)
    {
        if (string.IsNullOrEmpty(value))
        {
            alpha = 1;
            return true;
        }

        var percent = value.EndsWith('%');
        if (percent)
            value = value[..^1];
        if (!TryNumber(value, out alpha))
            return false;
        if (percent)
            alpha /= 100;
        return alpha is >= 0 and <= 1;
    }

    private static double ToLinear(double srgb) => srgb <= 0.04045
        ? srgb / 12.92
        : Math.Pow((srgb + 0.055) / 1.055, 2.4);

    internal static IReadOnlyList<(string Name, string Value)> GetColorTokens(ShadcnColorScheme scheme) =>
    [
        ("background", scheme.Background),
        ("foreground", scheme.Foreground),
        ("card", scheme.Card),
        ("cardForeground", scheme.CardForeground),
        ("popover", scheme.Popover),
        ("popoverForeground", scheme.PopoverForeground),
        ("primary", scheme.Primary),
        ("primaryForeground", scheme.PrimaryForeground),
        ("secondary", scheme.Secondary),
        ("secondaryForeground", scheme.SecondaryForeground),
        ("muted", scheme.Muted),
        ("mutedForeground", scheme.MutedForeground),
        ("accent", scheme.Accent),
        ("accentForeground", scheme.AccentForeground),
        ("destructive", scheme.Destructive),
        ("destructiveForeground", scheme.DestructiveForeground),
        ("border", scheme.Border),
        ("input", scheme.Input),
        ("ring", scheme.Ring),
        ("chart1", scheme.Chart1),
        ("chart2", scheme.Chart2),
        ("chart3", scheme.Chart3),
        ("chart4", scheme.Chart4),
        ("chart5", scheme.Chart5),
        ("sidebar", scheme.Sidebar),
        ("sidebarForeground", scheme.SidebarForeground),
        ("sidebarPrimary", scheme.SidebarPrimary),
        ("sidebarPrimaryForeground", scheme.SidebarPrimaryForeground),
        ("sidebarAccent", scheme.SidebarAccent),
        ("sidebarAccentForeground", scheme.SidebarAccentForeground),
        ("sidebarBorder", scheme.SidebarBorder),
        ("sidebarRing", scheme.SidebarRing)
    ];

    internal static IReadOnlyList<(string Name, string Value)> GetShadowTokens(ShadcnColorScheme scheme) =>
    [
        ("shadowExtraSmall", scheme.ShadowExtraSmall),
        ("shadowSmall", scheme.ShadowSmall),
        ("shadowMedium", scheme.ShadowMedium)
    ];

    internal static IReadOnlyList<(string Name, double Value)> GetMetrics(ShadcnThemeMetrics metrics) =>
    [
        ("radiusRem", metrics.RadiusRem),
        ("radiusSmallScale", metrics.RadiusSmallScale),
        ("radiusMediumScale", metrics.RadiusMediumScale),
        ("radiusLargeScale", metrics.RadiusLargeScale),
        ("radiusExtraLargeScale", metrics.RadiusExtraLargeScale),
        ("radius2ExtraLargeScale", metrics.Radius2ExtraLargeScale),
        ("radius3ExtraLargeScale", metrics.Radius3ExtraLargeScale),
        ("radius4ExtraLargeScale", metrics.Radius4ExtraLargeScale),
        ("controlHeightRem", metrics.ControlHeightRem),
        ("controlHeightSmallRem", metrics.ControlHeightSmallRem),
        ("controlHeightLargeRem", metrics.ControlHeightLargeRem)
    ];

    private static readonly HashSet<string> MotionEasings = new(StringComparer.Ordinal)
    {
        "linear",
        "ease",
        "ease-in",
        "ease-out",
        "ease-in-out"
    };

    [GeneratedRegex("^#(?<value>[0-9a-fA-F]{3,4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", RegexOptions.CultureInvariant)]
    private static partial Regex HexPattern();

    [GeneratedRegex("^rgb\\(\\s*(?<red>(?:\\d+(?:\\.\\d+)?|\\.\\d+))\\s+(?<green>(?:\\d+(?:\\.\\d+)?|\\.\\d+))\\s+(?<blue>(?:\\d+(?:\\.\\d+)?|\\.\\d+))(?:\\s*/\\s*(?<alpha>(?:\\d+(?:\\.\\d+)?|\\.\\d+)%?))?\\s*\\)$", RegexOptions.CultureInvariant)]
    private static partial Regex RgbPattern();

    [GeneratedRegex("^oklch\\(\\s*(?<lightness>(?:\\d+(?:\\.\\d+)?|\\.\\d+))\\s+(?<chroma>(?:\\d+(?:\\.\\d+)?|\\.\\d+))\\s+(?<hue>(?:\\d+(?:\\.\\d+)?|\\.\\d+))(?:\\s*/\\s*(?<alpha>(?:\\d+(?:\\.\\d+)?|\\.\\d+)%?))?\\s*\\)$", RegexOptions.CultureInvariant)]
    private static partial Regex OklchPattern();

    [GeneratedRegex("^(?:-?(?:0|(?:\\d+(?:\\.\\d+)?|\\.\\d+)px)\\s+){2,3}-?(?:0|(?:\\d+(?:\\.\\d+)?|\\.\\d+)px)\\s+(?<color>rgb\\(.+\\))$", RegexOptions.CultureInvariant)]
    private static partial Regex ShadowLayerPattern();

    [GeneratedRegex("^[\\p{L}\\p{N}\\s,'\"._-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex FontFamilyPattern();

    private readonly record struct Rgba(double Red, double Green, double Blue, double Alpha)
    {
        public static Rgba White { get; } = new(1, 1, 1, 1);

        public Rgba WithOpacity(double opacity) => this with { Alpha = Alpha * opacity };

        public Rgba Over(Rgba background) => new(
            ToLinear((ToSrgb(Red) * Alpha) + (ToSrgb(background.Red) * (1 - Alpha))),
            ToLinear((ToSrgb(Green) * Alpha) + (ToSrgb(background.Green) * (1 - Alpha))),
            ToLinear((ToSrgb(Blue) * Alpha) + (ToSrgb(background.Blue) * (1 - Alpha))),
            1);
    }
}
