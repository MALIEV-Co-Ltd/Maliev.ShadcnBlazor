namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class DocumentationShellStyleContractTests
{
    [Fact]
    public void DocumentationShellHasReferenceNavigationAndAReadableThreeColumnMeasure()
    {
        var root = FindRoot();
        var header = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Layout", "DocumentationHeader.razor"));
        var css = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css"));

        Assert.Contains("documentation-topnav", header, StringComparison.Ordinal);
        Assert.Contains("documentation-brand__mark", header, StringComparison.Ordinal);
        Assert.Contains("documentation-topnav a", css, StringComparison.Ordinal);
        Assert.Contains("grid-template-columns: minmax(15rem, 18rem) minmax(0, 1fr) minmax(13rem, 16rem)", css, StringComparison.Ordinal);
        Assert.Contains(".documentation-on-this-page ul", css, StringComparison.Ordinal);
        Assert.Contains(".component-api__table code", css, StringComparison.Ordinal);
        Assert.Contains(".component-api__identifier", css, StringComparison.Ordinal);
        Assert.Contains(".component-api__value", css, StringComparison.Ordinal);
        Assert.Contains("display: inline-block", css, StringComparison.Ordinal);
        Assert.Contains("max-inline-size: 100%", css, StringComparison.Ordinal);
        Assert.Contains("overflow-wrap: anywhere", css, StringComparison.Ordinal);
        Assert.Contains(".component-api__required", css, StringComparison.Ordinal);
        Assert.Contains("background: color-mix(in oklch, var(--shadcn-primary) 12%, var(--shadcn-background))", css, StringComparison.Ordinal);
        Assert.Contains("color: var(--shadcn-primary)", css, StringComparison.Ordinal);
        Assert.Contains(".documentation-prose-list", css, StringComparison.Ordinal);
        Assert.Contains("padding-inline-start: 1rem", css, StringComparison.Ordinal);
        Assert.DoesNotContain(".component-reference li {", css, StringComparison.Ordinal);
    }

    [Fact]
    public void MobileHeaderAndOutlineUseLogicalOrderSafeAreasAndAccessibleTargets()
    {
        var root = FindRoot();
        var header = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Layout", "DocumentationHeader.razor"));
        var layout = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Layout", "DocumentationLayout.razor"));
        var css = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css"))
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        var drawer = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "js", "documentation-drawer.js"));

        Assert.True(header.IndexOf("documentation-brand", StringComparison.Ordinal) < header.IndexOf("catalog-trigger", StringComparison.Ordinal));
        Assert.Contains("role=\"@(NavigationState.OutlineOpen ? \"dialog\" : null)\"", layout, StringComparison.Ordinal);
        Assert.Contains("aria-modal=\"@(NavigationState.OutlineOpen ? \"true\" : null)\"", layout, StringComparison.Ordinal);
        Assert.Contains("data-testid=\"outline-close\"", layout, StringComparison.Ordinal);
        Assert.Contains("min-inline-size: 2.75rem", css, StringComparison.Ordinal);
        Assert.Contains("env(safe-area-inset-top)", css, StringComparison.Ordinal);
        Assert.Contains("env(safe-area-inset-bottom)", css, StringComparison.Ordinal);
        Assert.Contains("drawer.dataset.drawerReady = 'true'", drawer, StringComparison.Ordinal);
        Assert.Contains("event.key !== 'Tab'", drawer, StringComparison.Ordinal);
        Assert.Contains("drawer.dataset.drawerFocusReady", drawer, StringComparison.Ordinal);
        Assert.Contains("requestAnimationFrame", drawer, StringComparison.Ordinal);
        Assert.Contains("cancelAnimationFrame", drawer, StringComparison.Ordinal);
        Assert.Contains("Dispose();", layout, StringComparison.Ordinal);
    }

    [Fact]
    public void DocumentationHeaderUsesViewportGuttersWithoutConstrainingTheReadingColumn()
    {
        var root = FindRoot();
        var css = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css"));

        Assert.Contains("--documentation-shell-gutter: clamp(0.75rem, 2vw, 1.5rem)", css, StringComparison.Ordinal);
        Assert.Contains("padding-inline-start: max(var(--documentation-shell-gutter), var(--documentation-safe-inline-start))", css, StringComparison.Ordinal);
        Assert.Contains("padding-inline-end: max(var(--documentation-shell-gutter), var(--documentation-safe-inline-end))", css, StringComparison.Ordinal);
        Assert.Contains("grid-template-columns: minmax(0, 1fr) auto minmax(0, 1fr)", css, StringComparison.Ordinal);
        Assert.Contains(".documentation-header:dir(rtl)", css, StringComparison.Ordinal);
        Assert.DoesNotContain("calc((100vw - 96rem) / 2)", css, StringComparison.Ordinal);
        Assert.Contains(".documentation-content > :where(.component-dossier, .documentation-landing)", css, StringComparison.Ordinal);
        Assert.Contains("inline-size: min(100%, 58rem)", css, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
