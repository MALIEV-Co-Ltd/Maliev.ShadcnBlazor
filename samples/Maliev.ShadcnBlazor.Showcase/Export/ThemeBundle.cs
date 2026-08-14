using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Showcase.Export;

public sealed record ThemeBundleOptions(string PresetAncestry, string PackageVersion);

public sealed record ThemeBundleFile(string Path, byte[] Bytes, string Sha256)
{
    public long Size => Bytes.LongLength;
}

public sealed record ThemeBundle(
    string FileName,
    ShadcnThemeValidationResult Validation,
    IReadOnlyList<ThemeBundleFile> Files,
    byte[] ZipBytes);

public sealed record ThemeImportResult(
    bool Succeeded,
    ShadcnTheme? Theme,
    IReadOnlyList<string> Diagnostics)
{
    public static ThemeImportResult Failure(params string[] diagnostics) => new(false, null, diagnostics);
    public static ThemeImportResult Success(ShadcnTheme theme, params string[] diagnostics) => new(true, theme, diagnostics);
}
