using Bunit;
using Maliev.ShadcnBlazor.Showcase;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Api;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;
using Maliev.ShadcnBlazor.Showcase.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class DisclosureNavigationShowcaseContractTests : BunitContext
{
    private static readonly string[] Slugs = ["accordion", "breadcrumb", "collapsible", "navigation-menu", "pagination", "resizable", "scroll-area", "sidebar", "tabs"];

    public DisclosureNavigationShowcaseContractTests() { JSInterop.Mode = JSRuntimeMode.Loose; Services.AddMalievShadcn(); Services.AddSingleton<ShowcaseState>(); }

    [Fact]
    public void EveryPlanSixComponentOwnsOneLiveDossierWithControlsStatesAndUsage()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());
        foreach (var slug in Slugs)
        {
            var example = Assert.Single(registry.GetBySlug(slug));
            Assert.Equal($"{slug}-primary", example.Id);
            Assert.NotEmpty(example.Controls);
            Assert.NotEmpty(example.StateTags);
            Assert.Contains("<Shadcn", example.RazorSource, StringComparison.Ordinal);
            Assert.NotEmpty(Render(example.Preview).FindAll("[data-slot]"));
        }
    }

    [Fact]
    public void EveryPlanSixDossierOwnsItsFullCompositionApiSurface()
    {
        var catalog = new ComponentDocumentationCatalog(); var api = new ComponentApiCatalog();
        var minimum = new Dictionary<string, int> { ["accordion"] = 4, ["breadcrumb"] = 8, ["collapsible"] = 3, ["navigation-menu"] = 8, ["pagination"] = 8, ["resizable"] = 3, ["scroll-area"] = 5, ["sidebar"] = 20, ["tabs"] = 4 };
        foreach (var (slug, count) in minimum) Assert.True(api.GetByEntry(catalog.FindBySlug(slug)!).Count >= count, $"{slug} API ownership is incomplete.");
    }

    [Fact]
    public void EveryPlanSixDossierControlMutatesItsRenderedCanvas()
    {
        var expectedControls = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["accordion"] = ["accordion-multiple", "accordion-horizontal", "accordion-disabled"],
            ["breadcrumb"] = ["breadcrumb-ellipsis"],
            ["collapsible"] = ["collapsible-open", "collapsible-disabled"],
            ["navigation-menu"] = ["navigation-open", "navigation-vertical", "navigation-disabled"],
            ["pagination"] = ["pagination-current", "pagination-disabled"],
            ["resizable"] = ["resizable-vertical", "resizable-collapsible", "resizable-disabled"],
            ["scroll-area"] = ["scroll-always", "scroll-horizontal"],
            ["sidebar"] = ["sidebar-open", "sidebar-right", "sidebar-none"],
            ["tabs"] = ["tabs-history", "tabs-vertical", "tabs-manual", "tabs-force"],
        };
        var alternates = new Dictionary<ComponentParameterControlKind, string> { [ComponentParameterControlKind.Toggle] = "true", [ComponentParameterControlKind.Number] = "3" };
        foreach (var slug in Slugs)
        {
            Assert.Equal(expectedControls[slug], new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug(slug).Single().Controls.Select(control => control.Id));
            foreach (var controlId in new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug(slug).Single().Controls.Select(control => control.Id).ToArray())
            {
                var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug(slug).Single();
                var control = example.Controls.Single(candidate => candidate.Id == controlId);
                var before = Render(example.Preview).Markup;
                var alternate = control.Kind == ComponentParameterControlKind.Select ? control.Options.First(option => option != control.Value) : alternates[control.Kind];
                if (control.Value.Equals(alternate, StringComparison.OrdinalIgnoreCase)) alternate = "false";
                control.Apply(alternate);
                Assert.NotEqual(before, Render(example.Preview).Markup);
            }
        }
    }

    [Fact]
    public void DisclosureExamplesExposeRealisticContentAndPaginationEllipsis()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());
        var accordion = Assert.Single(registry.GetBySlug("accordion"));
        Assert.Contains("What are the delivery options?", accordion.RazorSource, StringComparison.Ordinal);
        Assert.Contains("How can I contact support?", accordion.RazorSource, StringComparison.Ordinal);

        var pagination = Assert.Single(registry.GetBySlug("pagination"));
        Assert.Contains("ShadcnPaginationEllipsis", pagination.RazorSource, StringComparison.Ordinal);

        var tabs = Assert.Single(registry.GetBySlug("tabs"));
        Assert.Contains("Files", tabs.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Activity", tabs.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void EvidencePairSurfaceOwnsOneDeterministicNamedStatePerComponent()
    {
        var cut = Render<DisclosureNavigationEvidence>();
        foreach (var id in new[] { "accordion-open", "breadcrumb-current", "collapsible-open", "navigation-menu-open", "pagination-current", "resizable-horizontal", "scroll-area-scrolled", "sidebar-expanded", "tabs-vertical" })
            Assert.Single(cut.FindAll($"[data-pair-id='{id}']"));
    }
}
