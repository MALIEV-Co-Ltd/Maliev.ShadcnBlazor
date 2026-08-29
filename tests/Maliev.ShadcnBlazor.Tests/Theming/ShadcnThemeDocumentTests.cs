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
        Assert.Equal(ShadcnPaletteRecipe.MaterializedAlgorithmVersion, document.Palette.AlgorithmVersion);
        Assert.Empty(document.Palette.LockedTokens);
    }

    [Fact]
    public void RawSchemaOneThemeMigratesWithoutChangingRuntimeTokens()
    {
        var theme = ShadcnThemePresets.BaseVegaNeutral.CreateTheme() with { Name = "Legacy one" };

        var document = ShadcnThemeDocumentSerializer.Deserialize(ShadcnThemeSerializer.Serialize(theme));

        Assert.Equal(theme, document.Theme);
        Assert.Equal(theme.Name, document.Name);
        Assert.Equal(ShadcnPaletteRecipe.MaterializedAlgorithmVersion, document.Palette.AlgorithmVersion);
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
        Assert.Equal(ShadcnPaletteRecipe.MaterializedAlgorithmVersion, document.Palette.AlgorithmVersion);
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
    public void ValidatorRequiresEverySemanticTypographyRole()
    {
        var source = CreateDocument();
        var roles = source.Typography.Roles
            .Where(item => item.Key is ShadcnTypographyRole.Body or ShadcnTypographyRole.Code)
            .ToDictionary();
        var incomplete = source with
        {
            Typography = new ShadcnTypographyScale(
                source.Typography.Body,
                source.Typography.ThaiFallback,
                source.Typography.Code,
                roles)
        };

        var result = ShadcnThemeDocumentValidator.Validate(incomplete);

        Assert.Contains(result.Errors, item =>
            item.Code == "required-typography-role" &&
            item.Path == "typography.roles.heading1");
        Assert.Contains(result.Errors, item =>
            item.Code == "required-typography-role" &&
            item.Path == "typography.roles.label");
        Assert.Contains(result.Errors, item =>
            item.Code == "required-typography-role" &&
            item.Path == "typography.roles.button");
    }

    [Theory]
    [InlineData(99, 1, 1.5, 0)]
    [InlineData(950, 1, 1.5, 0)]
    [InlineData(400, 0.624, 1.5, 0)]
    [InlineData(400, 4.001, 1.5, 0)]
    [InlineData(400, 1, 0.999, 0)]
    [InlineData(400, 1, 2.501, 0)]
    [InlineData(400, 1, 1.5, -0.101)]
    [InlineData(400, 1, 1.5, 0.201)]
    public void ValidatorRejectsTypographyRoleValuesOutsideSafeEditorBounds(
        int weight,
        double scale,
        double lineHeight,
        double letterSpacing)
    {
        var source = CompleteTypographyRoles(CreateDocument());
        var roles = source.Typography.Roles.ToDictionary();
        roles[ShadcnTypographyRole.Body] = new(weight, scale, lineHeight, letterSpacing);
        var invalid = source with
        {
            Typography = new ShadcnTypographyScale(
                source.Typography.Body,
                source.Typography.ThaiFallback,
                source.Typography.Code,
                roles)
        };

        var result = ShadcnThemeDocumentValidator.Validate(invalid);

        Assert.Contains(result.Errors, item =>
            item.Code == "invalid-typography-role" &&
            item.Path == "typography.roles.body");
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
    public void VersionOneRecipeSerializationRemainsByteIdentical()
    {
        var recipe = new ShadcnPaletteRecipe(1, 42, "neutral", ["light.primary"]);
        var document = CreateDocument() with { Palette = recipe };

        var json = ShadcnThemeDocumentSerializer.Serialize(document);

        Assert.DoesNotContain("\"anchors\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"harmony\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"lockedAnchors\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"isVersion2\"", json, StringComparison.Ordinal);
        Assert.Contains("""
          "palette": {
            "algorithmVersion": 1,
            "seed": 42,
            "baseColor": "neutral",
            "lockedTokens": [
              "light.primary"
            ]
          },
        """, json, StringComparison.Ordinal);
        Assert.Equal(json, ShadcnThemeDocumentSerializer.Serialize(
            ShadcnThemeDocumentSerializer.Deserialize(json)));
    }

    [Fact]
    public void VersionTwoRecipeTakesDefensiveAnchorLockSnapshotAndRoundTrips()
    {
        var locks = new[] { ShadcnPaletteAnchorRole.Brand };
        var anchors = new ShadcnPaletteAnchors("#2563eb", "#14b8a6", "#f59e0b", "#8b5cf6", "#ec4899");
        var recipe = ShadcnPaletteRecipe.CreateV2(42, "neutral", [], anchors,
            ShadcnPaletteHarmony.Triadic, locks);
        locks[0] = ShadcnPaletteAnchorRole.DataB;

        var restored = ShadcnThemeDocumentSerializer.Deserialize(
            ShadcnThemeDocumentSerializer.Serialize(CreateDocument() with { Palette = recipe })).Palette;

        Assert.Equal(2, restored.AlgorithmVersion);
        Assert.Equal(anchors, restored.Anchors);
        Assert.Equal(ShadcnPaletteHarmony.Triadic, restored.Harmony);
        Assert.Equal([ShadcnPaletteAnchorRole.Brand], restored.LockedAnchors);
    }

    [Fact]
    public void VersionTwoFactoryRejectsUndefinedHarmony()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => ShadcnPaletteRecipe.CreateV2(
            42,
            "neutral",
            [],
            new ShadcnPaletteAnchors("#2563eb", "#14b8a6", "#f59e0b", "#8b5cf6", "#ec4899"),
            (ShadcnPaletteHarmony)99,
            [ShadcnPaletteAnchorRole.Brand]));

        Assert.Equal("harmony", exception.ParamName);
        Assert.StartsWith("Unknown palette harmony.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidatorRejectsUndefinedVersionTwoHarmony()
    {
        var recipe = new ShadcnPaletteRecipe(
            ShadcnPaletteRecipe.CurrentAlgorithmVersion,
            42,
            "neutral",
            [],
            new ShadcnPaletteAnchors("#2563eb", "#14b8a6", "#f59e0b", "#8b5cf6", "#ec4899"),
            (ShadcnPaletteHarmony)99,
            [ShadcnPaletteAnchorRole.Brand]);

        var validation = ShadcnThemeDocumentValidator.Validate(CreateDocument() with { Palette = recipe });

        Assert.Contains(validation.Errors, error =>
            error.Code == "invalid-palette-harmony" &&
            error.Path == "palette.harmony" &&
            error.Message == "Palette harmony must be a supported value.");
    }

    [Fact]
    public void DuplicateVersionTwoAnchorLocksAreRejectedByDocumentValidation()
    {
        var recipe = new ShadcnPaletteRecipe(
            ShadcnPaletteRecipe.CurrentAlgorithmVersion,
            42,
            "neutral",
            [],
            new ShadcnPaletteAnchors("#2563eb", "#14b8a6", "#f59e0b", "#8b5cf6", "#ec4899"),
            ShadcnPaletteHarmony.Triadic,
            [ShadcnPaletteAnchorRole.Brand, ShadcnPaletteAnchorRole.Brand]);
        var document = CreateDocument() with { Palette = recipe };

        var validation = ShadcnThemeDocumentValidator.Validate(document);

        Assert.Contains(validation.Errors, error =>
            error.Code == "invalid-locked-anchor" && error.Path == "palette.lockedAnchors");
        Assert.Throws<JsonException>(() => ShadcnThemeDocumentSerializer.Serialize(document));
    }

    [Theory]
    [InlineData(ShadcnPaletteRecipe.MaterializedAlgorithmVersion, "anchors")]
    [InlineData(ShadcnPaletteRecipe.MaterializedAlgorithmVersion, "harmony")]
    [InlineData(ShadcnPaletteRecipe.MaterializedAlgorithmVersion, "lockedAnchors")]
    [InlineData(ShadcnPaletteRecipe.LegacyAlgorithmVersion, "anchors")]
    [InlineData(ShadcnPaletteRecipe.LegacyAlgorithmVersion, "harmony")]
    [InlineData(ShadcnPaletteRecipe.LegacyAlgorithmVersion, "lockedAnchors")]
    public void CanonicalVersionZeroOrOneRejectsExplicitNullVersionTwoMembers(int algorithmVersion, string member)
    {
        var document = CreateDocument() with
        {
            Palette = new ShadcnPaletteRecipe(algorithmVersion, 42, "neutral", ["light.primary"])
        };
        var root = JsonNode.Parse(ShadcnThemeDocumentSerializer.Serialize(document))!.AsObject();
        root["palette"]!.AsObject()[member] = null;

        var exception = Assert.Throws<JsonException>(() =>
            ShadcnThemeDocumentSerializer.Deserialize(root.ToJsonString()));

        Assert.Equal(
            "Theme document is invalid: unexpected-palette-v2-field at palette: Version-two palette fields are not allowed on materialized or version-one recipes.",
            exception.Message);
    }

    [Fact]
    public void CanonicalVersionTwoRejectsDuplicateAnchorLocksFromRawJson()
    {
        var recipe = ShadcnPaletteRecipe.CreateV2(
            42,
            "neutral",
            [],
            new ShadcnPaletteAnchors("#2563eb", "#14b8a6", "#f59e0b", "#8b5cf6", "#ec4899"),
            ShadcnPaletteHarmony.Triadic,
            [ShadcnPaletteAnchorRole.Brand]);
        var root = JsonNode.Parse(ShadcnThemeDocumentSerializer.Serialize(CreateDocument() with { Palette = recipe }))!.AsObject();
        root["palette"]!.AsObject()["lockedAnchors"] = new JsonArray("brand", "brand");

        var exception = Assert.Throws<JsonException>(() =>
            ShadcnThemeDocumentSerializer.Deserialize(root.ToJsonString()));

        Assert.Equal(
            "Theme document is invalid: invalid-locked-anchor at palette.lockedAnchors: Locked anchors must be unique supported roles.",
            exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CanonicalVersionTwoRejectsMissingOrNullAnchorMembersFromRawJson(bool removeMember)
    {
        var recipe = ShadcnPaletteRecipe.CreateV2(
            42,
            "neutral",
            [],
            new ShadcnPaletteAnchors("#2563eb", "#14b8a6", "#f59e0b", "#8b5cf6", "#ec4899"),
            ShadcnPaletteHarmony.Triadic,
            [ShadcnPaletteAnchorRole.Brand]);
        var root = JsonNode.Parse(ShadcnThemeDocumentSerializer.Serialize(CreateDocument() with { Palette = recipe }))!.AsObject();
        var anchors = root["palette"]!.AsObject()["anchors"]!.AsObject();
        if (removeMember)
            anchors.Remove("dataB");
        else
            anchors["dataB"] = null;

        var exception = Assert.Throws<JsonException>(() =>
            ShadcnThemeDocumentSerializer.Deserialize(root.ToJsonString()));

        Assert.Equal(
            "Theme document is invalid: invalid-palette-anchors at palette.anchors: Palette anchors must define all five non-null string values.",
            exception.Message);
    }

    [Fact]
    public void CanonicalVersionTwoRejectsNonStringAnchorMembersFromRawJson()
    {
        var recipe = ShadcnPaletteRecipe.CreateV2(
            42,
            "neutral",
            [],
            new ShadcnPaletteAnchors("#2563eb", "#14b8a6", "#f59e0b", "#8b5cf6", "#ec4899"),
            ShadcnPaletteHarmony.Triadic,
            [ShadcnPaletteAnchorRole.Brand]);
        var root = JsonNode.Parse(ShadcnThemeDocumentSerializer.Serialize(CreateDocument() with { Palette = recipe }))!.AsObject();
        root["palette"]!.AsObject()["anchors"]!.AsObject()["dataB"] = 42;

        Assert.Throws<JsonException>(() => ShadcnThemeDocumentSerializer.Deserialize(root.ToJsonString()));
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
                    [ShadcnTypographyRole.Heading1] = new(700, 2.25, 1.1, -0.03),
                    [ShadcnTypographyRole.Heading2] = new(700, 1.875, 1.15, -0.025),
                    [ShadcnTypographyRole.Heading3] = new(600, 1.5, 1.2, -0.02),
                    [ShadcnTypographyRole.Heading4To6] = new(600, 1.125, 1.3, -0.01),
                    [ShadcnTypographyRole.Label] = new(500, 0.875, 1.4, 0),
                    [ShadcnTypographyRole.Button] = new(500, 0.875, 1, 0),
                    [ShadcnTypographyRole.Caption] = new(400, 0.75, 1.4, 0),
                    [ShadcnTypographyRole.Code] = new(400, 0.875, 1.5, 0)
                })
        };
    }

    private static ShadcnThemeDocument CompleteTypographyRoles(ShadcnThemeDocument document)
    {
        var roles = Enum.GetValues<ShadcnTypographyRole>().ToDictionary(
            role => role,
            role => document.Typography.Roles.TryGetValue(role, out var style)
                ? style
                : new ShadcnTypographyRoleStyle(400, 1, 1.5, 0));
        return document with
        {
            Typography = new ShadcnTypographyScale(
                document.Typography.Body,
                document.Typography.ThaiFallback,
                document.Typography.Code,
                roles)
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
