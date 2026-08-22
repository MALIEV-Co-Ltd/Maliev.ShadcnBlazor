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
