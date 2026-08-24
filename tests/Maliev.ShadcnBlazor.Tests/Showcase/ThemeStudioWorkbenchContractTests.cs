namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeStudioWorkbenchContractTests
{
    [Fact]
    public void WorkbenchUsesOfficialBrandPackageControlsAndNamedLandmarks()
    {
        var root = FindRoot();
        var page = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "ThemeStudio.razor");
        var layout = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeStudioLayout.razor");
        var header = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Layout", "DocumentationHeader.razor");
        var sidebar = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeStudioSidebar.razor");
        var inspector = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeInspector.razor");

        Assert.Contains("<DocumentationHeader", layout, StringComparison.Ordinal);
        Assert.Contains("<ThemeStudioSidebar", page, StringComparison.Ordinal);
        Assert.Contains("<ThemeRunway", page, StringComparison.Ordinal);
        Assert.Contains("aria-label=\"Theme preview\"", page, StringComparison.Ordinal);
        Assert.Contains("images/brand/MALIEV_BLACK.svg", header, StringComparison.Ordinal);
        Assert.Contains("<ShadcnSidebarProvider", sidebar, StringComparison.Ordinal);
        Assert.Contains("<ShadcnSidebar", sidebar, StringComparison.Ordinal);
        Assert.Contains("<ShadcnSelect", inspector, StringComparison.Ordinal);
        Assert.DoesNotContain("<Mud", header + sidebar + inspector, StringComparison.Ordinal);
        Assert.Contains("Label=\"Theme settings\"", sidebar, StringComparison.Ordinal);
        Assert.Contains("id=\"theme-studio-sidebar-region\"", sidebar, StringComparison.Ordinal);
        Assert.Contains("data-testid=\"theme-studio-sidebar\"", sidebar, StringComparison.Ordinal);
    }

    [Fact]
    public void WorkbenchExposesAllPreviewAndSettingsSectionsWithStableHooks()
    {
        var root = FindRoot();
        var page = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "ThemeStudio.razor");
        var inspector = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeInspector.razor");
        var combined = page + inspector;

        foreach (var hook in new[] { "preview-reduced-motion", "preview-high-contrast", "runway-pause", "theme-icon-library-select" })
            Assert.Contains($"data-testid=\"{hook}\"", combined, StringComparison.Ordinal);

        Assert.Contains("viewport-{viewport.Id}", inspector, StringComparison.Ordinal);
        Assert.DoesNotContain("<PreviewToolbar", page, StringComparison.Ordinal);
        Assert.DoesNotContain("ThemeColorGroup", inspector, StringComparison.Ordinal);
        Assert.DoesNotContain("ThemeGeneratorOptions", inspector, StringComparison.Ordinal);

        foreach (var section in new[] { "preview", "preset", "typography", "icons", "accessibility", "transfer" })
            Assert.Contains($"id=\"theme-settings-{section}\"", inspector, StringComparison.Ordinal);
    }

    [Fact]
    public void WorkbenchCssKeepsTheDocumentBoundedAndSupportsDrawerAccessibilityModes()
    {
        var root = FindRoot();
        var css = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css");

        Assert.Contains(".theme-preview-scope[data-preview-reduced-motion=\"true\"]", css, StringComparison.Ordinal);
        Assert.Contains(".theme-preview-scope[data-preview-high-contrast=\"true\"]", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-sidebar-backdrop", css, StringComparison.Ordinal);
        Assert.Contains("position: fixed", css, StringComparison.Ordinal);
        Assert.Contains("overflow-x: clip", css, StringComparison.Ordinal);
        Assert.Contains("@media (forced-colors: active)", css, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", css, StringComparison.Ordinal);
        Assert.Contains(":focus-visible", css, StringComparison.Ordinal);
    }

    [Fact]
    public void PreviewThemeNeverMutatesTheCompanyShell()
    {
        var root = FindRoot();
        var page = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "ThemeStudio.razor");
        var typography = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeTypographyEditor.razor");

        Assert.Contains("data-shadcn-theme=\"@(State.EffectiveDarkMode", page, StringComparison.Ordinal);
        Assert.Contains("dir=\"@(State.Direction", page, StringComparison.Ordinal);
        Assert.DoesNotContain("ShellState.SetTheme", page, StringComparison.Ordinal);
        Assert.DoesNotContain("ShellState.SetDirection", page, StringComparison.Ordinal);
        Assert.DoesNotContain("style=\"@TypographyStyle\"", typography, StringComparison.Ordinal);
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
