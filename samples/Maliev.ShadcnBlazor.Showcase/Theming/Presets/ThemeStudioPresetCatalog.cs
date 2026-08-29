using System.Reflection;
using System.Text.Json;
using Maliev.ShadcnBlazor.Theming;
using Maliev.ShadcnBlazor.Components.Styling;

namespace Maliev.ShadcnBlazor.Showcase.Theming.Presets;

public interface IThemeStudioPresetCatalog
{
    IReadOnlyList<ThemeStudioPresetDefinition> All { get; }
    ThemeStudioPresetDefinition Get(string id);
}

public sealed class ThemeStudioPresetCatalog : IThemeStudioPresetCatalog
{
    private static readonly IReadOnlyList<ThemeStudioPresetDefinition> Definitions = Load();

    public IReadOnlyList<ThemeStudioPresetDefinition> All => Definitions;

    public ThemeStudioPresetDefinition Get(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return Definitions.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.Ordinal))
            ?? throw new ArgumentException($"Unknown curated theme preset '{id}'.", nameof(id));
    }

    private static IReadOnlyList<ThemeStudioPresetDefinition> Load()
    {
        var assembly = typeof(ThemeStudioPresetCatalog).Assembly;
        var resource = assembly.GetManifestResourceNames().Single(name => name.EndsWith("ThemeStudioPresetCatalog.json", StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resource) ?? throw new InvalidOperationException("The curated preset catalog is unavailable.");
        var entries = JsonSerializer.Deserialize<PresetEntry[]>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false,
            UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow
        }) ?? throw new JsonException("The curated preset catalog is empty.");

        if (entries.Length < 12 || entries.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count() != entries.Length)
            throw new JsonException("The curated preset catalog requires at least twelve unique preset identifiers.");

        return entries.Select(Materialize).ToArray();
    }

    private static ThemeStudioPresetDefinition Materialize(PresetEntry entry)
    {
        if (!Enum.TryParse<ThemeStudioRadiusPreset>(entry.Radius, true, out var radius) ||
            !Enum.TryParse<ThemeStudioIconLibrary>(entry.IconLibrary, true, out var icons) ||
            !Enum.TryParse<ShadcnVisualStyle>(entry.VisualStyle, true, out var visualStyle) || visualStyle == ShadcnVisualStyle.Inherit ||
            !Enum.TryParse<ShadcnColorTreatment>(entry.ColorTreatment, true, out var colorTreatment) ||
            !Enum.TryParse<ShadcnDepthTreatment>(entry.DepthTreatment, true, out var depthTreatment) ||
            !Enum.TryParse<ShadcnMotionTreatment>(entry.MotionTreatment, true, out var motionTreatment) ||
            !Enum.TryParse<ShadcnStyleIntensity>(entry.StyleIntensity, true, out var styleIntensity) ||
            !ThemeStudioGeneratorCatalog.IsKnownStyle(entry.Style) ||
            !ThemeStudioGeneratorCatalog.IsKnownBaseColor(entry.BaseColor))
            throw new JsonException($"Curated preset '{entry.Id}' contains an unsupported option.");

        var baseTheme = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        var theme = string.Equals(entry.Id, ShadcnThemePresets.BaseVegaNeutral.Id, StringComparison.Ordinal)
            ? baseTheme with { Name = entry.DisplayName }
            : baseTheme with
            {
                Name = entry.DisplayName,
                Light = ApplyPalette(baseTheme.Light, entry, dark: false),
                Dark = ApplyPalette(baseTheme.Dark, entry, dark: true),
                Metrics = ApplyMetrics(baseTheme.Metrics, entry, radius)
            };
        var document = ShadcnThemeDocumentSerializer.Deserialize(ShadcnThemeSerializer.Serialize(theme)) with
        {
            Name = entry.DisplayName,
            Theme = theme,
            Application = new ShadcnThemeApplication(entry.Id, entry.Style, entry.BaseColor, entry.IconLibrary, entry.MenuAccent, entry.MenuColor, false, ShadcnDirection.LeftToRight, "en", theme.Metrics.ReducedMotionBehavior),
            Palette = new ShadcnPaletteRecipe(ShadcnPaletteRecipe.LegacyAlgorithmVersion, entry.Seed, entry.BaseColor, [])
        };
        var validation = ShadcnThemeDocumentValidator.Validate(document);
        if (!validation.IsValid)
            throw new JsonException($"Curated preset '{entry.Id}' failed document validation.");

        return new(entry.Id, entry.DisplayName, entry.Style, entry.BaseColor, entry.Accent, radius, entry.Density,
            entry.BorderTreatment, entry.SurfaceTreatment, entry.ControlTreatment, entry.MotionProfile, icons,
            visualStyle, colorTreatment, depthTreatment, motionTreatment, styleIntensity, document);
    }

    private static ShadcnColorScheme ApplyPalette(ShadcnColorScheme source, PresetEntry entry, bool dark)
    {
        var primary = dark ? entry.DarkPrimary : entry.LightPrimary;
        var foreground = dark ? "oklch(0.16 0.02 255)" : "oklch(0.985 0 0)";
        var accent = dark ? entry.DarkAccent : entry.LightAccent;
        return source with
        {
            Primary = primary,
            PrimaryForeground = foreground,
            Accent = accent,
            AccentForeground = dark ? "oklch(0.985 0 0)" : "oklch(0.205 0 0)",
            Ring = primary,
            SidebarPrimary = primary,
            SidebarPrimaryForeground = foreground,
            SidebarAccent = accent,
            Chart1 = primary,
            Chart2 = entry.Chart2,
            Chart3 = entry.Chart3,
            Border = entry.BorderTreatment == "strong" ? (dark ? "oklch(1 0 0 / 22%)" : "oklch(0.82 0.01 255)") : source.Border,
            Input = entry.BorderTreatment == "strong" ? (dark ? "oklch(1 0 0 / 28%)" : "oklch(0.82 0.01 255)") : source.Input,
            ShadowSmall = entry.SurfaceTreatment == "flat" ? "0 0 0 1px rgb(0 0 0 / 0.05)" : source.ShadowSmall,
            ShadowMedium = entry.SurfaceTreatment == "lifted" ? "0 12px 30px rgb(0 0 0 / 0.14)" : source.ShadowMedium
        };
    }

    private static ShadcnThemeMetrics ApplyMetrics(ShadcnThemeMetrics source, PresetEntry entry, ThemeStudioRadiusPreset radius) => source with
    {
        RadiusRem = Math.Max(0.125, ThemeStudioGeneratorCatalog.RadiusRem(radius)),
        ControlHeightRem = entry.Density == "compact" ? 2.125 : entry.Density == "relaxed" ? 2.625 : 2.25,
        ControlHeightSmallRem = entry.Density == "compact" ? 1.875 : 2,
        ControlHeightLargeRem = entry.Density == "relaxed" ? 2.875 : 2.5,
        SpacingScaleMultiplier = entry.Density == "compact" ? 0.875 : entry.Density == "relaxed" ? 1.125 : 1,
        MotionDurationMilliseconds = entry.MotionProfile == "calm" ? 240 : entry.MotionProfile == "snappy" ? 110 : 160,
        MotionEasing = entry.MotionProfile == "snappy" ? "ease-in-out" : "ease-out"
    };

    private sealed record PresetEntry(
        string Id, string DisplayName, string Style, string BaseColor, string Accent, string Radius,
        string Density, string BorderTreatment, string SurfaceTreatment, string ControlTreatment,
        string MotionProfile, string IconLibrary, string VisualStyle, string ColorTreatment,
        string DepthTreatment, string MotionTreatment, string StyleIntensity,
        string MenuAccent, string MenuColor, ulong Seed,
        string LightPrimary, string DarkPrimary, string LightAccent, string DarkAccent, string Chart2, string Chart3);
}
