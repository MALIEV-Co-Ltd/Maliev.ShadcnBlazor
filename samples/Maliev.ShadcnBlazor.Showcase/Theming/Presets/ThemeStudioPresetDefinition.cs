using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Showcase.Theming.Presets;

public sealed record ThemeStudioPresetDefinition(
    string Id,
    string DisplayName,
    string Style,
    string BaseColor,
    string Accent,
    ThemeStudioRadiusPreset Radius,
    string Density,
    string BorderTreatment,
    string SurfaceTreatment,
    string ControlTreatment,
    string MotionProfile,
    ThemeStudioIconLibrary IconLibrary,
    ShadcnThemeDocument Document)
{
    public ShadcnThemeDocument CreateDocument() =>
        ShadcnThemeDocumentSerializer.Deserialize(ShadcnThemeDocumentSerializer.Serialize(Document));
}

