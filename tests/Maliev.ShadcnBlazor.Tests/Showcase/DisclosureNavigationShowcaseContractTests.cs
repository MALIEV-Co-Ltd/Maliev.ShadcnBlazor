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
            ["sidebar"] = ["sidebar-open", "sidebar-side", "sidebar-mode"],
            ["tabs"] = ["tabs-value", "tabs-orientation", "tabs-activation", "tabs-variant", "tabs-loop", "tabs-force"],
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
        var accordionMarkup = Render(accordion.Preview).Markup;
        Assert.Contains("showcase-accordion-dossier", accordionMarkup, StringComparison.Ordinal);
        Assert.Contains("Quotation support", accordionMarkup, StringComparison.Ordinal);
        Assert.Contains("What are the delivery options?", accordion.RazorSource, StringComparison.Ordinal);
        Assert.Contains("How can I contact support?", accordion.RazorSource, StringComparison.Ordinal);
        Assert.Contains("How are production changes approved?", accordion.RazorSource, StringComparison.Ordinal);
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

        var scrollArea = Assert.Single(registry.GetBySlug("scroll-area"));
        Assert.Contains("Production activity", scrollArea.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Aluminum housing", scrollArea.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Material @index", scrollArea.RazorSource, StringComparison.Ordinal);

        var horizontal = scrollArea.Controls.Single(control => control.Id == "scroll-horizontal");
        horizontal.Apply("true");
        Assert.Contains("ShadcnScrollAreaOrientation.Horizontal", scrollArea.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Weekly machine schedule", scrollArea.RazorSource, StringComparison.Ordinal);

        var visibility = scrollArea.Controls.Single(control => control.Id == "scroll-always");
        visibility.Apply("false");
        Assert.Contains("ShadcnScrollAreaType.Auto", scrollArea.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ShadcnScrollAreaType.Always", scrollArea.RazorSource, StringComparison.Ordinal);

        var tabs = Assert.Single(registry.GetBySlug("tabs"));
        Assert.Contains("Files", tabs.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Activity", tabs.RazorSource, StringComparison.Ordinal);
        Assert.Contains("CNC enclosure · Revision C", tabs.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShadcnTabsListVariant.Default", tabs.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Loop=\"true\"", tabs.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ForceMount=\"true\"", tabs.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Project workspace views", Render(tabs.Preview).Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void TabsDossierSourceTracksEveryMeaningfulControl()
    {
        var tabs = Assert.Single(new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("tabs"));

        tabs.Controls.Single(control => control.Id == "tabs-value").Apply("files");
        tabs.Controls.Single(control => control.Id == "tabs-orientation").Apply("Vertical");
        tabs.Controls.Single(control => control.Id == "tabs-activation").Apply("Manual");
        tabs.Controls.Single(control => control.Id == "tabs-variant").Apply("Line");
        tabs.Controls.Single(control => control.Id == "tabs-loop").Apply("false");
        tabs.Controls.Single(control => control.Id == "tabs-force").Apply("false");

        Assert.Contains("private string value = \"files\"", tabs.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShadcnTabsOrientation.Vertical", tabs.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShadcnTabsActivationMode.Manual", tabs.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShadcnTabsListVariant.Line", tabs.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Loop=\"false\"", tabs.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ForceMount=\"false\"", tabs.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void TabsDossierSupportsDirectTabSelection()
    {
        var tabs = Assert.Single(new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("tabs"));
        var cut = Render(tabs.Preview);

        cut.Find("[role='tab'][data-value='files']").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal("true", cut.Find("[role='tab'][data-value='files']").GetAttribute("aria-selected"));
            Assert.Contains("inspection-plan.pdf", cut.Markup, StringComparison.Ordinal);
        });
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
    public void AccordionExampleSourceTracksEveryInteractiveSetting()
    {
        var accordion = Assert.Single(new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("accordion"));

        Assert.Contains("Multiple=\"false\"", accordion.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShadcnAccordionOrientation.Vertical", accordion.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Disabled=\"false\"", accordion.RazorSource, StringComparison.Ordinal);

        accordion.Controls.Single(control => control.Id == "accordion-multiple").Apply("true");
        accordion.Controls.Single(control => control.Id == "accordion-horizontal").Apply("true");
        accordion.Controls.Single(control => control.Id == "accordion-disabled").Apply("true");

        Assert.Contains("Multiple=\"true\"", accordion.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShadcnAccordionOrientation.Horizontal", accordion.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Disabled=\"true\"", accordion.RazorSource, StringComparison.Ordinal);
        Assert.Contains("new[] { \"delivery\", \"returns\" }", accordion.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void CollapsibleDossierUsesRealOrderContextAndSourceTracksSettings()
    {
        var collapsible = Assert.Single(new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("collapsible"));
        var markup = Render(collapsible.Preview).Markup;
        Assert.Contains("showcase-collapsible-dossier", markup, StringComparison.Ordinal);
        Assert.Contains("Order #4189", markup, StringComparison.Ordinal);
        Assert.Contains("Shipped", markup, StringComparison.Ordinal);
        Assert.Contains("Shipping address", collapsible.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Open=\"false\"", collapsible.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Disabled=\"false\"", collapsible.RazorSource, StringComparison.Ordinal);

        collapsible.Controls.Single(control => control.Id == "collapsible-open").Apply("true");
        collapsible.Controls.Single(control => control.Id == "collapsible-disabled").Apply("true");

        Assert.Contains("Open=\"true\"", collapsible.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Disabled=\"true\"", collapsible.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void BreadcrumbDossierMatchesItsCollapsedAndExpandedPreviewSource()
    {
        var breadcrumb = Assert.Single(new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("breadcrumb"));

        Assert.Contains("Aster Precision", breadcrumb.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Quotation #4189", breadcrumb.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnBreadcrumbItem><ShadcnBreadcrumbEllipsis", breadcrumb.RazorSource, StringComparison.Ordinal);
        Assert.Contains("aria-current", Render(breadcrumb.Preview).Markup, StringComparison.Ordinal);
        Assert.Single(Render(breadcrumb.Preview).FindAll("[data-slot='breadcrumb-ellipsis']"));

        breadcrumb.Controls.Single(control => control.Id == "breadcrumb-ellipsis").Apply("false");

        Assert.DoesNotContain("ShadcnBreadcrumbEllipsis", breadcrumb.RazorSource, StringComparison.Ordinal);
        Assert.Contains("/projects/aster-precision", breadcrumb.RazorSource, StringComparison.Ordinal);
        Assert.Contains("/projects/aster-precision/quotations", breadcrumb.RazorSource, StringComparison.Ordinal);
        Assert.Empty(Render(breadcrumb.Preview).FindAll("[data-slot='breadcrumb-ellipsis']"));
        Assert.Equal(5, Render(breadcrumb.Preview).FindAll("[data-slot='breadcrumb-item']").Count);
    }

    [Fact]
    public void SidebarDossierUsesCompleteCompositionAndStateAwareSource()
    {
        var sidebar = Assert.Single(new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("sidebar"));
        var rendered = Render(sidebar.Preview);

        Assert.NotNull(rendered.Find(".showcase-sidebar-shell"));
        Assert.NotNull(rendered.Find("[data-slot='sidebar-rail']"));
        Assert.NotNull(rendered.Find("[data-slot='sidebar-menu-badge']"));
        Assert.Contains("ShadcnSidebarMenuButton", sidebar.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShadcnSidebarRail", sidebar.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Production queue", sidebar.RazorSource, StringComparison.Ordinal);

        sidebar.Controls.Single(control => control.Id == "sidebar-side").Apply(nameof(Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarSide.Right));
        sidebar.Controls.Single(control => control.Id == "sidebar-mode").Apply(nameof(Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarCollapsible.None));

        Assert.Contains("Side=\"ShadcnSidebarSide.Right\"", sidebar.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Collapsible=\"ShadcnSidebarCollapsible.None\"", sidebar.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void EvidencePairSurfaceOwnsOneDeterministicNamedStatePerComponent()
    {
        var cut = Render<DisclosureNavigationEvidence>();
        foreach (var id in new[] { "accordion-open", "breadcrumb-current", "collapsible-open", "navigation-menu-open", "pagination-current", "resizable-horizontal", "scroll-area-scrolled", "sidebar-expanded", "tabs-vertical" })
            Assert.Single(cut.FindAll($"[data-pair-id='{id}']"));
    }
}
