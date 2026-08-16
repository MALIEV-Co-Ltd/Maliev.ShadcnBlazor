using Bunit;
using Maliev.ShadcnBlazor.Components.Conversation;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace Maliev.ShadcnBlazor.Tests.Components.Conversation;

public sealed class MessageScrollerTests : BunitContext
{
    [Fact]
    public void ScrollerRendersPinnedCompositionAndAccessibleScrollControls()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = Render<ShadcnMessageScrollerProvider>(p => p
            .Add(c => c.AutoScroll, true)
            .Add(c => c.ChildContent, (RenderFragment)(builder =>
            {
                builder.OpenComponent<ShadcnMessageScroller>(0);
                builder.AddAttribute(1, nameof(ShadcnMessageScroller.ChildContent), (RenderFragment)(root =>
                {
                    root.OpenComponent<ShadcnMessageScrollerViewport>(0);
                    root.AddAttribute(1, nameof(ShadcnMessageScrollerViewport.AccessibleName), "บทสนทนา");
                    root.AddAttribute(2, nameof(ShadcnMessageScrollerViewport.ChildContent), (RenderFragment)(viewport =>
                    {
                        viewport.OpenComponent<ShadcnMessageScrollerContent>(0);
                        viewport.AddAttribute(1, nameof(ShadcnMessageScrollerContent.ChildContent), (RenderFragment)(content =>
                        {
                            content.OpenComponent<ShadcnMessageScrollerItem>(0);
                            content.AddAttribute(1, nameof(ShadcnMessageScrollerItem.MessageId), "turn-1");
                            content.AddAttribute(2, nameof(ShadcnMessageScrollerItem.ScrollAnchor), true);
                            content.AddAttribute(3, nameof(ShadcnMessageScrollerItem.ChildContent), Text("สวัสดี"));
                            content.CloseComponent();
                        }));
                        viewport.CloseComponent();
                    }));
                    root.CloseComponent();
                    root.OpenComponent<ShadcnMessageScrollerButton>(3);
                    root.AddAttribute(4, nameof(ShadcnMessageScrollerButton.Direction), ShadcnMessageScrollDirection.End);
                    root.AddAttribute(5, nameof(ShadcnMessageScrollerButton.AccessibleName), "ไปข้อความล่าสุด");
                    root.CloseComponent();
                }));
                builder.CloseComponent();
            })));

        Assert.NotNull(cut.Find("[data-slot='message-scroller']"));
        var viewport = cut.Find("[data-slot='message-scroller-viewport']");
        Assert.Equal("region", viewport.GetAttribute("role"));
        Assert.Equal("บทสนทนา", viewport.GetAttribute("aria-label"));
        Assert.Equal("polite", cut.Find("[data-slot='message-scroller-content']").GetAttribute("aria-live"));
        var item = cut.Find("[data-slot='message-scroller-item']");
        Assert.Equal("turn-1", item.GetAttribute("data-message-id"));
        Assert.Equal("true", item.GetAttribute("data-scroll-anchor"));
        Assert.Equal("ไปข้อความล่าสุด", cut.Find("[data-slot='message-scroller-button']").GetAttribute("aria-label"));
    }

    [Fact]
    public void PartsRejectInvalidCompositionAndIds()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnMessageScroller>());
        Assert.ThrowsAny<Exception>(() => Render<ShadcnMessageScrollerViewport>(p => p.Add(c => c.AccessibleName, "Messages")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnMessageScrollerItem>(p => p.Add(c => c.MessageId, " ").AddChildContent("bad")));
    }

    [Fact]
    public void BrowserModuleOwnsObserversIntentAndDisposalContracts()
    {
        var script = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Maliev.ShadcnBlazor", "wwwroot", "js", "shadcn-message-scroller.js"));
        Assert.Contains("ResizeObserver", script, StringComparison.Ordinal);
        Assert.Contains("IntersectionObserver", script, StringComparison.Ordinal);
        Assert.Contains("selectionchange", script, StringComparison.Ordinal);
        Assert.Contains("pointerdown", script, StringComparison.Ordinal);
        Assert.Contains("focusin", script, StringComparison.Ordinal);
        Assert.Contains("keydown", script, StringComparison.Ordinal);
        Assert.Contains("preserveScrollOnPrepend", script, StringComparison.Ordinal);
        Assert.Contains("dispose", script, StringComparison.Ordinal);
        Assert.Contains("data-autoscrolling", script, StringComparison.Ordinal);
        Assert.Contains("selection?.anchorNode", script, StringComparison.Ordinal);
        Assert.Contains("sequence", script, StringComparison.Ordinal);
        Assert.Contains("root.setAttribute('data-autoscrolling'", script, StringComparison.Ordinal);
        Assert.DoesNotContain("viewport.scrollTop +=", script, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProviderRejectsStaleSnapshotsAndRefreshesDynamicOptions()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = Render<ShadcnMessageScrollerProvider>(p => p.Add(c => c.AutoScroll, false).Add(c => c.ChildContent, Text("content")));
        await cut.InvokeAsync(() => cut.Instance.OnScrollerMeasurementAsync(new(200, 200, 400, [new("row", 0, 400, false)], Sequence: 2)));
        var state = cut.Instance.CurrentState;
        await cut.InvokeAsync(() => cut.Instance.OnScrollerMeasurementAsync(new(0, 200, 400, [new("row", 0, 400, false)], Sequence: 1)));
        Assert.Equal(state, cut.Instance.CurrentState);
        cut.Render(p => p.Add(c => c.AutoScroll, true).Add(c => c.ChildContent, Text("content")));
        await cut.InvokeAsync(() => cut.Instance.OnScrollerMeasurementAsync(new(0, 200, 500, [new("row", 0, 500, false)], Sequence: 3)));
        Assert.False(cut.Instance.CurrentState.Following);
    }

    [Fact]
    public void ProviderExposesPublicQueuedCommandsAndAttachFailureSurface()
    {
        Assert.NotNull(typeof(ShadcnMessageScrollerProvider).GetMethod(nameof(ShadcnMessageScrollerProvider.ScrollToStartAsync)));
        Assert.NotNull(typeof(ShadcnMessageScrollerProvider).GetMethod(nameof(ShadcnMessageScrollerProvider.ScrollToEndAsync)));
        Assert.NotNull(typeof(ShadcnMessageScrollerProvider).GetMethod(nameof(ShadcnMessageScrollerProvider.ScrollToMessageAsync)));
        Assert.NotNull(typeof(ShadcnMessageScrollerProvider).GetProperty(nameof(ShadcnMessageScrollerProvider.AttachError)));
        Assert.NotNull(typeof(ShadcnMessageScrollerProvider).GetProperty(nameof(ShadcnMessageScrollerProvider.AttachFailed)));
    }

    [Fact]
    public void ProviderSurfacesInteropAttachFailureInsteadOfSilentlyDisablingCommands()
    {
        _ = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-message-scroller.js");
        string? failure = null;
        var cut = Render<ShadcnMessageScrollerProvider>(p => p.Add(c => c.AttachFailed, message => failure = message).Add(c => c.ChildContent, (RenderFragment)(builder =>
        {
            builder.OpenComponent<ShadcnMessageScroller>(0);
            builder.AddAttribute(1, nameof(ShadcnMessageScroller.ChildContent), (RenderFragment)(root =>
            {
                root.OpenComponent<ShadcnMessageScrollerViewport>(0);
                root.AddAttribute(1, nameof(ShadcnMessageScrollerViewport.AccessibleName), "Messages");
                root.AddAttribute(2, nameof(ShadcnMessageScrollerViewport.ChildContent), (RenderFragment)(viewport => { viewport.OpenComponent<ShadcnMessageScrollerContent>(0); viewport.AddAttribute(1, nameof(ShadcnMessageScrollerContent.ChildContent), Text("row")); viewport.CloseComponent(); }));
                root.CloseComponent();
            }));
            builder.CloseComponent();
        })));
        Assert.False(string.IsNullOrWhiteSpace(cut.Instance.AttachError));
        Assert.Equal(cut.Instance.AttachError, failure);
    }

    [Fact]
    public async Task PremountCommandQueuesAndDisposalCompletesItDeterministically()
    {
        var provider = new ShadcnMessageScrollerProvider();
        var command = provider.ScrollToEndAsync(focusViewport: true);
        Assert.False(command.IsCompleted);
        await provider.DisposeAsync();
        Assert.False(await command);
    }

    private static RenderFragment Text(string value) => builder => builder.AddContent(0, value);
}
