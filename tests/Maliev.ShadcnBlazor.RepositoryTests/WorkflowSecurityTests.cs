using System.Text.Json;

namespace Maliev.ShadcnBlazor.RepositoryTests;

public sealed class WorkflowSecurityTests
{
    [Theory]
    [InlineData(".github/workflows/ci.yml")]
    [InlineData(".github/workflows/codeql.yml")]
    [InlineData(".github/workflows/dependency-review.yml")]
    [InlineData(".github/workflows/release.yml")]
    [InlineData(".github/workflows/pages.yml")]
    [InlineData(".github/workflows/visual-proof.yml")]
    [InlineData(".github/dependabot.yml")]
    public void RequiredAutomationFilesExist(string relativePath)
    {
        Assert.True(File.Exists(Path.Combine(RepositoryRoot.Find(), relativePath)), $"Missing {relativePath}.");
    }

    [Fact]
    public void WorkflowActionsArePinnedToCommitShas()
    {
        var root = RepositoryRoot.Find();
        foreach (var workflow in Directory.EnumerateFiles(Path.Combine(root, ".github", "workflows"), "*.yml"))
        {
            foreach (var line in File.ReadLines(workflow).Where(line => line.TrimStart().StartsWith("uses:", StringComparison.Ordinal)))
            {
                var reference = line[(line.IndexOf('@') + 1)..].Trim();
                Assert.Matches("^[0-9a-f]{40}(?:\\s+#.*)?$", reference);
            }
        }
    }

    [Fact]
    public void ReleaseUsesOidcTrustedPublishingAndNoApiKeySecret()
    {
        var release = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), ".github", "workflows", "release.yml"));

        Assert.Contains("release:", release, StringComparison.Ordinal);
        Assert.Contains("types: [published]", release, StringComparison.Ordinal);
        Assert.Contains("id-token: write", release, StringComparison.Ordinal);
        Assert.Contains("environment: nuget", release, StringComparison.Ordinal);
        Assert.Contains("runs-on: windows-2022", release, StringComparison.Ordinal);
        Assert.Contains("timeout-minutes: 90", release, StringComparison.Ordinal);
        Assert.Contains("NuGet/login@", release, StringComparison.Ordinal);
        Assert.Contains("steps.nuget-login.outputs.NUGET_API_KEY", release, StringComparison.Ordinal);
        Assert.DoesNotContain("secrets.NUGET_API_KEY", release, StringComparison.Ordinal);
        Assert.DoesNotContain("--skip-duplicate", release, StringComparison.Ordinal);
    }

    [Fact]
    public void ReleasePublishesCoreAndEveryCompanionIconPackage()
    {
        var release = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), ".github", "workflows", "release.yml"));
        var packageIds = new[]
        {
            "Maliev.ShadcnBlazor",
            "Maliev.ShadcnBlazor.Icons.Lucide",
            "Maliev.ShadcnBlazor.Icons.Tabler",
            "Maliev.ShadcnBlazor.Icons.Phosphor",
            "Maliev.ShadcnBlazor.Icons.Hugeicons"
        };

        foreach (var packageId in packageIds)
        {
            Assert.Contains($"artifacts/package/{packageId}.${{{{ steps.version.outputs.version }}}}.nupkg", release, StringComparison.Ordinal);
            Assert.Contains($"artifacts/package/{packageId}.${{{{ steps.version.outputs.version }}}}.snupkg", release, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ContinuousIntegrationUsesLockedRestoreAndPublicSurfaceGuard()
    {
        var ci = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), ".github", "workflows", "ci.yml"));

        Assert.Contains("dotnet workload restore", ci, StringComparison.Ordinal);
        Assert.Contains("runs-on: windows-2022", ci, StringComparison.Ordinal);
        Assert.Contains("--locked-mode", ci, StringComparison.Ordinal);
        Assert.Contains("Verify-PublicSurface.ps1", ci, StringComparison.Ordinal);
        Assert.Contains("dotnet format", ci, StringComparison.Ordinal);
        Assert.Contains("playwright.ps1 install chromium", ci, StringComparison.Ordinal);
        Assert.Contains("browser:\n    name: Browser tests\n    runs-on: windows-2022\n    timeout-minutes: 60", ci.ReplaceLineEndings("\n"), StringComparison.Ordinal);
    }

    [Fact]
    public void PagesDeploymentUsesScopedOidcPermissionsAndCannotPublishPackages()
    {
        var pages = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), ".github", "workflows", "pages.yml"));

        Assert.Contains("release:\n    types: [published]", pages.ReplaceLineEndings("\n"), StringComparison.Ordinal);
        Assert.Contains("ref: ${{ github.event_name == 'release' && github.event.release.tag_name || github.ref }}", pages, StringComparison.Ordinal);
        Assert.Contains("permissions:\n  contents: read", pages.ReplaceLineEndings("\n"), StringComparison.Ordinal);
        Assert.Contains("pages: write", pages, StringComparison.Ordinal);
        Assert.Contains("id-token: write", pages, StringComparison.Ordinal);
        Assert.Contains("environment:\n      name: github-pages", pages.ReplaceLineEndings("\n"), StringComparison.Ordinal);
        Assert.Contains("actions/upload-pages-artifact@", pages, StringComparison.Ordinal);
        Assert.Contains("actions/deploy-pages@", pages, StringComparison.Ordinal);
        Assert.DoesNotContain("NuGet/login", pages, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("dotnet nuget push", pages, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secrets.", pages, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void VisualProofWorkflowIsReadOnlyAndAlwaysUploadsDiagnostics()
    {
        var workflow = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), ".github", "workflows", "visual-proof.yml"));

        Assert.Contains("ComponentCatalogVisualProofTests", workflow, StringComparison.Ordinal);
        Assert.Contains("timeout-minutes: 30", workflow, StringComparison.Ordinal);
        Assert.Contains("if: always()", workflow, StringComparison.Ordinal);
        Assert.Contains("artifacts/visual-proof", workflow, StringComparison.Ordinal);
        Assert.Contains("update-baselines:", workflow, StringComparison.Ordinal);
        Assert.Contains("default: false", workflow, StringComparison.Ordinal);
        Assert.Contains("SHADCN_UPDATE_VISUAL_BASELINES: ${{ inputs.update-baselines && '1' || '0' }}", workflow, StringComparison.Ordinal);
        Assert.Contains("docs/evidence/component-catalog-baselines", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("SHADCN_UPDATE_VISUAL_BASELINES: 1", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("contents: write", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("secrets.", workflow, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DotNetSdkAndWorkloadVersionsArePinnedTogether()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "global.json")));
        var sdk = document.RootElement.GetProperty("sdk");

        Assert.Equal("10.0.111", sdk.GetProperty("version").GetString());
        Assert.Equal("10.0.111", sdk.GetProperty("workloadVersion").GetString());
        Assert.Equal("disable", sdk.GetProperty("rollForward").GetString());
    }

    [Theory]
    [InlineData(".github/workflows/ci.yml")]
    [InlineData(".github/workflows/codeql.yml")]
    [InlineData(".github/workflows/release.yml")]
    [InlineData(".github/workflows/pages.yml")]
    [InlineData(".github/workflows/visual-proof.yml")]
    public void DotNetWorkflowsInstallPinnedWorkloadManifests(string relativePath)
    {
        var workflow = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), relativePath));

        Assert.Contains("dotnet workload restore Maliev.ShadcnBlazor.slnx", workflow, StringComparison.Ordinal);
    }
}
