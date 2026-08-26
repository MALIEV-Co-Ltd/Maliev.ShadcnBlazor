namespace Maliev.ShadcnBlazor.Tests.Contracts;

public sealed class VisualStyleContractTests
{
    [Theory]
    [InlineData("minimal")]
    [InlineData("glass")]
    [InlineData("neo-brutalist")]
    [InlineData("liquid-glass")]
    public void EveryVisualTreatmentOwnsAScopedTokenRule(string treatment)
    {
        var css = ReadStylesheet();

        Assert.Contains($"[data-visual-style=\"{treatment}\"]", css, StringComparison.Ordinal);
        Assert.Contains("--shadcn-style-surface:", css, StringComparison.Ordinal);
    }

    [Fact]
    public void VibrantDarkIsAComposableColorLayer()
    {
        var css = ReadStylesheet();

        Assert.Contains("[data-color-treatment=\"vibrant-dark\"]", css, StringComparison.Ordinal);
        Assert.Contains("--shadcn-style-accent-glow:", css, StringComparison.Ordinal);
    }

    [Fact]
    public void AccessibilityAndPlatformFallbacksAreExplicit()
    {
        var css = ReadStylesheet();

        Assert.Contains("@media (forced-colors: active)", css, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-contrast: more)", css, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", css, StringComparison.Ordinal);
        Assert.Contains("@supports not ((backdrop-filter:", css, StringComparison.Ordinal);
    }

    [Fact]
    public void StyleScopeDoesNotOwnGlobalOrPositionalLayout()
    {
        var css = ReadStylesheet();

        Assert.DoesNotContain(":root", css, StringComparison.Ordinal);
        Assert.DoesNotContain("body", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("position: fixed", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("display: contents", css, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void StyleScopePreservesSwitchPillGeometry()
    {
        var css = ReadStylesheet();

        Assert.DoesNotContain("  .shadcn-switch,", css, StringComparison.Ordinal);
    }

    [Fact]
    public void SpatialGlassOwnsRefractionAndLayeredMaterialTokens()
    {
        var css = ReadStylesheet();

        Assert.Contains("--shadcn-style-environment:", css, StringComparison.Ordinal);
        Assert.Contains("--shadcn-style-specular:", css, StringComparison.Ordinal);
        Assert.Contains("--shadcn-style-lowlight:", css, StringComparison.Ordinal);
        Assert.Contains("--shadcn-style-control-surface:", css, StringComparison.Ordinal);
        Assert.Contains("--shadcn-style-overlay-surface:", css, StringComparison.Ordinal);
        Assert.Contains("--shadcn-style-refraction-filter:", css, StringComparison.Ordinal);
        Assert.Contains("var(--shadcn-style-refraction-filter)", css, StringComparison.Ordinal);
        Assert.Contains("var(--shadcn-style-card-edge)", css, StringComparison.Ordinal);
    }

    [Fact]
    public void SpatialGlassTreatsCardsControlsAndOverlaysAsDistinctMaterials()
    {
        var css = ReadStylesheet();

        Assert.Contains("data-visual-style=\"liquid-glass\"] :where(\n  .shadcn-card", css, StringComparison.Ordinal);
        Assert.Contains("background: var(--shadcn-style-card-material)", css, StringComparison.Ordinal);
        Assert.Contains("background: var(--shadcn-style-control-material)", css, StringComparison.Ordinal);
        Assert.Contains("background: var(--shadcn-style-overlay-material)", css, StringComparison.Ordinal);
        Assert.Contains("--shadcn-style-overlay-shadow:", css, StringComparison.Ordinal);
    }

    [Fact]
    public void SpatialGlassKeepsAOnePixelOpticalEdgeAtStrongIntensity()
    {
        var css = ReadStylesheet();

        Assert.Contains("[data-visual-style=\"liquid-glass\"][data-intensity=\"strong\"]", css, StringComparison.Ordinal);
        Assert.Contains("--shadcn-style-border-width: 1px", css, StringComparison.Ordinal);
    }

    [Fact]
    public void ThemeStudioProvidesAThemeAwareEnvironmentBehindSpatialGlass()
    {
        var root = FindRoot();
        var css = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css"));

        Assert.Contains(".theme-preview-scope .shadcn-visual-style-scope[data-visual-style=\"liquid-glass\"]", css, StringComparison.Ordinal);
        Assert.Contains("--shadcn-style-environment:", css, StringComparison.Ordinal);
    }

    [Fact]
    public void ShowcaseAndGeneratedIntegrationLoadTheStyleScopeAsset()
    {
        var root = FindRoot();
        var index = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "index.html"));
        var templates = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Export", "ThemeBundleTemplates.cs"));

        Assert.Contains("_content/Maliev.ShadcnBlazor/css/shadcn-visual-styles.css", index, StringComparison.Ordinal);
        Assert.Contains("_content/Maliev.ShadcnBlazor/css/shadcn-visual-styles.css", templates, StringComparison.Ordinal);
    }

    private static string ReadStylesheet() => File.ReadAllText(Path.Combine(
        FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-visual-styles.css"));

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
