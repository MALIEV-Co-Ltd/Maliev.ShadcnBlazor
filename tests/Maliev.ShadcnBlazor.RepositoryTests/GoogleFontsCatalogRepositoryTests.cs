using System.Diagnostics;

namespace Maliev.ShadcnBlazor.RepositoryTests;

public sealed class GoogleFontsCatalogRepositoryTests
{
    [Fact]
    public void RefreshToolIsMaintainerOnlyAndDocumentsOfflineRuntimeContract()
    {
        var root = FindRepositoryRoot();
        var scriptPath = Path.Combine(root, "eng", "Refresh-GoogleFontsCatalog.ps1");
        var documentationPath = Path.Combine(root, "docs", "theming.md");

        Assert.True(File.Exists(scriptPath), "Missing the maintainer Google Fonts catalog refresh tool.");
        var script = File.ReadAllText(scriptPath);
        Assert.Contains("GOOGLE_FONTS_API_KEY", script, StringComparison.Ordinal);
        Assert.Contains("www.googleapis.com/webfonts/v1/webfonts", script, StringComparison.Ordinal);
        Assert.Contains("google-fonts-catalog.json", script, StringComparison.Ordinal);
        Assert.DoesNotContain("AIza", script, StringComparison.Ordinal);

        var documentation = File.ReadAllText(documentationPath);
        Assert.Contains("checked-in Google Fonts catalog", documentation, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("GOOGLE_FONTS_API_KEY", documentation, StringComparison.Ordinal);
        Assert.Contains("no runtime network", documentation, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("https://developers.google.com/fonts/docs/developer_api", documentation, StringComparison.Ordinal);
        Assert.Contains("https://developers.google.com/fonts/docs/css2", documentation, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RefreshToolFailsClosedWithoutApiKeyAndDoesNotModifySnapshot()
    {
        var root = FindRepositoryRoot();
        var scriptPath = Path.Combine(root, "eng", "Refresh-GoogleFontsCatalog.ps1");
        var snapshotPath = Path.Combine(
            root,
            "samples",
            "Maliev.ShadcnBlazor.Showcase",
            "wwwroot",
            "data",
            "google-fonts-catalog.json");
        var before = File.Exists(snapshotPath) ? await File.ReadAllBytesAsync(snapshotPath) : [];
        var start = new ProcessStartInfo("pwsh")
        {
            WorkingDirectory = root,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        start.ArgumentList.Add("-NoProfile");
        start.ArgumentList.Add("-File");
        start.ArgumentList.Add(scriptPath);
        start.Environment.Remove("GOOGLE_FONTS_API_KEY");

        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start PowerShell.");
        var standardError = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        Assert.NotEqual(0, process.ExitCode);
        Assert.Contains("GOOGLE_FONTS_API_KEY", standardError, StringComparison.Ordinal);
        Assert.Equal(before, File.Exists(snapshotPath) ? await File.ReadAllBytesAsync(snapshotPath) : []);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
