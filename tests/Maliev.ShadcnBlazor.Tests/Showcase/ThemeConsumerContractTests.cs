using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeConsumerContractTests
{
    [Fact]
    public async Task CheckedConsumerLoadsCanonicalDocumentAndMatchingCss()
    {
        var sample = Path.Combine(FindRoot(), "samples", "Maliev.ShadcnBlazor.ThemeConsumer");
        await using var stream = File.OpenRead(Path.Combine(sample, "wwwroot", "theme.json"));

        var document = await ShadcnThemeDocumentLoader.LoadAsync(stream);
        var css = await File.ReadAllTextAsync(Path.Combine(sample, "wwwroot", "theme.css"));

        Assert.Equal(ShadcnThemeDocument.CurrentSchemaVersion, document.SchemaVersion);
        Assert.Equal(ShadcnThemeCssWriter.Write(document), css);
    }

    [Fact]
    public void CheckedConsumerUsesRuntimeLoaderAndOptionsBackedProvider()
    {
        var sample = Path.Combine(FindRoot(), "samples", "Maliev.ShadcnBlazor.ThemeConsumer");
        var program = File.ReadAllText(Path.Combine(sample, "Program.cs"));
        var layout = File.ReadAllText(Path.Combine(sample, "Layout", "MainLayout.razor"));
        var project = File.ReadAllText(Path.Combine(sample, "Maliev.ShadcnBlazor.ThemeConsumer.csproj"));

        AssertOrdered(
            program,
            "ShadcnThemeDocumentLoader.LoadAsync",
            "AddMalievShadcn(options => options.Theme = themeDocument.Theme)");
        Assert.Contains("<ShadcnThemeProvider", layout, StringComparison.Ordinal);
        Assert.DoesNotContain(" Theme=", layout, StringComparison.Ordinal);
        Assert.Contains("UseMalievShadcnPackage", project, StringComparison.Ordinal);
        Assert.Contains("<ImplicitUsings>enable</ImplicitUsings>", project, StringComparison.Ordinal);
        Assert.Contains("PackageReference Include=\"Maliev.ShadcnBlazor\"", project, StringComparison.Ordinal);
        Assert.Contains("ProjectReference Include=\"..\\..\\src\\Maliev.ShadcnBlazor", project, StringComparison.Ordinal);
    }

    private static void AssertOrdered(string value, params string[] fragments)
    {
        var previous = -1;
        foreach (var fragment in fragments)
        {
            var current = value.IndexOf(fragment, StringComparison.Ordinal);
            Assert.True(current > previous, $"Expected '{fragment}' after index {previous}.");
            previous = current;
        }
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
