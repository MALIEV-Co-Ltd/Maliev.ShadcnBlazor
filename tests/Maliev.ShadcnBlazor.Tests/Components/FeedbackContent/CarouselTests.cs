using Bunit;
using Maliev.ShadcnBlazor.Components.Content;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Maliev.ShadcnBlazor.Tests.Components.FeedbackContent;

public sealed class CarouselTests : BunitContext
{
    public CarouselTests() => JSInterop.Mode = JSRuntimeMode.Loose;
    [Fact]
    public void CarouselRendersOfficialCompositionAndAccessibleLabels()
    {
        var cut = RenderCarousel(new ShadcnCarouselOptions(), 1);

        var root = cut.Find("[data-slot='carousel']");
        Assert.Equal("region", root.GetAttribute("role"));
        Assert.Equal("carousel", root.GetAttribute("aria-roledescription"));
        Assert.Equal("งานผลิต", root.GetAttribute("aria-label"));
        Assert.Equal("horizontal", root.GetAttribute("data-orientation"));
        Assert.Equal("slide", cut.FindAll("[data-slot='carousel-item']")[0].GetAttribute("aria-roledescription"));
        Assert.Equal("สไลด์ 1 จาก 3", cut.FindAll("[data-slot='carousel-item']")[0].GetAttribute("aria-label"));
        Assert.Equal("ก่อนหน้า", cut.Find("[data-slot='carousel-previous']").GetAttribute("aria-label"));
        Assert.Equal("ถัดไป", cut.Find("[data-slot='carousel-next']").GetAttribute("aria-label"));
    }

    [Fact]
    public async Task NextPreviousAndGoToClampAndRaiseControlledSelection()
    {
        var changes = new List<int>();
        var cut = RenderCarousel(new ShadcnCarouselOptions { SlidesToScroll = 2 }, 0, changes);

        cut.Find("[data-slot='carousel-next']").Click();
        Assert.Equal([2], changes);
        cut.Render(parameters => BaseParameters(parameters, new ShadcnCarouselOptions { SlidesToScroll = 2 }, 2, changes));
        cut.Find("[data-slot='carousel-next']").Click();
        Assert.Equal([2], changes);
        cut.Find("[data-slot='carousel-previous']").Click();
        Assert.Equal([2, 0], changes);

        await cut.Instance.GoToAsync(99);
        Assert.Equal([2, 0], changes);
    }

    [Fact]
    public void LoopWrapsAndDisabledSuppressesCallbacks()
    {
        var changes = new List<int>();
        var loop = RenderCarousel(new ShadcnCarouselOptions { Loop = true }, 2, changes);
        loop.Find("[data-slot='carousel-next']").Click();
        Assert.Equal([0], changes);

        var disabled = RenderCarousel(new ShadcnCarouselOptions { Loop = true }, 2, changes, disabled: true);
        disabled.Find("[data-slot='carousel-next']").Click();
        Assert.Equal([0], changes);
        Assert.True(disabled.Find("[data-slot='carousel-next']").HasAttribute("disabled"));
    }

    [Theory]
    [InlineData(ShadcnCarouselOrientation.Horizontal, "ArrowRight", 1)]
    [InlineData(ShadcnCarouselOrientation.Horizontal, "ArrowLeft", 0)]
    [InlineData(ShadcnCarouselOrientation.Vertical, "ArrowDown", 1)]
    [InlineData(ShadcnCarouselOrientation.Vertical, "ArrowUp", 0)]
    [InlineData(ShadcnCarouselOrientation.Vertical, "End", 2)]
    [InlineData(ShadcnCarouselOrientation.Vertical, "Home", 0)]
    public void KeyboardUsesOrientationAndBoundaries(ShadcnCarouselOrientation orientation, string key, int expected)
    {
        var changes = new List<int>();
        var cut = RenderCarousel(new ShadcnCarouselOptions(), key is "ArrowLeft" or "ArrowUp" ? 1 : 0, changes, orientation);
        cut.Find("[data-slot='carousel']").KeyDown(new KeyboardEventArgs { Key = key });
        if (key == "Home") Assert.Empty(changes);
        else Assert.Equal(expected, changes.Last());
    }

    [Fact]
    public void HorizontalRtlReversesPhysicalArrowMeaning()
    {
        var changes = new List<int>();
        var cut = RenderCarousel(new ShadcnCarouselOptions { RightToLeft = true }, 1, changes);
        cut.Find("[data-slot='carousel']").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        Assert.Equal(0, changes.Last());
        cut.Find("[data-slot='carousel']").KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });
        Assert.Equal(2, changes.Last());
    }

    [Fact]
    public void PointerSwipeUsesThresholdAndLogicalDirection()
    {
        var changes = new List<int>();
        var cut = RenderCarousel(new ShadcnCarouselOptions { DragThreshold = 30 }, 1, changes);
        var content = cut.Find("[data-slot='carousel-content']");
        content.PointerDown(new PointerEventArgs { ClientX = 100, ClientY = 20, PointerId = 1 });
        content.PointerUp(new PointerEventArgs { ClientX = 80, ClientY = 20, PointerId = 1 });
        Assert.Empty(changes);
        content.PointerDown(new PointerEventArgs { ClientX = 100, ClientY = 20, PointerId = 1 });
        content.PointerUp(new PointerEventArgs { ClientX = 50, ClientY = 20, PointerId = 1 });
        Assert.Equal(2, changes.Last());
    }

    [Theory]
    [InlineData(ShadcnCarouselOrientation.Horizontal, 48, 36, "--shadcn-carousel-drag-x: 48px", "--shadcn-carousel-drag-y")]
    [InlineData(ShadcnCarouselOrientation.Vertical, 48, 36, "--shadcn-carousel-drag-y: 36px", "--shadcn-carousel-drag-x")]
    public void PointerPreviewIsConstrainedToTheConfiguredAxis(
        ShadcnCarouselOrientation orientation,
        double clientX,
        double clientY,
        string activeAxis,
        string crossAxis)
    {
        var cut = RenderCarousel(new ShadcnCarouselOptions(), 0, orientation: orientation);
        var content = cut.Find("[data-slot='carousel-content']");

        content.PointerDown(new PointerEventArgs { ClientX = 0, ClientY = 0, PointerId = 7 });
        content.PointerMove(new PointerEventArgs { ClientX = clientX, ClientY = clientY, PointerId = 7 });

        var trackStyle = cut.Find("[data-slot='carousel-track']").GetAttribute("style");
        Assert.Contains(activeAxis, trackStyle, StringComparison.Ordinal);
        Assert.DoesNotContain(crossAxis, trackStyle, StringComparison.Ordinal);
    }

    [Fact]
    public void OptionsExposeAlignAndReducedMotionWithoutRawPluginLeakage()
    {
        var cut = RenderCarousel(new ShadcnCarouselOptions
        {
            Align = ShadcnCarouselAlign.Center,
            ReducedMotion = true
        }, 0);

        var root = cut.Find("[data-slot='carousel']");
        Assert.Equal("center", root.GetAttribute("data-align"));
        Assert.Equal("true", root.GetAttribute("data-reduced-motion"));
    }

    [Fact]
    public void VerticalViewportBlockSizeDefinesOneItemPerSnapGeometry()
    {
        var cut = Render<ShadcnCarousel>(parameters => parameters
            .Add(component => component.Orientation, ShadcnCarouselOrientation.Vertical)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnCarouselContent>(0);
                builder.AddAttribute(1, nameof(ShadcnCarouselContent.ViewportBlockSize), 240d);
                builder.AddAttribute(2, nameof(ShadcnCarouselContent.ChildContent), (RenderFragment)(items =>
                {
                    items.OpenComponent<ShadcnCarouselItem>(0);
                    items.AddAttribute(1, nameof(ShadcnCarouselItem.Index), 0);
                    items.AddAttribute(2, nameof(ShadcnCarouselItem.ChildContent), Text("One"));
                    items.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        Assert.Contains("--shadcn-carousel-viewport-block-size: 240px", cut.Find("[data-slot='carousel-content']").GetAttribute("style"), StringComparison.Ordinal);
        var css = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-feedback-content.css"));
        Assert.Contains("block-size: var(--shadcn-carousel-viewport-block-size)", css, StringComparison.Ordinal);
        Assert.Contains("flex-basis: var(--shadcn-carousel-viewport-block-size)", css, StringComparison.Ordinal);
        Assert.Contains("[data-slot=\"carousel-track\"] { block-size: 100%", css, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidViewportBlockSizeFailsClosed(double value)
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnCarousel>(parameters => parameters
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnCarouselContent>(0);
                builder.AddAttribute(1, nameof(ShadcnCarouselContent.ViewportBlockSize), value);
                builder.CloseComponent();
            })));
    }

    [Fact]
    public void UnboundSelectionMovesTrackAndSelectedSlideImmediately()
    {
        var cut = RenderCarousel(new ShadcnCarouselOptions(), 0);

        cut.Find("[data-slot='carousel-next']").Click();

        Assert.Contains("translate: -100% 0", cut.Find("[data-slot='carousel-track']").GetAttribute("style"), StringComparison.Ordinal);
        Assert.Null(cut.FindAll("[data-slot='carousel-item']")[0].GetAttribute("data-selected"));
        Assert.Equal("true", cut.FindAll("[data-slot='carousel-item']")[1].GetAttribute("data-selected"));
        Assert.Equal(1, ((IShadcnCarouselApi)cut.Instance).SelectedIndex);
    }

    [Fact]
    public void UnboundSelectionUpdatesPreviousAndNextButtonState()
    {
        var cut = RenderCarousel(new ShadcnCarouselOptions(), 0);
        var previous = cut.Find("[data-slot='carousel-previous']");
        var next = cut.Find("[data-slot='carousel-next']");

        Assert.True(previous.HasAttribute("disabled"));
        Assert.False(next.HasAttribute("disabled"));

        next.Click();

        Assert.False(previous.HasAttribute("disabled"));
        Assert.False(next.HasAttribute("disabled"));

        previous.Click();

        Assert.True(previous.HasAttribute("disabled"));
    }

    [Fact]
    public void SparsePublicItemIndicesUseOrderedRegistrationIdentity()
    {
        var cut = Render<ShadcnCarousel>(parameters => parameters
            .Add(component => component.SlideLabelFormatter, (index, count) => $"Slide {index + 1} of {count}")
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnCarouselContent>(0);
                builder.AddAttribute(1, nameof(ShadcnCarouselContent.ChildContent), (RenderFragment)(content =>
                {
                    content.OpenComponent<ShadcnCarouselItem>(0);
                    content.AddAttribute(1, nameof(ShadcnCarouselItem.Index), 10);
                    content.AddAttribute(2, nameof(ShadcnCarouselItem.ChildContent), Text("Ten"));
                    content.CloseComponent();
                    content.OpenComponent<ShadcnCarouselItem>(3);
                    content.AddAttribute(4, nameof(ShadcnCarouselItem.Index), 30);
                    content.AddAttribute(5, nameof(ShadcnCarouselItem.ChildContent), Text("Thirty"));
                    content.CloseComponent();
                }));
                builder.CloseComponent();
                builder.OpenComponent<ShadcnCarouselNext>(2);
                builder.CloseComponent();
            }));

        Assert.Equal(2, ((IShadcnCarouselApi)cut.Instance).Count);
        Assert.Equal("Slide 1 of 2", cut.FindAll("[data-slot='carousel-item']")[0].GetAttribute("aria-label"));
        cut.Find("[data-slot='carousel-next']").Click();
        Assert.Equal("true", cut.FindAll("[data-slot='carousel-item']")[1].GetAttribute("data-selected"));
    }

    [Fact]
    public async Task PluginReceivesInitializationSelectionAndDisposal()
    {
        var plugin = new RecordingPlugin();
        var cut = RenderCarousel(new ShadcnCarouselOptions(), 0, plugins: [plugin]);
        Assert.Equal(3, plugin.InitializedCount);
        cut.Find("[data-slot='carousel-next']").Click();
        Assert.Equal(1, plugin.SelectedIndex);
        await cut.Instance.DisposeAsync();
        Assert.True(plugin.Disposed);
    }

    [Fact]
    public void LiveAnnouncementWaitsForRegistrationAndReportsTheFinalOrderedCount()
    {
        var cut = RenderCarousel(new ShadcnCarouselOptions(), 0);

        var announcement = cut.Find("[data-slot='carousel-announcement']");
        Assert.Equal("สไลด์ 1 จาก 3", announcement.TextContent);
        Assert.Equal("polite", announcement.GetAttribute("aria-live"));
        Assert.Contains("shadcn-sr-only", announcement.ClassList);

        var css = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-base.css"));
        Assert.Contains(".shadcn-sr-only", css, StringComparison.Ordinal);
        Assert.Contains("clip: rect(0, 0, 0, 0)", css, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReinitializeRebuildsPluginLifecycle()
    {
        var plugin = new RecordingPlugin();
        var cut = RenderCarousel(new ShadcnCarouselOptions(), 0, plugins: [plugin]);

        await cut.InvokeAsync(() => cut.Instance.ReinitializeAsync().AsTask());
        cut.Render();

        Assert.Equal(2, plugin.InitializeCalls);
        Assert.Equal(1, plugin.DisposeCalls);
    }

    [Fact]
    public void AutoplayPluginUsesTimeProviderAndStopsAfterUserSelection()
    {
        var time = new CarouselTimeProvider();
        var autoplay = new ShadcnCarouselAutoplayPlugin(time, TimeSpan.FromSeconds(3));
        var changes = new List<int>();
        var cut = RenderCarousel(new ShadcnCarouselOptions { Loop = true }, 0, changes, plugins: [autoplay]);
        time.Advance(TimeSpan.FromSeconds(3));
        Assert.Equal([1], changes);
        cut.Render(parameters => BaseParameters(parameters, new ShadcnCarouselOptions { Loop = true }, 1, changes, plugins: [autoplay]));
        cut.Find("[data-slot='carousel-next']").Click();
        Assert.Equal([1, 2], changes);
        time.Advance(TimeSpan.FromSeconds(30));
        Assert.Equal([1, 2], changes);
    }

    [Fact]
    public void AutoplayDispatchesThroughRendererAndReportsCallbackFailures()
    {
        var time = new CarouselTimeProvider();
        var api = new FailingCarouselApi();
        var plugin = new ShadcnCarouselAutoplayPlugin(time, TimeSpan.FromSeconds(1));
        Exception? failure = null;
        plugin.Failed += exception => failure = exception;
        plugin.Initialize(api);
        time.Advance(TimeSpan.FromSeconds(1));
        Assert.True(api.Dispatched);
        Assert.IsType<InvalidOperationException>(failure);
    }

    [Fact]
    public void InvalidOptionsAndEnumsFailClosed()
    {
        Assert.ThrowsAny<Exception>(() => RenderCarousel(new ShadcnCarouselOptions { SlidesToScroll = 0 }, 0));
        Assert.ThrowsAny<Exception>(() => RenderCarousel(new ShadcnCarouselOptions { DragThreshold = -1 }, 0));
        Assert.ThrowsAny<Exception>(() => RenderCarousel(new ShadcnCarouselOptions { Align = (ShadcnCarouselAlign)999 }, 0));
        Assert.ThrowsAny<Exception>(() => RenderCarousel(new ShadcnCarouselOptions(), 0, orientation: (ShadcnCarouselOrientation)999));
    }

    private IRenderedComponent<ShadcnCarousel> RenderCarousel(
        ShadcnCarouselOptions options,
        int selected,
        List<int>? changes = null,
        ShadcnCarouselOrientation orientation = ShadcnCarouselOrientation.Horizontal,
        bool disabled = false,
        IReadOnlyList<IShadcnCarouselPlugin>? plugins = null) =>
        Render<ShadcnCarousel>(parameters => BaseParameters(parameters, options, selected, changes, orientation, disabled, plugins));

    private void BaseParameters(
        ComponentParameterCollectionBuilder<ShadcnCarousel> parameters,
        ShadcnCarouselOptions options,
        int selected,
        List<int>? changes,
        ShadcnCarouselOrientation orientation = ShadcnCarouselOrientation.Horizontal,
        bool disabled = false,
        IReadOnlyList<IShadcnCarouselPlugin>? plugins = null)
    {
        parameters.Add(component => component.Options, options)
            .Add(component => component.SelectedIndex, selected)
            .Add(component => component.Orientation, orientation)
            .Add(component => component.Disabled, disabled)
            .Add(component => component.Label, "งานผลิต")
            .Add(component => component.PreviousLabel, "ก่อนหน้า")
            .Add(component => component.NextLabel, "ถัดไป")
            .Add(component => component.SlideLabelFormatter, (index, count) => $"สไลด์ {index + 1} จาก {count}")
            .Add(component => component.Plugins, plugins ?? [])
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnCarouselContent>(0);
                builder.AddAttribute(1, nameof(ShadcnCarouselContent.ChildContent), (RenderFragment)(content =>
                {
                    for (var index = 0; index < 3; index++)
                    {
                        content.OpenComponent<ShadcnCarouselItem>(index * 3);
                        content.AddAttribute(index * 3 + 1, nameof(ShadcnCarouselItem.Index), index);
                        content.AddAttribute(index * 3 + 2, nameof(ShadcnCarouselItem.ChildContent), Text((index + 1).ToString()));
                        content.CloseComponent();
                    }
                }));
                builder.CloseComponent();
                builder.OpenComponent<ShadcnCarouselPrevious>(2);
                builder.CloseComponent();
                builder.OpenComponent<ShadcnCarouselNext>(3);
                builder.CloseComponent();
            });
        if (changes is not null)
            parameters.Add(component => component.SelectedIndexChanged, EventCallback.Factory.Create<int>(this, changes.Add));
    }

    private static RenderFragment Text(string value) => builder => builder.AddContent(0, value);

    private sealed class RecordingPlugin : IShadcnCarouselPlugin
    {
        public int InitializedCount { get; private set; }
        public int SelectedIndex { get; private set; } = -1;
        public bool Disposed { get; private set; }
        public int InitializeCalls { get; private set; }
        public int DisposeCalls { get; private set; }
        public void Initialize(IShadcnCarouselApi api) { InitializedCount = api.Count; InitializeCalls++; }
        public void Selected(int index) => SelectedIndex = index;
        public ValueTask DisposeAsync() { Disposed = true; DisposeCalls++; return ValueTask.CompletedTask; }
    }

    private sealed class CarouselTimeProvider : TimeProvider
    {
        private DateTimeOffset _now = DateTimeOffset.UnixEpoch;
        private readonly List<Timer> _timers = [];
        public override DateTimeOffset GetUtcNow() => _now;
        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period) { var timer = new Timer(this, callback, state, dueTime); _timers.Add(timer); return timer; }
        public void Advance(TimeSpan amount) { _now += amount; foreach (var timer in _timers.ToArray()) timer.Fire(_now); }
        private sealed class Timer(CarouselTimeProvider owner, TimerCallback callback, object? state, TimeSpan due) : ITimer
        {
            private DateTimeOffset? _due = owner._now + due;
            public bool Change(TimeSpan dueTime, TimeSpan period) { _due = owner._now + dueTime; return true; }
            public void Fire(DateTimeOffset now) { if (_due is not null && now >= _due) { _due = null; callback(state); } }
            public void Dispose() => _due = null;
            public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
        }
    }
    private sealed class FailingCarouselApi : IShadcnCarouselApi
    {
        public int Count => 1;
        public int SelectedIndex => 0;
        public bool Dispatched { get; private set; }
        public ValueTask PreviousAsync() => ValueTask.CompletedTask;
        public ValueTask NextAsync() => ValueTask.FromException(new InvalidOperationException("renderer"));
        public ValueTask GoToAsync(int index) => ValueTask.CompletedTask;
        public ValueTask ReinitializeAsync() => ValueTask.CompletedTask;
        public async ValueTask DispatchAsync(Func<ValueTask> callback) { Dispatched = true; await callback(); }
    }
}
