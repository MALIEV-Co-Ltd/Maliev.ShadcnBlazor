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
        Assert.Equal("img", reactions.GetAttribute("role"));
        Assert.Equal("top", reactions.GetAttribute("data-side"));
        Assert.Equal("start", reactions.GetAttribute("data-align"));
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
        Assert.Equal("status", marker.GetAttribute("role"));
        Assert.Equal("polite", marker.GetAttribute("aria-live"));
        Assert.Equal("true", cut.Find("[data-slot='marker-icon']").GetAttribute("aria-hidden"));
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
    public void PresentationPartsRejectUnknownEnumsAndInvalidParents()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnBubble>());
        Assert.ThrowsAny<Exception>(() => Render<ShadcnBubble>(p => p.Add(c => c.Variant, (ShadcnBubbleVariant)99).AddChildContent("x")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnBubbleContent>(p => p.AddChildContent("orphan")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnMarkerIcon>(p => p.AddChildContent("orphan")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnMessageFooter>(p => p.AddChildContent("orphan")));
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

    private static RenderFragment Text(string value) => builder => builder.AddContent(0, value);
}
