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
        Assert.Contains("white-space: nowrap", css, StringComparison.Ordinal);
        Assert.Contains("overflow-wrap: normal", css, StringComparison.Ordinal);
        Assert.Contains("word-break: normal", css, StringComparison.Ordinal);
        Assert.Contains(".component-api__required", css, StringComparison.Ordinal);
        Assert.Contains("background: color-mix(in oklch, var(--shadcn-primary) 12%, var(--shadcn-background))", css, StringComparison.Ordinal);
        Assert.Contains("color: var(--shadcn-primary)", css, StringComparison.Ordinal);
        Assert.Contains(".component-reference li", css, StringComparison.Ordinal);
        Assert.Contains("margin-block-start: .5rem", css, StringComparison.Ordinal);
        Assert.Contains("margin-inline-start: 1rem", css, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
