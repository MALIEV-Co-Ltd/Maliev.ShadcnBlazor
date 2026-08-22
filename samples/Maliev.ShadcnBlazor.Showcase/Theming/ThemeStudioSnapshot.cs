using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Showcase.Theming;

public sealed record ThemeStudioSnapshot(
    ShadcnTheme Draft,
    ShadcnTheme Applied,
    ShadcnTheme Baseline,
    ShadcnThemeDocument DocumentTemplate,
    ShadcnThemeDocument BaselineDocumentTemplate,
    IReadOnlyDictionary<string, string> TokenEditorValues,
    IReadOnlyDictionary<string, string> MetricEditorValues,
    string SelectedPresetId,
    string StyleId,
    string BaseColorId,
    ThemeStudioIconLibrary IconLibrary,
    ThemeStudioMenuAccent MenuAccent,
    ThemeStudioMenuColor MenuColor,
    string BaselineStyleId,
    string BaselineBaseColorId,
    ThemeStudioIconLibrary BaselineIconLibrary,
    ThemeStudioMenuAccent BaselineMenuAccent,
    ThemeStudioMenuColor BaselineMenuColor);
