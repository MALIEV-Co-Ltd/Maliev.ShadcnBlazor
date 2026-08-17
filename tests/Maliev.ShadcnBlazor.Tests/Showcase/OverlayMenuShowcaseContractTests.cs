using Bunit;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Api;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class OverlayMenuShowcaseContractTests : BunitContext
{
    private static readonly string[] Slugs = ["alert-dialog", "command", "context-menu", "dialog", "drawer", "dropdown-menu", "hover-card", "menubar", "popover", "sheet", "tooltip"];

    public OverlayMenuShowcaseContractTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void ShowcaseRegistersExampleRegistryWithNonCachingLifetime()
    {
        var source = File.ReadAllText(Path.Combine(FindRoot(), "samples", "Maliev.ShadcnBlazor.Showcase", "Program.cs"));
        Assert.Contains("AddTransient<IComponentExampleRegistry, ComponentExampleRegistry>()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AddSingleton<IComponentExampleRegistry, ComponentExampleRegistry>()", source, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryPlanSevenComponentHasARealCompleteDossierAndAuthoritativeApi()
    {
        var catalog = new ComponentDocumentationCatalog(); var api = new ComponentApiCatalog(); var registry = new ComponentExampleRegistry(catalog);
        foreach (var slug in Slugs)
        {
            var entry = Assert.IsType<ComponentDocumentationEntry>(catalog.FindBySlug(slug));
            Assert.Equal(ComponentDocumentationStatus.Complete, entry.Status);
            Assert.All(new[] { entry.Evidence.Api, entry.Evidence.ComponentTests, entry.Evidence.Accessibility, entry.Evidence.Interaction, entry.Evidence.ComputedStyle, entry.Evidence.Visual, entry.Evidence.Integration }, Assert.True);
            Assert.Equal("Maliev.ShadcnBlazor.Components.Overlays", entry.Namespace);
            Assert.NotNull(entry.PrimaryType);
            Assert.True(api.GetByEntry(entry).Count >= 3, $"{slug} API ownership is incomplete.");
            var example = Assert.Single(registry.GetBySlug(slug));
            Assert.Equal($"{slug}-primary", example.Id);
            Assert.NotEmpty(example.Controls); Assert.NotEmpty(example.StateTags);
            Assert.Contains("<Shadcn", example.RazorSource, StringComparison.Ordinal);
            Assert.NotEmpty(Render(example.Preview).FindAll("[data-slot]"));
        }
    }

    [Fact]
    public void EveryPlanSevenDossierControlMutatesItsRenderedCanvas()
    {
        foreach (var slug in Slugs)
            foreach (var controlId in new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug(slug).Single().Controls.Select(control => control.Id).ToArray())
            {
                var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug(slug).Single();
                var control = example.Controls.Single(candidate => candidate.Id == controlId);
                var before = Render(example.Preview).Markup;
                control.Apply(bool.Parse(control.Value) ? "false" : "true");
                Assert.NotEqual(before, Render(example.Preview).Markup);
            }
    }

    [Fact]
    public void InteractiveOverlaysStartClosedAndWaitForAUserTrigger()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());
        foreach (var slug in new[] { "alert-dialog", "dialog", "drawer", "dropdown-menu", "hover-card", "popover", "sheet", "tooltip" })
        {
            var markup = Render(registry.GetBySlug(slug).Single().Preview).Markup;
            Assert.DoesNotContain("data-state=\"open\"", markup, StringComparison.Ordinal);
        }

        var menubar = Render(registry.GetBySlug("menubar").Single().Preview).Markup;
        Assert.DoesNotContain("data-state=\"open\"", menubar, StringComparison.Ordinal);
    }

    [Fact]
    public void MenubarDossierUsesTheCompleteLibraryCompositionAndKeepsSourceInSync()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("menubar").Single();
        var rendered = Render(example.Preview);
        var markup = rendered.Markup;

        Assert.Contains("data-testid=\"menubar-dossier-preview\"", markup, StringComparison.Ordinal);
        Assert.Equal(4, rendered.FindAll("[data-slot='menubar-trigger']").Count);
        Assert.Contains("<ShadcnMenubarCheckboxItem", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMenubarRadioItem", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMenubarSubTrigger>", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMenubarContent>", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Loop=\"true\"", example.RazorSource, StringComparison.Ordinal);

        example.Controls.Single(control => control.Id == "menubar-loop").Apply("false");
        example.Controls.Single(control => control.Id == "menubar-status").Apply("false");

        Assert.Contains("Loop=\"false\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Checked=\"false\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Equal("false", Render(example.Preview).Find("[data-slot='menubar']").GetAttribute("data-loop"));
    }

    [Fact]
    public void PopoverDossierUsesARealClosedByDefaultCompositionAndStateAwareSource()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("popover").Single();
        var markup = Render(example.Preview).Markup;

        Assert.Contains("showcase-popover-dossier", markup, StringComparison.Ordinal);
        Assert.Contains("Edit part dimensions", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("data-state=\"open\"", markup, StringComparison.Ordinal);
        Assert.DoesNotContain(example.Controls, control => control.Id == "popover-open");
        Assert.Contains("@bind-Open=\"Open\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShadcnPopoverHeader", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShadcnInput", example.RazorSource, StringComparison.Ordinal);

        example.Controls.Single(control => control.Id == "popover-top").Apply("true");
        Assert.Contains("Side=\"ShadcnOverlaySide.Top\"", example.RazorSource, StringComparison.Ordinal);
        example.Controls.Single(control => control.Id == "popover-outside").Apply("false");
        Assert.Contains("CloseOnOutsidePress=\"false\"", example.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void DocumentationRouteLinksEveryPlanSevenPinnedAndCurrentReference()
    {
        var root = FindRoot();
        var route = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "Docs", "ComponentDocumentation.razor"));
        foreach (var slug in Slugs)
        {
            Assert.Contains($"\"{slug}\" =>", route, StringComparison.Ordinal);
            Assert.Contains($"ui/{slug}.tsx", route, StringComparison.Ordinal);
        }
        Assert.Contains("6261bd89f72d794aea491482cc2acfd8dc3d63e2", route, StringComparison.Ordinal);
        Assert.Contains("https://ui.shadcn.com/docs/components/base/", route, StringComparison.Ordinal);
    }

    private static string FindRoot() { var directory = new DirectoryInfo(AppContext.BaseDirectory); while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent; return directory?.FullName ?? throw new DirectoryNotFoundException(); }
}
