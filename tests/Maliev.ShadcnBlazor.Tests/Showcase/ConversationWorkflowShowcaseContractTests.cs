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
        Assert.Equal(3, cut.FindAll("img.showcase-attachment-artwork[src^='images/attachments/']").Count);
        Assert.Empty(cut.FindAll("svg.showcase-attachment-artwork"));
        Assert.Equal(2, cut.FindAll("[data-slot='attachment-media'][data-variant='icon']").Count);
        Assert.Single(cut.FindAll("[data-slot='attachment-progress']"));
        Assert.Contains("sales-dashboard.pdf", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("message-renderer.tsx", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void AttachmentDossierSourceIsTheComposedCopyableExample()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog())
            .GetBySlug("attachment").Single();

        Assert.DoesNotContain("...", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("@using Maliev.ShadcnBlazor.Components.Feedback", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnAttachmentGroup", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnAttachmentMedia", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnAttachmentContent", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnAttachmentActions", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("workspace-plan.png", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnSpinner", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Progress=\"64\"", example.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void BubbleDossierUsesAnInteractiveConversationThread()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog())
            .GetBySlug("bubble").Single();
        var cut = Render(example.Preview);

        Assert.Single(cut.FindAll("[data-slot='bubble-group']"));
        Assert.Equal(5, cut.FindAll("[data-slot='bubble']").Count);
        Assert.Equal(5, cut.FindAll("[data-slot='bubble-content']").Count);
        Assert.Equal(2, cut.FindAll("[data-slot='bubble-reactions']").Count);
        var incoming = cut.FindAll("[data-bubble-role='incoming']");
        Assert.Equal(3, incoming.Count);
        Assert.All(incoming, bubble =>
        {
            Assert.Equal("secondary", bubble.GetAttribute("data-variant"));
            Assert.Equal("start", bubble.GetAttribute("data-align"));
        });
        Assert.Contains("Hey there! what's up?", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("I can group messages, switch sides, and keep the whole thread easy to scan.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Very meta. Very on-brand.", cut.Markup, StringComparison.Ordinal);
        Assert.NotEmpty(cut.FindAll("button[data-slot='bubble-content']"));
    }

    [Fact]
    public void MarkerDossierShowsStatusSeparatorAndStreamingCompositions()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog())
            .GetBySlug("marker").Single();
        var cut = Render(example.Preview);

        Assert.Equal(3, cut.FindAll("[data-slot='marker']").Count);
        Assert.Equal(3, cut.FindAll("[data-slot='marker-icon']").Count);
        Assert.Contains("ตรวจสอบ 4 ไฟล์แล้ว", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("กำลังประมวลผล", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("showcase-marker-loader", cut.Markup, StringComparison.Ordinal);
        Assert.Equal("true", cut.Find("[data-slot='marker'][role='status']").GetAttribute("data-live"));
        Assert.Equal("true", cut.Find("[data-slot='marker'][role='status'] [data-slot='marker-icon']").GetAttribute("data-streaming"));
        Assert.Equal("true", cut.Find("[data-slot='marker'][role='status'] [data-slot='marker-content']").GetAttribute("data-streaming"));
    }

    [Fact]
    public void MarkerDossierSourceDocumentsTheStreamingLoaderAndReducedMotionPath()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog())
            .GetBySlug("marker").Single();

        Assert.DoesNotContain("...", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMarker Live=\"true\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMarkerIcon>", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<span class=\"showcase-marker-loader shadcn-marker-loader\"", example.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("style=\"display:none\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMarkerContent Streaming=\"true\">", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("กำลังประมวลผล", example.RazorSource, StringComparison.Ordinal);

        var cssPath = Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-conversation.css");
        var css = File.ReadAllText(cssPath);
        Assert.Contains("@keyframes shadcn-marker-dots", css, StringComparison.Ordinal);
        Assert.Contains("prefers-reduced-motion", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-marker-content[data-streaming=\"true\"]", css, StringComparison.Ordinal);
    }

    [Fact]
    public void MessageDossierShowsGroupedRowsWithAvatarsHeadersAndActions()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog())
            .GetBySlug("message").Single();
        var cut = Render(example.Preview);

        Assert.Single(cut.FindAll("[data-slot='message-group']"));
        Assert.Equal(3, cut.FindAll("[data-slot='message']").Count);
        Assert.Equal(3, cut.FindAll("[data-slot='message-avatar']").Count);
        Assert.Equal(2, cut.FindAll("img[data-avatar]").Count);
        Assert.Single(cut.FindAll("[data-slot='message-avatar'] [data-avatar='placeholder']"));
        Assert.Equal(3, cut.FindAll("[data-slot='message-header']").Count);
        Assert.Equal(3, cut.FindAll("[data-slot='bubble-content']").Count);
        Assert.Equal(2, cut.FindAll("[data-slot='message-footer']").Count);
        Assert.Single(cut.FindAll(".shadcn-message-action"));
        Assert.Single(cut.FindAll(".shadcn-message-reply-icon"));
        Assert.Single(cut.FindAll(".showcase-message-status"));
        Assert.Contains("พร้อมส่งแบบให้ตรวจ", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void MessageDossierSourceIsCompleteAndDocumentsFooterVisibility()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog())
            .GetBySlug("message").Single();

        Assert.DoesNotContain("...", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("@using Maliev.ShadcnBlazor.Components.Conversation", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessageGroup", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessageAvatar", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessageFooter", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("data-visibility=\"always\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("shadcn-message-reply-icon", example.RazorSource, StringComparison.Ordinal);

        var cssPath = Path.Combine(FindRoot(), "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css");
        var css = File.ReadAllText(cssPath);
        Assert.DoesNotContain("message-action:not(.showcase-message-action--sent)::before", css, StringComparison.Ordinal);
    }

    [Fact]
    public void ScrollerDossierShowsAReadableTranscriptWithAnchorsAndLatestAction()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog())
            .GetBySlug("message-scroller").Single();
        var cut = Render(example.Preview);

        Assert.Single(cut.FindAll("[data-slot='message-scroller']"));
        Assert.Equal(5, cut.FindAll("[data-slot='message-scroller-item']").Count);
        Assert.Equal(5, cut.FindAll("[data-slot='message-scroller-item'][data-scroll-anchor='true']").Count);
        Assert.Equal(5, cut.FindAll("[data-slot='bubble-content']").Count);
        Assert.Single(cut.FindAll("[data-slot='message-scroller-button']"));
        Assert.Contains("ตรวจสอบชิ้นงาน", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void QuestionnaireDossierShowsProgressDescriptionsChoiceCardsAndActions()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog())
            .GetBySlug("questionnaire").Single();
        var cut = Render(example.Preview);

        Assert.Single(cut.FindAll("form[data-slot='questionnaire']"));
        Assert.Single(cut.FindAll("[data-slot='questionnaire-progress']"));
        Assert.Equal(2, cut.FindAll("[data-slot='questionnaire-item']").Count);
        Assert.Equal(2, cut.FindAll("[data-slot='questionnaire-choice']").Count);
        Assert.Equal(2, cut.FindAll("[data-slot='questionnaire-choice-description']").Count);
        Assert.Equal(2, cut.FindAll("[data-slot='questionnaire-description']").Count);
        Assert.Single(cut.FindAll("[data-slot='questionnaire-actions']"));
        Assert.Contains("เลือกประเภทการตรวจ", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void DocumentationRouteLinksEveryExactPlanNinePinnedSource()
    {
        var route = File.ReadAllText(Path.Combine(FindRoot(), "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "Docs", "ComponentDocumentation.razor"));
        foreach (var slug in Slugs) Assert.Contains($"bases/base/ui/{slug}.tsx", route, StringComparison.Ordinal);
    }

    private static string FindRoot() { var directory = new DirectoryInfo(AppContext.BaseDirectory); while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent; return directory?.FullName ?? throw new DirectoryNotFoundException(); }
}
