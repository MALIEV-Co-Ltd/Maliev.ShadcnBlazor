using Bunit;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Api;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;
using Maliev.ShadcnBlazor.Showcase.Pages.Docs;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class DocumentationRouteTests : BunitContext
{
    public DocumentationRouteTests()
    {
        var catalog = new ComponentDocumentationCatalog();
        Services.AddSingleton<IComponentDocumentationCatalog>(catalog);
        Services.AddSingleton<ComponentApiCatalog>();
        Services.AddSingleton<IComponentExampleRegistry>(new ComponentExampleRegistry(catalog));
        Services.AddScoped<DocumentationPageState>();
    }

    [Fact]
    public void UnknownSlug_RendersShadcnEmptyWithCatalogLink()
    {
        var cut = Render<ComponentDocumentation>(parameters => parameters.Add(component => component.Slug, "missing-component"));

        Assert.Equal("empty", cut.Find("[data-slot='empty']").GetAttribute("data-slot"));
        Assert.Equal("docs/components", cut.Find("[data-slot='empty-content'] a").GetAttribute("href"));
    }

    [Fact]
    public void PlanSixCertifiedSlug_RendersReviewablePreviewWithCompleteEvidenceGates()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
        var cut = Render<ComponentDocumentation>(parameters => parameters.Add(component => component.Slug, "accordion"));

        Assert.Contains("ShadcnAccordion", cut.Markup, StringComparison.Ordinal);
        Assert.NotEmpty(cut.FindAll("[data-slot='accordion']"));
        Assert.Equal(7, cut.FindAll("[data-testid='evidence-row']").Count);
        Assert.All(cut.FindAll("[data-testid='evidence-row']"), row => Assert.Equal("true", row.GetAttribute("data-complete")));
    }

    [Fact]
    public void CompleteSlug_RendersTheAuthoritativeCertificationEvidenceMatrix()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
        var cut = Render<ComponentDocumentation>(parameters => parameters.Add(component => component.Slug, "aspect-ratio"));

        var rows = cut.FindAll("[data-testid='evidence-row']");
        Assert.Equal(7, rows.Count);
        Assert.Equal(6, rows.Count(row => row.GetAttribute("data-complete") == "true"));
        Assert.Equal("false", cut.Find("[data-evidence='integration']").GetAttribute("data-complete"));
    }

    [Fact]
    public void CompleteSlug_RendersAConsumptionArticleAndPublishesItsOutline()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();

        var cut = Render<ComponentDocumentation>(parameters => parameters.Add(component => component.Slug, "button"));
        var expectedSections = new[]
        {
            "overview", "preview", "installation", "usage", "composition", "accessibility",
            "api-reference", "theming", "evidence", "references"
        };

        Assert.All(expectedSections, id => Assert.Single(cut.FindAll($"#{id}")));
        var installationCode = cut.Find("#installation .component-code code");
        Assert.Contains("dotnet add package Maliev.ShadcnBlazor", installationCode.TextContent, StringComparison.Ordinal);
        var usageCode = cut.Find("#usage .component-code code");
        Assert.Contains("@using Maliev.ShadcnBlazor.Components.Actions", usageCode.TextContent, StringComparison.Ordinal);
        Assert.Contains("shadcn-code-token-directive", usageCode.InnerHtml, StringComparison.Ordinal);
        Assert.Contains("ShadcnButton", cut.Markup, StringComparison.Ordinal);

        var outline = Services.GetRequiredService<DocumentationPageState>().Sections;
        Assert.Equal(expectedSections, outline.Select(section => section.Id));
        Assert.Contains(cut.FindAll(".component-dossier__pagination a"), link => link.TextContent.Contains("Previous", StringComparison.Ordinal));
        Assert.Contains(cut.FindAll(".component-dossier__pagination a"), link => link.TextContent.Contains("Next", StringComparison.Ordinal));
    }

    [Fact]
    public void DocsOverviewProvidesATaskOrientedFiveMinuteQuickstart()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();

        var cut = Render<DocsOverview>();

        Assert.Contains("Get a themed Blazor interface running in five minutes", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Prerequisites", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("dotnet add package Maliev.ShadcnBlazor", cut.Find("#install").TextContent, StringComparison.Ordinal);
        Assert.Contains("AddMalievShadcn", cut.Find("#register").TextContent, StringComparison.Ordinal);
        Assert.Contains("ShadcnThemeProvider", cut.Find("#compose").TextContent, StringComparison.Ordinal);
        Assert.Contains("ShadcnButton", cut.Find("#compose").TextContent, StringComparison.Ordinal);
        Assert.Contains("Core concepts", cut.Markup, StringComparison.Ordinal);
        var agentic = cut.Find("[data-testid='agentic-integration-showcase']");
        Assert.Contains("Agentic development with Maliev", agentic.TextContent, StringComparison.Ordinal);
        Assert.Contains("AGENTS.md", agentic.TextContent, StringComparison.Ordinal);
        Assert.Contains("maliev-shadcnblazor", agentic.TextContent, StringComparison.Ordinal);
        Assert.Equal("fallback", agentic.GetAttribute("data-loop-active"));
        Assert.Equal(3, agentic.QuerySelectorAll("[data-agentic-scene]").Length);
        Assert.True(agentic.QuerySelectorAll("[data-slot='bubble']").Length >= 6);
        Assert.True(agentic.QuerySelectorAll("[data-slot='marker']").Length >= 3);
        Assert.Empty(agentic.QuerySelectorAll("a, button, input, select, textarea, [tabindex]"));
        Assert.Contains("Troubleshooting", cut.Markup, StringComparison.Ordinal);
        var contribute = cut.Find("#contribute");
        Assert.Contains("Get help and contribute", contribute.TextContent, StringComparison.Ordinal);
        var contributionLinks = contribute.QuerySelectorAll("a");
        Assert.Contains(contributionLinks, link => link.GetAttribute("href") == "https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/issues/new?template=bug_report.yml");
        Assert.Contains(contributionLinks, link => link.GetAttribute("href") == "https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/issues/new?template=feature_request.yml");
        Assert.Contains(contributionLinks, link => link.GetAttribute("href") == "https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/discussions");
        Assert.Contains(contributionLinks, link => link.GetAttribute("href") == "https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/blob/main/CONTRIBUTING.md");
        Assert.Contains(contributionLinks, link => link.GetAttribute("href") == "https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/security/advisories/new");
        Assert.Contains(cut.FindAll("a"), link => link.GetAttribute("href") == "docs/components");
        Assert.Contains(cut.FindAll("a"), link => link.GetAttribute("href") == "theme");
    }

    [Fact]
    public void AgenticShowcaseLoopIsVisibilityAwareAndMotionSafe()
    {
        var root = FindRoot();
        var css = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css"));
        var scriptPath = Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "js", "agentic-showcase.js");

        Assert.True(File.Exists(scriptPath), "The agentic showcase visibility observer is missing.");
        var script = File.ReadAllText(scriptPath);
        Assert.Contains("IntersectionObserver", script, StringComparison.Ordinal);
        Assert.Contains("visibilitychange", script, StringComparison.Ordinal);
        Assert.Contains("data-loop-active", script, StringComparison.Ordinal);
        Assert.Contains(".agentic-showcase", css, StringComparison.Ordinal);
        Assert.Contains("pointer-events: none", css, StringComparison.Ordinal);
        Assert.Contains("cursor: default", css, StringComparison.Ordinal);
        Assert.Contains("@keyframes agentic-message-cycle", css, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", css, StringComparison.Ordinal);
    }

    [Fact]
    public void CatalogLandingIntroducesTheLibraryAndUsesAProgressiveDirectory()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
        var cut = Render<ComponentCatalog>();

        Assert.Contains("Build accessible Blazor interfaces with shadcn primitives", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("dotnet add package Maliev.ShadcnBlazor", cut.Find(".documentation-landing__installation").TextContent, StringComparison.Ordinal);
        Assert.Contains("Interactive, typed, and themeable", cut.Markup, StringComparison.Ordinal);
        Assert.Single(cut.FindAll("[data-testid='component-directory-search']"));
        Assert.NotEmpty(cut.FindAll(".documentation-directory-group"));
        Assert.Equal(69, cut.FindAll(".documentation-directory-link").Count);
        Assert.Empty(cut.FindAll(".documentation-catalog-card"));
        Assert.Equal("docs/components#component-directory", cut.Find(".documentation-landing__actions a").GetAttribute("href"));
        Assert.Contains(cut.FindAll(".documentation-landing__actions a"), link => link.GetAttribute("href") == "theme");
    }

    [Fact]
    public void ShowcaseBootShellIsCenteredAccessibleAndMotionAware()
    {
        var root = FindRoot();
        var index = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "index.html"));
        var css = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css"));

        Assert.Contains("class=\"showcase-boot\"", index, StringComparison.Ordinal);
        Assert.Contains("role=\"status\"", index, StringComparison.Ordinal);
        Assert.Contains("class=\"showcase-boot__logo\"", index, StringComparison.Ordinal);
        Assert.Contains("src=\"images/brand/MALIEV_BLACK.svg\"", index, StringComparison.Ordinal);
        Assert.DoesNotContain("class=\"showcase-boot__mark\"", index, StringComparison.Ordinal);
        Assert.Contains("showcase-boot__spinner", index, StringComparison.Ordinal);
        Assert.Contains("place-content: center", css, StringComparison.Ordinal);
        Assert.Contains(".showcase-boot__logo", css, StringComparison.Ordinal);
        Assert.Contains("@keyframes showcase-boot-spin", css, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", css, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
