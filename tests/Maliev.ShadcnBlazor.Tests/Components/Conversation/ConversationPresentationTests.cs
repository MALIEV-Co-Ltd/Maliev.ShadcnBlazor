using Bunit;
using Maliev.ShadcnBlazor.Components.Conversation;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Tests.Components.Conversation;

public sealed class ConversationPresentationTests : BunitContext
{
    [Theory]
    [InlineData(ShadcnBubbleVariant.Default, "default")]
    [InlineData(ShadcnBubbleVariant.Secondary, "secondary")]
    [InlineData(ShadcnBubbleVariant.Muted, "muted")]
    [InlineData(ShadcnBubbleVariant.Tinted, "tinted")]
    [InlineData(ShadcnBubbleVariant.Outline, "outline")]
    [InlineData(ShadcnBubbleVariant.Ghost, "ghost")]
    [InlineData(ShadcnBubbleVariant.Destructive, "destructive")]
    public void BubbleOwnsAllPinnedVariantsAndLogicalAlignment(ShadcnBubbleVariant variant, string value)
    {
        var cut = Render<ShadcnBubble>(p => p
            .Add(c => c.Variant, variant)
            .Add(c => c.Align, ShadcnLogicalAlign.End)
            .AddChildContent<ShadcnBubbleContent>(content => content.AddChildContent("ตรวจสอบแล้ว")));

        var bubble = cut.Find("[data-slot='bubble']");
        Assert.Equal(value, bubble.GetAttribute("data-variant"));
        Assert.Equal("end", bubble.GetAttribute("data-align"));
        Assert.Equal("ตรวจสอบแล้ว", cut.Find("[data-slot='bubble-content']").TextContent);
    }

    [Fact]
    public void BubbleDefaultsToGhostVariant()
    {
        var cut = Render<ShadcnBubble>(parameters => parameters.AddChildContent<ShadcnBubbleContent>(content => content.AddChildContent("Default bubble")));

        Assert.Equal("ghost", cut.Find("[data-slot='bubble']").GetAttribute("data-variant"));
    }

    [Fact]
    public void BubbleSupportsInteractiveContentAndAccessibleReactions()
    {
        var clicked = false;
        var cut = Render<ShadcnBubble>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnBubbleContent>(0);
            builder.AddAttribute(1, nameof(ShadcnBubbleContent.OnActivate), EventCallback.Factory.Create(this, () => clicked = true));
            builder.AddAttribute(2, nameof(ShadcnBubbleContent.ChildContent), Text("Reply"));
            builder.CloseComponent();
            builder.OpenComponent<ShadcnBubbleReactions>(3);
            builder.AddAttribute(4, nameof(ShadcnBubbleReactions.Side), ShadcnReactionSide.Top);
            builder.AddAttribute(5, nameof(ShadcnBubbleReactions.Align), ShadcnLogicalAlign.Start);
            builder.AddAttribute(6, nameof(ShadcnBubbleReactions.AccessibleName), "Reactions: thumbs up");
            builder.AddAttribute(7, nameof(ShadcnBubbleReactions.ChildContent), Text("👍"));
            builder.CloseComponent();
        }));

        var button = cut.Find("button[data-slot='bubble-content']");
        button.Click();
        Assert.True(clicked);
        var reactions = cut.Find("[data-slot='bubble-reactions']");
        Assert.Equal("group", reactions.GetAttribute("role"));
        Assert.Equal("top", reactions.GetAttribute("data-side"));
        Assert.Equal("start", reactions.GetAttribute("data-align"));
    }

    [Fact]
    public void BubbleReactionUsesAvatarPresentationAndAccessibleReactionName()
    {
        var cut = Render<ShadcnBubble>(parameters => parameters.AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnBubbleContent>(0);
            builder.AddAttribute(1, nameof(ShadcnBubbleContent.ChildContent), Text("Approved"));
            builder.CloseComponent();
            builder.OpenComponent<ShadcnBubbleReactions>(2);
            builder.AddAttribute(3, nameof(ShadcnBubbleReactions.AccessibleName), "Reactions to Approved");
            builder.AddAttribute(4, nameof(ShadcnBubbleReactions.ChildContent), (RenderFragment)(reactions =>
            {
                reactions.OpenComponent<ShadcnBubbleReaction>(0);
                reactions.AddAttribute(1, nameof(ShadcnBubbleReaction.AccessibleName), "Narin reacted with thumbs up");
                reactions.AddAttribute(2, nameof(ShadcnBubbleReaction.Fallback), "NS");
                reactions.CloseComponent();
            }));
            builder.CloseComponent();
        }));

        var reaction = cut.Find("[data-slot='bubble-reaction']");
        Assert.Equal("img", reaction.GetAttribute("role"));
        Assert.Equal("Narin reacted with thumbs up", reaction.GetAttribute("aria-label"));
        Assert.Equal("NS", reaction.QuerySelector("[data-slot='avatar-fallback']")?.TextContent);
    }

    [Fact]
    public void BubbleReactionOverflowRevealsAndHidesAdditionalReactions()
    {
        var cut = Render<ShadcnBubble>(parameters => parameters.AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnBubbleContent>(0);
            builder.AddAttribute(1, nameof(ShadcnBubbleContent.ChildContent), Text("Approved"));
            builder.CloseComponent();
            builder.OpenComponent<ShadcnBubbleReactions>(2);
            builder.AddAttribute(3, nameof(ShadcnBubbleReactions.ChildContent), (RenderFragment)(reactions =>
            {
                reactions.OpenComponent<ShadcnBubbleReactionOverflow>(0);
                reactions.AddAttribute(1, nameof(ShadcnBubbleReactionOverflow.Count), 2);
                reactions.AddAttribute(2, nameof(ShadcnBubbleReactionOverflow.ChildContent), (RenderFragment)(overflow =>
                {
                    overflow.OpenComponent<ShadcnBubbleReaction>(0);
                    overflow.AddAttribute(1, nameof(ShadcnBubbleReaction.AccessibleName), "Mali reacted with fire");
                    overflow.AddAttribute(2, nameof(ShadcnBubbleReaction.Fallback), "ML");
                    overflow.CloseComponent();
                }));
                reactions.CloseComponent();
            }));
            builder.CloseComponent();
        }));

        var trigger = cut.Find("button[data-slot='bubble-reaction-overflow-trigger']");
        Assert.Equal("+2", trigger.TextContent.Trim());
        Assert.Equal("false", trigger.GetAttribute("aria-expanded"));
        Assert.Empty(cut.FindAll("[data-slot='bubble-reaction-overflow-content']"));

        trigger.Click();
        Assert.Equal("true", trigger.GetAttribute("aria-expanded"));
        Assert.Single(cut.FindAll("[data-slot='bubble-reaction-overflow-content']"));
        Assert.Contains("Mali reacted with fire", cut.Markup, StringComparison.Ordinal);

        trigger.Click();
        Assert.Equal("false", trigger.GetAttribute("aria-expanded"));
        Assert.Empty(cut.FindAll("[data-slot='bubble-reaction-overflow-content']"));
    }

    [Theory]
    [InlineData(ShadcnMarkerVariant.Default, "default")]
    [InlineData(ShadcnMarkerVariant.Separator, "separator")]
    [InlineData(ShadcnMarkerVariant.Border, "border")]
    public void MarkerOwnsVariantsAndDecorativeIcon(ShadcnMarkerVariant variant, string value)
    {
        var cut = Render<ShadcnMarker>(p => p
            .Add(c => c.Variant, variant)
            .Add(c => c.Live, true)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnMarkerIcon>(0);
                builder.AddAttribute(1, nameof(ShadcnMarkerIcon.ChildContent), Text("✓"));
                builder.CloseComponent();
                builder.OpenComponent<ShadcnMarkerContent>(2);
                builder.AddAttribute(3, nameof(ShadcnMarkerContent.Streaming), true);
                builder.AddAttribute(4, nameof(ShadcnMarkerContent.ChildContent), Text("กำลังประมวลผล"));
                builder.CloseComponent();
            }));

        var marker = cut.Find("[data-slot='marker']");
        Assert.Equal(value, marker.GetAttribute("data-variant"));
        Assert.Equal("true", marker.GetAttribute("data-live"));
        Assert.Equal("status", marker.GetAttribute("role"));
        Assert.Equal("polite", marker.GetAttribute("aria-live"));
        Assert.Equal("true", cut.Find("[data-slot='marker-icon']").GetAttribute("aria-hidden"));
        Assert.Equal("true", cut.Find("[data-slot='marker-icon']").GetAttribute("data-streaming"));
        Assert.Equal("true", cut.Find("[data-slot='marker-content']").GetAttribute("data-streaming"));
    }

    [Fact]
    public void MarkerCanOwnNativeLinkOrButtonActivationWithoutNestedControls()
    {
        var link = Render<ShadcnMarker>(p => p.Add(c => c.Href, "/activity/1").Add(c => c.AccessibleName, "Open activity").AddChildContent<ShadcnMarkerContent>(content => content.AddChildContent("Activity")));
        Assert.Equal("/activity/1", link.Find("a[data-slot='marker']").GetAttribute("href"));
        var invoked = 0;
        var button = Render<ShadcnMarker>(p => p.Add(c => c.OnActivate, () => invoked++).Add(c => c.AccessibleName, "Retry activity").AddChildContent<ShadcnMarkerContent>(content => content.AddChildContent("Retry")));
        button.Find("button[data-slot='marker']").Click();
        Assert.Equal(1, invoked);
        Assert.ThrowsAny<Exception>(() => Render<ShadcnMarker>(p => p.Add(c => c.Href, "/x").Add(c => c.OnActivate, () => { }).Add(c => c.AccessibleName, "Bad").AddChildContent("x")));
    }

    [Fact]
    public void MessageRendersExactRowCompositionAndEndAlignedFooter()
    {
        var cut = Render<ShadcnMessage>(p => p
            .Add(c => c.Align, ShadcnLogicalAlign.End)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnMessageAvatar>(0); builder.AddAttribute(1, nameof(ShadcnMessageAvatar.ChildContent), Text("ม")); builder.CloseComponent();
                builder.OpenComponent<ShadcnMessageContent>(2);
                builder.AddAttribute(3, nameof(ShadcnMessageContent.ChildContent), (RenderFragment)(content =>
                {
                    content.OpenComponent<ShadcnMessageHeader>(0); content.AddAttribute(1, nameof(ShadcnMessageHeader.ChildContent), Text("มาลีฟ")); content.CloseComponent();
                    content.OpenComponent<ShadcnBubble>(2); content.AddAttribute(3, nameof(ShadcnBubble.ChildContent), (RenderFragment)(bubble => { bubble.OpenComponent<ShadcnBubbleContent>(0); bubble.AddAttribute(1, nameof(ShadcnBubbleContent.ChildContent), Text("พร้อมแล้ว")); bubble.CloseComponent(); })); content.CloseComponent();
                    content.OpenComponent<ShadcnMessageFooter>(4); content.AddAttribute(5, nameof(ShadcnMessageFooter.ChildContent), Text("ส่งแล้ว")); content.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        Assert.Equal("end", cut.Find("[data-slot='message']").GetAttribute("data-align"));
        Assert.Equal("ม", cut.Find("[data-slot='message-avatar']").TextContent);
        Assert.Equal("มาลีฟ", cut.Find("[data-slot='message-header']").TextContent);
        Assert.Equal("ส่งแล้ว", cut.Find("[data-slot='message-footer']").TextContent);
    }

    [Fact]
    public void MessageStructuredCompositionKeepsActionsAndStatusInSeparateSlots()
    {
        var cut = Render<ShadcnMessage>(parameters => parameters
            .Add(component => component.Align, ShadcnLogicalAlign.End)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnMessageAvatar>(0);
                builder.AddAttribute(1, nameof(ShadcnMessageAvatar.ChildContent), (RenderFragment)(avatar =>
                {
                    avatar.OpenElement(0, "img");
                    avatar.AddAttribute(1, "src", "avatar.png");
                    avatar.AddAttribute(2, "alt", "Operator");
                    avatar.CloseElement();
                }));
                builder.CloseComponent();
                builder.OpenComponent<ShadcnMessageContent>(2);
                builder.AddAttribute(3, nameof(ShadcnMessageContent.ChildContent), (RenderFragment)(content =>
                {
                    content.OpenComponent<ShadcnMessageBody>(0);
                    content.AddAttribute(1, nameof(ShadcnMessageBody.ChildContent), Text("Message body"));
                    content.CloseComponent();
                    content.OpenComponent<ShadcnMessageFooter>(2);
                    content.AddAttribute(3, nameof(ShadcnMessageFooter.ChildContent), (RenderFragment)(footer =>
                    {
                        footer.OpenComponent<ShadcnMessageActions>(0);
                        footer.AddAttribute(1, nameof(ShadcnMessageActions.ChildContent), Text("Actions"));
                        footer.CloseComponent();
                        footer.OpenComponent<ShadcnMessageStatus>(2);
                        footer.AddAttribute(3, nameof(ShadcnMessageStatus.ChildContent), Text("Sent"));
                        footer.CloseComponent();
                    }));
                    content.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        Assert.Equal("Message body", cut.Find("[data-slot='message-body']").TextContent);
        Assert.Equal("Actions", cut.Find("[data-slot='message-actions']").TextContent);
        Assert.Equal("Sent", cut.Find("[data-slot='message-status']").TextContent);
        Assert.Equal("Operator", cut.Find("[data-slot='message-avatar'] img").GetAttribute("alt"));
    }

    [Fact]
    public void MessageCopyActionShowsSuccessOnlyAfterClipboardWriteAndCanCopyAgain()
    {
        JSInterop.SetupVoid("navigator.clipboard.writeText", "ตรวจสอบแล้ว").SetVoidResult();
        var copied = 0;
        var cut = RenderMessageFooter(builder =>
        {
            builder.OpenComponent<ShadcnMessageCopyAction>(0);
            builder.AddAttribute(1, nameof(ShadcnMessageCopyAction.Text), "ตรวจสอบแล้ว");
            builder.AddAttribute(2, nameof(ShadcnMessageCopyAction.FeedbackDuration), TimeSpan.FromMilliseconds(200));
            builder.AddAttribute(3, nameof(ShadcnMessageCopyAction.OnCopied), EventCallback.Factory.Create(this, () => copied++));
            builder.CloseComponent();
        });

        var action = cut.Find("button[data-slot='message-copy-action']");
        Assert.Equal("idle", action.GetAttribute("data-copy-state"));
        Assert.Contains("--shadcn-message-copy-feedback-duration:200ms", action.GetAttribute("style"), StringComparison.Ordinal);
        action.Click();
        cut.WaitForAssertion(() => Assert.Equal("copied", action.GetAttribute("data-copy-state")));
        Assert.Equal("Copied", action.GetAttribute("aria-label"));
        Assert.Equal(1, copied);

        cut.WaitForAssertion(() => Assert.Equal("idle", action.GetAttribute("data-copy-state")), TimeSpan.FromSeconds(1));
        action.Click();
        cut.WaitForAssertion(() => Assert.Equal("copied", action.GetAttribute("data-copy-state")));
        Assert.Equal(2, copied);
        Assert.Equal(2, JSInterop.Invocations.Count(invocation => invocation.Identifier == "navigator.clipboard.writeText"));
    }

    [Fact]
    public void MessageReplyActionReturnsQuoteAndReplyQuoteDismisses()
    {
        string? selectedQuote = null;
        var action = RenderMessageFooter(builder =>
        {
            builder.OpenComponent<ShadcnMessageReplyAction>(0);
            builder.AddAttribute(1, nameof(ShadcnMessageReplyAction.Quote), "Previous reply");
            builder.AddAttribute(2, nameof(ShadcnMessageReplyAction.OnReply), EventCallback.Factory.Create<string>(this, quote => selectedQuote = quote));
            builder.CloseComponent();
        });

        action.Find("button[data-slot='message-reply-action']").Click();
        Assert.Equal("Previous reply", selectedQuote);

        var dismissed = false;
        var quote = Render<ShadcnMessageReplyQuote>(parameters => parameters
            .Add(component => component.Quote, selectedQuote!)
            .Add(component => component.OnDismiss, EventCallback.Factory.Create(this, () => dismissed = true)));
        Assert.Equal("note", quote.Find("[data-slot='message-reply-quote']").GetAttribute("role"));
        Assert.Contains("Previous reply", quote.Markup, StringComparison.Ordinal);
        quote.Find("button[data-slot='message-reply-dismiss']").Click();
        Assert.True(dismissed);
    }

    [Fact]
    public void MessageCopyActionReportsClipboardFailureWithoutShowingSuccess()
    {
        JSInterop.SetupVoid("navigator.clipboard.writeText", "Restricted").SetException(new Microsoft.JSInterop.JSException("Clipboard denied."));
        var copied = false;
        var cut = RenderMessageFooter(builder =>
        {
            builder.OpenComponent<ShadcnMessageCopyAction>(0);
            builder.AddAttribute(1, nameof(ShadcnMessageCopyAction.Text), "Restricted");
            builder.AddAttribute(2, nameof(ShadcnMessageCopyAction.FeedbackDuration), TimeSpan.FromSeconds(1));
            builder.AddAttribute(3, nameof(ShadcnMessageCopyAction.OnCopied), EventCallback.Factory.Create(this, () => copied = true));
            builder.CloseComponent();
        });

        var action = cut.Find("button[data-slot='message-copy-action']");
        action.Click();
        cut.WaitForAssertion(() => Assert.Equal("error", action.GetAttribute("data-copy-state")));
        Assert.Equal("Copy failed", action.GetAttribute("aria-label"));
        Assert.DoesNotContain("Copied", cut.Markup, StringComparison.Ordinal);
        Assert.False(copied);
    }

    [Fact]
    public void PresentationPartsRejectUnknownEnumsAndInvalidParents()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnBubble>());
        Assert.ThrowsAny<Exception>(() => Render<ShadcnBubble>(p => p.Add(c => c.Variant, (ShadcnBubbleVariant)99).AddChildContent("x")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnBubbleContent>(p => p.AddChildContent("orphan")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnMarkerIcon>(p => p.AddChildContent("orphan")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnMessageFooter>(p => p.AddChildContent("orphan")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnMessageBody>(p => p.AddChildContent("orphan")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnMessageActions>(p => p.AddChildContent("orphan")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnMessageStatus>(p => p.AddChildContent("orphan")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnMessageReplyAction>(p => p.Add(c => c.Quote, "orphan")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnBubbleReaction>(p => p.Add(c => c.AccessibleName, "Orphan reaction").Add(c => c.Fallback, "OR")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnBubbleReactionOverflow>(p => p.Add(c => c.Count, 2).AddChildContent("Orphan reactions")));
    }

    [Fact]
    public void ConversationCssIncludesLogicalForcedColorAndMotionContracts()
    {
        var css = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-conversation.css"));
        Assert.Contains("[data-align=\"end\"]", css, StringComparison.Ordinal);
        Assert.Contains("inset-inline", css, StringComparison.Ordinal);
        Assert.Contains("prefers-reduced-motion", css, StringComparison.Ordinal);
        Assert.Contains("forced-colors", css, StringComparison.Ordinal);
    }

    [Fact]
    public void MessageFooterSupportsHoverFocusAndAlwaysVisibleModes()
    {
        var css = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-conversation.css"));

        Assert.Contains(".shadcn-message:hover .shadcn-message-footer", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-message:focus-within .shadcn-message-footer", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-message-footer[data-visibility=\"always\"]", css, StringComparison.Ordinal);
        Assert.Contains("pointer-events: none;", css, StringComparison.Ordinal);
        Assert.Contains("--shadcn-message-copy-feedback-duration", css, StringComparison.Ordinal);
        Assert.Contains("82%", css, StringComparison.Ordinal);
    }

    [Fact]
    public void BubbleCssScopesVariantsAndPreservesChatTailShape()
    {
        var css = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-conversation.css"));

        Assert.Contains(".shadcn-bubble[data-variant=\"secondary\"] > .shadcn-bubble-content", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-bubble[data-variant=\"tinted\"] > .shadcn-bubble-content", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-bubble[data-variant=\"destructive\"] > .shadcn-bubble-content", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-bubble[data-align=\"start\"] > .shadcn-bubble-content", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-bubble[data-align=\"end\"] > .shadcn-bubble-content", css, StringComparison.Ordinal);
        Assert.Contains("border-end-start-radius", css, StringComparison.Ordinal);
        Assert.Contains("border-end-end-radius", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-bubble-reaction > .shadcn-avatar", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-bubble-reaction-overflow-trigger:focus-visible", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-bubble[data-variant=\"ghost\"] { width:fit-content; max-width:80%; }", css, StringComparison.Ordinal);
        Assert.Contains(":has(> [data-slot=\"bubble-reactions\"][data-side=\"top\"])", css, StringComparison.Ordinal);
        Assert.Contains("color:var(--shadcn-muted-foreground)", css, StringComparison.Ordinal);
        Assert.Contains("mask:linear-gradient(-60deg,#000 30%,#0005,#000 70%) right / 350% 100%", css, StringComparison.Ordinal);
        Assert.Contains("-webkit-mask:linear-gradient(-60deg,#000 30%,#0005,#000 70%) right / 350% 100%", css, StringComparison.Ordinal);
        Assert.Contains("@keyframes shadcn-marker-wave { from { mask-position:right; -webkit-mask-position:right; } to { mask-position:left; -webkit-mask-position:left; } }", css, StringComparison.Ordinal);
        Assert.DoesNotContain("background-clip:text", css, StringComparison.Ordinal);
        Assert.DoesNotContain("-webkit-text-fill-color:transparent", css, StringComparison.Ordinal);
        Assert.Contains("-webkit-text-fill-color:CanvasText", css, StringComparison.Ordinal);
    }

    private static RenderFragment Text(string value) => builder => builder.AddContent(0, value);

    private IRenderedComponent<ShadcnMessage> RenderMessageFooter(RenderFragment footerContent)
        => Render<ShadcnMessage>(parameters => parameters.AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnMessageContent>(0);
            builder.AddAttribute(1, nameof(ShadcnMessageContent.ChildContent), (RenderFragment)(content =>
            {
                content.OpenComponent<ShadcnMessageBody>(0);
                content.AddAttribute(1, nameof(ShadcnMessageBody.ChildContent), Text("Message"));
                content.CloseComponent();
                content.OpenComponent<ShadcnMessageFooter>(2);
                content.AddAttribute(3, nameof(ShadcnMessageFooter.ChildContent), (RenderFragment)(footer =>
                {
                    footer.OpenComponent<ShadcnMessageActions>(0);
                    footer.AddAttribute(1, nameof(ShadcnMessageActions.ChildContent), footerContent);
                    footer.CloseComponent();
                }));
                content.CloseComponent();
            }));
            builder.CloseComponent();
        }));
}
