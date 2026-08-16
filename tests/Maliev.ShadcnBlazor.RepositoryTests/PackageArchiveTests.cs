using System.Diagnostics;
using System.IO.Compression;
using System.Xml.Linq;

namespace Maliev.ShadcnBlazor.RepositoryTests;

public sealed class PackageArchiveTests
{
    [Fact]
    public void ReleasePackageContainsOnlyTheExpectedPublicDistributionAssets()
    {
        var root = RepositoryRoot.Find();
        var output = Path.Combine(Path.GetTempPath(), $"maliev-shadcn-package-{Guid.NewGuid():N}");
        Directory.CreateDirectory(output);

        try
        {
            var project = Path.Combine(root, "src", "Maliev.ShadcnBlazor", "Maliev.ShadcnBlazor.csproj");
            var result = Run(
                "dotnet",
                ["pack", project, "-c", "Release", "-o", output, "-p:NuGetAudit=false"],
                root);
            Assert.True(result.ExitCode == 0, result.Output);
            Assert.DoesNotContain("warning", result.Output, StringComparison.OrdinalIgnoreCase);

            var package = Path.Combine(output, "Maliev.ShadcnBlazor.1.0.3.nupkg");
            var symbols = Path.Combine(output, "Maliev.ShadcnBlazor.1.0.3.snupkg");
            Assert.True(File.Exists(package), result.Output);
            Assert.True(File.Exists(symbols), result.Output);

            using var archive = ZipFile.OpenRead(package);
            var entries = archive.Entries.Select(entry => entry.FullName).ToHashSet(StringComparer.Ordinal);
            Assert.Contains("LICENSE", entries);
            Assert.Contains("README.md", entries);
            Assert.Contains("THIRD-PARTY-NOTICES.md", entries);
            Assert.Contains("licenses/MudBlazor-LICENSE.txt", entries);
            Assert.Contains("licenses/shadcn-ui-LICENSE.md", entries);
            Assert.Contains("lib/net10.0/Maliev.ShadcnBlazor.dll", entries);
            Assert.Contains("staticwebassets/css/shadcn-base.css", entries);
            Assert.Contains("staticwebassets/js/shadcn-selection.js", entries);
            Assert.DoesNotContain(entries, entry => entry.Contains("/bin/", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(entries, entry => entry.Contains("/obj/", StringComparison.OrdinalIgnoreCase));

            var nuspecEntry = archive.Entries.Single(entry => entry.FullName == "Maliev.ShadcnBlazor.nuspec");
            using var nuspecStream = nuspecEntry.Open();
            var nuspec = XDocument.Load(nuspecStream);
            XNamespace ns = "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd";
            var metadata = nuspec.Root!.Element(ns + "metadata")!;
            Assert.Equal("Maliev.ShadcnBlazor", metadata.Element(ns + "id")!.Value);
            Assert.Equal("1.0.3", metadata.Element(ns + "version")!.Value);
            Assert.Equal("MIT", metadata.Element(ns + "license")!.Value);
            Assert.Equal("README.md", metadata.Element(ns + "readme")!.Value);
            Assert.Equal(
                "https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor",
                metadata.Element(ns + "repository")!.Attribute("url")!.Value);

            var guard = Run(
                "pwsh",
                ["-NoProfile", "-File", Path.Combine(root, "eng", "Verify-PublicSurface.ps1"), "-Root", root, "-Package", package],
                root);
            Assert.True(guard.ExitCode == 0, guard.Output);
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    private static (int ExitCode, string Output) Run(string fileName, IReadOnlyList<string> arguments, string workingDirectory)
    {
        var start = new ProcessStartInfo(fileName)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory,
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
