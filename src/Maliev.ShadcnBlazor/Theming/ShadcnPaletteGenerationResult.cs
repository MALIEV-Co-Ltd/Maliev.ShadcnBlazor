namespace Maliev.ShadcnBlazor.Theming;

/// <summary>Contains a deterministic palette candidate and its diagnostics.</summary>
public sealed record ShadcnPaletteGenerationResult(
    ShadcnTheme Theme,
    IReadOnlyList<ShadcnThemeValidationMessage> Errors,
    IReadOnlyList<ShadcnThemeValidationMessage> Warnings)
{
    /// <summary>Gets the normalized materialized version-two anchors used to produce <see cref="Theme"/>.</summary>
    public ShadcnPaletteAnchors? ActiveAnchors { get; init; }

    /// <summary>Gets whether the generated palette can be applied safely.</summary>
    public bool IsValid => Errors.Count == 0;
}
