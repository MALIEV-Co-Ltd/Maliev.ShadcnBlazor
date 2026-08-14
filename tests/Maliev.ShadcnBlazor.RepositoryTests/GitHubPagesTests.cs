using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Maliev.ShadcnBlazor.RepositoryTests;

public sealed class GitHubPagesTests
{
    [Fact]
    public void ShowcaseInternalLinksRespectThePublishedBasePath()
    {
        var root = RepositoryRoot.Find();
        var showcase = Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase");
        var rootRelativeAttribute = new Regex("(?:href|Href)=\\\"/(?!/)", RegexOptions.CultureInvariant);

        var offenders = Directory
            .EnumerateFiles(showcase, "*.razor", SearchOption.AllDirectories)
            .SelectMany(path => File.ReadLines(path).Select((line, index) => new { path, line, number = index + 1 }))
            .Where(candidate => rootRelativeAttribute.IsMatch(candidate.line))
            .Select(candidate => $"{Path.GetRelativePath(root, candidate.path)}:{candidate.number}")
            .ToArray();

        Assert.Empty(offenders);

        var documentationEntry = File.ReadAllText(Path.Combine(
            showcase,
            "Documentation",
            "ComponentDocumentationEntry.cs"));
        Assert.DoesNotContain("$\"/docs/components/", documentationEntry, StringComparison.Ordinal);
    }

    [Fact]
    public void PreparationTransformsOnlyThePublishedArtifact()
    {
        using var fixture = GitHubPagesFixture.Create();

        var result = fixture.RunPreparation("/Maliev.ShadcnBlazor/");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("<base href=\"/Maliev.ShadcnBlazor/\" />", fixture.Read("index.html"), StringComparison.Ordinal);
        Assert.Equal(fixture.Read("index.html"), fixture.Read("404.html"));
        Assert.True(fixture.Exists(".nojekyll"));
        Assert.Equal(fixture.OriginalSourceIndex, fixture.ReadSourceIndex());
    }

    [Theory]
    [InlineData("Maliev.ShadcnBlazor/")]
    [InlineData("/Maliev.ShadcnBlazor")]
    [InlineData("/")]
    public void PreparationRejectsInvalidRepositoryBasePaths(string basePath)
    {
        using var fixture = GitHubPagesFixture.Create();

        var result = fixture.RunPreparation(basePath);

        Assert.NotEqual(0, result.ExitCode);
        Assert.Equal(fixture.OriginalPublishedIndex, fixture.Read("index.html"));
        Assert.False(fixture.Exists("404.html"));
        Assert.False(fixture.Exists(".nojekyll"));
    }

    private sealed class GitHubPagesFixture : IDisposable
    {
        private readonly string _root;
        private readonly string _sourceIndexPath;

        private GitHubPagesFixture(string root, string sourceIndexPath)
        {
            _root = root;
            _sourceIndexPath = sourceIndexPath;
        }

        public string OriginalPublishedIndex { get; } = "<!doctype html><html><head><base href=\"/\" /></head><body></body></html>";

        public string OriginalSourceIndex { get; } = "source-index-sentinel";

        public static GitHubPagesFixture Create()
        {
            var root = Path.Combine(Path.GetTempPath(), $"maliev-shadcn-pages-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path.Combine(root, "published", "_framework"));
            var sourceIndexPath = Path.Combine(root, "source-index.html");
            var fixture = new GitHubPagesFixture(root, sourceIndexPath);
            File.WriteAllText(Path.Combine(root, "published", "index.html"), fixture.OriginalPublishedIndex);
            File.WriteAllText(sourceIndexPath, fixture.OriginalSourceIndex);
            return fixture;
        }

        public (int ExitCode, string StandardError) RunPreparation(string basePath)
        {
            var script = Path.Combine(RepositoryRoot.Find(), "eng", "Prepare-GitHubPages.ps1");
            var startInfo = new ProcessStartInfo
            {
                FileName = "pwsh",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };
            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-File");
            startInfo.ArgumentList.Add(script);
            startInfo.ArgumentList.Add("-PublishDirectory");
            startInfo.ArgumentList.Add(Path.Combine(_root, "published"));
            startInfo.ArgumentList.Add("-BasePath");
            startInfo.ArgumentList.Add(basePath);

            using var process = Process.Start(startInfo)!;
            var standardError = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return (process.ExitCode, standardError);
        }

        public string Read(string relativePath) => File.ReadAllText(Path.Combine(_root, "published", relativePath));

        public string ReadSourceIndex() => File.ReadAllText(_sourceIndexPath);

        public bool Exists(string relativePath) => File.Exists(Path.Combine(_root, "published", relativePath));

        public void Dispose()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
    }
}
