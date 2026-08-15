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
        Assert.Contains("@using Maliev.ShadcnBlazor.Components.Actions", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("ShadcnButton", cut.Markup, StringComparison.Ordinal);

        var outline = Services.GetRequiredService<DocumentationPageState>().Sections;
        Assert.Equal(expectedSections, outline.Select(section => section.Id));
        Assert.Contains(cut.FindAll(".component-dossier__pagination a"), link => link.TextContent.Contains("Previous", StringComparison.Ordinal));
        Assert.Contains(cut.FindAll(".component-dossier__pagination a"), link => link.TextContent.Contains("Next", StringComparison.Ordinal));
    }
}
