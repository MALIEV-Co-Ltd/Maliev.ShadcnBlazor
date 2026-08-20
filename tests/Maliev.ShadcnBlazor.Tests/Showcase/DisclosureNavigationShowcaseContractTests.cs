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
            ["pagination"] = ["pagination-current", "pagination-visible", "pagination-disabled"],
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
        Assert.Contains("Express delivery", accordion.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Revision notes", accordion.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("...", accordion.RazorSource, StringComparison.Ordinal);

        var pagination = Assert.Single(registry.GetBySlug("pagination"));
        Assert.Contains("ShadcnPaginationPages", pagination.RazorSource, StringComparison.Ordinal);
        Assert.Contains("TotalPages=\"12\"", pagination.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Production quotations", Render(pagination.Preview).Markup, StringComparison.Ordinal);

        var initialSource = pagination.RazorSource;
        pagination.Controls.Single(control => control.Id == "pagination-visible").Apply("7");
        Assert.NotEqual(initialSource, pagination.RazorSource);
        Assert.Contains("VisiblePageCount=\"7\"", pagination.RazorSource, StringComparison.Ordinal);

        var tabs = Assert.Single(registry.GetBySlug("tabs"));
        Assert.Contains("Files", tabs.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Activity", tabs.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void NavigationMenuMatchesReferenceCompositionAndKeepsSourceInSync()
    {
        var example = Assert.Single(new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("navigation-menu"));
        var initialMarkup = Render(example.Preview).Markup;

        Assert.Contains("Getting started", initialMarkup, StringComparison.Ordinal);
        Assert.Contains("Components", initialMarkup, StringComparison.Ordinal);
        Assert.Contains("Project status", initialMarkup, StringComparison.Ordinal);
        Assert.Contains("Documentation", initialMarkup, StringComparison.Ordinal);
        Assert.Contains("showcase-navigation-menu__component-grid", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("showcase-navigation-menu__status-link", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Confirm a destructive or sensitive action.", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("showcase-navigation-menu__entry", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("showcase-navigation-menu__wide-item", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<svg viewBox=\"0 0 24 24\"", example.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("data-state=\"open\"", initialMarkup, StringComparison.Ordinal);

        example.Controls.Single(control => control.Id == "navigation-open").Apply("true");
        Assert.Contains("Value=\"getting-started\"", example.RazorSource, StringComparison.Ordinal);
        example.Controls.Single(control => control.Id == "navigation-vertical").Apply("true");
        Assert.Contains("Orientation=\"ShadcnNavigationMenuOrientation.Vertical\"", example.RazorSource, StringComparison.Ordinal);
        example.Controls.Single(control => control.Id == "navigation-disabled").Apply("true");
        Assert.Contains("Disabled=\"true\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShadcnNavigationMenuViewport", example.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void ResizableDossierMirrorsOrientationCollapseAndDisabledStateInSource()
    {
        var example = Assert.Single(new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("resizable"));
        Assert.Contains("showcase-resizable-dossier", Render(example.Preview).Markup, StringComparison.Ordinal);
        Assert.Contains("Production queue", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShadcnResizableDirection.Horizontal", example.RazorSource, StringComparison.Ordinal);

        example.Controls.Single(control => control.Id == "resizable-vertical").Apply("true");
        example.Controls.Single(control => control.Id == "resizable-collapsible").Apply("true");
        example.Controls.Single(control => control.Id == "resizable-disabled").Apply("true");

        Assert.Contains("ShadcnResizableDirection.Vertical", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Collapsible=\"true\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Disabled=\"true\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("data-direction=\"vertical\"", Render(example.Preview).Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void EvidencePairSurfaceOwnsOneDeterministicNamedStatePerComponent()
    {
        var cut = Render<DisclosureNavigationEvidence>();
        foreach (var id in new[] { "accordion-open", "breadcrumb-current", "collapsible-open", "navigation-menu-open", "pagination-current", "resizable-horizontal", "scroll-area-scrolled", "sidebar-expanded", "tabs-vertical" })
            Assert.Single(cut.FindAll($"[data-pair-id='{id}']"));
    }
}
