namespace Maliev.ShadcnBlazor.Build;

internal sealed record ThemeBuildDiagnostic(
    string Code,
    string Path,
    string Message,
    int Line,
    int Column,
    bool IsWarning);
