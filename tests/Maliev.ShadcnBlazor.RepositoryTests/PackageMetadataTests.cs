using System.Xml.Linq;

namespace Maliev.ShadcnBlazor.RepositoryTests;

public sealed class PackageMetadataTests
{
    private const string ReleaseVersion = "2.1.4";
    private const string PreviousReleaseVersion = "2.1.3";

    [Fact]
    public void PackageMetadataIsReadyForPublicNuGetDistribution()
    {
        var root = RepositoryRoot.Find();
        var project = XDocument.Load(Path.Combine(
            root,
            "src",
            "Maliev.ShadcnBlazor",
            "Maliev.ShadcnBlazor.csproj"));

        string Property(string name) => project.Descendants(name).Single().Value;

        Assert.Equal("Maliev.ShadcnBlazor", Property("PackageId"));
        Assert.Equal(ReleaseVersion, Property("VersionPrefix"));
        Assert.Equal("MALIEV Co., Ltd.", Property("Authors"));
        Assert.Equal("MIT", Property("PackageLicenseExpression"));
        Assert.Equal("README.md", Property("PackageReadmeFile"));
        Assert.Equal("https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor", Property("PackageProjectUrl"));
        Assert.Equal("https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor", Property("RepositoryUrl"));
        Assert.Equal("git", Property("RepositoryType"));
        Assert.Equal("true", Property("PublishRepositoryUrl"));
        Assert.Equal("true", Property("EmbedUntrackedSources"));
        Assert.Equal("true", Property("IncludeSymbols"));
        Assert.Equal("snupkg", Property("SymbolPackageFormat"));
        Assert.Equal("true", Property("EnablePackageValidation"));
        Assert.Contains("blazor", Property("PackageTags"), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("shadcn", Property("PackageTags"), StringComparison.OrdinalIgnoreCase);

        var sourceLink = project.Descendants("PackageReference")
            .Single(element => string.Equals(
                (string?)element.Attribute("Include"),
                "Microsoft.SourceLink.GitHub",
                StringComparison.Ordinal));
        Assert.Equal("all", (string?)sourceLink.Attribute("PrivateAssets"));
    }

    [Fact]
    public void ReleaseVersionIsConsistentAcrossPublicDocumentationAndConsumers()
    {
        var root = RepositoryRoot.Find();
        var installCommand = $"dotnet add package Maliev.ShadcnBlazor --version {ReleaseVersion}";

        Assert.Contains(installCommand, File.ReadAllText(Path.Combine(root, "README.md")), StringComparison.Ordinal);
        Assert.Contains(
            installCommand,
            File.ReadAllText(Path.Combine(root, "docs", "getting-started.md")),
            StringComparison.Ordinal);

        var index = File.ReadAllText(Path.Combine(
            root,
            "samples",
            "Maliev.ShadcnBlazor.Showcase",
            "wwwroot",
            "index.html"));
        Assert.Contains($"shadcn-base.css?v={ReleaseVersion}", index, StringComparison.Ordinal);
        Assert.Contains($"showcase.css?v={ReleaseVersion}", index, StringComparison.Ordinal);

        var consumer = File.ReadAllText(Path.Combine(
            root,
            "samples",
            "Maliev.ShadcnBlazor.ThemeConsumer",
            "Maliev.ShadcnBlazor.ThemeConsumer.csproj"));
        Assert.Contains($">{ReleaseVersion}</MalievShadcnPackageVersion>", consumer, StringComparison.Ordinal);

        var changelog = File.ReadAllText(Path.Combine(root, "CHANGELOG.md"));
        Assert.Contains($"## [{ReleaseVersion}]", changelog, StringComparison.Ordinal);
        Assert.Contains($"compare/v{ReleaseVersion}...HEAD", changelog, StringComparison.Ordinal);
        Assert.Contains($"compare/v{PreviousReleaseVersion}...v{ReleaseVersion}", changelog, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("LICENSE")]
    [InlineData("README.md")]
    [InlineData("THIRD-PARTY-NOTICES.md")]
    public void PublicPackageDocumentsExistAtRepositoryRoot(string fileName)
    {
        var root = RepositoryRoot.Find();
        Assert.True(File.Exists(Path.Combine(root, fileName)), $"Missing {fileName}.");
    }

    [Fact]
    public void ShippedStylesDoNotContainSampleOnlySelectors()
    {
        var root = RepositoryRoot.Find();
        var cssRoot = Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "css");
        var sampleOnlySelectors = new[] { ".showcase-", ".documentation-page" };
        var leakedFiles = Directory.EnumerateFiles(cssRoot, "*.css", SearchOption.AllDirectories)
            .Where(file => sampleOnlySelectors.Any(selector => File.ReadAllText(file).Contains(selector, StringComparison.Ordinal)))
            .Select(file => Path.GetRelativePath(root, file))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            leakedFiles.Length == 0,
            $"Sample-only selectors must stay in the sample app, not the package: {string.Join(", ", leakedFiles)}");
    }
}
