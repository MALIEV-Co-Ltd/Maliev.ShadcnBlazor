namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeConsumerDeliveryTests
{
    [Fact]
    public void PublicGuidesDocumentThePortableThemeJourney()
    {
        var root = FindRoot();
        var readme = File.ReadAllText(Path.Combine(root, "README.md"));
        var theming = File.ReadAllText(Path.Combine(root, "docs", "theming.md"));

        foreach (var expected in new[]
        {
            "theme.json",
            "theme.css",
            "ShadcnThemeDocumentLoader.LoadAsync",
            "options.Theme",
            "MalievShadcnTheme Include",
            "MSHCN001",
            "MSHCN101",
            "offline"
        })
        {
            Assert.Contains(expected, theming, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("samples/Maliev.ShadcnBlazor.ThemeConsumer", readme, StringComparison.Ordinal);
        Assert.Contains("export", readme, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(".github/workflows/ci.yml")]
    [InlineData(".github/workflows/release.yml")]
    public void DeliveryWorkflowsBuildAgainstThePackedPackage(string relativePath)
    {
        var workflow = File.ReadAllText(Path.Combine(FindRoot(), relativePath));

        Assert.Contains("Validate-ThemeConsumerPackage.ps1", workflow, StringComparison.Ordinal);
        Assert.Contains("Maliev.ShadcnBlazor.ThemeConsumer", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("Select-Object -Single", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void CleanConsumerValidationUsesOnlyThePackedPackage()
    {
        var script = File.ReadAllText(Path.Combine(FindRoot(), "eng", "Validate-ThemeConsumerPackage.ps1"));

        Assert.Contains("UseMalievShadcnPackage=true", script, StringComparison.Ordinal);
        Assert.Contains("MalievShadcnPackageVersion", script, StringComparison.Ordinal);
        Assert.Contains("--locked-mode", script, StringComparison.Ordinal);
        Assert.Contains("--no-restore", script, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectReference", script, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
