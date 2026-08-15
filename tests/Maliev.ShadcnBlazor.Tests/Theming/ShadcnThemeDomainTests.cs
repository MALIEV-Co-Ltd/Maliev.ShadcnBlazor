using System.Diagnostics;
using System.Globalization;
using System.Text;
using Bunit;
using Maliev.ShadcnBlazor.Components;
using Maliev.ShadcnBlazor.Theming;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Utilities;

#pragma warning disable MUD0012 // Assertions observe the rendered provider's public parameter state.

namespace Maliev.ShadcnBlazor.Tests.Theming;

public sealed class ShadcnThemeDomainTests
{
    private static readonly string[] CanonicalVariables =
    [
        "--shadcn-font-sans",
        "--shadcn-font-mono",
        "--shadcn-typeset-font-mono",
        "--shadcn-background",
        "--shadcn-foreground",
        "--shadcn-card",
        "--shadcn-card-foreground",
        "--shadcn-popover",
        "--shadcn-popover-foreground",
        "--shadcn-primary",
        "--shadcn-primary-foreground",
        "--shadcn-secondary",
        "--shadcn-secondary-foreground",
        "--shadcn-muted",
        "--shadcn-muted-foreground",
        "--shadcn-accent",
        "--shadcn-accent-foreground",
        "--shadcn-destructive",
        "--shadcn-destructive-foreground",
        "--shadcn-border",
        "--shadcn-input",
        "--shadcn-ring",
        "--shadcn-chart-1",
        "--shadcn-chart-2",
        "--shadcn-chart-3",
        "--shadcn-chart-4",
        "--shadcn-chart-5",
        "--shadcn-sidebar",
        "--shadcn-sidebar-foreground",
        "--shadcn-sidebar-primary",
        "--shadcn-sidebar-primary-foreground",
        "--shadcn-sidebar-accent",
        "--shadcn-sidebar-accent-foreground",
        "--shadcn-sidebar-border",
        "--shadcn-sidebar-ring",
        "--shadcn-radius",
        "--shadcn-radius-sm",
        "--shadcn-radius-md",
        "--shadcn-radius-lg",
        "--shadcn-radius-xl",
        "--shadcn-radius-2xl",
        "--shadcn-radius-3xl",
        "--shadcn-radius-4xl",
        "--shadcn-control-height",
        "--shadcn-control-height-sm",
        "--shadcn-control-height-lg",
        "--shadcn-spacing-multiplier",
        "--shadcn-focus-ring-width",
        "--shadcn-focus-ring-offset",
        "--shadcn-motion-duration",
        "--shadcn-motion-duration-fast",
        "--shadcn-motion-duration-slow",
        "--shadcn-motion-easing",
        "--shadcn-motion-easing-standard",
        "--shadcn-motion-easing-enter",
        "--shadcn-reduced-motion-duration",
        "--shadcn-shadow-xs",
        "--shadcn-shadow-sm",
        "--shadcn-shadow-md"
    ];

    [Fact]
    public void CanonicalJsonRoundTripPreservesEveryTypedTokenAndPropertyOrder()
    {
        var theme = CreateTheme();

        var first = ShadcnThemeSerializer.Serialize(theme);
        var second = ShadcnThemeSerializer.Serialize(theme);
        var roundTrip = ShadcnThemeSerializer.Deserialize(first);

        Assert.Equal(theme, roundTrip);
        Assert.Equal(first, second);
        Assert.DoesNotContain('\r', first);
        Assert.Equal(first, Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(first)));
        AssertOrdered(first,
            "\"schemaVersion\"", "\"name\"", "\"light\"", "\"dark\"", "\"metrics\"");
        AssertOrdered(first,
            "\"background\"", "\"foreground\"", "\"card\"", "\"cardForeground\"",
            "\"popover\"", "\"popoverForeground\"", "\"primary\"", "\"primaryForeground\"",
            "\"secondary\"", "\"secondaryForeground\"", "\"muted\"", "\"mutedForeground\"",
            "\"accent\"", "\"accentForeground\"", "\"destructive\"", "\"destructiveForeground\"",
            "\"border\"", "\"input\"", "\"ring\"", "\"chart1\"", "\"chart2\"", "\"chart3\"",
            "\"chart4\"", "\"chart5\"", "\"sidebar\"", "\"sidebarForeground\"",
            "\"sidebarPrimary\"", "\"sidebarPrimaryForeground\"", "\"sidebarAccent\"",
            "\"sidebarAccentForeground\"", "\"sidebarBorder\"", "\"sidebarRing\"",
            "\"shadowExtraSmall\"", "\"shadowSmall\"", "\"shadowMedium\"");
        AssertOrdered(first,
            "\"fontFamily\"", "\"monospaceFontFamily\"", "\"radiusRem\"", "\"radiusSmallScale\"", "\"radiusMediumScale\"",
            "\"radiusLargeScale\"", "\"radiusExtraLargeScale\"", "\"radius2ExtraLargeScale\"",
            "\"radius3ExtraLargeScale\"", "\"radius4ExtraLargeScale\"", "\"controlHeightRem\"",
            "\"controlHeightSmallRem\"", "\"controlHeightLargeRem\"", "\"spacingScaleMultiplier\"",
            "\"focusRingWidthPx\"", "\"focusRingOffsetPx\"", "\"motionDurationMilliseconds\"",
            "\"motionEasing\"", "\"reducedMotionBehavior\"");
    }

    [Fact]
    public void SchemaOneJsonWithoutNewSharedControlsMigratesDeterministically()
    {
        var legacy = ShadcnThemeSerializer.Serialize(CreateTheme())
            .Replace("    \"monospaceFontFamily\": \"'JetBrains Mono', ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace\",\n", string.Empty, StringComparison.Ordinal)
            .Replace("    \"spacingScaleMultiplier\": 1,\n", string.Empty, StringComparison.Ordinal)
            .Replace("    \"focusRingWidthPx\": 3,\n", string.Empty, StringComparison.Ordinal)
            .Replace("    \"focusRingOffsetPx\": 0,\n", string.Empty, StringComparison.Ordinal)
            .Replace("    \"motionDurationMilliseconds\": 150,\n", string.Empty, StringComparison.Ordinal)
            .Replace("    \"motionEasing\": \"ease-out\",\n", string.Empty, StringComparison.Ordinal)
            .Replace("    \"reducedMotionBehavior\": \"respectSystemPreference\"\n", string.Empty, StringComparison.Ordinal)
            .Replace("    \"controlHeightLargeRem\": 2.5,\n", "    \"controlHeightLargeRem\": 2.5\n", StringComparison.Ordinal);

        var migrated = ShadcnThemeSerializer.Deserialize(legacy);

        Assert.Equal("'JetBrains Mono', ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace", migrated.Metrics.MonospaceFontFamily);
        Assert.Equal(1, migrated.Metrics.SpacingScaleMultiplier);
        Assert.Equal(3, migrated.Metrics.FocusRingWidthPx);
        Assert.Equal(0, migrated.Metrics.FocusRingOffsetPx);
        Assert.Equal(150, migrated.Metrics.MotionDurationMilliseconds);
        Assert.Equal("ease-out", migrated.Metrics.MotionEasing);
        Assert.Equal(ShadcnReducedMotionBehavior.RespectSystemPreference, migrated.Metrics.ReducedMotionBehavior);
    }

    [Theory]
    [InlineData("#fff; color: red")]
    [InlineData("#fff}")]
    [InlineData("{#fff")]
    [InlineData("<style>")]
    [InlineData("url(https://example.test/x)")]
    [InlineData("hsl(0 0% 100%)")]
    [InlineData("rgb(0 0 0)")]
    [InlineData("var(--consumer-color)")]
    public void ValidationRejectsDeclarationInjectionAndUnsupportedColorSyntax(string value)
    {
        var theme = CreateTheme() with { Light = CreateScheme() with { Primary = value } };

        var result = ShadcnThemeValidator.Validate(theme);

        var error = Assert.Single(result.Errors, candidate => candidate.Path == "light.primary");
        Assert.Equal("invalid-color", error.Code);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void ValidationRejectsNonfiniteMetrics(double value)
    {
        var theme = CreateTheme() with { Metrics = CreateMetrics() with { RadiusRem = value } };

        var result = ShadcnThemeValidator.Validate(theme);

        var error = Assert.Single(result.Errors, candidate => candidate.Path == "metrics.radiusRem");
        Assert.Equal("invalid-metric", error.Code);
    }

    [Fact]
    public void ValidationAllowsZeroFocusRingOffsetWithinItsUsabilityBound()
    {
        var metrics = CreateMetrics() with { FocusRingOffsetPx = 0 };

        var result = ShadcnThemeValidator.Validate(CreateTheme() with { Metrics = metrics });

        Assert.DoesNotContain(result.Errors, error => error.Path == "metrics.focusRingOffsetPx");
    }

    [Theory]
    [InlineData("MonospaceFontFamily", "")]
    [InlineData("MotionEasing", "linear; color: red")]
    public void ValidationRejectsUnsafeSharedMetricText(string property, string value)
    {
        var metrics = property switch
        {
            "MonospaceFontFamily" => CreateMetrics() with { MonospaceFontFamily = value },
            "MotionEasing" => CreateMetrics() with { MotionEasing = value },
            _ => throw new ArgumentOutOfRangeException(nameof(property))
        };

        var result = ShadcnThemeValidator.Validate(CreateTheme() with { Metrics = metrics });

        Assert.Contains(result.Errors, error => error.Path == $"metrics.{char.ToLowerInvariant(property[0])}{property[1..]}");
    }

    [Theory]
    [InlineData("SpacingScaleMultiplier", 0.24)]
    [InlineData("SpacingScaleMultiplier", 4.01)]
    [InlineData("FocusRingWidthPx", 0.99)]
    [InlineData("FocusRingWidthPx", 8.01)]
    [InlineData("FocusRingOffsetPx", 8.01)]
    [InlineData("MotionDurationMilliseconds", 49)]
    [InlineData("MotionDurationMilliseconds", 2001)]
    public void ValidationAppliesDocumentedSharedMetricUsabilityBounds(string property, double value)
    {
        var metrics = property switch
        {
            "SpacingScaleMultiplier" => CreateMetrics() with { SpacingScaleMultiplier = value },
            "FocusRingWidthPx" => CreateMetrics() with { FocusRingWidthPx = value },
            "FocusRingOffsetPx" => CreateMetrics() with { FocusRingOffsetPx = value },
            "MotionDurationMilliseconds" => CreateMetrics() with { MotionDurationMilliseconds = checked((int)value) },
            _ => throw new ArgumentOutOfRangeException(nameof(property))
        };

        var result = ShadcnThemeValidator.Validate(CreateTheme() with { Metrics = metrics });

        Assert.Contains(result.Errors, error => error.Path == $"metrics.{char.ToLowerInvariant(property[0])}{property[1..]}");
    }

    [Fact]
    public void ValidationRejectsUnknownReducedMotionBehavior()
    {
        var metrics = CreateMetrics() with { ReducedMotionBehavior = (ShadcnReducedMotionBehavior)99 };

        var result = ShadcnThemeValidator.Validate(CreateTheme() with { Metrics = metrics });

        Assert.Contains(result.Errors, error => error.Path == "metrics.reducedMotionBehavior");
    }

    [Theory]
    [InlineData("sans-serif; color: red")]
    [InlineData("sans-serif}")]
    [InlineData("<script>")]
    [InlineData("url(evil.woff2)")]
    [InlineData("sans-serif\nserif")]
    public void ValidationRejectsUnsafeFontText(string value)
    {
        var theme = CreateTheme() with { Metrics = CreateMetrics() with { FontFamily = value } };

        var result = ShadcnThemeValidator.Validate(theme);

        var error = Assert.Single(result.Errors, candidate => candidate.Path == "metrics.fontFamily");
        Assert.Equal("unsafe-font-family", error.Code);
    }

    [Fact]
    public void ValidationReportsMeasuredContrastAndWarningsWithoutMutatingInput()
    {
        var original = CreateTheme();
        var lowContrast = original with
        {
            Light = original.Light with { Background = "#ffffff", Foreground = "#777777" }
        };

        var result = ShadcnThemeValidator.Validate(lowContrast);

        Assert.True(result.IsValid);
        var measurement = Assert.Single(result.ContrastResults,
            candidate => candidate.Scheme == "light" && candidate.ForegroundToken == "foreground");
        Assert.Equal("background", measurement.BackgroundToken);
        Assert.Equal(4.478, measurement.Ratio, 3);
        Assert.Equal(4.5, measurement.RequiredRatio);
        Assert.False(measurement.Passes);
        Assert.Contains(result.Warnings,
            candidate => candidate.Code == "low-contrast" && candidate.Path == "light.foreground");
        Assert.Equal("oklch(1 0 0)", original.Light.Background);
        Assert.Equal("oklch(0.145 0 0)", original.Light.Foreground);
    }

    [Fact]
    public void ValidationMeasuresOklchAndHexContrastInvariantly()
    {
        var currentCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("th-TH");
            var first = ShadcnThemeValidator.Validate(CreateTheme());
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var second = ShadcnThemeValidator.Validate(CreateTheme());

            Assert.Equal(first.ContrastResults, second.ContrastResults);
            Assert.All(first.ContrastResults, result => Assert.True(double.IsFinite(result.Ratio)));
            Assert.Contains(first.ContrastResults,
                result => result.Scheme == "dark" && result.ForegroundToken == "primaryForeground");
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
        }
    }

    [Fact]
    public void ValidationReturnsDedicatedFocusBoundaryDestructiveAndDisabledMeasurements()
    {
        var source = CreateTheme();
        var lowContrast = source with
        {
            Light = source.Light with
            {
                Ring = "#ffffff",
                Border = "#ffffff",
                Input = "#ffffff",
                Destructive = "#ffffff",
                Muted = "#777777",
                MutedForeground = "#777777"
            },
            Dark = source.Dark with
            {
                Ring = "oklch(0.145 0 0)",
                Border = "oklch(0.145 0 0)",
                Input = "oklch(0.145 0 0)",
                Destructive = "oklch(0.145 0 0)",
                Muted = "#777777",
                MutedForeground = "#777777"
            }
        };
        var result = ShadcnThemeValidator.Validate(lowContrast);
        var dedicated = result.ContrastResults
            .Where(measurement => measurement.Kind is ShadcnContrastKind.FocusRing or
                ShadcnContrastKind.Boundary or ShadcnContrastKind.DestructiveAdjacency or
                ShadcnContrastKind.DisabledState)
            .ToArray();

        Assert.Equal(10, dedicated.Length);
        Assert.Collection(dedicated,
            item => AssertMeasurement(item, ShadcnContrastKind.FocusRing, "light", "ring", "background", 3),
            item => AssertMeasurement(item, ShadcnContrastKind.Boundary, "light", "border", "background", 3),
            item => AssertMeasurement(item, ShadcnContrastKind.Boundary, "light", "input", "background", 3),
            item => AssertMeasurement(item, ShadcnContrastKind.DestructiveAdjacency, "light", "destructive", "background", 3),
            item => AssertMeasurement(item, ShadcnContrastKind.DisabledState, "light", "primaryForeground", "primary", 4.5),
            item => AssertMeasurement(item, ShadcnContrastKind.FocusRing, "dark", "ring", "background", 3),
            item => AssertMeasurement(item, ShadcnContrastKind.Boundary, "dark", "border", "background", 3),
            item => AssertMeasurement(item, ShadcnContrastKind.Boundary, "dark", "input", "background", 3),
            item => AssertMeasurement(item, ShadcnContrastKind.DestructiveAdjacency, "dark", "destructive", "background", 3),
            item => AssertMeasurement(item, ShadcnContrastKind.DisabledState, "dark", "primaryForeground", "primary", 4.5));
        Assert.Contains(result.Warnings, warning => warning.Code == "low-focus-ring-contrast");
        Assert.Contains(result.Warnings, warning => warning.Code == "low-boundary-contrast");
        Assert.Contains(result.Warnings, warning => warning.Code == "low-destructive-adjacency-contrast");
        Assert.Contains(result.Warnings, warning => warning.Code == "low-disabled-state-contrast");
    }

    [Fact]
    public void DefaultDarkTranslucentBoundariesCompositeAgainstTheirActualBackground()
    {
        var result = ShadcnThemeValidator.Validate(ShadcnThemePresets.BaseVegaNeutral.CreateTheme());
        var boundaries = result.ContrastResults
            .Where(measurement => measurement.Kind == ShadcnContrastKind.Boundary && measurement.Scheme == "dark")
            .ToArray();

        Assert.Collection(boundaries,
            border =>
            {
                Assert.Equal("border", border.ForegroundToken);
                Assert.Equal(1.252, border.Ratio, 3);
                Assert.Equal(ShadcnThemeValidator.NonTextContrastRatio, border.RequiredRatio);
                Assert.False(border.Passes);
            },
            input =>
            {
                Assert.Equal("input", input.ForegroundToken);
                Assert.Equal(1.474, input.Ratio, 3);
                Assert.Equal(ShadcnThemeValidator.NonTextContrastRatio, input.RequiredRatio);
                Assert.False(input.Passes);
            });
        Assert.Single(result.Warnings,
            warning => warning.Code == "low-boundary-contrast" && warning.Path == "dark.border");
        Assert.Single(result.Warnings,
            warning => warning.Code == "low-boundary-contrast" && warning.Path == "dark.input");
    }

    [Fact]
    public void FocusContrastMeasuresTheRenderedFiftyPercentIndicatorMix()
    {
        var scheme = CreateScheme() with { Background = "#ffffff", Ring = "#777777" };

        var result = ShadcnThemeValidator.Validate(CreateTheme() with { Light = scheme });
        var focus = Assert.Single(result.ContrastResults,
            measurement => measurement.Kind == ShadcnContrastKind.FocusRing && measurement.Scheme == "light");

        Assert.Equal(1.92, focus.Ratio, 2);
        Assert.False(focus.Passes);
        Assert.Contains(result.Warnings,
            warning => warning.Code == "low-focus-ring-contrast" && warning.Path == "light.ring");
    }

    [Fact]
    public void DisabledContrastMeasuresWholeControlOpacityAgainstThePageBackground()
    {
        var scheme = CreateScheme() with
        {
            Background = "#ffffff",
            Primary = "#333333",
            PrimaryForeground = "#ffffff"
        };

        var result = ShadcnThemeValidator.Validate(CreateTheme() with { Light = scheme });
        var disabled = Assert.Single(result.ContrastResults,
            measurement => measurement.Kind == ShadcnContrastKind.DisabledState && measurement.Scheme == "light");

        Assert.Equal("primaryForeground", disabled.ForegroundToken);
        Assert.Equal("primary", disabled.BackgroundToken);
        Assert.Equal(2.85, disabled.Ratio, 2);
        Assert.Equal(ShadcnThemeValidator.TextContrastRatio, disabled.RequiredRatio);
        Assert.False(disabled.Passes);
        Assert.Contains(result.Warnings,
            warning => warning.Code == "low-disabled-state-contrast" && warning.Path == "light.primaryForeground");
    }

    [Fact]
    public void DeserializeRejectsUnsupportedFutureSchema()
    {
        var json = ShadcnThemeSerializer.Serialize(CreateTheme())
            .Replace("\"schemaVersion\": 1", "\"schemaVersion\": 2", StringComparison.Ordinal);

        var exception = Assert.Throws<NotSupportedException>(() => ShadcnThemeSerializer.Deserialize(json));

        Assert.Contains("schema version 2", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DeserializeMigratesMissingVersionZeroSchemaToCurrentVersion()
    {
        var current = CreateTheme();
        var oldJson = ShadcnThemeSerializer.Serialize(current)
            .Replace("  \"schemaVersion\": 1,\n", string.Empty, StringComparison.Ordinal);

        var migrated = ShadcnThemeSerializer.Deserialize(oldJson);

        Assert.Equal(ShadcnTheme.CurrentSchemaVersion, migrated.SchemaVersion);
        Assert.Equal(current, migrated);
    }

    [Fact]
    public void PresetReturnsIndependentBaseVegaNeutralClones()
    {
        var first = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        var second = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        var customized = first with { Light = first.Light with { Primary = "#123456" } };

        Assert.NotSame(first, second);
        Assert.NotSame(first.Light, second.Light);
        Assert.NotSame(first.Dark, second.Dark);
        Assert.NotSame(first.Metrics, second.Metrics);
        Assert.Equal("base-vega-neutral", ShadcnThemePresets.BaseVegaNeutral.Id);
        Assert.Equal("Base / Vega / Neutral", first.Name);
        Assert.Equal("oklch(0.205 0 0)", second.Light.Primary);
        Assert.NotEqual(customized.Light.Primary, second.Light.Primary);
    }

    [Fact]
    public void CssWriterIsDeterministicCompleteScopedAndLfOnly()
    {
        var theme = CreateTheme();

        var css = ShadcnThemeCssWriter.Write(theme);

        Assert.Equal(css, ShadcnThemeCssWriter.Write(theme));
        Assert.StartsWith(
            ".shadcn-scope[data-shadcn-theme=\"light\"],\n.shadcn-overlay-scope[data-shadcn-theme=\"light\"] {\n",
            css, StringComparison.Ordinal);
        Assert.Contains(
            ".shadcn-scope[data-shadcn-theme=\"dark\"],\n.shadcn-overlay-scope[data-shadcn-theme=\"dark\"] {\n",
            css, StringComparison.Ordinal);
        Assert.DoesNotContain(":root", css, StringComparison.Ordinal);
        Assert.DoesNotContain('\r', css);
        Assert.EndsWith("}\n", css, StringComparison.Ordinal);
        Assert.Equal(css, Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(css)));
        foreach (var variable in CanonicalVariables)
            Assert.Equal(2, Count(css, $"  {variable}:"));
    }

    [Fact]
    public void WritersRejectInvalidThemesBeforeProducingText()
    {
        var invalid = CreateTheme() with
        {
            Dark = CreateScheme() with { ShadowSmall = "0 1px 2px red;display:block" }
        };

        Assert.Throws<ArgumentException>(() => ShadcnThemeCssWriter.Write(invalid));
        Assert.Throws<ArgumentException>(() => ShadcnThemeCSharpWriter.Write(invalid));
        Assert.Throws<ArgumentException>(() => ShadcnThemeSerializer.Serialize(invalid));
    }

    [Fact]
    public void CSharpWriterEscapesStringLiteralsAndJsonDoesNotEmitMarkupInjection()
    {
        var theme = CreateTheme() with
        {
            Name = "Neutral & \"Quoted\"",
            Metrics = CreateMetrics() with { FontFamily = "\"Noto Sans Thai\", sans-serif" }
        };

        var csharp = ShadcnThemeCSharpWriter.Write(theme);
        var json = ShadcnThemeSerializer.Serialize(theme);

        Assert.Equal(csharp, ShadcnThemeCSharpWriter.Write(theme));
        Assert.DoesNotContain('\r', csharp);
        Assert.Contains("Name = \"Neutral & \\\"Quoted\\\"\"", csharp, StringComparison.Ordinal);
        Assert.Contains("FontFamily = \"\\\"Noto Sans Thai\\\", sans-serif\"", csharp, StringComparison.Ordinal);
        Assert.Contains("Neutral \\u0026 \\u0022Quoted\\u0022", json, StringComparison.Ordinal);
        Assert.DoesNotContain("</script", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CSharpWriterEscapesLiteralHazardsAndEmittedExpressionCompiles()
    {
        var theme = CreateTheme() with
        {
            Name = "Line\u2028Paragraph\u2029Bom\ufeffSurrogate\ud800",
            Metrics = CreateMetrics() with
            {
                FontFamily = "Noto Sans Thai, sans-serif",
                MonospaceFontFamily = "JetBrains Mono, monospace"
            }
        };

        var csharp = ShadcnThemeCSharpWriter.Write(theme);

        Assert.Contains("Line\\u2028Paragraph\\u2029Bom\\ufeffSurrogate\\ud800", csharp, StringComparison.Ordinal);
        Assert.DoesNotContain('\u2028', csharp);
        Assert.DoesNotContain('\u2029', csharp);
        await AssertGeneratedCSharpCompiles(csharp);
    }

    [Fact]
    public async Task WritersPreserveEveryValidDoubleExactlyAcrossCssJsonAndExecutableCSharp()
    {
        var theme = CreateTheme() with
        {
            Metrics = CreateMetrics() with
            {
                RadiusRem = 0.9999999999999999,
                RadiusSmallScale = double.Epsilon,
                RadiusMediumScale = 0.25000000000000006,
                RadiusLargeScale = 1.0000000000000002,
                RadiusExtraLargeScale = 1.4000000000000001,
                Radius2ExtraLargeScale = 1.8000000000000003,
                Radius3ExtraLargeScale = 2.2000000000000002,
                Radius4ExtraLargeScale = 2.6000000000000001,
                ControlHeightRem = 2.2500000000000004,
                ControlHeightSmallRem = 2.0000000000000004,
                ControlHeightLargeRem = 2.5000000000000004,
                SpacingScaleMultiplier = 0.25000000000000006,
                FocusRingWidthPx = 1.0000000000000002,
                FocusRingOffsetPx = double.Epsilon
            }
        };
        Assert.True(ShadcnThemeValidator.Validate(theme).IsValid);

        var css = ShadcnThemeCssWriter.Write(theme);
        var json = ShadcnThemeSerializer.Serialize(theme);
        var csharp = ShadcnThemeCSharpWriter.Write(theme);

        Assert.Contains("--shadcn-radius: 0.9999999999999999rem", css, StringComparison.Ordinal);
        Assert.Contains("--shadcn-radius-sm: calc(var(--shadcn-radius) * 5E-324)", css, StringComparison.Ordinal);
        Assert.Contains("--shadcn-spacing-multiplier: 0.25000000000000006", css, StringComparison.Ordinal);
        Assert.Contains("--shadcn-focus-ring-offset: 5E-324px", css, StringComparison.Ordinal);
        Assert.Contains("\"radiusRem\": 0.9999999999999999", json, StringComparison.Ordinal);
        Assert.Contains("RadiusSmallScale = 5E-324", csharp, StringComparison.Ordinal);
        Assert.Contains("FocusRingOffsetPx = 5E-324", csharp, StringComparison.Ordinal);

        var executed = await ExecuteGeneratedCSharpFactory(csharp);
        Assert.Equal(theme, executed);
        Assert.True(ShadcnThemeValidator.Validate(executed).IsValid);
    }

    [Fact]
    public async Task DefaultProviderOutputAndMudMappingRemainUnchangedWhenThemeIsOmitted()
    {
        await using var context = CreateBunitContext();

        var cut = context.Render<ShadcnThemeProvider>();
        var root = cut.Find("[data-shadcn-scope]");
        var mud = Assert.IsType<MudTheme>(cut.FindComponent<MudThemeProvider>().Instance.Theme);

        Assert.Equal("--shadcn-font-sans: 'Geist', 'Noto Sans Thai', ui-sans-serif, system-ui, sans-serif", root.GetAttribute("style"));
        Assert.Equal(new MudColor("#171717"), mud.PaletteLight.Primary);
        Assert.Equal(new MudColor("#ffffff"), mud.PaletteLight.Background);
        Assert.Equal(new MudColor("#e4e4e7"), mud.PaletteDark.Primary);
        Assert.Equal(new MudColor("#252525"), mud.PaletteDark.Background);
        Assert.Equal(
            ["'Geist', 'Noto Sans Thai', ui-sans-serif, system-ui, sans-serif"],
            Assert.IsType<string[]>(mud.Typography.Default.FontFamily));
    }

    [Fact]
    public async Task TypedProviderWritesCurrentSchemePropertiesAndMapsBothMudPalettes()
    {
        await using var context = CreateBunitContext();
        var theme = CreateTheme() with
        {
            Light = CreateScheme() with
            {
                Primary = "#123456",
                Background = "#fafafa",
                Card = "#f0f0f0"
            },
            Dark = CreateScheme() with
            {
                Primary = "#abcdef",
                Background = "#101010",
                Card = "#181818"
            },
            Metrics = CreateMetrics() with { FontFamily = "Noto Sans Thai, sans-serif" }
        };

        var cut = context.Render<ShadcnThemeProvider>(parameters => parameters
            .Add(component => component.Theme, theme)
            .Add(component => component.IsDarkMode, true));
        var root = cut.Find("[data-shadcn-scope]");
        var style = root.GetAttribute("style")!;
        var mud = Assert.IsType<MudTheme>(cut.FindComponent<MudThemeProvider>().Instance.Theme);

        Assert.Contains("--shadcn-background: #101010", style, StringComparison.Ordinal);
        Assert.Contains("--shadcn-primary: #abcdef", style, StringComparison.Ordinal);
        Assert.Contains("--shadcn-radius: 0.625rem", style, StringComparison.Ordinal);
        Assert.DoesNotContain("--shadcn-primary: #123456", style, StringComparison.Ordinal);
        Assert.Equal(new MudColor("#123456"), mud.PaletteLight.Primary);
        Assert.Equal(new MudColor("#abcdef"), mud.PaletteDark.Primary);
        Assert.Equal(new MudColor("#f0f0f0"), mud.PaletteLight.Surface);
        Assert.Equal(new MudColor("#181818"), mud.PaletteDark.Surface);
        Assert.Equal(
            ["Noto Sans Thai, sans-serif"],
            Assert.IsType<string[]>(mud.Typography.Button.FontFamily));
        Assert.Contains("--shadcn-font-mono: 'JetBrains Mono', ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace", style, StringComparison.Ordinal);
        Assert.Contains("--shadcn-typeset-font-mono: 'JetBrains Mono', ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace", style, StringComparison.Ordinal);
        Assert.Contains("--shadcn-spacing-multiplier: 1", style, StringComparison.Ordinal);
        Assert.Contains("--shadcn-focus-ring-width: 3px", style, StringComparison.Ordinal);
        Assert.Contains("--shadcn-motion-duration: 150ms", style, StringComparison.Ordinal);
        Assert.Contains("--shadcn-reduced-motion-duration: 0.01ms", style, StringComparison.Ordinal);
        Assert.Equal("system", root.GetAttribute("data-shadcn-reduced-motion"));
    }

    [Fact]
    public void TypedMudMappingPreservesDefaultAndCustomAlpha()
    {
        var preset = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        var custom = preset with { Dark = preset.Dark with { Border = "#12345680" } };

        var defaultMud = ShadcnThemeFactory.Create(preset);
        var customMud = ShadcnThemeFactory.Create(custom);

        Assert.Equal(new MudColor("#ffffff1a"), defaultMud.PaletteDark.LinesDefault);
        Assert.Equal(new MudColor("#ffffff26"), defaultMud.PaletteDark.LinesInputs);
        Assert.Equal(new MudColor("#12345680"), customMud.PaletteDark.Divider);
    }

    [Fact]
    public async Task AlwaysReduceMarksTheProviderWithoutReplacingConfiguredMotion()
    {
        await using var context = CreateBunitContext();
        var theme = CreateTheme() with
        {
            Metrics = CreateMetrics() with { ReducedMotionBehavior = ShadcnReducedMotionBehavior.AlwaysReduce }
        };

        var cut = context.Render<ShadcnThemeProvider>(parameters => parameters
            .Add(component => component.Theme, theme));
        var root = cut.Find("[data-shadcn-scope]");

        Assert.Equal("always", root.GetAttribute("data-shadcn-reduced-motion"));
        Assert.Contains("--shadcn-motion-duration: 150ms", root.GetAttribute("style"), StringComparison.Ordinal);
    }

    private static BunitContext CreateBunitContext()
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddMalievShadcn();
        return context;
    }

    private static ShadcnTheme CreateTheme() => new()
    {
        SchemaVersion = ShadcnTheme.CurrentSchemaVersion,
        Name = "Test Theme",
        Light = CreateScheme(),
        Dark = CreateScheme() with
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
        },
        Metrics = CreateMetrics()
    };

    private static ShadcnColorScheme CreateScheme() => new()
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

    private static ShadcnThemeMetrics CreateMetrics() => new()
    {
        FontFamily = "'Geist', 'Noto Sans Thai', ui-sans-serif, system-ui, sans-serif",
        MonospaceFontFamily = "'JetBrains Mono', ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace",
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

    private static void AssertMeasurement(
        ShadcnContrastResult actual,
        ShadcnContrastKind kind,
        string scheme,
        string foreground,
        string background,
        double requiredRatio)
    {
        Assert.Equal(kind, actual.Kind);
        Assert.Equal(scheme, actual.Scheme);
        Assert.Equal(foreground, actual.ForegroundToken);
        Assert.Equal(background, actual.BackgroundToken);
        Assert.Equal(requiredRatio, actual.RequiredRatio);
        Assert.True(double.IsFinite(actual.Ratio));
    }

    private static async Task AssertGeneratedCSharpCompiles(string expression)
    {
        var root = FindRoot();
        var directory = Path.Combine(Path.GetTempPath(), $"maliev-theme-csharp-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(directory, "GeneratedTheme.cs"), $$"""
                using Maliev.ShadcnBlazor.Theming;
                using System;

                public static class GeneratedTheme
                {
                    public static ShadcnTheme Create()
                    {
                        return {{expression}}
                    }
                }
                """, new UTF8Encoding(false));
            await File.WriteAllTextAsync(Path.Combine(directory, "GeneratedTheme.csproj"), $$"""
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                    <Nullable>enable</Nullable>
                    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
                  </PropertyGroup>
                  <ItemGroup>
                    <ProjectReference Include="{{Path.Combine(root, "src", "Maliev.ShadcnBlazor", "Maliev.ShadcnBlazor.csproj")}}" />
                  </ItemGroup>
                </Project>
                """, new UTF8Encoding(false));

            var startInfo = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = directory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            foreach (var argument in new[] { "build", "GeneratedTheme.csproj", "-c", "Release", "--nologo" })
                startInfo.ArgumentList.Add(argument);
            using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start generated C# build.");
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            var stdout = await stdoutTask;
            var stderr = await stderrTask;
            Assert.True(process.ExitCode == 0, $"Generated C# build failed.\n{stdout}\n{stderr}");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static async Task<ShadcnTheme> ExecuteGeneratedCSharpFactory(string expression)
    {
        var root = FindRoot();
        var directory = Path.Combine(Path.GetTempPath(), $"maliev-theme-execute-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(directory, "Program.cs"), $$"""
                using Maliev.ShadcnBlazor.Theming;
                using System;

                var theme = GeneratedTheme.Create();
                Console.Write(ShadcnThemeSerializer.Serialize(theme));

                public static class GeneratedTheme
                {
                    public static ShadcnTheme Create()
                    {
                        return {{expression}}
                    }
                }
                """, new UTF8Encoding(false));
            await File.WriteAllTextAsync(Path.Combine(directory, "GeneratedTheme.csproj"), $$"""
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <OutputType>Exe</OutputType>
                    <TargetFramework>net10.0</TargetFramework>
                    <Nullable>enable</Nullable>
                    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
                  </PropertyGroup>
                  <ItemGroup>
                    <ProjectReference Include="{{Path.Combine(root, "src", "Maliev.ShadcnBlazor", "Maliev.ShadcnBlazor.csproj")}}" />
                  </ItemGroup>
                </Project>
                """, new UTF8Encoding(false));

            var startInfo = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = directory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            foreach (var argument in new[] { "run", "--project", "GeneratedTheme.csproj", "-c", "Release", "--nologo" })
                startInfo.ArgumentList.Add(argument);
            using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not execute generated C# factory.");
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            var stdout = await stdoutTask;
            var stderr = await stderrTask;
            Assert.True(process.ExitCode == 0, $"Generated C# execution failed.\n{stdout}\n{stderr}");
            return ShadcnThemeSerializer.Deserialize(stdout);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }

    private static void AssertOrdered(string text, params string[] values)
    {
        var offset = -1;
        foreach (var value in values)
        {
            var current = text.IndexOf(value, offset + 1, StringComparison.Ordinal);
            Assert.True(current > offset, $"Expected '{value}' after offset {offset}.\n{text}");
            offset = current;
        }
    }

    private static int Count(string text, string value)
    {
        var count = 0;
        var offset = 0;
        while ((offset = text.IndexOf(value, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += value.Length;
        }

        return count;
    }
}
