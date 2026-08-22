using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Globalization;
using Bunit;
using Maliev.ShadcnBlazor.Components;
using Maliev.ShadcnBlazor.Showcase.Components.Theming;
using Maliev.ShadcnBlazor.Showcase.Export;
using Maliev.ShadcnBlazor.Showcase.Theming;
using Maliev.ShadcnBlazor.Theming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeBundleTests
{
    [Fact]
    public void CanonicalJsonBytesMatchStateCodeBundleAndImportSurfaces()
    {
        var state = new ThemeStudioState(new NullStorage());
        state.SetToken(ThemeStudioScheme.Light, "primary", "#123456");
        state.SetDirection(ShadcnDirection.RightToLeft);
        state.SetLocale(ThemeStudioLocale.Thai);
        var document = state.CreateDocument();
        var expected = state.SerializeDocument();

        var codeJson = ThemeStudioCodeGenerator.WriteJson(document);
        var bundle = ThemeBundleBuilder.Build(document, new ThemeBundleOptions(state.SelectedPresetId, "1.0.0"));
        var bundleJson = Encoding.UTF8.GetString(bundle.Files.Single(file => file.Path == "theme.json").Bytes);
        var imported = new ThemeImportService().Import(Encoding.UTF8.GetBytes(expected), "theme.json", "application/json");

        Assert.Equal(expected, codeJson);
        Assert.Equal(expected, bundleJson);
        Assert.True(imported.Succeeded);
        Assert.Equal(expected, ShadcnThemeDocumentSerializer.Serialize(imported.Document!));
    }

    [Fact]
    public void GeneratedCSharpContainsThePortableMetadataAndTypedThemeFactory()
    {
        var document = new ThemeStudioState(new NullStorage()).CreateDocument();

        var code = ThemeStudioCodeGenerator.WriteCSharp(document);

        Assert.Contains("public static ShadcnTheme Create()", code, StringComparison.Ordinal);
        Assert.Contains("public const string IconLibrary = \"lucide\"", code, StringComparison.Ordinal);
        Assert.Contains("options.FontFamily", code, StringComparison.Ordinal);
        Assert.Contains("FontFamily = \"'Geist'", code, StringComparison.Ordinal);
        Assert.Contains("new ShadcnTheme", code, StringComparison.Ordinal);
    }

    private static readonly string[] ExpectedPaths =
    [
        "theme.css",
        "MalievShadcnTheme.cs",
        "theme.json",
        "README.md",
        "Examples/Program.cs.txt",
        "Examples/AppShell.razor.txt",
        "Examples/FormExample.razor.txt",
        "Examples/OverlayExample.razor.txt",
        "manifest.json"
    ];

    [Fact]
    public void BuildProducesAByteIdenticalFixedInventoryWithCanonicalContentsAndMetadata()
    {
        var theme = ShadcnThemePresets.BaseVegaNeutral.CreateTheme() with
        {
            Name = "MALIEV Factory / Night",
            Light = ShadcnThemePresets.BaseVegaNeutral.CreateTheme().Light with { Primary = "#123456" }
        };
        var options = new ThemeBundleOptions("Base / Vega / Neutral", "1.0.0");

        var first = ThemeBundleBuilder.Build(theme, options);
        var second = ThemeBundleBuilder.Build(theme, options);

        Assert.Equal("maliev-shadcn-theme-maliev-factory-night-2.zip", first.FileName);
        Assert.Equal(first.ZipBytes, second.ZipBytes);
        Assert.Equal(ExpectedPaths, first.Files.Select(file => file.Path));
        Assert.True(first.Validation.IsValid);

        using var archive = new ZipArchive(new MemoryStream(first.ZipBytes), ZipArchiveMode.Read);
        Assert.Equal(ExpectedPaths, archive.Entries.Select(entry => entry.FullName));
        Assert.All(archive.Entries, entry =>
        {
            Assert.Equal(new DateTime(1980, 1, 1, 0, 0, 0), entry.LastWriteTime.DateTime);
            Assert.Equal(0, entry.ExternalAttributes);
            Assert.Equal(entry.Length, entry.CompressedLength);
            Assert.DoesNotContain("..", entry.FullName, StringComparison.Ordinal);
            Assert.DoesNotContain('\\', entry.FullName);
            Assert.False(entry.FullName.StartsWith('/'));
        });

        var extracted = archive.Entries.ToDictionary(entry => entry.FullName, ReadEntry, StringComparer.Ordinal);
        var document = ShadcnThemeDocumentSerializer.Deserialize(ShadcnThemeSerializer.Serialize(theme));
        Assert.Equal(ShadcnThemeDocumentSerializer.Serialize(document), extracted["theme.json"]);
        Assert.Equal(ShadcnThemeCssWriter.Write(document), extracted["theme.css"]);
        Assert.Contains("#123456", extracted["theme.css"], StringComparison.Ordinal);
        Assert.Contains("#123456", extracted["theme.json"], StringComparison.Ordinal);
        Assert.Contains("#123456", extracted["MalievShadcnTheme.cs"], StringComparison.Ordinal);
        Assert.All(extracted.Values, content =>
        {
            Assert.DoesNotContain('\r', content);
            Assert.False(content.Length > 0 && content[0] == '\ufeff');
        });

        using var manifest = JsonDocument.Parse(extracted["manifest.json"]);
        Assert.Equal(ShadcnThemeDocument.CurrentSchemaVersion, manifest.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(theme.Name, manifest.RootElement.GetProperty("themeName").GetString());
        Assert.Equal(options.PresetAncestry, manifest.RootElement.GetProperty("presetAncestry").GetString());
        var manifestFiles = manifest.RootElement.GetProperty("files").EnumerateArray().ToArray();
        Assert.Equal(ExpectedPaths[..^1], manifestFiles.Select(item => item.GetProperty("path").GetString()));
        foreach (var item in manifestFiles)
        {
            var path = item.GetProperty("path").GetString()!;
            var bytes = archive.GetEntry(path)!.Open();
            using var memory = new MemoryStream();
            bytes.CopyTo(memory);
            Assert.Equal(memory.Length, item.GetProperty("size").GetInt64());
            Assert.Equal(Convert.ToHexString(SHA256.HashData(memory.ToArray())).ToLowerInvariant(), item.GetProperty("sha256").GetString());
        }
    }

    [Fact]
    public void DocumentBundleCssPreservesFontSelectionsAndEverySemanticRole()
    {
        var state = new ThemeStudioState(new NullStorage());
        var roles = state.Document.Typography.Roles.ToDictionary();
        roles[ShadcnTypographyRole.Heading1] = new(800, 2.5, 1.2, -0.04);
        var typography = new ShadcnTypographyScale(
            new("'IBM Plex Sans', ui-sans-serif, sans-serif", "ui-sans-serif, sans-serif", "ibm-plex-sans"),
            new("'Noto Sans Thai', sans-serif", "sans-serif", "noto-sans-thai"),
            new("'Fira Code', ui-monospace, monospace", "ui-monospace, monospace", "fira-code"),
            roles);
        var document = state.Document with
        {
            Theme = state.Document.Theme with
            {
                Metrics = state.Document.Theme.Metrics with
                {
                    FontFamily = typography.Body.Family,
                    MonospaceFontFamily = typography.Code.Family
                }
            },
            Typography = typography
        };

        var bundle = ThemeBundleBuilder.Build(document, new("Custom", "1.0.0"));
        var css = Encoding.UTF8.GetString(bundle.Files.Single(file => file.Path == "theme.css").Bytes);
        var restored = ShadcnThemeDocumentSerializer.Deserialize(
            Encoding.UTF8.GetString(bundle.Files.Single(file => file.Path == "theme.json").Bytes));

        Assert.Equal(
            ShadcnThemeDocumentSerializer.Serialize(document),
            ShadcnThemeDocumentSerializer.Serialize(restored));
        Assert.Contains("--shadcn-font-thai: 'Noto Sans Thai', sans-serif", css, StringComparison.Ordinal);
        Assert.Contains("--shadcn-typography-heading-1-weight: 800", css, StringComparison.Ordinal);
        Assert.Contains("--shadcn-typography-heading-1-scale: 2.5", css, StringComparison.Ordinal);
        Assert.Contains("--shadcn-typography-code-line-height: 1.5", css, StringComparison.Ordinal);
        Assert.Equal(Enum.GetValues<ShadcnTypographyRole>().Length * 4 * 2,
            css.Split('\n').Count(line => line.Contains("--shadcn-typography-", StringComparison.Ordinal)));
    }

    [Fact]
    public void BundleBytesAreIndependentOfCurrentCultureAndTimeZone()
    {
        var theme = ShadcnThemePresets.BaseVegaNeutral.CreateTheme() with
        {
            Metrics = ShadcnThemePresets.BaseVegaNeutral.CreateTheme().Metrics with
            {
                RadiusRem = 0.9999999999999999,
                FocusRingOffsetPx = double.Epsilon
            }
        };
        var options = new ThemeBundleOptions("Base / Vega / Neutral", "1.0.0");
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("th-TH");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("th-TH");
            var thai = ThemeBundleBuilder.Build(theme, options).ZipBytes;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
            var french = ThemeBundleBuilder.Build(theme, options).ZipBytes;

            Assert.Equal(thai, french);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Theory]
    [InlineData("../escape", "maliev-shadcn-theme-escape-2.zip")]
    [InlineData("CON", "maliev-shadcn-theme-theme-2.zip")]
    [InlineData(" สวัสดี โลก ", "maliev-shadcn-theme-theme-2.zip")]
    [InlineData("Name<>:\"/\\|?* Value", "maliev-shadcn-theme-name-value-2.zip")]
    [InlineData("---", "maliev-shadcn-theme-theme-2.zip")]
    public void BuildNormalizesUntrustedThemeNamesToSafePortableFileNames(string name, string expected)
    {
        var theme = ShadcnThemePresets.BaseVegaNeutral.CreateTheme() with { Name = name.Replace("<", string.Empty).Replace(">", string.Empty).Replace(";", string.Empty) };

        var bundle = ThemeBundleBuilder.Build(theme, new ThemeBundleOptions("Custom", "1.0.0"));

        Assert.Equal(expected, bundle.FileName);
        Assert.Matches("^[a-z0-9][a-z0-9-]*-[0-9]+\\.zip$", bundle.FileName);
        Assert.DoesNotContain("..", bundle.FileName, StringComparison.Ordinal);
    }

    [Fact]
    public void GeneratedReadmeAndExamplesUseOnlyRealPackageContractsAndContainNoEnvironmentData()
    {
        var bundle = ThemeBundleBuilder.Build(
            ShadcnThemePresets.BaseVegaNeutral.CreateTheme(),
            new ThemeBundleOptions("Base / Vega / Neutral", "1.0.0"));
        var text = string.Join('\n', bundle.Files.Select(file => Encoding.UTF8.GetString(file.Bytes)));

        Assert.Contains("dotnet add package Maliev.ShadcnBlazor --version 1.0.0", text, StringComparison.Ordinal);
        Assert.Contains("AddMalievShadcn", text, StringComparison.Ordinal);
        Assert.Contains("<ShadcnThemeProvider Theme=\"@MalievShadcnTheme.Create()\"", text, StringComparison.Ordinal);
        Assert.Contains("MudPopoverProvider", text, StringComparison.Ordinal);
        Assert.Contains("ShadcnDirection.RightToLeft", text, StringComparison.Ordinal);
        Assert.Contains("_content/Maliev.ShadcnBlazor/css/shadcn-actions.css", text, StringComparison.Ordinal);
        Assert.Contains("ShadcnField", text, StringComparison.Ordinal);
        Assert.Contains("ShadcnButton", text, StringComparison.Ordinal);
        Assert.Contains("ShadcnCheckbox", text, StringComparison.Ordinal);
        Assert.DoesNotContain("http://", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("api-key", text, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(typeof(ServiceCollectionExtensions).GetMethod(nameof(ServiceCollectionExtensions.AddMalievShadcn)));
        Assert.NotNull(typeof(ShadcnThemeProvider).GetProperty(nameof(ShadcnThemeProvider.Theme)));
        Assert.NotNull(typeof(Maliev.ShadcnBlazor.Components.Forms.ShadcnField));
        Assert.NotNull(typeof(Maliev.ShadcnBlazor.Components.Actions.ShadcnButton));
        Assert.NotNull(typeof(Maliev.ShadcnBlazor.Components.Selection.ShadcnCheckbox));
    }

    [Fact]
    public void GeneratedThemeClassAndExampleTemplatesCompileAgainstTheCurrentPublicApi()
    {
        var bundle = ThemeBundleBuilder.Build(
            ShadcnThemePresets.BaseVegaNeutral.CreateTheme(),
            new ThemeBundleOptions("Base / Vega / Neutral", "1.0.0"));
        var files = bundle.Files.ToDictionary(file => file.Path, file => Encoding.UTF8.GetString(file.Bytes), StringComparer.Ordinal);
        var testAssemblyDirectory = Path.GetDirectoryName(typeof(ThemeBundleTests).Assembly.Location)!;
        var rclAssembly = Path.Combine(testAssemblyDirectory, "Maliev.ShadcnBlazor.dll");
        var generatedAssembly = Path.Combine(Path.GetTempPath(), $"maliev-theme-{Guid.NewGuid():N}.dll");

        try
        {
            var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(
                files["MalievShadcnTheme.cs"],
                new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Latest));
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
                .Select(assembly => assembly.Location)
                .Append(rclAssembly)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(path => Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(path));
            var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
                "GeneratedTheme",
                [syntaxTree],
                references,
                new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));
            using var generatedStream = File.Create(generatedAssembly);
            var emit = compilation.Emit(generatedStream);
            Assert.True(emit.Success, string.Join(Environment.NewLine, emit.Diagnostics));

            var appShell = files["Examples/AppShell.razor.txt"];
            Assert.Contains(nameof(ShadcnThemeProvider.Theme), appShell, StringComparison.Ordinal);
            Assert.Contains(nameof(ShadcnThemeProvider.Direction), appShell, StringComparison.Ordinal);
            var form = files["Examples/FormExample.razor.txt"];
            Assert.Contains("@bind-Value", form, StringComparison.Ordinal);
            Assert.NotNull(typeof(Maliev.ShadcnBlazor.Components.Selection.ShadcnCheckbox)
                .GetProperty(nameof(Maliev.ShadcnBlazor.Components.Selection.ShadcnCheckbox.Value)));
        }
        finally
        {
            if (File.Exists(generatedAssembly)) File.Delete(generatedAssembly);
        }
    }

    [Fact]
    public void ReadmeRecordsValidationWarningsAndSchemaInformation()
    {
        var theme = ShadcnThemePresets.BaseVegaNeutral.CreateTheme() with
        {
            Light = ShadcnThemePresets.BaseVegaNeutral.CreateTheme().Light with { Foreground = "#777777" }
        };

        var bundle = ThemeBundleBuilder.Build(theme, new ThemeBundleOptions("Custom", "1.0.0"));
        var readme = Encoding.UTF8.GetString(bundle.Files.Single(file => file.Path == "README.md").Bytes);

        Assert.Contains("Schema version: 1", readme, StringComparison.Ordinal);
        Assert.Contains("Contrast warnings", readme, StringComparison.Ordinal);
        Assert.Contains("low-contrast", readme, StringComparison.Ordinal);
    }

    [Fact]
    public void JsonImportRoundTripsEverySupportedThemeValueAndCreatesOneUndoTransaction()
    {
        var importer = new ThemeImportService();
        var state = new ThemeStudioState(new NullStorage());
        var source = ShadcnThemePresets.BaseVegaNeutral.CreateTheme() with
        {
            Name = "Imported",
            Dark = ShadcnThemePresets.BaseVegaNeutral.CreateTheme().Dark with { Primary = "#abcdef" },
            Metrics = ShadcnThemePresets.BaseVegaNeutral.CreateTheme().Metrics with { RadiusRem = 1.25 }
        };

        var result = importer.Import(Encoding.UTF8.GetBytes(ShadcnThemeSerializer.Serialize(source)), "imported.json", "application/json");

        Assert.True(result.Succeeded, string.Join("; ", result.Diagnostics));
        Assert.Equal(source, result.Theme);
        Assert.True(state.Import(result.Theme!));
        Assert.Equal(source, state.Applied);
        Assert.True(state.Undo());
        Assert.NotEqual(source, state.Applied);
        Assert.False(state.CanUndo);
    }

    [Theory]
    [InlineData("theme.txt", "application/json", "wrong file extension")]
    [InlineData("theme.json", "text/plain", "content type")]
    [InlineData("theme.json", "application/json", "malformed JSON")]
    public void JsonImportRejectsWrongFileContractOrMalformedContentWithoutMutation(string fileName, string contentType, string expectedDiagnostic)
    {
        var importer = new ThemeImportService();
        var state = new ThemeStudioState(new NullStorage());
        var beforeDraft = state.Draft;
        var beforeApplied = state.Applied;
        var bytes = fileName.EndsWith(".txt", StringComparison.Ordinal) || contentType == "text/plain"
            ? Encoding.UTF8.GetBytes(ShadcnThemeSerializer.Serialize(beforeDraft))
            : Encoding.UTF8.GetBytes("{not-json");

        var result = importer.Import(bytes, fileName, contentType);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, item => item.Contains(expectedDiagnostic, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(beforeDraft, state.Draft);
        Assert.Equal(beforeApplied, state.Applied);
        Assert.False(state.CanUndo);
    }

    [Fact]
    public void JsonImportRejectsOversizedInvalidUtf8InjectionUnknownFieldsAndFutureSchemas()
    {
        var importer = new ThemeImportService();
        var valid = ShadcnThemeSerializer.Serialize(ShadcnThemePresets.BaseVegaNeutral.CreateTheme());
        var cases = new (byte[] Bytes, string Diagnostic)[]
        {
            (new byte[ThemeImportService.MaxImportBytes + 1], "maximum"),
            ([0xff, 0xfe, 0xfd], "UTF-8"),
            (Encoding.UTF8.GetBytes(valid.Replace("oklch(0.205 0 0)", "red; background:url(evil)", StringComparison.Ordinal)), "invalid"),
            (Encoding.UTF8.GetBytes(valid.Replace("\"name\":", "\"unknown\": true,\n  \"name\":", StringComparison.Ordinal)), "unmapped"),
            (Encoding.UTF8.GetBytes(valid.Replace("\"schemaVersion\": 1", "\"schemaVersion\": 999", StringComparison.Ordinal)), "schema version 999")
        };

        foreach (var item in cases)
        {
            var result = importer.Import(item.Bytes, "theme.json", "application/json");
            Assert.False(result.Succeeded);
            Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Contains(item.Diagnostic, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void JsonImportMigratesTheSupportedLegacySchemaAndReportsIt()
    {
        var importer = new ThemeImportService();
        var legacy = ShadcnThemeSerializer.Serialize(ShadcnThemePresets.BaseVegaNeutral.CreateTheme())
            .Replace("  \"schemaVersion\": 1,\n", string.Empty, StringComparison.Ordinal);

        var result = importer.Import(Encoding.UTF8.GetBytes(legacy), "theme.json", "application/json");

        Assert.True(result.Succeeded);
        Assert.Equal(ShadcnTheme.CurrentSchemaVersion, result.Theme!.SchemaVersion);
        Assert.Equal(ShadcnThemeDocument.CurrentSchemaVersion, result.Document!.SchemaVersion);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Contains("schema 2", StringComparison.OrdinalIgnoreCase));
    }

    private static string ReadEntry(ZipArchiveEntry entry)
    {
        using var reader = new StreamReader(entry.Open(), new UTF8Encoding(false, true));
        return reader.ReadToEnd();
    }

    private sealed class NullStorage : IThemeStudioStorage
    {
        public ValueTask<ThemeStudioStorageResult> LoadAsync() => ValueTask.FromResult(ThemeStudioStorageResult.Success(null));
        public ValueTask<ThemeStudioStorageResult> SaveAsync(ShadcnThemeDocument document) => ValueTask.FromResult(ThemeStudioStorageResult.Success(document));
    }
}

public sealed class ThemeImportExportComponentTests : BunitContext
{
    public ThemeImportExportComponentTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
        Services.AddSingleton<IThemeStudioStorage>(new NullStorage());
        Services.AddSingleton<ThemeStudioState>();
        Services.AddSingleton<ThemeImportService>();
    }

    [Fact]
    public void ImportDialogHasAccessibleJsonOnlyInputAndPoliteTransactionalStatus()
    {
        var state = Services.GetRequiredService<ThemeStudioState>();
        var cut = Render<ThemeImportDialog>(parameters => parameters
            .Add(component => component.State, state)
            .Add(component => component.Open, true));

        var dialog = cut.Find("dialog[data-testid='theme-import-dialog']");
        Assert.Equal("theme-import-title", dialog.GetAttribute("aria-labelledby"));
        var input = cut.Find("input[type='file']");
        Assert.Equal("application/json,.json", input.GetAttribute("accept"));
        Assert.Equal("theme-import-file", input.Id);
        Assert.Equal("polite", cut.Find("[data-testid='theme-import-status']").GetAttribute("aria-live"));
        Assert.Contains(ThemeImportService.MaxImportBytes.ToString(), cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ExportDialogListsFixedInventoryAndRequiresWarningAcknowledgement()
    {
        var state = Services.GetRequiredService<ThemeStudioState>();
        var cut = Render<ThemeExportDialog>(parameters => parameters
            .Add(component => component.State, state)
            .Add(component => component.Open, true));

        Assert.Equal(ExpectedPaths, cut.FindAll("[data-bundle-path]")
            .Select(item => item.GetAttribute("data-bundle-path")));
        Assert.Equal("polite", cut.Find("[data-testid='theme-export-status']").GetAttribute("aria-live"));
        Assert.Contains("validation", cut.Markup, StringComparison.OrdinalIgnoreCase);
        if (state.Validation.Warnings.Count > 0)
        {
            Assert.NotEmpty(cut.FindAll("input[data-testid='theme-export-warning-ack']"));
            Assert.True(cut.Find("button[data-testid='theme-download']").HasAttribute("disabled"));
        }
    }

    [Fact]
    public void ExportUsesTheLastValidAppliedThemeWhenEditorLocalTextIsInvalid()
    {
        var downloadModule = JSInterop.SetupModule("./js/shadcn-download.js");
        downloadModule.SetupVoid("downloadBytes", _ => true);
        var state = Services.GetRequiredService<ThemeStudioState>();
        state.SetToken(ThemeStudioScheme.Light, "primary", "#123456");
        state.SetToken(ThemeStudioScheme.Light, "primary", "red; background:url(evil)");
        Assert.False(state.Validation.IsValid);
        Assert.Equal("#123456", state.Applied.Light.Primary);

        var cut = Render<ThemeExportDialog>(parameters => parameters
            .Add(component => component.State, state)
            .Add(component => component.Open, true));
        cut.FindAll("input[data-testid='theme-export-warning-ack']").FirstOrDefault()?.Change(true);
        cut.Find("button[data-testid='theme-download']").Click();
        cut.WaitForAssertion(() => Assert.Contains(JSInterop.Invocations, invocation => invocation.Identifier == "downloadBytes"));
        var invocation = Assert.Single(JSInterop.Invocations, invocation => invocation.Identifier == "downloadBytes");
        var zipBytes = Assert.IsType<byte[]>(invocation.Arguments[2]);
        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        using var reader = new StreamReader(archive.GetEntry("theme.css")!.Open(), new UTF8Encoding(false, true));
        var css = reader.ReadToEnd();

        Assert.Contains("Bundle preview ready", cut.Find("[data-testid='theme-export-status']").TextContent, StringComparison.Ordinal);
        Assert.Contains("#123456", css, StringComparison.Ordinal);
        Assert.DoesNotContain("background:url(evil)", css, StringComparison.Ordinal);
    }

    [Fact]
    public void DownloadFailureIsObservedAndAnnouncedWithoutEscapingTheRenderLoop()
    {
        var downloadModule = JSInterop.SetupModule("./js/shadcn-download.js");
        downloadModule.SetupVoid("downloadBytes", _ => true).SetException(new JSException("Download blocked."));
        var state = Services.GetRequiredService<ThemeStudioState>();
        var cut = Render<ThemeExportDialog>(parameters => parameters
            .Add(component => component.State, state)
            .Add(component => component.Open, true));
        var acknowledgement = cut.FindAll("input[data-testid='theme-export-warning-ack']").FirstOrDefault();
        acknowledgement?.Change(true);

        cut.Find("button[data-testid='theme-download']").Click();

        cut.WaitForAssertion(() => Assert.Contains("Download blocked", cut.Find("[data-testid='theme-export-status']").TextContent, StringComparison.Ordinal));
    }

    private sealed class NullStorage : IThemeStudioStorage
    {
        public ValueTask<ThemeStudioStorageResult> LoadAsync() => ValueTask.FromResult(ThemeStudioStorageResult.Success(null));
        public ValueTask<ThemeStudioStorageResult> SaveAsync(ShadcnThemeDocument document) => ValueTask.FromResult(ThemeStudioStorageResult.Success(document));
    }

    private static readonly string[] ExpectedPaths =
    [
        "theme.css", "MalievShadcnTheme.cs", "theme.json", "README.md",
        "Examples/Program.cs.txt", "Examples/AppShell.razor.txt", "Examples/FormExample.razor.txt",
        "Examples/OverlayExample.razor.txt", "manifest.json"
    ];
}
