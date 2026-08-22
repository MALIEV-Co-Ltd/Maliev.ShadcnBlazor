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
    ShadcnThemeDocument? Document,
    IReadOnlyList<string> Diagnostics)
{
    public ShadcnTheme? Theme => Document?.Theme;

    public static ThemeImportResult Failure(params string[] diagnostics) => new(false, null, diagnostics);
    public static ThemeImportResult Success(ShadcnThemeDocument document, params string[] diagnostics) => new(true, document, diagnostics);
}
