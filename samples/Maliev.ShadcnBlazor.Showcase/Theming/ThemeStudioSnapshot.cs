using System.Collections.Immutable;
using Maliev.ShadcnBlazor.Theming;
using Maliev.ShadcnBlazor.Components.Styling;

namespace Maliev.ShadcnBlazor.Showcase.Theming;

public sealed record ThemeStudioSnapshot(
    ShadcnTheme Draft,
    ShadcnTheme Applied,
    ShadcnTheme Baseline,
    ShadcnThemeDocument DocumentTemplate,
    ShadcnThemeDocument BaselineDocumentTemplate,
    ImmutableArray<ShadcnThemeValidationMessage> PaletteDiagnostics,
    IReadOnlyDictionary<string, string> TokenEditorValues,
    IReadOnlyDictionary<string, string> MetricEditorValues,
    string SelectedPresetId,
    string StyleId,
    string BaseColorId,
    ThemeStudioIconLibrary IconLibrary,
    ThemeStudioMenuAccent MenuAccent,
    ThemeStudioMenuColor MenuColor,
    ShadcnVisualStyle VisualStyle,
    ShadcnColorTreatment ColorTreatment,
    ShadcnDepthTreatment DepthTreatment,
    ShadcnMotionTreatment MotionTreatment,
    ShadcnStyleIntensity StyleIntensity,
    string BaselineStyleId,
    string BaselineBaseColorId,
    ThemeStudioIconLibrary BaselineIconLibrary,
    ThemeStudioMenuAccent BaselineMenuAccent,
    ThemeStudioMenuColor BaselineMenuColor,
    ShadcnVisualStyle BaselineVisualStyle,
    ShadcnColorTreatment BaselineColorTreatment,
    ShadcnDepthTreatment BaselineDepthTreatment,
    ShadcnMotionTreatment BaselineMotionTreatment,
    ShadcnStyleIntensity BaselineStyleIntensity);
