using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Showcase.Theming;

public sealed record ThemeStudioSnapshot(
    ShadcnTheme Draft,
    ShadcnTheme Applied,
    ShadcnTheme Baseline,
    IReadOnlyDictionary<string, string> TokenEditorValues,
    IReadOnlyDictionary<string, string> MetricEditorValues,
    string SelectedPresetId);
