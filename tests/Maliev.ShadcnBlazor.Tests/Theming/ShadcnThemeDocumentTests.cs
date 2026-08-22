using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Tests.Theming;

public sealed class ShadcnThemeDocumentTests
{
    [Fact]
    public void CanonicalDocumentRoundTripIsDeterministicAndLossless()
    {
        var source = CreateDocument();

        var first = ShadcnThemeDocumentSerializer.Serialize(source);
        var restored = ShadcnThemeDocumentSerializer.Deserialize(first);
        var second = ShadcnThemeDocumentSerializer.Serialize(restored);

        AssertEquivalent(source, restored);
        Assert.Equal(first, second);
        Assert.EndsWith("\n", first, StringComparison.Ordinal);
        Assert.DoesNotContain('\r', first);
        AssertOrdered(first, "\"schemaVersion\"", "\"name\"", "\"theme\"", "\"application\"", "\"palette\"", "\"typography\"");
    }

    [Fact]
    public void Utf8DeserializeMatchesStringDeserialize()
    {
        var json = ShadcnThemeDocumentSerializer.Serialize(CreateDocument());

        var fromBytes = ShadcnThemeDocumentSerializer.Deserialize(Encoding.UTF8.GetBytes(json));

        AssertEquivalent(ShadcnThemeDocumentSerializer.Deserialize(json), fromBytes);
    }

    [Theory]
    [InlineData("\"schemaVersion\": 2", "\"schemaVersion\": 999")]
    [InlineData("\"name\": \"Factory Night\"", "\"name\": \"Factory Night\", \"unknown\": true")]
    [InlineData("\"style\": \"vega\"", "\"style\": \"vega\", \"unknown\": true")]
    public void CanonicalDocumentRejectsFutureOrUnknownMembers(string source, string replacement)
    {
        var json = ShadcnThemeDocumentSerializer.Serialize(CreateDocument()).Replace(source, replacement, StringComparison.Ordinal);

        Assert.ThrowsAny<Exception>(() => ShadcnThemeDocumentSerializer.Deserialize(json));
    }

    [Fact]
    public void CanonicalDocumentRejectsDuplicateMembers()
    {
        var json = ShadcnThemeDocumentSerializer.Serialize(CreateDocument())
            .Replace("\"schemaVersion\": 2", "\"schemaVersion\": 2, \"schemaVersion\": 2", StringComparison.Ordinal);

        var exception = Assert.Throws<JsonException>(() => ShadcnThemeDocumentSerializer.Deserialize(json));

        Assert.Contains("duplicate", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void StringDeserializerRejectsUnpairedUnicodeSurrogates()
    {
        var json = ShadcnThemeDocumentSerializer.Serialize(CreateDocument())
            .Replace("Factory Night", "Factory \ud800 Night", StringComparison.Ordinal);

        var exception = Assert.Throws<JsonException>(() => ShadcnThemeDocumentSerializer.Deserialize(json));

        Assert.Contains("Unicode", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RawSchemaZeroThemeMigratesWithoutChangingRuntimeTokens()
    {
        var theme = ShadcnThemePresets.BaseVegaNeutral.CreateTheme() with { Name = "Legacy zero" };
        var json = ShadcnThemeSerializer.Serialize(theme).Replace("  \"schemaVersion\": 1,\n", string.Empty, StringComparison.Ordinal);

        var document = ShadcnThemeDocumentSerializer.Deserialize(json);

        Assert.Equal(ShadcnTheme.CurrentSchemaVersion, document.Theme.SchemaVersion);
        Assert.Equal(theme, document.Theme);
        Assert.Equal("custom", document.Application.Preset);
        Assert.Empty(document.Palette.LockedTokens);
    }

    [Fact]
    public void RawSchemaOneThemeMigratesWithoutChangingRuntimeTokens()
    {
        var theme = ShadcnThemePresets.BaseVegaNeutral.CreateTheme() with { Name = "Legacy one" };

        var document = ShadcnThemeDocumentSerializer.Deserialize(ShadcnThemeSerializer.Serialize(theme));

        Assert.Equal(theme, document.Theme);
        Assert.Equal(theme.Name, document.Name);
        Assert.Equal(theme.Metrics.FontFamily, document.Typography.Body.Family);
        Assert.Equal(theme.Metrics.MonospaceFontFamily, document.Typography.Code.Family);
    }

    [Fact]
    public void LegacyGeneratorConfigMigratesAllApplicationMetadata()
    {
        var theme = ShadcnThemePresets.BaseVegaNeutral.CreateTheme() with { Name = "Legacy generator" };
        var nested = ShadcnThemeSerializer.Serialize(theme).Trim();
        var json = $$"""
        {
          "schemaVersion": 1,
          "preset": "base-vega-neutral",
          "style": "vega",
          "baseColor": "neutral",
          "iconLibrary": "tabler",
          "menuAccent": "bold",
          "menuColor": "translucent",
          "radiusPreset": "default",
          "fontFamily": {{JsonSerializer.Serialize(theme.Metrics.FontFamily)}},
          "monospaceFontFamily": {{JsonSerializer.Serialize(theme.Metrics.MonospaceFontFamily)}},
          "theme": {{nested}}
        }
        """;

        var document = ShadcnThemeDocumentSerializer.Deserialize(json);

        Assert.Equal(theme, document.Theme);
        Assert.Equal("base-vega-neutral", document.Application.Preset);
        Assert.Equal("tabler", document.Application.IconLibrary);
        Assert.Equal("bold", document.Application.MenuAccent);
        Assert.Equal("translucent", document.Application.MenuColor);
    }

    [Theory]
    [InlineData("fontFamily", "'Wrong', sans-serif")]
    [InlineData("monospaceFontFamily", "'Wrong Mono', monospace")]
    public void LegacyGeneratorConfigRejectsDivergentDuplicatedFonts(string property, string divergentValue)
    {
        var theme = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        var root = JsonNode.Parse(LegacyGeneratorJson(theme))!.AsObject();
        root[property] = divergentValue;
        var json = root.ToJsonString();

        var exception = Assert.Throws<JsonException>(() => ShadcnThemeDocumentSerializer.Deserialize(json));

        Assert.Contains(property, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void LegacyGeneratorConfigRejectsDivergentRadiusPreset()
    {
        var theme = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        var json = LegacyGeneratorJson(theme).Replace("\"radiusPreset\": \"default\"", "\"radiusPreset\": \"pill\"", StringComparison.Ordinal);

        var exception = Assert.Throws<JsonException>(() => ShadcnThemeDocumentSerializer.Deserialize(json));

        Assert.Contains("radiusPreset", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AmbiguousRawAndGeneratorShapeIsRejected()
    {
        var raw = ShadcnThemeSerializer.Serialize(ShadcnThemePresets.BaseVegaNeutral.CreateTheme());
        var json = raw.Replace("  \"name\":", $"  \"theme\": {raw.Trim()},\n  \"name\":", StringComparison.Ordinal);

        Assert.Throws<JsonException>(() => ShadcnThemeDocumentSerializer.Deserialize(json));
    }

    [Fact]
    public void ValidatorRejectsTypographyCompatibilityDrift()
    {
        var source = CreateDocument();
        var invalid = source with
        {
            Typography = source.Typography with
            {
                Body = source.Typography.Body with { Family = "'Different', sans-serif" }
            }
        };

        var result = ShadcnThemeDocumentValidator.Validate(invalid);

        Assert.Contains(result.Errors, item => item.Code == "incompatible-font" && item.Path == "typography.body.family");
    }

    [Fact]
    public async Task LoaderBoundsInputAndSupportsSyncAndAsyncStreams()
    {
        var bytes = Encoding.UTF8.GetBytes(ShadcnThemeDocumentSerializer.Serialize(CreateDocument()));
        using var syncStream = new MemoryStream(bytes);
        await using var asyncStream = new MemoryStream(bytes);

        var sync = ShadcnThemeDocumentLoader.Load(syncStream);
        var asyncResult = await ShadcnThemeDocumentLoader.LoadAsync(asyncStream);

        AssertEquivalent(sync, asyncResult);
        AssertEquivalent(CreateDocument(), sync);
    }

    [Fact]
    public void LoaderRejectsOversizedInvalidUtf8AndExcessiveDepth()
    {
        using var oversized = new MemoryStream(new byte[ShadcnThemeDocumentLoader.MaxDocumentBytes + 1]);
        using var invalidUtf8 = new MemoryStream([0xff, 0xfe, 0xfd]);
        using var deep = new MemoryStream(Encoding.UTF8.GetBytes(new string('[', 40) + new string(']', 40)));

        Assert.Throws<InvalidDataException>(() => ShadcnThemeDocumentLoader.Load(oversized));
        Assert.Throws<InvalidDataException>(() => ShadcnThemeDocumentLoader.Load(invalidUtf8));
        Assert.ThrowsAny<JsonException>(() => ShadcnThemeDocumentLoader.Load(deep));
    }

    [Fact]
    public void SerializationAndValidationAreCultureInvariant()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("th-TH");
            var thai = ShadcnThemeDocumentSerializer.Serialize(CreateDocument());
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var french = ShadcnThemeDocumentSerializer.Serialize(CreateDocument());
            Assert.Equal(thai, french);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void PaletteRecipeTakesAnImmutableSnapshotOfLockedTokens()
    {
        var source = new List<string> { "light.primary" };
        var recipe = new ShadcnPaletteRecipe(1, 42, "neutral", source);

        source[0] = "dark.primary";
        source.Add("light.accent");

        Assert.Equal(["light.primary"], recipe.LockedTokens);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)recipe.LockedTokens).Add("light.border"));
    }

    [Fact]
    public void TypographyScaleTakesAnImmutableSnapshotOfRoleStyles()
    {
        var source = new Dictionary<ShadcnTypographyRole, ShadcnTypographyRoleStyle>
        {
            [ShadcnTypographyRole.Body] = new(400, 1, 1.5, 0)
        };
        var scale = new ShadcnTypographyScale(
            new("Body", "sans-serif", null),
            new("Thai", "sans-serif", null),
            new("Code", "monospace", null),
            source);

        source[ShadcnTypographyRole.Body] = new(700, 2, 1, 0);
        source[ShadcnTypographyRole.Code] = new(400, 1, 1.5, 0);

        Assert.Equal(400, scale.Roles[ShadcnTypographyRole.Body].Weight);
        Assert.False(scale.Roles.ContainsKey(ShadcnTypographyRole.Code));
        Assert.Throws<NotSupportedException>(() =>
            ((IDictionary<ShadcnTypographyRole, ShadcnTypographyRoleStyle>)scale.Roles)
            .Add(ShadcnTypographyRole.Code, new(400, 1, 1.5, 0)));
    }

    private static ShadcnThemeDocument CreateDocument()
    {
        var theme = ShadcnThemePresets.BaseVegaNeutral.CreateTheme() with { Name = "Factory Night" };
        return new ShadcnThemeDocument
        {
            Name = theme.Name,
            Theme = theme,
            Application = new ShadcnThemeApplication(
                "base-vega-neutral", "vega", "neutral", "lucide", "default", "default",
                false, ShadcnDirection.LeftToRight, "en", ShadcnReducedMotionBehavior.RespectSystemPreference),
            Palette = new ShadcnPaletteRecipe(1, 42, "neutral", ["light.primary"]),
            Typography = new ShadcnTypographyScale(
                new ShadcnFontSelection(theme.Metrics.FontFamily, "sans-serif", null),
                new ShadcnFontSelection("'Noto Sans Thai'", "sans-serif", null),
                new ShadcnFontSelection(theme.Metrics.MonospaceFontFamily, "monospace", null),
                new Dictionary<ShadcnTypographyRole, ShadcnTypographyRoleStyle>
                {
                    [ShadcnTypographyRole.Body] = new(400, 1, 1.5, 0),
                    [ShadcnTypographyRole.Code] = new(400, 0.875, 1.5, 0)
                })
        };
    }

    private static string LegacyGeneratorJson(ShadcnTheme theme) => $$"""
    {
      "schemaVersion": 1,
      "preset": "base-vega-neutral",
      "style": "vega",
      "baseColor": "neutral",
      "iconLibrary": "lucide",
      "menuAccent": "default",
      "menuColor": "default",
      "radiusPreset": "default",
      "fontFamily": {{JsonSerializer.Serialize(theme.Metrics.FontFamily)}},
      "monospaceFontFamily": {{JsonSerializer.Serialize(theme.Metrics.MonospaceFontFamily)}},
      "theme": {{ShadcnThemeSerializer.Serialize(theme).Trim()}}
    }
    """;

    private static void AssertOrdered(string value, params string[] fragments)
    {
        var previous = -1;
        foreach (var fragment in fragments)
        {
            var current = value.IndexOf(fragment, StringComparison.Ordinal);
            Assert.True(current > previous, $"Expected {fragment} after index {previous}.");
            previous = current;
        }
    }

    private static void AssertEquivalent(ShadcnThemeDocument expected, ShadcnThemeDocument actual) =>
        Assert.Equal(
            ShadcnThemeDocumentSerializer.Serialize(expected),
            ShadcnThemeDocumentSerializer.Serialize(actual));
}
