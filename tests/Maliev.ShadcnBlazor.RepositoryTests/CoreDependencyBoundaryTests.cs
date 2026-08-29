using System.Xml.Linq;

namespace Maliev.ShadcnBlazor.RepositoryTests;

public sealed class CoreDependencyBoundaryTests
{
    [Fact]
    public void CoreProjectAndSourcesDoNotReferenceMudBlazor()
    {
        var root = RepositoryRoot.Find();
        var projectDirectory = Path.Combine(root, "src", "Maliev.ShadcnBlazor");
        var project = XDocument.Load(Path.Combine(projectDirectory, "Maliev.ShadcnBlazor.csproj"));

        Assert.DoesNotContain(
            project.Descendants("PackageReference"),
            reference => string.Equals(reference.Attribute("Include")?.Value, "MudBlazor", StringComparison.OrdinalIgnoreCase));

        var sourceFiles = Directory.EnumerateFiles(projectDirectory, "*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase));

        foreach (var sourceFile in sourceFiles)
            Assert.DoesNotContain("MudBlazor", File.ReadAllText(sourceFile), StringComparison.OrdinalIgnoreCase);
    }
}
