namespace Maliev.ShadcnBlazor.RepositoryTests;

public sealed class PublicDocumentationTests
{
    private static readonly string[] RequiredFiles =
    [
        "README.md",
        "CONTRIBUTING.md",
        "SECURITY.md",
        "CODE_OF_CONDUCT.md",
        "SUPPORT.md",
        "CHANGELOG.md",
        "docs/getting-started.md",
        "docs/components.md",
        "docs/theming.md",
        "docs/releasing.md",
    ];

    [Fact]
    public void PublicDocumentationSetIsComplete()
    {
        var root = RepositoryRoot.Find();
        foreach (var relative in RequiredFiles)
            Assert.True(File.Exists(Path.Combine(root, relative)), $"Missing public document: {relative}");
    }

    [Fact]
    public void ReadmeDocumentsInstallAssetsAndSupportedRuntime()
    {
        var root = RepositoryRoot.Find();
        var readme = File.ReadAllText(Path.Combine(root, "README.md"));

        Assert.Contains("dotnet add package Maliev.ShadcnBlazor", readme, StringComparison.Ordinal);
        Assert.Contains("AddMalievShadcn", readme, StringComparison.Ordinal);
        Assert.Contains("_content/Maliev.ShadcnBlazor/css/shadcn-base.css", readme, StringComparison.Ordinal);
        Assert.Contains(".NET 10", readme, StringComparison.Ordinal);
        Assert.Contains("MIT License", readme, StringComparison.Ordinal);
    }

    [Fact]
    public void SecurityPolicyUsesPrivateVulnerabilityReporting()
    {
        var root = RepositoryRoot.Find();
        var security = File.ReadAllText(Path.Combine(root, "SECURITY.md"));

        Assert.Contains("private vulnerability reporting", security, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("open a public issue", security, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("supported", security, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RelativeMarkdownLinksResolveInsideTheRepository()
    {
        var root = RepositoryRoot.Find();
        var markdownFiles = Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal));

        foreach (var markdownFile in markdownFiles)
        {
            var markdown = File.ReadAllText(markdownFile);
            foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(markdown, @"\[[^\]]+\]\(([^)]+)\)"))
            {
                var target = match.Groups[1].Value;
                if (target.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                    target.StartsWith('#') ||
                    target.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var pathOnly = target.Split('#', 2)[0].Replace('/', Path.DirectorySeparatorChar);
                var resolved = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(markdownFile)!, pathOnly));
                Assert.True(File.Exists(resolved), $"Broken link '{target}' in {Path.GetRelativePath(root, markdownFile)}.");
            }
        }
    }
}
