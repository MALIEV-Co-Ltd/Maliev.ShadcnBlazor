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
        Assert.Contains("grid-template-columns: minmax(15rem, 18rem) minmax(0, 60rem) minmax(13rem, 16rem)", css, StringComparison.Ordinal);
        Assert.Contains(".documentation-on-this-page ul", css, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
