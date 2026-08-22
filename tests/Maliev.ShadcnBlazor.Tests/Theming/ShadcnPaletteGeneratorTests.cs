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
        Assert.Equal(1, ShadcnPaletteRecipe.CurrentAlgorithmVersion);
        Assert.Equal(1, ShadcnPaletteGenerator.CurrentAlgorithmVersion);

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
    [InlineData(2, "neutral", "palette-unsupported-algorithm", "palette.algorithmVersion")]
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
