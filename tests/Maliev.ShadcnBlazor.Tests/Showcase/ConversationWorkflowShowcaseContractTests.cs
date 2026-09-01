using Bunit;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Api;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;
using Maliev.ShadcnBlazor.Components.Conversation;
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
    public void AttachmentRestingSurfaceDoesNotUseAnOutlineButInteractiveChildrenKeepFocusStyles()
    {
        var cssPath = Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-conversation.css");
        var css = File.ReadAllText(cssPath);
        var attachmentRuleStart = css.IndexOf(".shadcn-attachment {", StringComparison.Ordinal);
        Assert.True(attachmentRuleStart >= 0);
        var attachmentRuleEnd = css.IndexOf('}', attachmentRuleStart);
        Assert.True(attachmentRuleEnd > attachmentRuleStart);
        var attachmentRule = css[attachmentRuleStart..(attachmentRuleEnd + 1)];
        Assert.Contains("outline: none", attachmentRule, StringComparison.Ordinal);
        Assert.Contains(".shadcn-attachment-action:focus-visible", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-attachment-trigger:focus-visible", css, StringComparison.Ordinal);
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
            Assert.Equal("ghost", bubble.GetAttribute("data-variant"));
            Assert.Equal("start", bubble.GetAttribute("data-align"));
        });
        var outgoing = cut.FindAll("[data-bubble-role='outgoing']");
        Assert.Equal(2, outgoing.Count);
        Assert.All(outgoing, bubble => Assert.Equal("default", bubble.GetAttribute("data-variant")));
        Assert.Empty(cut.FindAll("[data-slot='bubble-reaction'] [data-slot='avatar']"));
        Assert.Equal(3, cut.FindAll("[data-slot='bubble-reaction-value'] .showcase-reaction-glyph").Count);
        Assert.NotNull(cut.Find("[aria-label^='Thumbs up reaction'] .showcase-reaction-glyph--thumbs-up"));
        Assert.NotNull(cut.Find("[aria-label^='Heart reaction'] .showcase-reaction-glyph--heart"));
        var heartReaction = cut.Find("button[data-testid='bubble-reaction-heart']");
        Assert.Equal("false", heartReaction.GetAttribute("aria-pressed"));
        Assert.Equal("2", heartReaction.QuerySelector("[data-slot='bubble-reaction-count']")?.TextContent);
        heartReaction.Click();
        Assert.Equal("true", heartReaction.GetAttribute("aria-pressed"));
        Assert.Equal("3", heartReaction.QuerySelector("[data-slot='bubble-reaction-count']")?.TextContent);
        Assert.Contains("Hey there! what's up?", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("I can group messages, switch sides, and keep the whole thread easy to scan.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Very meta. Very on-brand.", cut.Markup, StringComparison.Ordinal);
        Assert.NotEmpty(cut.FindAll("button[data-slot='bubble-content']"));
    }

    [Fact]
    public void BubbleDossierUsesSequentialRevealAndGhostRemovesItsSurface()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog())
            .GetBySlug("bubble").Single();
        var cut = Render(example.Preview);

        Assert.Equal("true", cut.Find("[data-slot='bubble-group']").GetAttribute("data-reveal"));

        var sentVariant = example.Controls.Single(control => control.Id == "bubble-variant");
        var receivedVariant = example.Controls.Single(control => control.Id == "bubble-received-variant");
        sentVariant.Apply(nameof(ShadcnBubbleVariant.Tinted));
        receivedVariant.Apply(nameof(ShadcnBubbleVariant.Muted));
        cut = Render(example.Preview);
        Assert.All(cut.FindAll("[data-bubble-role='outgoing']"), bubble => Assert.Equal("tinted", bubble.GetAttribute("data-variant")));
        Assert.All(cut.FindAll("[data-bubble-role='incoming']"), bubble => Assert.Equal("muted", bubble.GetAttribute("data-variant")));

        var cssPath = Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-conversation.css");
        var css = File.ReadAllText(cssPath);
        Assert.Contains("@keyframes shadcn-bubble-reveal", css, StringComparison.Ordinal);
        Assert.Contains("prefers-reduced-motion", css, StringComparison.Ordinal);
        Assert.Contains("data-variant=\"ghost\"", css, StringComparison.Ordinal);
        Assert.Contains("border: 0 !important", css, StringComparison.Ordinal);

        var showcaseCss = File.ReadAllText(Path.Combine(FindRoot(), "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css"));
        Assert.Contains("padding-inline: .875rem !important", showcaseCss, StringComparison.Ordinal);
        var ghostRuleStart = showcaseCss.IndexOf("html body .showcase-bubble-thread > [data-slot=\"bubble\"][data-variant=\"ghost\"] > .shadcn-bubble-content", StringComparison.Ordinal);
        Assert.True(ghostRuleStart >= 0);
        var ghostRuleEnd = showcaseCss.IndexOf('}', ghostRuleStart);
        Assert.True(ghostRuleEnd > ghostRuleStart);
        Assert.DoesNotContain("padding-inline: 0 !important", showcaseCss[ghostRuleStart..(ghostRuleEnd + 1)], StringComparison.Ordinal);
        Assert.DoesNotContain("[data-slot=\"bubble\"]:hover", showcaseCss, StringComparison.Ordinal);
    }

    [Fact]
    public void MarkerDossierShowsAvatarsCurrentBubbleVariantsAndMarkerCompositions()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog())
            .GetBySlug("marker").Single();
        var cut = Render(example.Preview);

        Assert.Equal(3, cut.FindAll("[data-slot='bubble']").Count);
        Assert.Equal(3, cut.FindAll("[data-slot='message']").Count);
        Assert.Equal(3, cut.FindAll("[data-slot='message-avatar']").Count);
        Assert.Equal(2, cut.FindAll("[data-slot='message'][data-align='start'] [data-slot='bubble'][data-variant='ghost']").Count);
        Assert.Single(cut.FindAll("[data-slot='message'][data-align='end'] [data-slot='bubble'][data-variant='default']"));
        Assert.Equal(3, cut.FindAll("[data-slot='marker']").Count);
        Assert.Equal(3, cut.FindAll("[data-slot='marker-icon']").Count);
        Assert.Contains("Four inspection files verified", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Preparing quality handoff", cut.Markup, StringComparison.Ordinal);
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
        Assert.Equal(3, example.RazorSource.Split("<ShadcnBubble ", StringSplitOptions.None).Length - 1);
        Assert.Contains("Bore 4 is ready for final inspection", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMarker Live=\"true\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMarkerIcon>", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<span class=\"showcase-marker-loader shadcn-marker-loader\"", example.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("style=\"display:none\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMarkerContent dir=\"auto\" Streaming=\"true\">", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Preparing quality handoff", example.RazorSource, StringComparison.Ordinal);

        var cssPath = Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-conversation.css");
        var css = File.ReadAllText(cssPath);
        Assert.Contains("@keyframes shadcn-marker-dots", css, StringComparison.Ordinal);
        Assert.Contains("@keyframes shadcn-marker-wave", css, StringComparison.Ordinal);
        Assert.Contains("mask:linear-gradient(-60deg,#000 30%,#0005,#000 70%) right / 350% 100%", css, StringComparison.Ordinal);
        Assert.Contains("-webkit-mask:linear-gradient(-60deg,#000 30%,#0005,#000 70%) right / 350% 100%", css, StringComparison.Ordinal);
        Assert.Contains("@keyframes shadcn-marker-wave { from { mask-position:right; -webkit-mask-position:right; } to { mask-position:left; -webkit-mask-position:left; } }", css, StringComparison.Ordinal);
        Assert.DoesNotContain("background-clip:text", css, StringComparison.Ordinal);
        Assert.Contains("prefers-reduced-motion", css, StringComparison.Ordinal);
        Assert.Contains("forced-colors", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-marker-content[data-streaming=\"true\"]", css, StringComparison.Ordinal);
        Assert.DoesNotContain("shadcn-marker-spin", css, StringComparison.Ordinal);
        Assert.DoesNotContain("shadcn-marker-pulse", css, StringComparison.Ordinal);
    }

    [Fact]
    public void MessageDossierShowsGroupedRowsWithAvatarsHeadersAndActions()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog())
            .GetBySlug("message").Single();
        var cut = Render(example.Preview);

        Assert.Single(cut.FindAll("[data-slot='message-group']"));
        Assert.Equal(4, cut.FindAll("[data-slot='message']").Count);
        Assert.Equal(3, cut.FindAll("[data-slot='message-avatar']").Count);
        Assert.Equal(2, cut.FindAll("img[data-avatar]").Count);
        Assert.Single(cut.FindAll("[data-slot='message-avatar'] [data-avatar='placeholder']"));
        Assert.Equal(3, cut.FindAll("[data-slot='message-header']").Count);
        Assert.Equal(4, cut.FindAll("[data-slot='bubble-content']").Count);
        Assert.Equal(4, cut.FindAll("[data-slot='message-footer']").Count);
        Assert.Equal(4, cut.FindAll("[data-testid='message-copy']").Count);
        Assert.Equal(4, cut.FindAll("[data-testid='message-reply']").Count);
        Assert.Equal(4, cut.FindAll("[data-slot='message-body']").Count);
        Assert.Equal(4, cut.FindAll("[data-slot='message-actions']").Count);
        Assert.Single(cut.FindAll("[data-slot='message-status']"));
        Assert.Single(cut.FindAll("[data-slot='message']")[0].QuerySelectorAll("[data-slot='bubble']"));
        Assert.Single(cut.FindAll("[data-slot='message']")[0].QuerySelectorAll("[data-slot='message-header']"));
        Assert.Single(cut.FindAll("[data-slot='message']")[0].QuerySelectorAll("[data-slot='message-avatar']"));
        Assert.Equal("true", cut.FindAll("[data-slot='message']")[1].GetAttribute("data-continuation"));
        Assert.Empty(cut.FindAll("[data-slot='message']")[1].QuerySelectorAll("[data-slot='message-header']"));
        Assert.Empty(cut.FindAll("[data-slot='message']")[1].QuerySelectorAll("[data-slot='message-avatar']"));
        Assert.Single(cut.FindAll("[data-slot='message']")[1].QuerySelectorAll("[data-slot='message-footer']"));
        Assert.Contains("กำลังตรวจสอบค่าความคลาดเคลื่อนต่อ", cut.Markup, StringComparison.Ordinal);
        Assert.Equal("start", cut.FindAll("[data-slot='message']")[2].GetAttribute("data-align"));
        Assert.Single(cut.FindAll("[data-slot='message']")[2].QuerySelectorAll("[data-slot='message-footer']"));
        Assert.Contains("พร้อมส่งแบบให้ตรวจ", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void MessageDossierAvatarToggleAndActionsRemainInteractive()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog())
            .GetBySlug("message").Single();

        var avatarToggle = example.Controls.Single(control => control.Id == "message-avatar");
        avatarToggle.Apply("False");
        var withoutAvatars = Render(example.Preview);
        Assert.Empty(withoutAvatars.FindAll("[data-slot='message-avatar']"));

        var withAvatars = example.Controls.Single(control => control.Id == "message-avatar");
        withAvatars.Apply("True");
        var cut = Render(example.Preview);
        var copy = cut.FindAll("[data-testid='message-copy']");
        var reply = cut.FindAll("[data-testid='message-reply']");
        Assert.Equal(4, copy.Count);
        Assert.Equal(4, reply.Count);

        copy[0].Click();
        Assert.Contains("Copied", cut.Markup, StringComparison.Ordinal);
        reply[0].Click();
        var quote = cut.Find("[data-testid='message-reply-quote']");
        Assert.Contains("ตรวจสอบไฟล์แล้ว 3 รายการ", quote.TextContent, StringComparison.Ordinal);
        quote.QuerySelector("[data-slot='message-reply-dismiss']")!.Click();

        reply[1].Click();
        quote = cut.Find("[data-testid='message-reply-quote']");
        Assert.Contains("กำลังตรวจสอบค่าความคลาดเคลื่อนต่อ", quote.TextContent, StringComparison.Ordinal);
        quote.QuerySelector("[data-slot='message-reply-dismiss']")!.Click();

        reply[3].Click();
        quote = cut.Find("[data-testid='message-reply-quote']");
        Assert.Contains("Sure. I’ll keep the thread easy to scan.", quote.TextContent, StringComparison.Ordinal);
        quote.QuerySelector("[data-slot='message-reply-dismiss']")!.Click();
        Assert.Empty(cut.FindAll("[data-testid='message-reply-quote']"));

        var always = example.Controls.Single(control => control.Id == "message-footer-always");
        always.Apply("True");
        cut = Render(example.Preview);
        Assert.All(cut.FindAll("[data-slot='message-footer']"), footer => Assert.Equal("always", footer.GetAttribute("data-visibility")));
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
        Assert.DoesNotContain("data-visibility=\"always\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessageBody>", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessageActions>", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessageCopyAction", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessageReplyAction", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessageStatus>", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessageReplyQuote", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("กำลังตรวจสอบค่าความคลาดเคลื่อนต่อ", example.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain(example.Controls, control => control.Id == "message-end");
        Assert.Contains("data-continuation=\"true\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Collection(System.Text.RegularExpressions.Regex.Matches(example.RazorSource, "<ShadcnMessage Align=\"ShadcnLogicalAlign.Start\""), _ => { }, _ => { }, _ => { });
        Assert.Collection(System.Text.RegularExpressions.Regex.Matches(example.RazorSource, "<ShadcnMessageFooter"), _ => { }, _ => { }, _ => { }, _ => { });

        example.Controls.Single(control => control.Id == "message-footer-always").Apply("True");
        Assert.Contains("data-visibility=\"always\"", example.RazorSource, StringComparison.Ordinal);
        example.Controls.Single(control => control.Id == "message-avatar").Apply("False");
        Assert.DoesNotContain("<ShadcnMessageAvatar", example.RazorSource, StringComparison.Ordinal);
        example.Controls.Single(control => control.Id == "message-footer").Apply("False");
        Assert.DoesNotContain("<ShadcnMessageFooter", example.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("<ShadcnMessageReplyQuote", example.RazorSource, StringComparison.Ordinal);

        var cssPath = Path.Combine(FindRoot(), "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css");
        var css = File.ReadAllText(cssPath);
        Assert.DoesNotContain("message-action:not(.showcase-message-action--sent)::before", css, StringComparison.Ordinal);
    }

    [Fact]
    public void MessageDossierKeepsAvatarsCircularAndFooterActionsInTheirOwnRow()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog())
            .GetBySlug("message").Single();
        var cut = Render(example.Preview);

        Assert.Equal(4, cut.FindAll("[data-slot='message-body']").Count);
        Assert.Equal(3, cut.FindAll("[data-slot='message-avatar'] > [data-slot='avatar']").Count);
        Assert.Equal(3, cut.FindAll("[data-slot='message-avatar'] [data-slot='avatar-fallback']").Count);
        Assert.Equal(4, cut.FindAll("[data-slot='message-footer']").Count);

        var conversationCss = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-conversation.css"));
        Assert.Contains("grid-template-rows: auto auto", conversationCss, StringComparison.Ordinal);
        Assert.Contains("align-self:end", conversationCss, StringComparison.Ordinal);
        Assert.Contains("justify-content:flex-start", conversationCss, StringComparison.Ordinal);
        Assert.DoesNotContain(".shadcn-message-status { min-width:0; margin-inline-start:auto;", conversationCss, StringComparison.Ordinal);
        Assert.Contains("aspect-ratio:1", conversationCss, StringComparison.Ordinal);
        Assert.Contains(".shadcn-message-action {", conversationCss, StringComparison.Ordinal);
        Assert.Contains(".shadcn-message-avatar > [data-slot=\"avatar\"]", conversationCss, StringComparison.Ordinal);
    }

    [Fact]
    public void ScrollerDossierShowsAReadableTranscriptWithAnchorsAndLatestAction()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog())
            .GetBySlug("message-scroller").Single();
        var cut = Render(example.Preview);

        Assert.Single(cut.FindAll("[data-slot='message-scroller']"));
        Assert.Equal(2, cut.FindAll("[data-slot='message-scroller-item']").Count);
        Assert.Equal(2, cut.FindAll("[data-slot='message-scroller-item'][data-scroll-anchor='true']").Count);
        Assert.Equal(2, cut.FindAll("[data-slot='bubble-content']").Count);
        Assert.Equal(2, cut.FindAll("[data-slot='message-body']").Count);
        Assert.Single(cut.FindAll("form.showcase-scroller-composer"));
        Assert.Single(cut.FindAll("[data-slot='message-scroller'] form.showcase-scroller-composer"));
        Assert.Single(cut.FindAll("[data-slot='message-scroller'] > .showcase-scroller-transcript"));
        Assert.Single(cut.FindAll(".showcase-scroller-transcript > [data-slot='message-scroller-viewport']"));
        Assert.Empty(cut.FindAll(".showcase-scroller-transcript form.showcase-scroller-composer"));
        Assert.Empty(cut.FindAll("[data-testid='scroller-demo'] > form.showcase-scroller-composer"));
        Assert.Equal("อธิบายวิธีติดตามข้อความล่าสุดให้หน่อย", cut.Find(".showcase-scroller-composer input").GetAttribute("value"));
        Assert.NotEmpty(cut.FindAll("button[data-testid='scroller-send']"));
        Assert.Single(cut.FindAll("[data-slot='message-scroller-button']"));
        Assert.Contains("ตรวจสอบชิ้นงาน", cut.Markup, StringComparison.Ordinal);
        Assert.Equal("true", cut.Find("[data-testid='scroller-demo']").GetAttribute("data-preview-auto"));
        Assert.Equal("end", cut.Find("[data-slot='message'][data-align='end']").GetAttribute("data-align"));
        Assert.Equal("start", cut.Find("[data-slot='message'][data-align='start']").GetAttribute("data-align"));
    }

    [Fact]
    public void ScrollerDossierSourceDocumentsTheInteractiveComposerAndStableAnchors()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog())
            .GetBySlug("message-scroller").Single();

        Assert.DoesNotContain("...", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessageScrollerProvider", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessageScrollerItem MessageId=\"user-1\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessageScrollerItem MessageId=\"assistant-1\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("AutoScroll=\"true\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Equal("True", example.Controls.Single(control => control.Id == "scroller-auto").Value);
        Assert.Contains("<ShadcnMessage Align=\"ShadcnLogicalAlign.End\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessage Align=\"ShadcnLogicalAlign.Start\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessageBody>", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("showcase-scroller-composer", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("@bind=\"message\"", example.RazorSource, StringComparison.Ordinal);
        Assert.True(example.RazorSource.IndexOf("showcase-scroller-composer", StringComparison.Ordinal) < example.RazorSource.LastIndexOf("</ShadcnMessageScroller>", StringComparison.Ordinal));

        var css = File.ReadAllText(Path.Combine(FindRoot(), "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css"));
        Assert.Contains(".showcase-scroller-frame .showcase-scroller-composer", css, StringComparison.Ordinal);
        Assert.Contains("grid-template-rows: minmax(0, 1fr) auto", css, StringComparison.Ordinal);
        Assert.Contains(".showcase-scroller-frame {", css, StringComparison.Ordinal);
        Assert.Contains("position: relative", css, StringComparison.Ordinal);
        Assert.Contains(".showcase-scroller-transcript::after", css, StringComparison.Ordinal);
        Assert.Contains("linear-gradient(to bottom", css, StringComparison.Ordinal);
        Assert.Contains("pointer-events: none", css, StringComparison.Ordinal);
        Assert.Contains("inset-inline-end: 1rem", css, StringComparison.Ordinal);
        Assert.Contains("env(safe-area-inset-bottom)", css, StringComparison.Ordinal);
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
        Assert.Equal(3, cut.FindAll("[data-slot='questionnaire-choice']").Count);
        Assert.Equal(3, cut.FindAll("[data-slot='questionnaire-choice-description']").Count);
        Assert.Equal(2, cut.FindAll("[data-slot='questionnaire-description']").Count);
        Assert.Single(cut.FindAll("[data-slot='questionnaire-actions']"));
        Assert.Contains("เลือกประเภทการตรวจ", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("อื่น ๆ · Other", cut.Markup, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll("fieldset[name='scope'] [data-slot='questionnaire-input']"));

        cut.Find("input[value='other']").Change(true);
        var custom = cut.Find("fieldset[name='scope'] [data-slot='questionnaire-input'][data-custom='true']");
        Assert.Equal("ระบุประเภทงาน · Describe the work", custom.GetAttribute("aria-label"));
        Assert.Equal("พิมพ์คำตอบ · Type your answer", custom.GetAttribute("placeholder"));
    }

    [Fact]
    public void QuestionnaireDossierSourceIsCompleteBilingualAndTracksControls()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog())
            .GetBySlug("questionnaire").Single();

        Assert.Contains("new(\"other\", Custom: true)", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("อื่น ๆ · Other", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnQuestionnaireInput", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("DefaultItem=\"scope\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Name=\"notes\"", example.RazorSource, StringComparison.Ordinal);

        example.Controls.Single(control => control.Id == "questionnaire-start").Apply("notes");
        Assert.Contains("DefaultItem=\"notes\"", example.RazorSource, StringComparison.Ordinal);
        example.Controls.Single(control => control.Id == "questionnaire-branch").Apply("False");
        Assert.DoesNotContain("Name=\"notes\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("DefaultItem=\"scope\"", example.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void DocumentationRouteLinksEveryExactPlanNinePinnedSource()
    {
        var route = File.ReadAllText(Path.Combine(FindRoot(), "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "Docs", "ComponentDocumentation.razor"));
        foreach (var slug in Slugs) Assert.Contains($"bases/base/ui/{slug}.tsx", route, StringComparison.Ordinal);
    }

    private static string FindRoot() { var directory = new DirectoryInfo(AppContext.BaseDirectory); while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent; return directory?.FullName ?? throw new DirectoryNotFoundException(); }
}
