namespace Maliev.ShadcnBlazor.RepositoryTests;

public sealed class WorkflowSecurityTests
{
    [Theory]
    [InlineData(".github/workflows/ci.yml")]
    [InlineData(".github/workflows/codeql.yml")]
    [InlineData(".github/workflows/dependency-review.yml")]
    [InlineData(".github/workflows/release.yml")]
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
        Assert.Contains("NuGet/login@", release, StringComparison.Ordinal);
        Assert.Contains("steps.nuget-login.outputs.NUGET_API_KEY", release, StringComparison.Ordinal);
        Assert.DoesNotContain("secrets.NUGET_API_KEY", release, StringComparison.Ordinal);
        Assert.DoesNotContain("--skip-duplicate", release, StringComparison.Ordinal);
    }

    [Fact]
    public void ContinuousIntegrationUsesLockedRestoreAndPublicSurfaceGuard()
    {
        var ci = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), ".github", "workflows", "ci.yml"));

        Assert.Contains("--locked-mode", ci, StringComparison.Ordinal);
        Assert.Contains("Verify-PublicSurface.ps1", ci, StringComparison.Ordinal);
        Assert.Contains("dotnet format", ci, StringComparison.Ordinal);
        Assert.Contains("playwright.ps1 install chromium", ci, StringComparison.Ordinal);
    }
}
