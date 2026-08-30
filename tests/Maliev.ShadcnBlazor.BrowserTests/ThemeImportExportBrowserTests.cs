using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ThemeImportExportBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
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
    public async Task AdvisoryOnlyValidationKeepsExportAvailableAndExplainsItsConsequence()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            AcceptDownloads = true,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();

        await Assertions.Expect(page.GetByTestId("theme-validation-status")).ToContainTextAsync("Ready to export ·");
        await Assertions.Expect(page.GetByTestId("theme-export-open")).ToBeEnabledAsync();
        await page.GetByTestId("theme-validation-status").ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-validation-summary")).ToContainTextAsync("Advisories do not block export");

        await page.GetByTestId("theme-export-open").ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-export-dialog")).ToBeVisibleAsync();
        var acknowledgement = page.GetByTestId("theme-export-warning-ack");
        await Assertions.Expect(acknowledgement).ToBeVisibleAsync();
        await Assertions.Expect(acknowledgement.Locator("xpath=.."))
            .ToContainTextAsync("contrast warnings recorded in README.md");
        await Assertions.Expect(page.GetByTestId("theme-download")).ToBeDisabledAsync();
        await acknowledgement.CheckAsync();
        await Assertions.Expect(page.GetByTestId("theme-download")).ToBeEnabledAsync();
        var download = await page.RunAndWaitForDownloadAsync(
            () => page.GetByTestId("theme-download").ClickAsync(),
            new() { Timeout = 90_000 });
        var downloadPath = await download.PathAsync();
        Assert.NotNull(downloadPath);
        using var archive = ZipFile.OpenRead(downloadPath);
        var readme = Encoding.UTF8.GetString(await ReadBytesAsync(archive.GetEntry("README.md")!));
        Assert.Contains("Contrast warnings", readme, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExportDownloadsAndInspectsARealDeterministicBundleThenReimportsItsCanonicalJson()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            AcceptDownloads = true,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        var errors = new List<string>();
        CapturePageErrors(page, errors);
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();

        await page.GetByTestId("documentation-theme-toggle").ClickAsync();
        await page.GetByTestId("documentation-direction-toggle").ClickAsync();
        await page.GetByTestId("locale-thai").ClickAsync();
        await page.GetByTestId("theme-preset").ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = "Cobalt Precision", Exact = true }).ClickAsync();
        await OpenCollapsibleAsync(page, "theme-typography-section");
        await page.GetByTestId("theme-font-search").FillAsync("DM Sans");
        await page.GetByTestId("theme-font-result-dm-sans").ClickAsync();
        await OpenCollapsibleAsync(page, "theme-advanced-typography");
        var headingScale = page.GetByTestId("theme-role-heading-1-scale");
        await headingScale.FillAsync("2.5");
        await headingScale.PressAsync("Tab");
        await page.GetByTestId("theme-export-open").ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-export-dialog")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-bundle-path]")).ToHaveCountAsync(ExpectedPaths.Length);
        await Assertions.Expect(page.GetByTestId("theme-export-status")).ToContainTextAsync("Bundle preview ready");
        var acknowledgement = page.GetByTestId("theme-export-warning-ack");
        if (await acknowledgement.CountAsync() > 0)
            await acknowledgement.CheckAsync();

        var download = await page.RunAndWaitForDownloadAsync(
            () => page.GetByTestId("theme-download").ClickAsync(),
            new() { Timeout = 90_000 });
        Assert.Equal("maliev-shadcn-theme-cobalt-precision-2.zip", download.SuggestedFilename);
        var downloadPath = await download.PathAsync();
        Assert.NotNull(downloadPath);

        string themeJson;
        using (var archive = ZipFile.OpenRead(downloadPath))
        {
            Assert.Equal(ExpectedPaths, archive.Entries.Select(entry => entry.FullName));
            var manifestEntry = archive.GetEntry("manifest.json")!;
            using var manifestDocument = JsonDocument.Parse(await ReadBytesAsync(manifestEntry));
            foreach (var item in manifestDocument.RootElement.GetProperty("files").EnumerateArray())
            {
                var path = item.GetProperty("path").GetString()!;
                var bytes = await ReadBytesAsync(archive.GetEntry(path)!);
                Assert.Equal(bytes.LongLength, item.GetProperty("size").GetInt64());
                Assert.Equal(Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(), item.GetProperty("sha256").GetString());
            }

            themeJson = Encoding.UTF8.GetString(await ReadBytesAsync(archive.GetEntry("theme.json")!));
            using var themeDocument = JsonDocument.Parse(themeJson);
            Assert.Equal(2, themeDocument.RootElement.GetProperty("schemaVersion").GetInt32());
            Assert.True(themeDocument.RootElement.TryGetProperty("application", out _));
            Assert.True(themeDocument.RootElement.TryGetProperty("palette", out _));
            Assert.True(themeDocument.RootElement.TryGetProperty("typography", out _));
            Assert.Equal("dm-sans", themeDocument.RootElement.GetProperty("typography").GetProperty("body").GetProperty("googleFontsId").GetString());
            Assert.Equal(2.5, themeDocument.RootElement.GetProperty("typography").GetProperty("roles").GetProperty("heading1").GetProperty("scale").GetDouble());
            Assert.True(themeDocument.RootElement.GetProperty("application").GetProperty("defaultDarkMode").GetBoolean());
            Assert.Equal("rightToLeft", themeDocument.RootElement.GetProperty("application").GetProperty("defaultDirection").GetString());
            Assert.Equal("th", themeDocument.RootElement.GetProperty("application").GetProperty("defaultLocale").GetString());
            Assert.Equal(303UL, themeDocument.RootElement.GetProperty("palette").GetProperty("seed").GetUInt64());
            Assert.Contains("oklch(0.49 0.22 264)", themeJson, StringComparison.Ordinal);
            var css = Encoding.UTF8.GetString(await ReadBytesAsync(archive.GetEntry("theme.css")!));
            Assert.Contains("oklch(0.49 0.22 264)", css, StringComparison.Ordinal);
            Assert.Contains("--shadcn-font-sans: 'DM Sans', 'Noto Sans Thai', ui-sans-serif, system-ui, sans-serif", css, StringComparison.Ordinal);
            Assert.Contains("--shadcn-typography-heading-1-scale: 2.5", css, StringComparison.Ordinal);
            Assert.Contains("oklch(0.49 0.22 264)", Encoding.UTF8.GetString(await ReadBytesAsync(archive.GetEntry("MalievShadcnTheme.cs")!)), StringComparison.Ordinal);
        }

        await page.GetByRole(AriaRole.Button, new() { Name = "Close theme export" }).ClickAsync();
        await page.GetByTestId("documentation-theme-toggle").ClickAsync();
        await page.GetByTestId("documentation-direction-toggle").ClickAsync();
        await page.GetByTestId("locale-english").ClickAsync();
        await page.GetByTestId("theme-preset").ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = "MALIEV Precision", Exact = true }).ClickAsync();
        await OpenCollapsibleAsync(page, "theme-advanced-transfer");
        await ClickInspectorControlAsync(page, "theme-import-open");
        await page.GetByTestId("theme-import-file").SetInputFilesAsync(new FilePayload
        {
            Name = "theme.json",
            MimeType = "application/json",
            Buffer = Encoding.UTF8.GetBytes(themeJson)
        });

        await Assertions.Expect(page.GetByTestId("theme-import-status")).ToContainTextAsync("successfully");
        await Assertions.Expect(page.GetByTestId("theme-preview-scope")).ToHaveAttributeAsync("data-shadcn-theme", "dark");
        await Assertions.Expect(page.GetByTestId("theme-preview-scope")).ToHaveAttributeAsync("dir", "rtl");
        await Assertions.Expect(page.Locator(".theme-studio-provider")).ToHaveAttributeAsync("data-shadcn-theme", "light");
        await Assertions.Expect(page.Locator(".theme-studio-provider")).ToHaveAttributeAsync("dir", "ltr");
        await Assertions.Expect(page.GetByTestId("locale-thai")).ToHaveAttributeAsync("aria-pressed", "true");
        await page.WaitForFunctionAsync("() => localStorage.getItem('maliev.shadcn.theme-studio.document.v2')?.includes('\\\"schemaVersion\\\": 2')");
        await page.ReloadAsync();
        await page.GetByTestId("theme-studio").WaitForAsync();
        await Assertions.Expect(page.GetByTestId("theme-preset")).ToContainTextAsync("Cobalt Precision");
        await Assertions.Expect(page.GetByTestId("theme-preview-scope")).ToHaveAttributeAsync("data-shadcn-theme", "dark");
        await Assertions.Expect(page.GetByTestId("theme-preview-scope")).ToHaveAttributeAsync("dir", "rtl");
        var restoredTypography = await page.GetByTestId("theme-preview-scope").GetAttributeAsync("style");
        Assert.Contains("--shadcn-font-sans: 'DM Sans', 'Noto Sans Thai', ui-sans-serif, system-ui, sans-serif", restoredTypography, StringComparison.Ordinal);
        Assert.Contains("--shadcn-typography-heading-1-scale: 2.5", restoredTypography, StringComparison.Ordinal);
        Assert.Null(await page.GetByTestId("theme-typography-editor").GetAttributeAsync("style"));
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public async Task PaletteWorkbenchExportsAndReimportsFiveNormalizedAnchorsAndHarmony()
    {
        string themeJson;
        string[] displayedAnchors;
        await using (var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            AcceptDownloads = true,
            ReducedMotion = ReducedMotion.Reduce
        }))
        {
            var page = await context.NewPageAsync();
            await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
            await page.GetByTestId("theme-studio").WaitForAsync();
            await page.GetByTestId("theme-palette-customize").ClickAsync();
            await page.GetByTestId("theme-palette-advanced").GetByRole(AriaRole.Button).ClickAsync();
            var roleKeys = new[] { "brand", "support", "highlight", "dataa", "datab" };
            var anchorInputs = new[] { "#2563eb", "#0f766e", "#b45309", "#7c3aed", "#8b5cf6" };
            for (var index = 0; index < roleKeys.Length; index++)
            {
                var editor = page.GetByTestId($"theme-palette-anchor-{roleKeys[index]}");
                await editor.Locator("input[type='text']").FillAsync(anchorInputs[index]);
                await editor.Locator("input[type='text']").PressAsync("Tab");
                var lockButton = editor.Locator("[data-palette-lock]");
                if (await lockButton.CountAsync() > 0)
                    await Assertions.Expect(lockButton).ToHaveAttributeAsync("aria-pressed", "true");
            }
            await page.GetByTestId("theme-palette-harmony").ClickAsync();
            await page.GetByRole(AriaRole.Option, new() { Name = "Triadic", Exact = true }).ClickAsync();
            await page.GetByTestId("theme-palette-generate").ClickAsync();

            displayedAnchors = new string[roleKeys.Length];
            for (var index = 0; index < roleKeys.Length; index++)
            {
                displayedAnchors[index] = await page.GetByTestId($"theme-palette-anchor-{roleKeys[index]}")
                    .Locator("input[type='text']")
                    .InputValueAsync();
                var liveToken = await page.GetByTestId("theme-preview-scope").EvaluateAsync<string>(
                    $"element => getComputedStyle(element).getPropertyValue('--shadcn-chart-{index + 1}').trim()");
                Assert.Equal(displayedAnchors[index], liveToken);
            }

            await page.GetByTestId("theme-export-open").ClickAsync();
            var acknowledgement = page.GetByTestId("theme-export-warning-ack");
            if (await acknowledgement.CountAsync() > 0)
                await acknowledgement.CheckAsync();
            var download = await page.RunAndWaitForDownloadAsync(
                () => page.GetByTestId("theme-download").ClickAsync(),
                new() { Timeout = 90_000 });
            var downloadPath = await download.PathAsync();
            Assert.NotNull(downloadPath);
            using var archive = ZipFile.OpenRead(downloadPath);
            themeJson = Encoding.UTF8.GetString(await ReadBytesAsync(archive.GetEntry("theme.json")!));
            using var document = JsonDocument.Parse(themeJson);
            var palette = document.RootElement.GetProperty("palette");
            var anchors = palette.GetProperty("anchors");
            var jsonKeys = new[] { "brand", "support", "highlight", "dataA", "dataB" };
            for (var index = 0; index < jsonKeys.Length; index++)
                Assert.Equal(displayedAnchors[index], anchors.GetProperty(jsonKeys[index]).GetString());
            Assert.Equal("triadic", palette.GetProperty("harmony").GetString());
        }

        await using var freshContext = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var freshPage = await freshContext.NewPageAsync();
        await freshPage.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await freshPage.GetByTestId("theme-studio").WaitForAsync();
        await OpenCollapsibleAsync(freshPage, "theme-advanced-transfer");
        await ClickInspectorControlAsync(freshPage, "theme-import-open");
        await freshPage.GetByTestId("theme-import-file").SetInputFilesAsync(new FilePayload
        {
            Name = "theme.json",
            MimeType = "application/json",
            Buffer = Encoding.UTF8.GetBytes(themeJson)
        });
        await Assertions.Expect(freshPage.GetByTestId("theme-import-status")).ToContainTextAsync("successfully");
        await freshPage.GetByRole(AriaRole.Button, new() { Name = "Close theme import" }).ClickAsync();
        await freshPage.GetByTestId("theme-palette-customize").ClickAsync();
        await freshPage.GetByTestId("theme-palette-advanced").GetByRole(AriaRole.Button).ClickAsync();
        var importedRoleKeys = new[] { "brand", "support", "highlight", "dataa", "datab" };
        for (var index = 0; index < importedRoleKeys.Length; index++)
            Assert.Equal(displayedAnchors[index], await freshPage.GetByTestId($"theme-palette-anchor-{importedRoleKeys[index]}").Locator("input[type='text']").InputValueAsync());
        await Assertions.Expect(freshPage.GetByTestId("theme-palette-harmony")).ToContainTextAsync("Triadic");
    }

    [Fact]
    public async Task NativeColorPickerGestureUpdatesPreviewAndPersistsOneNormalizedExportableHistoryEntry()
    {
        const string storageKey = "maliev.shadcn.theme-studio.document.v2";
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            AcceptDownloads = true,
            ReducedMotion = ReducedMotion.Reduce
        });
        await context.AddInitScriptAsync($$"""
            (() => {
                const originalSetItem = Storage.prototype.setItem;
                window.__themeStudioPaletteWriteCount = 0;
                Storage.prototype.setItem = function(key, value) {
                    if (key === '{{storageKey}}')
                        window.__themeStudioPaletteWriteCount++;
                    return originalSetItem.call(this, key, value);
                };
            })();
            """);
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();
        await page.GetByTestId("theme-palette-customize").ClickAsync();
        await page.WaitForFunctionAsync($"() => localStorage.getItem('{storageKey}') !== null");
        await Task.Delay(100);

        var editor = page.GetByTestId("theme-palette-anchor-brand");
        var picker = editor.Locator("input[type='color']");
        var anchor = editor.Locator("input[type='text']");
        var preview = page.GetByTestId("theme-preview-scope");
        var undo = page.GetByRole(AriaRole.Button, new() { Name = "Undo theme change", Exact = true });
        var originalAnchor = await anchor.InputValueAsync();
        var originalStorage = await page.EvaluateAsync<string>($"localStorage.getItem('{storageKey}')");
        var previousPrimary = await preview.EvaluateAsync<string>("element => getComputedStyle(element).getPropertyValue('--shadcn-primary').trim()");
        await Assertions.Expect(undo).ToBeDisabledAsync();
        await page.EvaluateAsync("() => window.__themeStudioPaletteWriteCount = 0");

        await picker.EvaluateAsync("input => input.dispatchEvent(new PointerEvent('pointerdown', { bubbles: true, pointerId: 1, pointerType: 'mouse', isPrimary: true, buttons: 1 }))");
        foreach (var value in new[] { "#dc2626", "#1d4ed8", "#2563eb" })
        {
            await picker.EvaluateAsync(
                "(input, value) => { input.value = value; input.dispatchEvent(new Event('input', { bubbles: true, composed: true })); }",
                value);
            await page.WaitForFunctionAsync(
                "before => getComputedStyle(document.querySelector('[data-testid=theme-preview-scope]')).getPropertyValue('--shadcn-primary').trim() !== before",
                previousPrimary);
            previousPrimary = await preview.EvaluateAsync<string>("element => getComputedStyle(element).getPropertyValue('--shadcn-primary').trim()");
            await Assertions.Expect(page.GetByTestId("theme-studio")).ToHaveAttributeAsync("data-pointer-interaction-active", "true");
        }

        Assert.Equal(originalStorage, await page.EvaluateAsync<string>($"localStorage.getItem('{storageKey}')"));
        Assert.Equal(0, await page.EvaluateAsync<int>("() => window.__themeStudioPaletteWriteCount"));
        await Assertions.Expect(undo).ToBeEnabledAsync();

        await picker.EvaluateAsync("input => input.dispatchEvent(new PointerEvent('pointerup', { bubbles: true, pointerId: 1, pointerType: 'mouse', isPrimary: true }))");
        await Assertions.Expect(page.GetByTestId("theme-studio")).ToHaveAttributeAsync("data-pointer-interaction-active", "false");
        await page.WaitForFunctionAsync(
            $"before => localStorage.getItem('{storageKey}') !== before",
            originalStorage);
        await Task.Delay(100);
        Assert.Equal(1, await page.EvaluateAsync<int>("() => window.__themeStudioPaletteWriteCount"));
        var normalizedAnchor = await anchor.InputValueAsync();
        Assert.StartsWith("oklch(", normalizedAnchor, StringComparison.Ordinal);
        var persistedJson = await page.EvaluateAsync<string>($"localStorage.getItem('{storageKey}')");
        using (var persisted = JsonDocument.Parse(persistedJson))
            Assert.Equal(normalizedAnchor, persisted.RootElement.GetProperty("palette").GetProperty("anchors").GetProperty("brand").GetString());

        await page.GetByTestId("theme-export-open").ClickAsync();
        var acknowledgement = page.GetByTestId("theme-export-warning-ack");
        if (await acknowledgement.CountAsync() > 0)
            await acknowledgement.CheckAsync();
        var download = await page.RunAndWaitForDownloadAsync(
            () => page.GetByTestId("theme-download").ClickAsync(),
            new() { Timeout = 90_000 });
        var downloadPath = await download.PathAsync();
        Assert.NotNull(downloadPath);
        using (var archive = ZipFile.OpenRead(downloadPath))
        using (var exported = JsonDocument.Parse(await ReadBytesAsync(archive.GetEntry("theme.json")!)))
            Assert.Equal(normalizedAnchor, exported.RootElement.GetProperty("palette").GetProperty("anchors").GetProperty("brand").GetString());

        await page.GetByRole(AriaRole.Button, new() { Name = "Close theme export" }).ClickAsync();
        await undo.ClickAsync();
        await Assertions.Expect(undo).ToBeDisabledAsync();
        await page.GetByTestId("theme-palette-customize").ClickAsync();
        Assert.Equal(originalAnchor, await page.GetByTestId("theme-palette-anchor-brand").Locator("input[type='text']").InputValueAsync());
    }

    [Fact]
    public async Task CorruptFutureAndOversizedImportsNeverChangeTheAppliedTheme()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();
        await OpenSettingsIfClosedAsync(page);
        await page.GetByTestId("theme-preset").ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = "Cobalt Precision", Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-preset")).ToHaveAttributeAsync("aria-expanded", "false");
        await Assertions.Expect(page.Locator("[data-testid='theme-preset'] + [data-slot='select-content']")).ToHaveCountAsync(0);
        var primary = await page.GetByTestId("theme-preview-scope").EvaluateAsync<string>("element => getComputedStyle(element).getPropertyValue('--shadcn-primary').trim()");
        await OpenCollapsibleAsync(page, "theme-advanced-transfer");
        await ClickInspectorControlAsync(page, "theme-import-open");

        foreach (var payload in new[]
        {
            new FilePayload { Name = "theme.json", MimeType = "application/json", Buffer = Encoding.UTF8.GetBytes("{not-json") },
            new FilePayload { Name = "theme.json", MimeType = "application/json", Buffer = Encoding.UTF8.GetBytes("{\"schemaVersion\":999}") },
            new FilePayload { Name = "theme.json", MimeType = "application/json", Buffer = new byte[1_048_577] }
        })
        {
            await page.GetByTestId("theme-import-file").SetInputFilesAsync(payload);
            await Assertions.Expect(page.GetByTestId("theme-import-status")).ToContainTextAsync("not changed");
            Assert.Equal(primary, await page.GetByTestId("theme-preview-scope").EvaluateAsync<string>("element => getComputedStyle(element).getPropertyValue('--shadcn-primary').trim()"));
        }
    }

    [Theory]
    [InlineData("theme-import-open", "theme-import-dialog", "Close theme import")]
    [InlineData("theme-export-open", "theme-export-dialog", "Close theme export")]
    public async Task NativeEscapeSynchronizesStateRestoresOpenerAndPermitsReopen(
        string openerTestId,
        string dialogTestId,
        string closeName)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1024, Height = 800 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        var errors = new List<string>();
        CapturePageErrors(page, errors);
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();
        await OpenSettingsIfClosedAsync(page);
        if (string.Equals(openerTestId, "theme-import-open", StringComparison.Ordinal))
            await OpenCollapsibleAsync(page, "theme-advanced-transfer");
        var opener = page.GetByTestId(openerTestId);

        await opener.ScrollIntoViewIfNeededAsync();
        await opener.ClickAsync();
        await Assertions.Expect(page.GetByTestId(dialogTestId)).ToBeVisibleAsync();
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(page.GetByTestId(dialogTestId)).Not.ToBeVisibleAsync();
        await Assertions.Expect(opener).ToBeFocusedAsync();

        await opener.ClickAsync();
        await Assertions.Expect(page.GetByTestId(dialogTestId)).ToBeVisibleAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = closeName }).ClickAsync();
        await Assertions.Expect(page.GetByTestId(dialogTestId)).Not.ToBeVisibleAsync();
        await Assertions.Expect(opener).ToBeFocusedAsync();
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public async Task DialogListenersAreRemovedWhenNavigatingAway()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1024, Height = 800 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        var errors = new List<string>();
        CapturePageErrors(page, errors);
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();
        await OpenSettingsIfClosedAsync(page);

        await OpenCollapsibleAsync(page, "theme-advanced-transfer");
        await ClickInspectorControlAsync(page, "theme-import-open");
        await Assertions.Expect(page.GetByTestId("theme-import-dialog")).ToBeVisibleAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs").ToString());
        await page.Locator("main").WaitForAsync();
        await page.Keyboard.PressAsync("Escape");

        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    private static void CapturePageErrors(IPage page, ICollection<string> errors)
    {
        page.Console += (_, message) =>
        {
            if (message.Type != "error" || IsOptionalGoogleFontFailure(message))
                return;

            var location = string.IsNullOrWhiteSpace(message.Location) ? string.Empty : $" ({message.Location})";
            errors.Add($"{message.Text}{location}");
        };
        page.PageError += (_, error) => errors.Add(error);
    }

    private static async Task OpenSettingsIfClosedAsync(IPage page)
    {
        var toggle = page.GetByTestId("theme-controls-toggle");
        if (string.Equals(await toggle.GetAttributeAsync("aria-expanded"), "false", StringComparison.Ordinal))
            await toggle.ClickAsync();
    }

    private static async Task ClickInspectorControlAsync(IPage page, string testId)
    {
        var control = page.GetByTestId(testId);
        await control.ScrollIntoViewIfNeededAsync();
        await control.ClickAsync();
    }

    private static async Task OpenCollapsibleAsync(IPage page, string testId)
    {
        var trigger = page.GetByTestId(testId).Locator(":scope > [data-slot='collapsible-trigger']");
        if (string.Equals(await trigger.GetAttributeAsync("aria-expanded"), "false", StringComparison.Ordinal))
            await trigger.ClickAsync();
    }

    private static bool IsOptionalGoogleFontFailure(IConsoleMessage message)
    {
        if (!message.Text.Contains("Failed to load resource", StringComparison.OrdinalIgnoreCase))
            return false;

        return message.Location.Contains("fonts.googleapis.com", StringComparison.OrdinalIgnoreCase)
            || message.Location.Contains("fonts.gstatic.com", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<byte[]> ReadBytesAsync(ZipArchiveEntry entry)
    {
        await using var stream = entry.Open();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        return memory.ToArray();
    }
}
