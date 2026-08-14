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
        page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };
        page.PageError += (_, error) => errors.Add(error);
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();

        var primary = page.Locator("input[data-testid='theme-token-light-primary']");
        await primary.FillAsync("#123456");
        await primary.PressAsync("Tab");
        await page.GetByTestId("theme-export-open").ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-export-dialog")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-bundle-path]")).ToHaveCountAsync(ExpectedPaths.Length);
        await Assertions.Expect(page.GetByTestId("theme-export-status")).ToContainTextAsync("Bundle preview ready");
        var acknowledgement = page.GetByTestId("theme-export-warning-ack");
        if (await acknowledgement.CountAsync() > 0)
            await acknowledgement.CheckAsync();

        var download = await page.RunAndWaitForDownloadAsync(
            () => page.GetByTestId("theme-download").ClickAsync());
        Assert.Equal("maliev-shadcn-theme-base-vega-neutral-1.zip", download.SuggestedFilename);
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
            Assert.Contains("#123456", themeJson, StringComparison.Ordinal);
            Assert.Contains("#123456", Encoding.UTF8.GetString(await ReadBytesAsync(archive.GetEntry("theme.css")!)), StringComparison.Ordinal);
            Assert.Contains("#123456", Encoding.UTF8.GetString(await ReadBytesAsync(archive.GetEntry("MalievShadcnTheme.cs")!)), StringComparison.Ordinal);
        }

        await page.GetByRole(AriaRole.Button, new() { Name = "Close theme export" }).ClickAsync();
        await primary.FillAsync("#654321");
        await primary.PressAsync("Tab");
        await Assertions.Expect(primary).ToHaveValueAsync("#654321");
        await page.GetByTestId("theme-import-open").ClickAsync();
        await page.GetByTestId("theme-import-file").SetInputFilesAsync(new FilePayload
        {
            Name = "theme.json",
            MimeType = "application/json",
            Buffer = Encoding.UTF8.GetBytes(themeJson)
        });

        await Assertions.Expect(page.GetByTestId("theme-import-status")).ToContainTextAsync("successfully");
        await Assertions.Expect(primary).ToHaveValueAsync("#123456");
        Assert.Empty(errors);
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
        var primary = page.Locator("input[data-testid='theme-token-light-primary']");
        await primary.FillAsync("#123456");
        await primary.PressAsync("Tab");
        await page.GetByTestId("theme-import-open").ClickAsync();

        foreach (var payload in new[]
        {
            new FilePayload { Name = "theme.json", MimeType = "application/json", Buffer = Encoding.UTF8.GetBytes("{not-json") },
            new FilePayload { Name = "theme.json", MimeType = "application/json", Buffer = Encoding.UTF8.GetBytes("{\"schemaVersion\":999}") },
            new FilePayload { Name = "theme.json", MimeType = "application/json", Buffer = new byte[1_048_577] }
        })
        {
            await page.GetByTestId("theme-import-file").SetInputFilesAsync(payload);
            await Assertions.Expect(page.GetByTestId("theme-import-status")).ToContainTextAsync("not changed");
            await Assertions.Expect(primary).ToHaveValueAsync("#123456");
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
        page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };
        page.PageError += (_, error) => errors.Add(error);
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();
        var opener = page.GetByTestId(openerTestId);

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
        Assert.Empty(errors);
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
        page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };
        page.PageError += (_, error) => errors.Add(error);
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();

        await page.GetByTestId("theme-import-open").ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-import-dialog")).ToBeVisibleAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs").ToString());
        await page.Locator("main").WaitForAsync();
        await page.Keyboard.PressAsync("Escape");

        Assert.Empty(errors);
    }

    private static async Task<byte[]> ReadBytesAsync(ZipArchiveEntry entry)
    {
        await using var stream = entry.Open();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        return memory.ToArray();
    }
}
