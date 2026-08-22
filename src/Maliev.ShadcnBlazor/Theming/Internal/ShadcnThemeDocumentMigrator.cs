using System.Text.Json;

namespace Maliev.ShadcnBlazor.Theming.Internal;

internal static class ShadcnThemeDocumentMigrator
{
    internal static ShadcnThemeDocument FromTheme(ShadcnTheme theme) => Create(
        theme,
        new("custom", "vega", "neutral", "lucide", "default", "default", false,
            ShadcnDirection.LeftToRight, "en", theme.Metrics.ReducedMotionBehavior),
        "neutral");

    internal static ShadcnThemeDocument FromGeneratorConfigV1(JsonElement root, JsonSerializerOptions options)
    {
        var legacy = root.Deserialize<LegacyGeneratorConfig>(options)
                     ?? throw new JsonException("Theme Studio generator JSON produced no value.");
        if (legacy.SchemaVersion != 1)
            throw new NotSupportedException($"Theme Studio generator schema version {legacy.SchemaVersion} is not supported.");
        if (!string.Equals(legacy.FontFamily, legacy.Theme.Metrics.FontFamily, StringComparison.Ordinal))
            throw new JsonException("fontFamily conflicts with theme.metrics.fontFamily.");
        if (!string.Equals(legacy.MonospaceFontFamily, legacy.Theme.Metrics.MonospaceFontFamily, StringComparison.Ordinal))
            throw new JsonException("monospaceFontFamily conflicts with theme.metrics.monospaceFontFamily.");
        var expectedRadius = legacy.RadiusPreset switch
        {
            "sharp" => 0,
            "compact" => 0.375,
            "default" => 0.625,
            "relaxed" => 0.875,
            "pill" => 1.25,
            _ => throw new JsonException($"radiusPreset '{legacy.RadiusPreset}' is unsupported.")
        };
        if (Math.Abs(expectedRadius - legacy.Theme.Metrics.RadiusRem) > 0.0001)
            throw new JsonException("radiusPreset conflicts with theme.metrics.radiusRem.");

        return Create(legacy.Theme,
            new(legacy.Preset, legacy.Style, legacy.BaseColor, legacy.IconLibrary, legacy.MenuAccent,
                legacy.MenuColor, false, ShadcnDirection.LeftToRight, "en", legacy.Theme.Metrics.ReducedMotionBehavior),
            legacy.BaseColor);
    }

    internal static ShadcnThemeDocument Create(ShadcnTheme theme, ShadcnThemeApplication application, string baseColor) => new()
    {
        Name = theme.Name,
        Theme = theme,
        Application = application,
        Palette = new ShadcnPaletteRecipe(ShadcnPaletteRecipe.MaterializedAlgorithmVersion, 0, baseColor, Array.Empty<string>()),
        Typography = new ShadcnTypographyScale(
            new(theme.Metrics.FontFamily, "ui-sans-serif, system-ui, sans-serif", null),
            new("'Noto Sans Thai', sans-serif", "sans-serif", null),
            new(theme.Metrics.MonospaceFontFamily, "ui-monospace, monospace", null),
            DefaultRoles())
    };

    private static IReadOnlyDictionary<ShadcnTypographyRole, ShadcnTypographyRoleStyle> DefaultRoles() =>
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
        };

    private sealed record LegacyGeneratorConfig
    {
        public int SchemaVersion { get; init; }
        public required string Preset { get; init; }
        public required string Style { get; init; }
        public required string BaseColor { get; init; }
        public required string IconLibrary { get; init; }
        public required string MenuAccent { get; init; }
        public required string MenuColor { get; init; }
        public required string RadiusPreset { get; init; }
        public required string FontFamily { get; init; }
        public required string MonospaceFontFamily { get; init; }
        public required ShadcnTheme Theme { get; init; }
    }
}
