using Bunit;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Api;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;
using Maliev.ShadcnBlazor.Components.DataDisplay;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ConversationWorkflowShowcaseContractTests : BunitContext
{
    private static readonly string[] Slugs = ["attachment", "bubble", "marker", "message", "message-scroller", "questionnaire"];

    public ConversationWorkflowShowcaseContractTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddScoped<IShadcnIdAllocator, ShadcnIdAllocator>();
    }

    [Fact]
    public void EveryPlanNineComponentHasCompleteEvidenceMetadataApiAndRealPreview()
    {
        var catalog = new ComponentDocumentationCatalog(); var api = new ComponentApiCatalog(); var registry = new ComponentExampleRegistry(catalog);
        foreach (var slug in Slugs)
        {
            var entry = Assert.IsType<ComponentDocumentationEntry>(catalog.FindBySlug(slug));
            Assert.Equal(ComponentDocumentationStatus.Complete, entry.Status);
            Assert.True(entry.Evidence.Api && entry.Evidence.ComponentTests && entry.Evidence.Accessibility && entry.Evidence.Interaction && entry.Evidence.ComputedStyle && entry.Evidence.Visual && entry.Evidence.Integration);
            Assert.Equal("Maliev.ShadcnBlazor.Components.Conversation", entry.Namespace);
            Assert.NotNull(entry.PrimaryType);
            Assert.NotEmpty(api.GetByEntry(entry));
            var example = Assert.Single(registry.GetBySlug(slug));
            Assert.NotEmpty(example.Controls); Assert.NotEmpty(example.StateTags);
            Assert.NotEmpty(Render(example.Preview).FindAll("[data-slot]"));
        }
    }

    [Fact]
    public void EveryPlanNineDossierControlMutatesItsActualCanvas()
    {
        foreach (var slug in Slugs)
            foreach (var id in new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug(slug).Single().Controls.Select(control => control.Id).ToArray())
            {
                var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug(slug).Single();
                var control = example.Controls.Single(candidate => candidate.Id == id);
                var before = Render(example.Preview).Markup;
                var next = control.Kind is ComponentParameterControlKind.Toggle ? (!bool.Parse(control.Value)).ToString() : control.Options.First(option => option != control.Value);
                control.Apply(next);
                Assert.NotEqual(before, Render(example.Preview).Markup);
            }
    }

    [Fact]
    public void AttachmentDossierUsesAComposedGalleryAndFileRows()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog())
            .GetBySlug("attachment").Single();
        var cut = Render(example.Preview);

        Assert.Single(cut.FindAll("[data-slot='attachment-group']"));
        Assert.Equal(5, cut.FindAll("[data-slot='attachment']").Count);
        Assert.Equal(3, cut.FindAll("[data-slot='attachment-media'][data-variant='image']").Count);
        Assert.Equal(2, cut.FindAll("[data-slot='attachment-media'][data-variant='icon']").Count);
        Assert.Single(cut.FindAll("[data-slot='attachment-progress']"));
        Assert.Contains("sales-dashboard.pdf", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("message-renderer.tsx", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void BubbleDossierUsesAnInteractiveConversationThread()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog())
            .GetBySlug("bubble").Single();
        var cut = Render(example.Preview);

        Assert.Single(cut.FindAll("[data-slot='bubble-group']"));
        Assert.Equal(4, cut.FindAll("[data-slot='bubble']").Count);
        Assert.Equal(4, cut.FindAll("[data-slot='bubble-content']").Count);
        Assert.Equal(2, cut.FindAll("[data-slot='bubble-reactions']").Count);
        Assert.Contains("Hey there! what's up?", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Very meta. Very on-brand.", cut.Markup, StringComparison.Ordinal);
        Assert.NotEmpty(cut.FindAll("button[data-slot='bubble-content']"));
    }

    [Fact]
    public void DocumentationRouteLinksEveryExactPlanNinePinnedSource()
    {
        var route = File.ReadAllText(Path.Combine(FindRoot(), "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "Docs", "ComponentDocumentation.razor"));
        foreach (var slug in Slugs) Assert.Contains($"bases/base/ui/{slug}.tsx", route, StringComparison.Ordinal);
    }

    private static string FindRoot() { var directory = new DirectoryInfo(AppContext.BaseDirectory); while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent; return directory?.FullName ?? throw new DirectoryNotFoundException(); }
}
