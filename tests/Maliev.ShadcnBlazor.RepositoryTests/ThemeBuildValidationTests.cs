using System.Diagnostics;
using System.Text;

namespace Maliev.ShadcnBlazor.RepositoryTests;

[Collection("Package validation")]
public sealed class ThemeBuildValidationTests : IClassFixture<ThemePackageFixture>
{
    private readonly ThemePackageFixture _package;

    public ThemeBuildValidationTests(ThemePackageFixture package) => _package = package;

    [Fact]
    public void ValidPortableThemeBuildsFromTheLocalPackageWithoutNetworkDuringCompilation()
    {
        var theme = File.ReadAllBytes(Path.Combine(_package.Root, "tests", "Maliev.ShadcnBlazor.RepositoryTests",
            "Fixtures", "Themes", "valid-theme.json"));

        var result = BuildConsumer(theme, warningsAsErrors: false);

        Assert.True(result.ExitCode == 0, result.Output);
        Assert.DoesNotContain("MSHCN", result.Output, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(InvalidDocuments))]
    public void InvalidAndSuspiciousThemesProduceStableMappedDiagnostics(
        string json,
        bool warningsAsErrors,
        string code,
        string path,
        int line,
        int column)
    {
        var result = BuildConsumer(new UTF8Encoding(false).GetBytes(json), warningsAsErrors);

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains($"theme.json({line},{column}): error {code}:", result.Output, StringComparison.Ordinal);
        Assert.Contains($"[{path}]", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void OversizedAndInvalidUtf8ThemesFailBeforeJsonValidation()
    {
        var oversized = BuildConsumer(new byte[1_048_577], warningsAsErrors: false);
        var invalidUtf8 = BuildConsumer([0xff, 0xfe, 0xfd], warningsAsErrors: false);

        Assert.NotEqual(0, oversized.ExitCode);
        Assert.Contains("theme.json(1,1): error MSHCN001:", oversized.Output, StringComparison.Ordinal);
        Assert.Contains("[$]", oversized.Output, StringComparison.Ordinal);
        Assert.NotEqual(0, invalidUtf8.ExitCode);
        Assert.Contains("theme.json(1,1): error MSHCN001:", invalidUtf8.Output, StringComparison.Ordinal);
        Assert.Contains("[$]", invalidUtf8.Output, StringComparison.Ordinal);
    }

    public static TheoryData<string, bool, string, string, int, int> InvalidDocuments => new()
    {
        {
            """
            {
              "schemaVersion": 2,
              "name": "Broken",
            }
            """, false, "MSHCN001", "$", 4, 1
        },
        {
            """
            {
              "schemaVersion": 99
            }
            """, false, "MSHCN002", "schemaVersion", 2, 3
        },
        {
            ReadFixture("invalid-token.json"), false, "MSHCN003", "theme.light.primary", 7, 5
        },
        {
            """
            {
              "schemaVersion": 2,
              "name": "Broken",
              "theme": {
                "schemaVersion": 1,
                "name": "Broken",
                "light": {
                  "background": "oklch(1 0 0)",
                  "primary": "url(https://example.invalid/theme)"
                },
                "dark": {},
                "metrics": {}
              },
              "application": {},
              "palette": {},
              "typography": {}
            }
            """, false, "MSHCN004", "theme.light.primary", 9, 7
        },
        {
            ReadFixture("contrast-warning.json"), true, "MSHCN101", "theme.light.foreground", 9, 7
        },
        {
            """
            {
              "schemaVersion": 2,
              "name": "Remote font",
              "theme": {},
              "application": {},
              "palette": {},
              "typography": {
                "body": {
                  "family": "Inter, sans-serif",
                  "fallback": "sans-serif",
                  "googleFontsId": "inter"
                }
              }
            }
            """, true, "MSHCN102", "typography.body.googleFontsId", 11, 7
        },
        {
            """
            {
              "schemaVersion": 1,
              "theme": {}
            }
            """, true, "MSHCN103", "schemaVersion", 2, 3
        }
    };

    private static string ReadFixture(string name) => File.ReadAllText(Path.Combine(
        FindRoot(), "tests", "Maliev.ShadcnBlazor.RepositoryTests", "Fixtures", "Themes", name));

    internal static string FindRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Maliev.ShadcnBlazor.slnx")))
                return current.FullName;
            current = current.Parent;
        }
        throw new InvalidOperationException("Repository root was not found.");
    }

    private (int ExitCode, string Output) BuildConsumer(byte[] theme, bool warningsAsErrors)
    {
        var directory = Path.Combine(Path.GetTempPath(), $"maliev-shadcn-theme-consumer-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(directory, "wwwroot"));
        try
        {
            File.WriteAllText(Path.Combine(directory, "Consumer.csproj"), $$"""
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
                    <MalievShadcnThemeWarningsAsErrors>{{warningsAsErrors.ToString().ToLowerInvariant()}}</MalievShadcnThemeWarningsAsErrors>
                  </PropertyGroup>
                  <ItemGroup>
                    <PackageReference Include="Maliev.ShadcnBlazor" Version="1.1.1" />
                  </ItemGroup>
                </Project>
                """, new UTF8Encoding(false));
            File.WriteAllBytes(Path.Combine(directory, "wwwroot", "theme.json"), theme);
            File.WriteAllText(Path.Combine(directory, "NuGet.config"), $$"""
                <?xml version="1.0" encoding="utf-8"?>
                <configuration>
                  <config>
                    <add key="globalPackagesFolder" value="{{Path.Combine(directory, ".packages")}}" />
                  </config>
                  <packageSources>
                    <clear />
                    <add key="local" value="{{_package.Output}}" />
                    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
                  </packageSources>
                </configuration>
                """, new UTF8Encoding(false));

            var restore = Run("dotnet", ["restore", "Consumer.csproj", "--configfile", "NuGet.config",
                "-p:NuGetAudit=false"], directory);
            Assert.True(restore.ExitCode == 0, restore.Output);
            return Run("dotnet", ["build", "Consumer.csproj", "-c", "Release", "--no-restore",
                "-p:NuGetAudit=false"], directory);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    internal static (int ExitCode, string Output) Run(
        string fileName,
        IReadOnlyList<string> arguments,
        string workingDirectory)
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

public sealed class ThemePackageFixture : IDisposable
{
    public ThemePackageFixture()
    {
        Root = ThemeBuildValidationTests.FindRoot();
        Output = Path.Combine(Path.GetTempPath(), $"maliev-shadcn-theme-package-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Output);
        var project = Path.Combine(Root, "src", "Maliev.ShadcnBlazor", "Maliev.ShadcnBlazor.csproj");
        var result = ThemeBuildValidationTests.Run("dotnet",
            ["pack", project, "-c", "Release", "-o", Output, "-p:NuGetAudit=false"], Root);
        Assert.True(result.ExitCode == 0, result.Output);
        Assert.DoesNotContain("warning", result.Output, StringComparison.OrdinalIgnoreCase);
    }

    public string Root { get; }
    public string Output { get; }

    public void Dispose() => Directory.Delete(Output, recursive: true);

}

[CollectionDefinition("Package validation", DisableParallelization = true)]
public sealed class PackageValidationCollection
{
}
