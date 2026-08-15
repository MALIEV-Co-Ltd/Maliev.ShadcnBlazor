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
        Assert.Contains("dotnet add package Maliev.ShadcnBlazor", cut.Markup, StringComparison.Ordinal);
        var usageCode = cut.Find("#usage .component-code code");
        Assert.Contains("@using Maliev.ShadcnBlazor.Components.Actions", usageCode.TextContent, StringComparison.Ordinal);
        Assert.Contains("code-token-keyword", usageCode.InnerHtml, StringComparison.Ordinal);
        Assert.Contains("ShadcnButton", cut.Markup, StringComparison.Ordinal);

        var outline = Services.GetRequiredService<DocumentationPageState>().Sections;
        Assert.Equal(expectedSections, outline.Select(section => section.Id));
        Assert.Contains(cut.FindAll(".component-dossier__pagination a"), link => link.TextContent.Contains("Previous", StringComparison.Ordinal));
        Assert.Contains(cut.FindAll(".component-dossier__pagination a"), link => link.TextContent.Contains("Next", StringComparison.Ordinal));
    }

    [Fact]
    public void CatalogLandingIntroducesTheLibraryAndKeepsEveryComponentDiscoverable()
    {
        var cut = Render<ComponentCatalog>();

        Assert.Contains("Build accessible Blazor interfaces with shadcn primitives", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("dotnet add package Maliev.ShadcnBlazor", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Interactive, typed, and themeable", cut.Markup, StringComparison.Ordinal);
        Assert.Equal(64, cut.FindAll(".documentation-catalog-card").Count);
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
        Assert.Contains("showcase-boot__spinner", index, StringComparison.Ordinal);
        Assert.Contains("place-content: center", css, StringComparison.Ordinal);
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
