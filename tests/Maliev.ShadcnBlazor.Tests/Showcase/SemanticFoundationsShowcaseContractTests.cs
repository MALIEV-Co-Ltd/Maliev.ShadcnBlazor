namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class SemanticFoundationsShowcaseContractTests
{
    [Fact]
    public void ShowcaseLoadsSemanticStylesAndExposesComponentRoute()
    {
        var root = FindRoot();
        var index = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "index.html"));
        var layout = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Layout", "MainLayout.razor"));
        var page = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "SemanticFoundations.razor"));

        Assert.Contains("_content/Maliev.ShadcnBlazor/css/shadcn-semantic-foundations.css", index, StringComparison.Ordinal);
        Assert.Contains("/components/semantic-foundations", layout, StringComparison.Ordinal);
        Assert.Contains("@page \"/components/semantic-foundations\"", page, StringComparison.Ordinal);
        Assert.Contains("<ShadcnAspectRatio", page, StringComparison.Ordinal);
        Assert.Contains("<ShadcnField", page, StringComparison.Ordinal);
        Assert.Contains("<ShadcnItem", page, StringComparison.Ordinal);
        Assert.Contains("<ShadcnEmpty", page, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
