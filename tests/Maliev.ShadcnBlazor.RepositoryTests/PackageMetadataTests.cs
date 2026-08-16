using System.Xml.Linq;

namespace Maliev.ShadcnBlazor.RepositoryTests;

public sealed class PackageMetadataTests
{
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
        Assert.Equal("1.0.5", Property("VersionPrefix"));
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

    [Theory]
    [InlineData("LICENSE")]
    [InlineData("README.md")]
    [InlineData("THIRD-PARTY-NOTICES.md")]
    public void PublicPackageDocumentsExistAtRepositoryRoot(string fileName)
    {
        var root = RepositoryRoot.Find();
        Assert.True(File.Exists(Path.Combine(root, fileName)), $"Missing {fileName}.");
    }
}
