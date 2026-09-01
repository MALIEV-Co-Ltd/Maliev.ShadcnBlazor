using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Xml.Linq;

namespace Maliev.ShadcnBlazor.RepositoryTests;

[Collection("Package validation")]
public sealed class IconCatalogRepositoryTests
{
    private static readonly (string Id, string Version, string Commit, string Sha256, string License, int MinimumIcons)[] ExpectedSources =
    [
        ("lucide", "1.33.0", "59978cecf84986af59f1f9f503bcebdc89c6d166", "9ffd3773c606d83a09f4230570026df94e133566ed9bb4528b30e5adb12aa8b0", "ISC", 1700),
        ("tabler", "3.46.0", "8ac7d81b72ece11072ef25ea9fd92e80c6f3c9fc", "a5f369b293d03a02752334a6277322ca95c7129c5f41e458c6929d20384dfc7f", "MIT", 5000),
        ("phosphor", "2.0.8", "d42782b2abe747d904b971ccab48b182a1455f86", "7e8dc880e9100b002099abad14ebc36dc0a3608bf239b1e309bceca36f94bbd1", "MIT", 1200),
        ("hugeicons", "free-3365154", "3365154e0ae2461fbfb6249b89649127207a4f9e", "790393c9760f90f79e1d96bc49ec31138d93345426fef81cda179c4af49705de", "MIT", 4400)
    ];

    [Fact]
    public void SourcesArePinnedToReviewedFreeArchives()
    {
        var root = FindRoot();
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "eng", "icon-sources.json")));
        var sources = document.RootElement.GetProperty("sources").EnumerateArray().ToDictionary(
            source => source.GetProperty("id").GetString()!,
            StringComparer.Ordinal);

        foreach (var expected in ExpectedSources)
        {
            var source = sources[expected.Id];
            Assert.Equal(expected.Version, source.GetProperty("version").GetString());
            Assert.Equal(expected.Commit, source.GetProperty("commit").GetString());
            Assert.Equal(expected.Sha256, source.GetProperty("archiveSha256").GetString());
            Assert.Equal(expected.License, source.GetProperty("licenseExpression").GetString());
            Assert.StartsWith("https://codeload.github.com/", source.GetProperty("archiveUrl").GetString(), StringComparison.Ordinal);
            Assert.DoesNotContain("pro", source.GetRawText(), StringComparison.OrdinalIgnoreCase);
        }

        Assert.Equal(ExpectedSources.Length, sources.Count);
    }

    [Fact]
    public void CheckedInCatalogsContainFullSanitizedFreeSets()
    {
        var root = FindRoot();
        foreach (var expected in ExpectedSources)
        {
            var projectName = expected.Id switch
            {
                "hugeicons" => "Hugeicons",
                _ => char.ToUpperInvariant(expected.Id[0]) + expected.Id[1..]
            };
            var catalogPath = Path.Combine(root, "src", $"Maliev.ShadcnBlazor.Icons.{projectName}", "Catalog", "icons.json");
            using var document = JsonDocument.Parse(File.ReadAllText(catalogPath));
            var icons = document.RootElement.GetProperty("icons").EnumerateArray().ToArray();

            Assert.True(icons.Length >= expected.MinimumIcons, $"{expected.Id} contained only {icons.Length} icons.");
            var names = icons.Select(icon => icon.GetProperty("name").GetString()!).ToArray();
            Assert.Equal(names.Order(StringComparer.Ordinal), names);
            Assert.Equal(names.Length, names.Distinct(StringComparer.Ordinal).Count());

            foreach (var icon in icons)
            {
                Assert.Equal(expected.Id, icon.GetProperty("library").GetString());
                var content = icon.GetProperty("svgContent").GetString()!;
                Assert.DoesNotContain("<script", content, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("foreignObject", content, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(" on", content, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("href", content, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("url(", content, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("http", content, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void ThirdPartyNoticesCreditEveryPinnedCatalogAndFreeLicense()
    {
        var notices = File.ReadAllText(Path.Combine(FindRoot(), "THIRD-PARTY-NOTICES.md"));

        Assert.Contains("lucide-icons/lucide", notices, StringComparison.Ordinal);
        Assert.Contains("tabler/tabler-icons", notices, StringComparison.Ordinal);
        Assert.Contains("phosphor-icons/core", notices, StringComparison.Ordinal);
        Assert.Contains("hugeicons/hugeicons", notices, StringComparison.Ordinal);
        foreach (var expected in ExpectedSources)
        {
            Assert.Contains(expected.Commit, notices, StringComparison.Ordinal);
            Assert.Contains(expected.License, notices, StringComparison.Ordinal);
        }
        Assert.DoesNotContain("Hugeicons Pro", notices, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShowcaseReferencesEverySelectableCatalogWhileCoreReferencesNone()
    {
        var root = FindRoot();
        var showcase = XDocument.Load(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Maliev.ShadcnBlazor.Showcase.csproj"));
        var core = XDocument.Load(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "Maliev.ShadcnBlazor.csproj"));
        var showcaseReferences = showcase.Descendants("ProjectReference").Select(reference => reference.Attribute("Include")?.Value).ToArray();
        var coreReferences = core.Descendants("ProjectReference").Select(reference => reference.Attribute("Include")?.Value).ToArray();

        foreach (var expected in ExpectedSources)
        {
            var suffix = expected.Id == "hugeicons" ? "Hugeicons" : char.ToUpperInvariant(expected.Id[0]) + expected.Id[1..];
            var projectName = $"Maliev.ShadcnBlazor.Icons.{suffix}";
            Assert.Contains(showcaseReferences, reference => reference?.Contains(projectName, StringComparison.Ordinal) == true);
            Assert.DoesNotContain(coreReferences, reference => reference?.Contains(projectName, StringComparison.Ordinal) == true);
        }
    }

    [Fact]
    public void CompanionPackagesContainOnlyCatalogAssemblyMetadataAndLicense()
    {
        var root = FindRoot();
        var output = Path.Combine(Path.GetTempPath(), $"maliev-icon-packages-{Guid.NewGuid():N}");
        Directory.CreateDirectory(output);

        try
        {
            foreach (var expected in ExpectedSources)
            {
                var projectName = expected.Id switch
                {
                    "hugeicons" => "Hugeicons",
                    _ => char.ToUpperInvariant(expected.Id[0]) + expected.Id[1..]
                };
                var packageId = $"Maliev.ShadcnBlazor.Icons.{projectName}";
                var project = Path.Combine(root, "src", packageId, $"{packageId}.csproj");
                var result = Run("dotnet", ["pack", project, "-c", "Release", "-o", output, "-p:NuGetAudit=false"], root);
                Assert.Equal(0, result.ExitCode);
                Assert.DoesNotContain("warning", result.Output, StringComparison.OrdinalIgnoreCase);

                var package = Directory.GetFiles(output, $"{packageId}.*.nupkg", SearchOption.TopDirectoryOnly)
                    .Single(path => !path.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase));
                using var archive = ZipFile.OpenRead(package);
                var entries = archive.Entries.Select(entry => entry.FullName).ToArray();
                Assert.Contains($"lib/net10.0/{packageId}.dll", entries);
                Assert.Contains($"lib/net10.0/{packageId}.xml", entries);
                Assert.Contains("README.md", entries);
                Assert.Contains(entries, entry => entry.StartsWith("licenses/", StringComparison.Ordinal));
                Assert.DoesNotContain(entries, entry => entry.Contains("icons.json", StringComparison.OrdinalIgnoreCase));
                Assert.DoesNotContain(entries, entry => entry.Contains("/bin/", StringComparison.OrdinalIgnoreCase));
                Assert.DoesNotContain(entries, entry => entry.Contains("/obj/", StringComparison.OrdinalIgnoreCase));

                var nuspecEntry = archive.Entries.Single(entry => entry.FullName == $"{packageId}.nuspec");
                using var nuspecStream = nuspecEntry.Open();
                var nuspec = XDocument.Load(nuspecStream);
                XNamespace ns = "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd";
                var dependencies = nuspec.Descendants(ns + "dependency").ToArray();
                Assert.Single(dependencies);
                Assert.Equal("Maliev.ShadcnBlazor", dependencies[0].Attribute("id")?.Value);
                var metadata = nuspec.Root!.Element(ns + "metadata")!;
                Assert.Equal(packageId, metadata.Element(ns + "id")?.Value);
                Assert.Equal("2.1.4", metadata.Element(ns + "version")?.Value);
                Assert.Equal("README.md", metadata.Element(ns + "readme")?.Value);
                Assert.Equal(expected.Id == "lucide" ? "MIT AND ISC" : "MIT", metadata.Element(ns + "license")?.Value);
            }
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }

    private static (int ExitCode, string Output) Run(string fileName, IReadOnlyList<string> arguments, string workingDirectory)
    {
        var start = new ProcessStartInfo(fileName)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory
        };
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);

        using var process = Process.Start(start)!;
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        return (process.ExitCode, standardOutput.GetAwaiter().GetResult() + standardError.GetAwaiter().GetResult());
    }
}
