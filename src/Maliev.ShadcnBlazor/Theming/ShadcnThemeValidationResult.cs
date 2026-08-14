namespace Maliev.ShadcnBlazor.Theming;

public sealed record ShadcnThemeValidationMessage(string Code, string Path, string Message);

public sealed record ShadcnContrastResult(
    ShadcnContrastKind Kind,
    string Scheme,
    string ForegroundToken,
    string BackgroundToken,
    double Ratio,
    double RequiredRatio,
    bool Passes);

public sealed record ShadcnThemeValidationResult(
    IReadOnlyList<ShadcnThemeValidationMessage> Errors,
    IReadOnlyList<ShadcnThemeValidationMessage> Warnings,
    IReadOnlyList<ShadcnContrastResult> ContrastResults)
{
    public bool IsValid => Errors.Count == 0;
}
