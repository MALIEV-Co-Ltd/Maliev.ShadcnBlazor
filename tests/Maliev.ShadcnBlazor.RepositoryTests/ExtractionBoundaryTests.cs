using System.Xml.Linq;

namespace Maliev.ShadcnBlazor.RepositoryTests;

public sealed class ExtractionBoundaryTests
{
    [Fact]
    public void StandaloneSolutionContainsOnlyPublicProjects()
    {
        var root = RepositoryRoot.Find();
        var solution = XDocument.Load(Path.Combine(root, "Maliev.ShadcnBlazor.slnx"));
        var projects = solution.Descendants("Project")
            .Select(element => (string?)element.Attribute("Path"))
            .OfType<string>()
            .ToArray();

        Assert.Equal(
        [
            "samples/Maliev.ShadcnBlazor.Showcase/Maliev.ShadcnBlazor.Showcase.csproj",
            "samples/Maliev.ShadcnBlazor.ThemeConsumer/Maliev.ShadcnBlazor.ThemeConsumer.csproj",
            "src/Maliev.ShadcnBlazor.Build/Maliev.ShadcnBlazor.Build.csproj",
            "src/Maliev.ShadcnBlazor.Icons.Hugeicons/Maliev.ShadcnBlazor.Icons.Hugeicons.csproj",
            "src/Maliev.ShadcnBlazor.Icons.Lucide/Maliev.ShadcnBlazor.Icons.Lucide.csproj",
            "src/Maliev.ShadcnBlazor.Icons.Phosphor/Maliev.ShadcnBlazor.Icons.Phosphor.csproj",
            "src/Maliev.ShadcnBlazor.Icons.Tabler/Maliev.ShadcnBlazor.Icons.Tabler.csproj",
            "src/Maliev.ShadcnBlazor/Maliev.ShadcnBlazor.csproj",
            "tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj",
            "tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj",
            "tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj",
        ],
        projects.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void ProjectReferencesStayInsideThePublicRepository()
    {
        var root = RepositoryRoot.Find();
        var projectFiles = Directory.EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();

        Assert.NotEmpty(projectFiles);
        foreach (var projectFile in projectFiles)
        {
            var document = XDocument.Load(projectFile);
            foreach (var reference in document.Descendants("ProjectReference"))
            {
                var include = (string?)reference.Attribute("Include");
                Assert.False(string.IsNullOrWhiteSpace(include), projectFile);
                var resolved = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectFile)!, include!));
                Assert.StartsWith(root + Path.DirectorySeparatorChar, resolved, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void BrowserTestsDependOnlyOnTheStandaloneSamples()
    {
        var root = RepositoryRoot.Find();
        var project = XDocument.Load(Path.Combine(
            root,
            "tests",
            "Maliev.ShadcnBlazor.BrowserTests",
            "Maliev.ShadcnBlazor.BrowserTests.csproj"));

        var references = project.Descendants("ProjectReference")
            .Select(element => ((string?)element.Attribute("Include"))?.Replace('\\', '/'))
            .Select(include => Path.GetFileName(include))
            .OfType<string>()
            .ToArray();

        Assert.Equal(
            ["Maliev.ShadcnBlazor.Showcase.csproj", "Maliev.ShadcnBlazor.ThemeConsumer.csproj"],
            references.Order(StringComparer.Ordinal));
    }
}

internal static class RepositoryRoot
{
    public static string Find()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var gitPath = Path.Combine(directory.FullName, ".git");
            if (Directory.Exists(gitPath) || File.Exists(gitPath))
                break;

            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
