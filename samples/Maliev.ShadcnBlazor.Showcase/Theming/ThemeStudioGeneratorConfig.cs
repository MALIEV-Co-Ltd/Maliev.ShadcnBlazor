using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Showcase.Theming;

/// <summary>Icon sets that can be selected for generated application metadata.</summary>
public enum ThemeStudioIconLibrary
{
    Lucide,
    Tabler,
    Phosphor,
    Hugeicons,
    Custom
}

/// <summary>How navigation accents are presented in the generated application metadata.</summary>
public enum ThemeStudioMenuAccent
{
    Default,
    Subtle,
    Bold
}

/// <summary>Surface treatment for generated application navigation.</summary>
public enum ThemeStudioMenuColor
{
    Default,
    Muted,
    Inverted,
    Translucent
}

/// <summary>Common radius presets used by the Theme Studio.</summary>
public enum ThemeStudioRadiusPreset
{
    Sharp,
    Compact,
    Default,
    Relaxed,
    Pill
}

/// <summary>Whitelisted style and base-color metadata exposed by the generator.</summary>
public sealed record ThemeStudioGeneratorOption(string Id, string DisplayName, string Description);

public static class ThemeStudioGeneratorCatalog
{
    public static IReadOnlyList<ThemeStudioGeneratorOption> Styles { get; } =
    [
        new("vega", "Vega", "Semantic tokens with the Vega component language."),
        new("base", "Base", "The neutral base composition for portable Blazor apps.")
    ];

    public static IReadOnlyList<ThemeStudioGeneratorOption> BaseColors { get; } =
    [
        new("neutral", "Neutral", "Balanced black-and-white surfaces."),
        new("stone", "Stone", "A warmer neutral foundation."),
        new("zinc", "Zinc", "A cooler neutral foundation."),
        new("slate", "Slate", "A blue-gray neutral foundation.")
    ];

    public static IReadOnlyList<ThemeStudioGeneratorOption> IconLibraries { get; } =
    [
        new("lucide", "Lucide", "The default outline icon language."),
        new("tabler", "Tabler", "A dense outline icon language."),
        new("phosphor", "Phosphor", "A flexible multi-weight icon language."),
        new("hugeicons", "Hugeicons", "A broad outline and filled icon library."),
        new("custom", "Custom", "Use your own icon adapter in the consuming app.")
    ];

    public static IReadOnlyList<ThemeStudioGeneratorOption> MenuAccents { get; } =
    [
        new("default", "Default", "Use the primary semantic action color."),
        new("subtle", "Subtle", "Use muted emphasis for navigation."),
        new("bold", "Bold", "Use a stronger active navigation treatment.")
    ];

    public static IReadOnlyList<ThemeStudioGeneratorOption> MenuColors { get; } =
    [
        new("default", "Default", "Use the sidebar semantic surface."),
        new("muted", "Muted", "Use a muted navigation surface."),
        new("inverted", "Inverted", "Use the foreground surface for navigation."),
        new("translucent", "Translucent", "Keep the underlying surface visible.")
    ];

    public static IReadOnlyList<(ThemeStudioRadiusPreset Preset, string DisplayName, double Rem)> Radii { get; } =
    [
        (ThemeStudioRadiusPreset.Sharp, "Sharp · 0", 0),
        (ThemeStudioRadiusPreset.Compact, "Compact · 0.375rem", 0.375),
        (ThemeStudioRadiusPreset.Default, "Default · 0.625rem", 0.625),
        (ThemeStudioRadiusPreset.Relaxed, "Relaxed · 0.875rem", 0.875),
        (ThemeStudioRadiusPreset.Pill, "Pill · 1.25rem", 1.25)
    ];

    public static double RadiusRem(ThemeStudioRadiusPreset preset) =>
        Radii.First(item => item.Preset == preset).Rem;

    public static ThemeStudioRadiusPreset RadiusPreset(double rem)
    {
        var nearest = Radii.OrderBy(item => Math.Abs(item.Rem - rem)).First();
        return Math.Abs(nearest.Rem - rem) < 0.0001 ? nearest.Preset : ThemeStudioRadiusPreset.Default;
    }

    public static bool IsKnownStyle(string value) => Styles.Any(item => string.Equals(item.Id, value, StringComparison.Ordinal));
    public static bool IsKnownBaseColor(string value) => BaseColors.Any(item => string.Equals(item.Id, value, StringComparison.Ordinal));
}

/// <summary>
/// A portable, versioned Theme Studio document. The nested <see cref="Theme"/>
/// remains the canonical typed theme while generator metadata describes the
/// surrounding application choices that a Blazor consumer must wire up.
/// </summary>
public sealed record ThemeStudioGeneratorConfig
{
    public const int CurrentSchemaVersion = 1;

    [JsonPropertyOrder(0)]
    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    [JsonPropertyOrder(1)]
    public required string Preset { get; init; }

    [JsonPropertyOrder(2)]
    public required string Style { get; init; }

    [JsonPropertyOrder(3)]
    public required string BaseColor { get; init; }

    [JsonPropertyOrder(4)]
    public required ThemeStudioIconLibrary IconLibrary { get; init; }

    [JsonPropertyOrder(5)]
    public required ThemeStudioMenuAccent MenuAccent { get; init; }

    [JsonPropertyOrder(6)]
    public required ThemeStudioMenuColor MenuColor { get; init; }

    [JsonPropertyOrder(7)]
    public required ThemeStudioRadiusPreset RadiusPreset { get; init; }

    [JsonPropertyOrder(8)]
    public required string FontFamily { get; init; }

    [JsonPropertyOrder(9)]
    public required string MonospaceFontFamily { get; init; }

    [JsonPropertyOrder(10)]
    public required ShadcnTheme Theme { get; init; }
}

public static class ThemeStudioGeneratorConfigSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Default,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
    };

    public static string Serialize(ThemeStudioGeneratorConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        Validate(config);
        var json = JsonSerializer.Serialize(config, Options).Replace("\r\n", "\n", StringComparison.Ordinal);
        return json.EndsWith('\n') ? json : json + "\n";
    }

    public static ThemeStudioGeneratorConfig Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new JsonException("A Theme Studio generator document must be a JSON object.");
        if (!document.RootElement.TryGetProperty("schemaVersion", out var versionProperty))
            throw new NotSupportedException("Theme Studio generator schemaVersion is required.");
        if (!versionProperty.TryGetInt32(out var version) || version != ThemeStudioGeneratorConfig.CurrentSchemaVersion)
            throw new NotSupportedException($"Theme Studio generator schema version {versionProperty.GetRawText()} is not supported.");

        var config = JsonSerializer.Deserialize<ThemeStudioGeneratorConfig>(json, Options)
                     ?? throw new JsonException("Theme Studio generator JSON produced no value.");
        Validate(config);
        return config;
    }

    public static void Validate(ThemeStudioGeneratorConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        if (config.SchemaVersion != ThemeStudioGeneratorConfig.CurrentSchemaVersion)
            throw new NotSupportedException($"Theme Studio generator schema version {config.SchemaVersion} is not supported.");
        if (string.IsNullOrWhiteSpace(config.Preset))
            throw new JsonException("preset is required.");
        if (!ThemeStudioGeneratorCatalog.IsKnownStyle(config.Style))
            throw new JsonException($"Unknown Theme Studio style '{config.Style}'.");
        if (!ThemeStudioGeneratorCatalog.IsKnownBaseColor(config.BaseColor))
            throw new JsonException($"Unknown Theme Studio base color '{config.BaseColor}'.");
        if (!Enum.IsDefined(config.IconLibrary) || !Enum.IsDefined(config.MenuAccent) ||
            !Enum.IsDefined(config.MenuColor) || !Enum.IsDefined(config.RadiusPreset))
            throw new JsonException("Theme Studio generator contains an unsupported option.");
        if (config.Theme is null)
            throw new JsonException("theme is required.");
        if (!ShadcnThemeValidator.Validate(config.Theme).IsValid)
            throw new JsonException("Theme Studio generator contains an invalid theme.");
    }
}
