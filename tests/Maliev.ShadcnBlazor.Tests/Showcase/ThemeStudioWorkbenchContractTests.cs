namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeStudioWorkbenchContractTests
{
    [Fact]
    public void WorkbenchUsesOfficialBrandPackageControlsAndNamedLandmarks()
    {
        var root = FindRoot();
        var page = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "ThemeStudio.razor");
        var appBar = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeStudioAppBar.razor");
        var toolbar = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "PreviewToolbar.razor");
        var sidebar = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeStudioSidebar.razor");
        var appBarControls = appBar + toolbar;

        Assert.Contains("<ThemeStudioAppBar", page, StringComparison.Ordinal);
        Assert.Contains("<ThemeStudioSidebar", page, StringComparison.Ordinal);
        Assert.Contains("aria-label=\"Theme preview\"", page, StringComparison.Ordinal);
        Assert.Contains("images/brand/MALIEV_BLACK.svg", appBar, StringComparison.Ordinal);
        Assert.DoesNotContain("theme-studio-appbar__mark", appBar, StringComparison.Ordinal);
        Assert.Contains("<ShadcnButton", appBarControls, StringComparison.Ordinal);
        Assert.Contains("<ShadcnToggle", appBarControls, StringComparison.Ordinal);
        Assert.Contains("<ShadcnSelect", appBarControls, StringComparison.Ordinal);
        Assert.DoesNotContain("<Mud", appBarControls + sidebar + Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeInspector.razor"), StringComparison.Ordinal);
        Assert.Contains("aria-label=\"Theme Studio\"", appBar, StringComparison.Ordinal);
        Assert.Contains("aria-label=\"Theme settings\"", sidebar, StringComparison.Ordinal);
        Assert.Contains("id=\"theme-studio-sidebar\"", sidebar, StringComparison.Ordinal);
    }

    [Fact]
    public void WorkbenchExposesAllPreviewAndSettingsSectionsWithStableHooks()
    {
        var root = FindRoot();
        var appBar = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeStudioAppBar.razor");
        var toolbar = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "PreviewToolbar.razor");
        var inspector = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeInspector.razor");
        var combined = appBar + toolbar + inspector;

        foreach (var hook in new[]
        {
            "direction-ltr", "direction-rtl", "locale-english", "locale-thai",
            "preview-reduced-motion", "preview-high-contrast"
        })
            Assert.Contains($"data-testid=\"{hook}\"", combined, StringComparison.Ordinal);

        Assert.Contains("viewport-{viewport.Id}", toolbar, StringComparison.Ordinal);
        Assert.Contains("mode-{mode.ToString().ToLowerInvariant()}", toolbar, StringComparison.Ordinal);

        foreach (var section in new[] { "colors", "typography", "generation", "transfer" })
        {
            Assert.Contains($"href=\"#theme-settings-{section}\"", inspector, StringComparison.Ordinal);
            Assert.Contains($"id=\"theme-settings-{section}\"", inspector, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void WorkbenchCssKeepsTheDocumentBoundedAndSupportsDrawerAccessibilityModes()
    {
        var root = FindRoot();
        var css = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css");

        Assert.Contains(".theme-studio-provider[data-preview-reduced-motion=\"true\"]", css, StringComparison.Ordinal);
        Assert.Contains(".theme-studio-provider[data-preview-high-contrast=\"true\"]", css, StringComparison.Ordinal);
        Assert.Contains(".theme-studio-sidebar-backdrop", css, StringComparison.Ordinal);
        Assert.Contains("position: fixed", css, StringComparison.Ordinal);
        Assert.Contains("overflow-x: clip", css, StringComparison.Ordinal);
        Assert.Contains("@media (forced-colors: active)", css, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", css, StringComparison.Ordinal);
        Assert.Contains(":focus-visible", css, StringComparison.Ordinal);
    }

    private static string Read(string root, params string[] segments) =>
        File.ReadAllText(Path.Combine([root, .. segments]));

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
