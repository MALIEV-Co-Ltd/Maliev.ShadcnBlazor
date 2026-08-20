using Bunit;
using Maliev.ShadcnBlazor.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Maliev.ShadcnBlazor.Tests.Components.DisclosureNavigation;

public sealed class ResizableScrollAreaTests : BunitContext
{
    public ResizableScrollAreaTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-disclosure-navigation.js");
        module.SetupVoid("attachResizableHandle", _ => true);
        module.SetupVoid("detachResizableHandle", _ => true);
        module.SetupVoid("syncScrollArea", _ => true);
        module.SetupVoid("detachScrollArea", _ => true);
    }

    [Fact]
    public void ResizableRendersConstrainedPercentPanelsAndSeparatorSemantics()
    {
        var cut = RenderResizable([40d, 60d]);
        var panels = cut.FindAll("[data-slot='resizable-panel']");
        var handle = cut.Find("[data-slot='resizable-handle']");

        Assert.Contains("--shadcn-resizable-size: 40%", panels[0].GetAttribute("style"), StringComparison.Ordinal);
        Assert.Equal("separator", handle.GetAttribute("role"));
        Assert.Equal("vertical", handle.GetAttribute("aria-orientation"));
        Assert.Equal("40", handle.GetAttribute("aria-valuenow"));
        Assert.Equal("10", handle.GetAttribute("aria-valuemin"));
        Assert.Equal("80", handle.GetAttribute("aria-valuemax"));
        Assert.Equal("0", handle.GetAttribute("tabindex"));
    }

    [Fact]
    public void ResizableKeyboardRequestsClampedPairwiseSizesAndSuppressesDisabled()
    {
        IReadOnlyList<double>? requested = null;
        var cut = RenderResizable([40d, 60d], sizes => requested = sizes);
        cut.Find("[data-slot='resizable-handle']").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        Assert.Equal([45d, 55d], requested);

        requested = null;
        var disabled = RenderResizable([40d, 60d], sizes => requested = sizes, disabled: true);
        disabled.Find("[data-slot='resizable-handle']").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        Assert.Null(requested);
        Assert.Equal("-1", disabled.Find("[data-slot='resizable-handle']").GetAttribute("tabindex"));
    }

    [Fact]
    public void ResizableValidatesLayoutContracts()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnResizableGroup>(p => p.Add(x => x.KeyboardStep, 0)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnResizableGroup>(p => p.Add(x => x.Direction, (ShadcnResizableDirection)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnResizablePanel>(p => p.Add(x => x.MinimumSize, 80).Add(x => x.MaximumSize, 20)));
    }

    [Fact]
    public void ResizableCollapsiblePanelsReachCollapsedSizeAndRuntimeConstraintsUpdate()
    {
        IReadOnlyList<double>? requested = null;
        var cut = RenderResizable([40d, 60d], sizes => requested = sizes, collapsible: true);
        cut.Find("[data-slot='resizable-handle']").KeyDown(new KeyboardEventArgs { Key = "Home" });
        Assert.Equal([0d, 100d], requested);

        cut.Render(p => p.Add(x => x.Sizes, new[] { 40d, 60d }).Add(x => x.SizesChanged, EventCallback.Factory.Create<IReadOnlyList<double>>(this, sizes => requested = sizes)).AddChildContent(builder =>
        {
            AddPanel(builder, 0, "renamed", 30, 70);
            builder.OpenComponent<ShadcnResizableHandle>(10); builder.CloseComponent();
            AddPanel(builder, 20, "secondary", 20, 90);
        }));
        Assert.Equal("30", cut.Find("[data-slot='resizable-handle']").GetAttribute("aria-valuemin"));
    }

    [Fact]
    public void ResizableRightPanelCollapsesAndHandleResolvesLiveLeftIdentity()
    {
        IReadOnlyList<double>? requested = null;
        var cut = RenderResizable([60d, 40d], sizes => requested = sizes, rightCollapsible: true);
        cut.Find("[data-slot='resizable-handle']").KeyDown(new KeyboardEventArgs { Key = "End" });
        Assert.Equal([100d, 0d], requested);
        Assert.Equal("primary", cut.Find("[data-slot='resizable-handle']").GetAttribute("data-left-panel-id"));
    }

    [Fact]
    public async Task ResizableRightPanelSnapsAcrossItsSubMinimumRange()
    {
        IReadOnlyList<double>? requested = null;
        var cut = RenderResizable([80d, 20d], sizes => requested = sizes, rightCollapsible: true);

        await cut.Instance.ResizeFromPointerAsync(0, 15);

        Assert.Equal([100d, 0d], requested);
    }

    [Fact]
    public async Task ResizableUnboundPointerUpdatesRerenderPanelsAndSeparatorValue()
    {
        var cut = RenderResizable([40d, 60d]);

        await cut.InvokeAsync(() => cut.Instance.ResizeFromPointerByPanelIdAsync("primary", 10));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("--shadcn-resizable-size: 50%", cut.FindAll("[data-slot='resizable-panel']")[0].GetAttribute("style"), StringComparison.Ordinal);
            Assert.Equal("50", cut.Find("[data-slot='resizable-handle']").GetAttribute("aria-valuenow"));
        });
    }

    [Fact]
    public void ResizableReattachesPointerInteractionWhenDirectionChanges()
    {
        var cut = RenderResizable([40d, 60d]);

        cut.Render(parameters => parameters
            .Add(x => x.Sizes, new[] { 40d, 60d })
            .Add(x => x.Direction, ShadcnResizableDirection.Vertical)
            .AddChildContent(builder =>
            {
                AddPanel(builder, 0, "primary", 10, 80);
                builder.OpenComponent<ShadcnResizableHandle>(10);
                builder.AddAttribute(11, nameof(ShadcnResizableHandle.WithHandle), true);
                builder.CloseComponent();
                AddPanel(builder, 20, "secondary", 20, 90);
            }));

        var attachments = JSInterop.Invocations
            .Where(invocation => invocation.Identifier == "attachResizableHandle")
            .ToArray();
        Assert.Equal(2, attachments.Length);
        Assert.Equal("horizontal", attachments[0].Arguments[2]);
        Assert.Equal("vertical", attachments[1].Arguments[2]);
        var source = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "Components", "Layout", "ShadcnResizableHandle.razor"));
        Assert.Contains("InvokeVoidAsync(\"detachResizableHandle\"", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ResizableUsesAnAxisLockedPointerDeltaAndCompactVisualGrip()
    {
        var root = FindRoot();
        var script = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "js", "shadcn-disclosure-navigation.js"));
        Assert.Contains("const coordinate = direction === 'horizontal' ? event.clientX : event.clientY", script, StringComparison.Ordinal);
        Assert.Contains("touchAction", script, StringComparison.Ordinal);

        var css = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-disclosure-navigation.css"));
        Assert.Contains("--shadcn-resizable-hit-target", css, StringComparison.Ordinal);
        Assert.Contains("grid-template-columns: repeat(2, 2px)", css, StringComparison.Ordinal);
        Assert.Contains("grid-template-rows: repeat(3, 2px)", css, StringComparison.Ordinal);
    }

    [Fact]
    public void ScrollAreaUsesNativeFocusableViewportAndBothLogicalScrollbars()
    {
        var cut = Render<ShadcnScrollArea>(p => p
            .Add(x => x.Type, ShadcnScrollAreaType.Always)
            .Add(x => x.HideDelay, 900)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnScrollAreaViewport>(0);
                builder.AddAttribute(1, nameof(ShadcnScrollAreaViewport.Label), "รายการวัสดุ");
                builder.AddAttribute(2, nameof(ShadcnScrollAreaViewport.ChildContent), (RenderFragment)(content => content.AddContent(0, new string('ก', 200))));
                builder.CloseComponent();
                AddScrollbar(builder, 10, ShadcnScrollAreaOrientation.Vertical);
                AddScrollbar(builder, 20, ShadcnScrollAreaOrientation.Horizontal);
                builder.OpenComponent<ShadcnScrollAreaCorner>(30); builder.CloseComponent();
            }));

        var root = cut.Find("[data-slot='scroll-area']");
        Assert.Equal("always", root.GetAttribute("data-type"));
        Assert.Equal("900", root.GetAttribute("data-hide-delay"));
        var viewport = cut.Find("[data-slot='scroll-area-viewport']");
        Assert.Equal("0", viewport.GetAttribute("tabindex"));
        Assert.Equal("region", viewport.GetAttribute("role"));
        Assert.Equal("รายการวัสดุ", viewport.GetAttribute("aria-label"));
        Assert.Equal(2, cut.FindAll("[data-slot='scroll-area-scrollbar']").Count);
        Assert.All(cut.FindAll("[data-slot='scroll-area-thumb']"), thumb => Assert.Equal("true", thumb.GetAttribute("aria-hidden")));
        var script = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "js", "shadcn-disclosure-navigation.js"));
        Assert.Contains("data-overflow-x", script, StringComparison.Ordinal);
        Assert.Contains("setPointerCapture", script, StringComparison.Ordinal);
        Assert.Contains("scrollLeft", script, StringComparison.Ordinal);
        Assert.Contains("detectRtlScrollType", script, StringComparison.Ordinal);
        Assert.Contains("grabOffset", script, StringComparison.Ordinal);
        Assert.Contains("pointercancel", script, StringComparison.Ordinal);
        var css = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-disclosure-navigation.css"));
        Assert.Contains("--shadcn-scroll-area-y-ratio", css, StringComparison.Ordinal);
    }

    [Fact]
    public void ScrollAreaValidatesTypeDelayAndOrientation()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnScrollArea>(p => p.Add(x => x.Type, (ShadcnScrollAreaType)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnScrollArea>(p => p.Add(x => x.HideDelay, -1)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnScrollAreaScrollbar>(p => p.Add(x => x.Orientation, (ShadcnScrollAreaOrientation)999)));
    }

    private IRenderedComponent<ShadcnResizableGroup> RenderResizable(IReadOnlyList<double> sizes, Action<IReadOnlyList<double>>? changed = null, bool disabled = false, bool collapsible = false, bool rightCollapsible = false) => Render<ShadcnResizableGroup>(p => p
        .Add(x => x.Sizes, sizes)
        .Add(x => x.SizesChanged, changed is null ? default : EventCallback.Factory.Create(this, changed))
        .Add(x => x.Disabled, disabled)
        .Add(x => x.KeyboardStep, 5)
        .AddChildContent(builder =>
        {
            AddPanel(builder, 0, "primary", 10, rightCollapsible ? 100 : 80, collapsible);
            builder.OpenComponent<ShadcnResizableHandle>(10);
            builder.AddAttribute(11, nameof(ShadcnResizableHandle.Label), "ปรับความกว้าง");
            builder.AddAttribute(12, nameof(ShadcnResizableHandle.WithHandle), true);
            builder.CloseComponent();
            AddPanel(builder, 20, "secondary", collapsible ? 0 : 20, collapsible || rightCollapsible ? 100 : 90, rightCollapsible);
        }));

    private static void AddPanel(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder, int sequence, string id, double min, double max, bool collapsible = false)
    {
        builder.OpenComponent<ShadcnResizablePanel>(sequence);
        builder.AddAttribute(sequence + 1, nameof(ShadcnResizablePanel.Id), id);
        builder.AddAttribute(sequence + 2, nameof(ShadcnResizablePanel.MinimumSize), min);
        builder.AddAttribute(sequence + 3, nameof(ShadcnResizablePanel.MaximumSize), max);
        builder.AddAttribute(sequence + 5, nameof(ShadcnResizablePanel.Collapsible), collapsible);
        builder.AddAttribute(sequence + 4, nameof(ShadcnResizablePanel.ChildContent), (RenderFragment)(content => content.AddContent(0, id)));
        builder.CloseComponent();
    }

    private static string FindRoot() { var directory = new DirectoryInfo(AppContext.BaseDirectory); while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent; return directory?.FullName ?? throw new DirectoryNotFoundException(); }

    private static void AddScrollbar(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder, int sequence, ShadcnScrollAreaOrientation orientation)
    {
        builder.OpenComponent<ShadcnScrollAreaScrollbar>(sequence);
        builder.AddAttribute(sequence + 1, nameof(ShadcnScrollAreaScrollbar.Orientation), orientation);
        builder.AddAttribute(sequence + 2, nameof(ShadcnScrollAreaScrollbar.ChildContent), (RenderFragment)(content => { content.OpenComponent<ShadcnScrollAreaThumb>(0); content.CloseComponent(); }));
        builder.CloseComponent();
    }
}
