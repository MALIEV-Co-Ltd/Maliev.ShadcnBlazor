using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Tests.Theming;

public sealed class ShadcnPaletteGeneratorTests
{
    [Fact]
    public void AlgorithmVersionsDistinguishMaterializedDocumentsFromDeterministicGeneration()
    {
        Assert.Equal(0, ShadcnPaletteRecipe.MaterializedAlgorithmVersion);
        Assert.Equal(1, ShadcnPaletteRecipe.LegacyAlgorithmVersion);
        Assert.Equal(2, ShadcnPaletteRecipe.CurrentAlgorithmVersion);
        Assert.Equal(2, ShadcnPaletteGenerator.CurrentAlgorithmVersion);

        var legacy = ShadcnThemeDocumentSerializer.Deserialize(
            ShadcnThemeSerializer.Serialize(ShadcnThemePresets.BaseVegaNeutral.CreateTheme()));

        Assert.Equal(ShadcnPaletteRecipe.MaterializedAlgorithmVersion, legacy.Palette.AlgorithmVersion);
    }

    [Fact]
    public void GoldenVectorsFreezeAlgorithmOneWithoutUsingGeneratorDerivedExpectations()
    {
        foreach (var vector in ReadGoldenVectors())
        {
            var result = ShadcnPaletteGenerator.Generate(
                ShadcnThemePresets.BaseVegaNeutral.CreateTheme(),
                new ShadcnPaletteRecipe(1, vector.Seed, vector.BaseColor, []));

            Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Errors.Select(error => error.Message)));
            Assert.Equal(vector.LightBackground, result.Theme.Light.Background);
            Assert.Equal(vector.LightPrimary, result.Theme.Light.Primary);
            Assert.Equal(vector.LightSecondary, result.Theme.Light.Secondary);
            Assert.Equal(vector.LightAccent, result.Theme.Light.Accent);
            Assert.Equal(vector.LightDestructive, result.Theme.Light.Destructive);
            Assert.Equal(vector.LightChart1, result.Theme.Light.Chart1);
            Assert.Equal(vector.DarkPrimary, result.Theme.Dark.Primary);
        }
    }

    [Fact]
    public void SameRecipeIsByteStableAcrossCulturesAndParallelCalls()
    {
        var source = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        var recipe = new ShadcnPaletteRecipe(1, 0x0123456789abcdef, "zinc", []);
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("th-TH");
            var expected = ShadcnThemeSerializer.Serialize(ShadcnPaletteGenerator.Generate(source, recipe).Theme);
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

            var actual = Enumerable.Range(0, 32)
                .AsParallel()
                .Select(_ => ShadcnThemeSerializer.Serialize(ShadcnPaletteGenerator.Generate(source, recipe).Theme))
                .ToArray();

            Assert.All(actual, value => Assert.Equal(expected, value));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData(ShadcnPaletteHarmony.Free)]
    [InlineData(ShadcnPaletteHarmony.Analogous)]
    [InlineData(ShadcnPaletteHarmony.Complementary)]
    [InlineData(ShadcnPaletteHarmony.Triadic)]
    public void VersionTwoIsDeterministicAndMapsAllFiveAnchors(ShadcnPaletteHarmony harmony)
    {
        var source = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        var before = ShadcnThemeSerializer.Serialize(source);
        var recipe = ShadcnPaletteRecipe.CreateV2(117, "neutral", [],
            new("#2563eb", "#14b8a6", "#f59e0b", "#8b5cf6", "#ec4899"), harmony, []);

        var first = ShadcnPaletteGenerator.Generate(source, recipe);
        var expected = ShadcnThemeSerializer.Serialize(first.Theme);
        var repeated = ShadcnThemeSerializer.Serialize(ShadcnPaletteGenerator.Generate(source, recipe).Theme);
        var parallel = Enumerable.Range(0, 32)
            .AsParallel()
            .Select(_ => ShadcnThemeSerializer.Serialize(ShadcnPaletteGenerator.Generate(source, recipe).Theme))
            .ToArray();

        Assert.True(first.IsValid, string.Join(Environment.NewLine, first.Errors));
        Assert.Equal(expected, repeated);
        Assert.All(parallel, value => Assert.Equal(expected, value));
        Assert.Equal(Hue(first.Theme.Light.Primary), Hue(first.Theme.Light.Chart1), precision: 2);
        Assert.Equal(Hue(first.Theme.Light.Secondary), Hue(first.Theme.Light.Chart2), precision: 2);
        Assert.Equal(Hue(first.Theme.Light.Accent), Hue(first.Theme.Light.Chart3), precision: 2);
        Assert.Equal(5, new[]
        {
            first.Theme.Light.Chart1,
            first.Theme.Light.Chart2,
            first.Theme.Light.Chart3,
            first.Theme.Light.Chart4,
            first.Theme.Light.Chart5
        }.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(before, ShadcnThemeSerializer.Serialize(source));
        Assert.Equal(source.Metrics, first.Theme.Metrics);
        Assert.Equal(source.Light.ShadowMedium, first.Theme.Light.ShadowMedium);
        Assert.Equal(source.Dark.ShadowMedium, first.Theme.Dark.ShadowMedium);
    }

    [Theory]
    [InlineData(ShadcnPaletteHarmony.Analogous, 30, -30, 60, -60)]
    [InlineData(ShadcnPaletteHarmony.Complementary, 180, 30, 210, -30)]
    [InlineData(ShadcnPaletteHarmony.Triadic, 120, 240, 60, 300)]
    public void NamedHarmonyOffsetsStayWithinSixDegreesOfGeneratedBrand(
        ShadcnPaletteHarmony harmony,
        double supportOffset,
        double highlightOffset,
        double dataAOffset,
        double dataBOffset)
    {
        var source = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        var recipe = ShadcnPaletteRecipe.CreateV2(117, "neutral", [],
            new("#2563eb", "#14b8a6", "#f59e0b", "#8b5cf6", "#ec4899"), harmony, []);

        var result = ShadcnPaletteGenerator.Generate(source, recipe);
        var brandHue = Hue(result.Theme.Light.Chart1);
        var actualHues = new[]
        {
            Hue(result.Theme.Light.Chart2),
            Hue(result.Theme.Light.Chart3),
            Hue(result.Theme.Light.Chart4),
            Hue(result.Theme.Light.Chart5)
        };
        var offsets = new[] { supportOffset, highlightOffset, dataAOffset, dataBOffset };

        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Errors));
        Assert.All(actualHues.Zip(offsets), pair =>
            Assert.InRange(CircularDistance(pair.First, brandHue + pair.Second), 0, 6.01));
    }

    [Fact]
    public void LockedBrandRemainsByteIdenticalWhileUnlockedAnchorsChangeWithTheSeed()
    {
        const string brand = "oklch(0.4500 0.0800 250.00)";
        var source = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        var anchors = new ShadcnPaletteAnchors(brand, "#14b8a6", "#f59e0b", "#8b5cf6", "#ec4899");
        var first = ShadcnPaletteGenerator.Generate(source, ShadcnPaletteRecipe.CreateV2(
            117, "neutral", [], anchors, ShadcnPaletteHarmony.Triadic, [ShadcnPaletteAnchorRole.Brand]));
        var second = ShadcnPaletteGenerator.Generate(source, ShadcnPaletteRecipe.CreateV2(
            118, "neutral", [], anchors, ShadcnPaletteHarmony.Triadic, [ShadcnPaletteAnchorRole.Brand]));

        Assert.True(first.IsValid, string.Join(Environment.NewLine, first.Errors));
        Assert.True(second.IsValid, string.Join(Environment.NewLine, second.Errors));
        Assert.Equal(brand, first.Theme.Light.Primary);
        Assert.Equal(first.Theme.Light.Primary, second.Theme.Light.Primary);
        Assert.NotEqual(
            new[] { first.Theme.Light.Chart2, first.Theme.Light.Chart3, first.Theme.Light.Chart4, first.Theme.Light.Chart5 },
            new[] { second.Theme.Light.Chart2, second.Theme.Light.Chart3, second.Theme.Light.Chart4, second.Theme.Light.Chart5 });
    }

    [Fact]
    public void LockedSemanticTokenWinsAfterVersionTwoMapping()
    {
        var original = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        var source = original with
        {
            Light = original.Light with { Primary = "oklch(0.3500 0.0800 25.00)" }
        };
        var recipe = ShadcnPaletteRecipe.CreateV2(117, "neutral", ["light.primary"],
            new("#2563eb", "#14b8a6", "#f59e0b", "#8b5cf6", "#ec4899"),
            ShadcnPaletteHarmony.Analogous, []);

        var result = ShadcnPaletteGenerator.Generate(source, recipe);

        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Errors));
        Assert.Equal(source.Light.Primary, result.Theme.Light.Primary);
    }

    [Theory]
    [InlineData("rgb(37 99 235)")]
    [InlineData("#2563ebff")]
    [InlineData("#256f")]
    [InlineData("oklch(0.55 0.20 260 / 50%)")]
    public void InvalidVersionTwoAnchorSyntaxReturnsPathSpecificError(string brand)
    {
        var source = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        var recipe = ShadcnPaletteRecipe.CreateV2(117, "neutral", [],
            new(brand, "#14b8a6", "#f59e0b", "#8b5cf6", "#ec4899"),
            ShadcnPaletteHarmony.Free, []);

        var result = ShadcnPaletteGenerator.Generate(source, recipe);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.Code == "palette-invalid-anchor" && error.Path == "palette.anchors.brand");
        Assert.Equal(ShadcnThemeSerializer.Serialize(source), ShadcnThemeSerializer.Serialize(result.Theme));
    }

    [Fact]
    public void OutOfGamutLockedAnchorIsNormalizedBeforeMapping()
    {
        const string brand = "oklch(0.7000 0.4000 40.00)";
        var source = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        var recipe = ShadcnPaletteRecipe.CreateV2(117, "neutral", [],
            new(brand, "#14b8a6", "#f59e0b", "#8b5cf6", "#ec4899"),
            ShadcnPaletteHarmony.Free, [ShadcnPaletteAnchorRole.Brand]);

        var result = ShadcnPaletteGenerator.Generate(source, recipe);

        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Errors));
        Assert.NotEqual(brand, result.Theme.Light.Primary);
        Assert.Equal(40, Hue(result.Theme.Light.Primary), precision: 2);
        Assert.Matches("^oklch\\([0-9]\\.[0-9]{4} [0-9]\\.[0-9]{4} [0-9]{1,3}\\.[0-9]{2}\\)$", result.Theme.Light.Primary);
    }

    [Fact]
    public void VersionTwoKeepsDestructiveTokensInTheRedFamily()
    {
        var source = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        var recipe = ShadcnPaletteRecipe.CreateV2(117, "slate", [],
            new("#2563eb", "#14b8a6", "#f59e0b", "#8b5cf6", "#ec4899"),
            ShadcnPaletteHarmony.Complementary, []);

        var result = ShadcnPaletteGenerator.Generate(source, recipe);

        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Errors));
        Assert.Equal(25, Hue(result.Theme.Light.Destructive), precision: 2);
        Assert.Equal(25, Hue(result.Theme.Dark.Destructive), precision: 2);
    }

    [Fact]
    public void IncompleteVersionTwoRecipeFailsClosed()
    {
        var source = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        var recipe = new ShadcnPaletteRecipe(
            ShadcnPaletteRecipe.CurrentAlgorithmVersion, 117, "neutral", [], null, null, null);

        var result = ShadcnPaletteGenerator.Generate(source, recipe);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Path == "palette.anchors");
        Assert.Contains(result.Errors, error => error.Path == "palette.harmony");
        Assert.Contains(result.Errors, error => error.Path == "palette.lockedAnchors");
        Assert.Equal(ShadcnThemeSerializer.Serialize(source), ShadcnThemeSerializer.Serialize(result.Theme));
    }

    [Fact]
    public void GeneratorPopulatesEveryColorWithoutChangingShadowsMetricsOrSource()
    {
        var source = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        var before = ShadcnThemeSerializer.Serialize(source);

        var result = ShadcnPaletteGenerator.Generate(source, new(1, 42, "neutral", []));

        Assert.True(result.IsValid);
        Assert.Equal(before, ShadcnThemeSerializer.Serialize(source));
        Assert.Equal(source.Metrics, result.Theme.Metrics);
        Assert.Equal(source.Light.ShadowExtraSmall, result.Theme.Light.ShadowExtraSmall);
        Assert.Equal(source.Light.ShadowSmall, result.Theme.Light.ShadowSmall);
        Assert.Equal(source.Light.ShadowMedium, result.Theme.Light.ShadowMedium);
        Assert.Equal(source.Dark.ShadowExtraSmall, result.Theme.Dark.ShadowExtraSmall);
        Assert.Equal(source.Dark.ShadowSmall, result.Theme.Dark.ShadowSmall);
        Assert.Equal(source.Dark.ShadowMedium, result.Theme.Dark.ShadowMedium);
        Assert.All(ColorValues(result.Theme), value => Assert.Matches("^oklch\\([0-9]\\.[0-9]{4} [0-9]\\.[0-9]{4} [0-9]{1,3}\\.[0-9]{2}\\)$", value));
        Assert.Equal(64, ColorValues(result.Theme).Count);
    }

    [Fact]
    public void LocksUseCurrentMaterializedValuesWithoutPerturbingOtherGeneratedTokens()
    {
        var source = ShadcnThemePresets.BaseVegaNeutral.CreateTheme() with
        {
            Light = ShadcnThemePresets.BaseVegaNeutral.CreateTheme().Light with
            {
                Primary = "oklch(0.4200 0.1200 20.00)"
            }
        };
        var unlocked = ShadcnPaletteGenerator.Generate(source, new(1, 73, "stone", []));
        var locked = ShadcnPaletteGenerator.Generate(source, new(1, 73, "stone", ["light.primary"]));

        Assert.True(locked.IsValid);
        Assert.Equal(source.Light.Primary, locked.Theme.Light.Primary);
        Assert.Equal(unlocked.Theme.Light.Accent, locked.Theme.Light.Accent);
        Assert.Equal(unlocked.Theme.Dark.Primary, locked.Theme.Dark.Primary);
    }

    [Fact]
    public void ImpossibleLockedContrastReturnsPathSpecificErrorAndDoesNotMutateSource()
    {
        var original = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        var source = original with
        {
            Light = original.Light with
            {
                Primary = "oklch(0.5000 0.0000 0.00)",
                PrimaryForeground = "oklch(0.5000 0.0000 0.00)"
            }
        };
        var before = ShadcnThemeSerializer.Serialize(source);

        var result = ShadcnPaletteGenerator.Generate(source,
            new(1, 5, "neutral", ["light.primary", "light.primaryForeground"]));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.Code == "palette-locked-constraint" && error.Path == "light.primaryForeground");
        Assert.Equal(before, ShadcnThemeSerializer.Serialize(source));
    }

    [Fact]
    public void SolverPreservesOneLockedEndpointAndRepairsItsUnlockedPartner()
    {
        var original = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        var source = original with
        {
            Light = original.Light with { Primary = "oklch(0.8500 0.0200 30.00)" }
        };

        var result = ShadcnPaletteGenerator.Generate(source, new(1, 5, "neutral", ["light.primary"]));

        Assert.True(result.IsValid);
        Assert.Equal(source.Light.Primary, result.Theme.Light.Primary);
        Assert.NotEqual("oklch(0.9850 0.0040 317.99)", result.Theme.Light.PrimaryForeground);
        Assert.Contains(ShadcnThemeValidator.Validate(result.Theme).ContrastResults, measurement =>
            measurement.Scheme == "light" && measurement.ForegroundToken == "primaryForeground" && measurement.Passes);
    }

    [Theory]
    [InlineData(0, "neutral", "palette-unsupported-algorithm", "palette.algorithmVersion")]
    [InlineData(3, "neutral", "palette-unsupported-algorithm", "palette.algorithmVersion")]
    [InlineData(1, "unknown", "palette-invalid-base-color", "palette.baseColor")]
    public void InvalidRecipesFailClosed(int version, string baseColor, string code, string path)
    {
        var source = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();

        var result = ShadcnPaletteGenerator.Generate(source, new(version, 0, baseColor, []));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Code == code && error.Path == path);
        Assert.Equal(ShadcnThemeSerializer.Serialize(source), ShadcnThemeSerializer.Serialize(result.Theme));
    }

    [Theory]
    [InlineData("light.primary", "light.primary")]
    [InlineData("light.shadowSmall", null)]
    [InlineData("Light.primary", null)]
    [InlineData("light.missing", null)]
    public void LockCatalogIsExplicitAndCaseSensitive(string token, string? expected)
    {
        var source = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        string[] locks = expected is null ? [token] : [token, token];

        var result = ShadcnPaletteGenerator.Generate(source, new(1, 0, "neutral", locks));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Code == (expected is null ? "palette-invalid-lock" : "palette-duplicate-lock"));
    }

    private static List<string> ColorValues(ShadcnTheme theme) =>
        new[] { theme.Light, theme.Dark }
            .SelectMany(scheme => typeof(ShadcnColorScheme).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => !property.Name.StartsWith("Shadow", StringComparison.Ordinal))
                .Select(property => Assert.IsType<string>(property.GetValue(scheme))))
            .ToList();

    private static double Hue(string value)
    {
        var hue = value.AsSpan(value.LastIndexOf(' ') + 1);
        hue = hue[..hue.IndexOf(')')];
        return double.Parse(hue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
    }

    private static double CircularDistance(double first, double second)
    {
        var difference = Math.Abs(first - second) % 360d;
        return Math.Min(difference, 360d - difference);
    }

    private static IReadOnlyList<GoldenVector> ReadGoldenVectors()
    {
        using var stream = typeof(ShadcnPaletteGeneratorTests).Assembly.GetManifestResourceStream(
            "Maliev.ShadcnBlazor.Tests.Theming.TestData.PaletteGeneratorV1Golden.json");
        Assert.NotNull(stream);
        return JsonSerializer.Deserialize<List<GoldenVector>>(stream!, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }

    private sealed record GoldenVector(
        string BaseColor,
        ulong Seed,
        string LightBackground,
        string LightPrimary,
        string LightSecondary,
        string LightAccent,
        string LightDestructive,
        string LightChart1,
        string DarkPrimary);
}
