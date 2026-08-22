using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Showcase.Export;

public static class ThemeBundleBuilder
{
    public static ThemeBundle Build(ShadcnTheme theme, ThemeBundleOptions options)
    {
        ArgumentNullException.ThrowIfNull(theme);
        var document = ShadcnThemeDocumentSerializer.Deserialize(ShadcnThemeSerializer.Serialize(theme));
        return Build(document, options);
    }

    private static readonly DateTimeOffset FixedTimestamp = new(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly UTF8Encoding Utf8 = new(false, true);
    private static readonly JsonSerializerOptions ManifestJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Default,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public static ThemeBundle Build(ShadcnThemeDocument document, ThemeBundleOptions options)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.PresetAncestry);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.PackageVersion);

        var documentValidation = ShadcnThemeDocumentValidator.Validate(document);
        if (!documentValidation.IsValid)
            throw new ArgumentException("Only a valid applied theme document can be exported.", nameof(document));
        var theme = document.Theme;
        var validation = ShadcnThemeValidator.Validate(theme);
        if (!validation.IsValid)
            throw new ArgumentException("Only a valid applied theme can be exported.", nameof(theme));

        var files = new List<ThemeBundleFile>
        {
            CreateFile("theme.css", ShadcnThemeCssWriter.Write(document)),
            CreateFile("MalievShadcnTheme.cs", ThemeBundleTemplates.WriteThemeClass(theme)),
            CreateFile("theme.json", ShadcnThemeDocumentSerializer.Serialize(document)),
            CreateFile("README.md", ThemeBundleTemplates.WriteReadme(theme, options, validation)),
            CreateFile("Examples/Program.cs.txt", ThemeBundleTemplates.ProgramExample(options.PackageVersion)),
            CreateFile("Examples/AppShell.razor.txt", ThemeBundleTemplates.AppShellExample()),
            CreateFile("Examples/FormExample.razor.txt", ThemeBundleTemplates.FormExample()),
            CreateFile("Examples/OverlayExample.razor.txt", ThemeBundleTemplates.OverlayExample())
        };
        files.Add(CreateFile("manifest.json", WriteManifest(document, options, files)));

        var zipBytes = WriteZip(files);
        return new ThemeBundle(
            $"maliev-shadcn-theme-{SafeName(theme.Name)}-{document.SchemaVersion}.zip",
            validation,
            files.AsReadOnly(),
            zipBytes);
    }

    private static ThemeBundleFile CreateFile(string path, string content)
    {
        var normalized = content.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        if (!normalized.EndsWith('\n'))
            normalized += "\n";
        var bytes = Utf8.GetBytes(normalized);
        return new ThemeBundleFile(path, bytes, Hash(bytes));
    }

    private static string WriteManifest(ShadcnThemeDocument document, ThemeBundleOptions options, IReadOnlyList<ThemeBundleFile> files)
    {
        var manifest = new ThemeBundleManifest(
            document.SchemaVersion,
            document.Name,
            options.PresetAncestry,
            options.PackageVersion,
            files.Select(file => new ThemeBundleManifestFile(file.Path, file.Size, file.Sha256)).ToArray());
        return JsonSerializer.Serialize(manifest, ManifestJsonOptions) + "\n";
    }

    private static byte[] WriteZip(IEnumerable<ThemeBundleFile> files)
    {
        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true, entryNameEncoding: Utf8))
        {
            foreach (var file in files)
            {
                var entry = archive.CreateEntry(file.Path, CompressionLevel.NoCompression);
                entry.LastWriteTime = FixedTimestamp;
                entry.ExternalAttributes = 0;
                using var stream = entry.Open();
                stream.Write(file.Bytes);
            }
        }
        return output.ToArray();
    }

    private static string SafeName(string name)
    {
        var builder = new StringBuilder();
        var pendingSeparator = false;
        foreach (var character in name)
        {
            if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                if (pendingSeparator && builder.Length > 0) builder.Append('-');
                builder.Append(character);
                pendingSeparator = false;
            }
            else if (character is >= 'A' and <= 'Z')
            {
                if (pendingSeparator && builder.Length > 0) builder.Append('-');
                builder.Append(char.ToLowerInvariant(character));
                pendingSeparator = false;
            }
            else
            {
                pendingSeparator = builder.Length > 0;
            }
        }

        var safe = builder.ToString().Trim('-');
        if (safe.Length > 60) safe = safe[..60].TrimEnd('-');
        return string.IsNullOrWhiteSpace(safe) || IsReservedDeviceName(safe) ? "theme" : safe;
    }

    private static bool IsReservedDeviceName(string name)
    {
        var stem = name.Split('.', 2)[0];
        return stem.Equals("con", StringComparison.OrdinalIgnoreCase) ||
               stem.Equals("prn", StringComparison.OrdinalIgnoreCase) ||
               stem.Equals("aux", StringComparison.OrdinalIgnoreCase) ||
               stem.Equals("nul", StringComparison.OrdinalIgnoreCase) ||
               Enumerable.Range(1, 9).Any(index => stem.Equals($"com{index}", StringComparison.OrdinalIgnoreCase) || stem.Equals($"lpt{index}", StringComparison.OrdinalIgnoreCase));
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private sealed record ThemeBundleManifest(
        [property: JsonPropertyOrder(0)] int SchemaVersion,
        [property: JsonPropertyOrder(1)] string ThemeName,
        [property: JsonPropertyOrder(2)] string PresetAncestry,
        [property: JsonPropertyOrder(3)] string PackageVersion,
        [property: JsonPropertyOrder(4)] IReadOnlyList<ThemeBundleManifestFile> Files);

    private sealed record ThemeBundleManifestFile(
        [property: JsonPropertyOrder(0)] string Path,
        [property: JsonPropertyOrder(1)] long Size,
        [property: JsonPropertyOrder(2)] string Sha256);
}
