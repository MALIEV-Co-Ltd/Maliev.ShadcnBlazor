using Bunit;
using Maliev.ShadcnBlazor.Components.Feedback.Toast;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.ShadcnBlazor.Tests.Components.FeedbackContent;

public sealed class ToastTests : BunitContext
{
    public ToastTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void ToasterImportsRuntimeRelativeToTheApplicationBasePath()
    {
        Services.AddSingleton<IShadcnToastService>(new ShadcnToastService(new ManualTimeProvider()));

        _ = Render<ShadcnToaster>();

        Assert.Contains(JSInterop.Invocations, invocation =>
            invocation.Identifier == "import" &&
            invocation.Arguments.Any(argument => string.Equals(
                argument?.ToString(),
                "./_content/Maliev.ShadcnBlazor/js/shadcn-feedback-content.js",
                StringComparison.Ordinal)));
    }

    [Fact]
    public void ShowUsesStableIdsFifoAndTypedAnnouncements()
    {
        var time = new ManualTimeProvider();
        var service = new ShadcnToastService(time);
        var first = service.Show(new ShadcnToastOptions("บันทึกแล้ว", Type: ShadcnToastType.Success));
        var second = service.Show(new ShadcnToastOptions("ผิดพลาด", Type: ShadcnToastType.Error));

        Assert.NotEqual(first, second);
        Assert.Equal([first, second], service.Items.Select(item => item.Id));
        Assert.Equal("polite", service.Items[0].Live);
        Assert.Equal("assertive", service.Items[1].Live);
    }

    [Fact]
    public void WarningIsPoliteUnlessExplicitlyUrgentAndPriorityControlsStackOrder()
    {
        var service = new ShadcnToastService(new ManualTimeProvider());
        var normal = service.Show(new ShadcnToastOptions("Normal", Type: ShadcnToastType.Warning));
        var urgent = service.Show(new ShadcnToastOptions("Urgent", Type: ShadcnToastType.Warning, Urgent: true, Priority: ShadcnToastPriority.High));

        Assert.Equal("polite", service.Items.Single(item => item.Id == normal).Live);
        Assert.Equal("assertive", service.Items.Single(item => item.Id == urgent).Live);
        Assert.Equal(urgent, service.Items[0].Id);
    }

    [Fact]
    public void RegistrationPreservesConsumerTimeProvider()
    {
        var time = new ManualTimeProvider();
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(time);
        services.AddMalievShadcn();
        using var provider = services.BuildServiceProvider();

        Assert.Same(time, provider.GetRequiredService<TimeProvider>());
    }

    [Fact]
    public void UpdatePreservesIdentityAndPosition()
    {
        var service = new ShadcnToastService(new ManualTimeProvider());
        var id = service.Show(new ShadcnToastOptions("กำลังโหลด", Type: ShadcnToastType.Loading));
        var other = service.Show(new ShadcnToastOptions("Other"));

        Assert.True(service.Update(id, new ShadcnToastOptions("สำเร็จ", Type: ShadcnToastType.Success)));
        Assert.Equal([id, other], service.Items.Select(item => item.Id));
        Assert.Equal("สำเร็จ", service.Items[0].Title);
        Assert.Equal(ShadcnToastType.Success, service.Items[0].Type);
        Assert.False(service.Update("missing", new ShadcnToastOptions("No")));
    }

    [Fact]
    public void TimeoutPauseAndResumeUseRemainingMonotonicDuration()
    {
        var time = new ManualTimeProvider();
        var service = new ShadcnToastService(time);
        var id = service.Show(new ShadcnToastOptions("Saved", Duration: TimeSpan.FromSeconds(10)));

        time.Advance(TimeSpan.FromSeconds(4));
        service.Pause(id);
        time.Advance(TimeSpan.FromSeconds(30));
        Assert.Single(service.Items);
        service.Resume(id);
        time.Advance(TimeSpan.FromSeconds(5));
        Assert.Single(service.Items);
        time.Advance(TimeSpan.FromSeconds(1));
        Assert.Empty(service.Items);
    }

    [Fact]
    public void LoadingToastDoesNotExpireUntilUpdated()
    {
        var time = new ManualTimeProvider();
        var service = new ShadcnToastService(time);
        var id = service.Show(new ShadcnToastOptions("Working", Type: ShadcnToastType.Loading));
        time.Advance(TimeSpan.FromHours(1));
        Assert.Single(service.Items);

        service.Update(id, new ShadcnToastOptions("Done", Type: ShadcnToastType.Success, Duration: TimeSpan.FromSeconds(2)));
        time.Advance(TimeSpan.FromSeconds(2));
        Assert.Empty(service.Items);
    }

    [Fact]
    public async Task PromiseUpdatesOneToastToSuccessOrError()
    {
        var service = new ShadcnToastService(new ManualTimeProvider());
        var success = await service.PromiseAsync(
            Task.FromResult(42),
            new ShadcnToastOptions("Loading", Type: ShadcnToastType.Loading),
            value => new ShadcnToastOptions($"Result {value}", Type: ShadcnToastType.Success),
            error => new ShadcnToastOptions(error.Message, Type: ShadcnToastType.Error));
        Assert.Equal("Result 42", service.Items.Single(item => item.Id == success).Title);

        var failed = await service.PromiseAsync<int>(
            Task.FromException<int>(new InvalidOperationException("Failed")),
            new ShadcnToastOptions("Loading", Type: ShadcnToastType.Loading),
            value => new ShadcnToastOptions(value.ToString()),
            error => new ShadcnToastOptions(error.Message, Type: ShadcnToastType.Error));
        Assert.Equal("Failed", service.Items.Single(item => item.Id == failed).Title);
    }

    [Fact]
    public async Task PromiseResultPreservesValueAndFailureWhileUpdatingToast()
    {
        var service = new ShadcnToastService(new ManualTimeProvider());
        var result = await service.PromiseResultAsync(Task.FromResult(42), new("Loading"), value => new($"Done {value}"), error => new(error.Message));
        Assert.Equal(42, result);
        var failure = new InvalidOperationException("failed");
        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => service.PromiseResultAsync(Task.FromException<int>(failure), new("Loading"), value => new(value.ToString()), error => new(error.Message, Type: ShadcnToastType.Error)));
        Assert.Same(failure, thrown);
        Assert.Equal("failed", service.Items.Last().Title);
    }

    [Fact]
    public async Task ActionRunsBeforeOneDismissAndDuplicateDismissIsSuppressed()
    {
        var events = new List<string>();
        var service = new ShadcnToastService(new ManualTimeProvider());
        var id = service.Show(new ShadcnToastOptions(
            "Deleted",
            ActionLabel: "Undo",
            Action: () => { events.Add("action"); return Task.CompletedTask; },
            OnDismiss: () => events.Add("dismiss")));

        Assert.True(await service.InvokeActionAsync(id));
        Assert.Equal(["action", "dismiss"], events);
        Assert.False(service.Dismiss(id));
        Assert.Equal(["action", "dismiss"], events);
    }

    [Fact]
    public async Task ConcurrentActionIsSerializedAndFailureStillDismisses()
    {
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        var service = new ShadcnToastService(new ManualTimeProvider());
        var id = service.Show(new ShadcnToastOptions("Action", ActionLabel: "Run", Action: async () => { calls++; await release.Task; throw new InvalidOperationException("boom"); }));
        var first = service.InvokeActionAsync(id);
        Assert.False(await service.InvokeActionAsync(id));
        release.SetResult();
        await Assert.ThrowsAsync<InvalidOperationException>(() => first);
        Assert.Equal(1, calls);
        Assert.Empty(service.Items);
    }

    [Fact]
    public void ZeroDurationCreatesPersistentToast()
    {
        var time = new ManualTimeProvider();
        var service = new ShadcnToastService(time);
        service.Show(new ShadcnToastOptions("Persistent", Duration: TimeSpan.Zero));
        time.Advance(TimeSpan.FromDays(1));
        Assert.Single(service.Items);
    }

    [Fact]
    public void BeginDismissExposesClosingStateThenRemovesExactlyOnce()
    {
        var time = new ManualTimeProvider();
        var dismissed = 0;
        var service = new ShadcnToastService(time);
        var id = service.Show(new ShadcnToastOptions("Closing", OnDismiss: () => dismissed++));
        Assert.True(service.BeginDismiss(id, TimeSpan.FromMilliseconds(180)));
        Assert.Equal("closing", service.Items.Single().State);
        Assert.False(service.BeginDismiss(id, TimeSpan.FromMilliseconds(180)));
        time.Advance(TimeSpan.FromMilliseconds(179));
        Assert.Single(service.Items);
        time.Advance(TimeSpan.FromMilliseconds(1));
        Assert.Empty(service.Items);
        Assert.Equal(1, dismissed);
    }

    [Fact]
    public void DismissNotifiesEvenWhenConsumerCallbackThrows()
    {
        var service = new ShadcnToastService(new ManualTimeProvider());
        var changes = 0;
        service.Changed += () => changes++;
        var id = service.Show(new ShadcnToastOptions("Dismiss", OnDismiss: () => throw new InvalidOperationException("consumer")));
        Assert.Throws<InvalidOperationException>(() => service.Dismiss(id));
        Assert.Equal(2, changes);
        Assert.Empty(service.Items);
    }

    [Fact]
    public void ToasterRendersLimitStackAndLocalizedControls()
    {
        var service = new ShadcnToastService(new ManualTimeProvider());
        Services.AddSingleton<IShadcnToastService>(service);
        service.Show(new ShadcnToastOptions("One"));
        service.Show(new ShadcnToastOptions("Two", Description: "Description"));
        service.Show(new ShadcnToastOptions("Three", Type: ShadcnToastType.Warning, ActionLabel: "เปิด"));

        var cut = Render<ShadcnToaster>(parameters => parameters
            .Add(component => component.MaximumVisible, 2)
            .Add(component => component.CloseLabel, "ปิดการแจ้งเตือน")
            .Add(component => component.Placement, ShadcnToastPlacement.BottomStart));

        var viewport = cut.Find("[data-slot='toast-viewport']");
        Assert.Equal("bottom-start", viewport.GetAttribute("data-placement"));
        Assert.Equal(3, cut.FindAll("[data-slot='toast']").Count);
        Assert.Single(cut.FindAll("[data-limited='true']"));
        var limitedToast = cut.Find("[data-limited='true']");
        Assert.Equal("off", limitedToast.GetAttribute("aria-live"));
        Assert.NotNull(limitedToast.GetAttribute("inert"));
        Assert.Equal("true", limitedToast.GetAttribute("aria-hidden"));
        var css = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-feedback-content.css"));
        Assert.Contains(".shadcn-toast[data-limited=\"true\"]", css, StringComparison.Ordinal);
        Assert.Contains("Two", cut.Markup);
        Assert.Contains("Three", cut.Markup);
        Assert.Equal("ปิดการแจ้งเตือน", cut.Find("[data-slot='toast-close']").GetAttribute("aria-label"));
        Assert.Equal("status", cut.FindAll("[data-slot='toast']")[1].GetAttribute("role"));
    }

    [Fact]
    public void BuiltInStatusAndCloseIconsUseTheSharedSvgVisualLanguage()
    {
        var service = new ShadcnToastService(new ManualTimeProvider());
        Services.AddSingleton<IShadcnToastService>(service);
        service.Show(new ShadcnToastOptions("Saved", Type: ShadcnToastType.Success));

        var cut = Render<ShadcnToaster>();

        Assert.Equal("region", cut.Find("[data-slot='toast-viewport']").GetAttribute("role"));
        Assert.Single(cut.FindAll("[data-slot='toast-icon'] svg[aria-hidden='true']"));
        Assert.Single(cut.FindAll("[data-slot='toast-close'] svg[aria-hidden='true']"));
        Assert.DoesNotContain(">×<", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ToastStylesRespectExplicitAndSystemMotionPreferencesAndForcedColors()
    {
        var css = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-feedback-content.css"));

        Assert.Contains(".shadcn-toast-viewport[data-reduced-motion=\"true\"] .shadcn-toast", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-toast-viewport[data-reduced-motion=\"true\"] .shadcn-toast-icon", css, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", css, StringComparison.Ordinal);
        Assert.Contains(":where(.shadcn-toast, .shadcn-toast-action, .shadcn-toast-close) { forced-color-adjust: auto; }", css, StringComparison.Ordinal);
    }

    [Fact]
    public void ToasterCloseDismissesAndInvalidLimitFailsClosed()
    {
        var service = new ShadcnToastService(new ManualTimeProvider());
        Services.AddSingleton<IShadcnToastService>(service);
        service.Show(new ShadcnToastOptions("One"));
        var cut = Render<ShadcnToaster>(parameters => parameters.Add(component => component.ReducedMotion, true));
        cut.Find("[data-slot='toast-close']").Click();
        Assert.Empty(service.Items);
        Assert.ThrowsAny<Exception>(() => Render<ShadcnToaster>(parameters => parameters.Add(component => component.MaximumVisible, 0)));
    }

    [Fact]
    public void SystemReducedMotionRemovesClosingToastImmediately()
    {
        var service = new ShadcnToastService(new ManualTimeProvider());
        Services.AddSingleton<IShadcnToastService>(service);
        service.Show(new ShadcnToastOptions("One"));
        var cut = Render<ShadcnToaster>();

        cut.Instance.SetSystemReducedMotion(true);
        cut.Find("[data-slot='toast-close']").Click();

        Assert.Empty(service.Items);
    }

    [Fact]
    public void TimerCallbacksDoNotSurfaceConsumerNotificationFailures()
    {
        var time = new ManualTimeProvider();
        var service = new ShadcnToastService(time, TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(10));
        service.Show(new ShadcnToastOptions("Timer"));
        service.Changed += () => throw new InvalidOperationException("consumer");

        var exception = Record.Exception(() =>
        {
            time.Advance(TimeSpan.FromMilliseconds(20));
            time.Advance(TimeSpan.FromMilliseconds(10));
        });

        Assert.Null(exception);
        Assert.Empty(service.Items);
    }

    [Fact]
    public void IndependentPauseReasonsAndClosingExitTimerDoNotInterfere()
    {
        var time = new ManualTimeProvider();
        var service = new ShadcnToastService(time, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(100));
        var id = service.Show(new ShadcnToastOptions("Reasons"));
        Assert.True(service.Pause(id, "pointer"));
        Assert.True(service.Pause(id, "focus"));
        Assert.True(service.Resume(id, "pointer"));
        time.Advance(TimeSpan.FromMinutes(1));
        Assert.Single(service.Items);
        Assert.True(service.Dismiss(id));
        Assert.Equal("closing", service.Items.Single().State);
        Assert.False(service.Pause(id, "pointer"));
        time.Advance(TimeSpan.FromMilliseconds(100));
        Assert.Empty(service.Items);
    }

    [Fact]
    public void EscapeDismissesFrontmostAndSwipeUsesConfiguredThreshold()
    {
        var service = new ShadcnToastService(new ManualTimeProvider());
        Services.AddSingleton<IShadcnToastService>(service);
        service.Show(new ShadcnToastOptions("One"));
        var second = service.Show(new ShadcnToastOptions("Two"));
        var cut = Render<ShadcnToaster>(parameters => parameters
            .Add(component => component.SwipeDirections, ShadcnToastSwipeDirections.Right)
            .Add(component => component.SwipeThreshold, 40)
            .Add(component => component.ReducedMotion, true));

        cut.Find("[data-slot='toast-viewport']").KeyDown(new KeyboardEventArgs { Key = "Escape" });
        Assert.DoesNotContain(service.Items, item => item.Id == second);

        var remaining = service.Items.Single().Id;
        var toast = cut.Find("[data-slot='toast']");
        toast.PointerDown(new PointerEventArgs { PointerId = 4, ClientX = 10, ClientY = 10 });
        toast.PointerUp(new PointerEventArgs { PointerId = 4, ClientX = 35, ClientY = 10 });
        Assert.Contains(service.Items, item => item.Id == remaining);
        toast.PointerDown(new PointerEventArgs { PointerId = 5, ClientX = 10, ClientY = 10 });
        toast.PointerUp(new PointerEventArgs { PointerId = 5, ClientX = 60, ClientY = 10 });
        Assert.Empty(service.Items);
    }

    [Fact]
    public void ViewportProvidesF6FocusTargetAndRejectsInvalidSwipeConfiguration()
    {
        var service = new ShadcnToastService(new ManualTimeProvider());
        Services.AddSingleton<IShadcnToastService>(service);
        var cut = Render<ShadcnToaster>();
        Assert.Equal("-1", cut.Find("[data-slot='toast-viewport']").GetAttribute("tabindex"));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnToaster>(parameters => parameters.Add(component => component.SwipeThreshold, -1)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnToaster>(parameters => parameters.Add(component => component.SwipeDirections, (ShadcnToastSwipeDirections)128)));
    }

    [Fact]
    public void CustomIconExpandedStackAndSwipeMovementAreRendered()
    {
        var service = new ShadcnToastService(new ManualTimeProvider());
        Services.AddSingleton<IShadcnToastService>(service);
        service.Show(new ShadcnToastOptions("Custom", Icon: builder => builder.AddMarkupContent(0, "<svg data-testid='custom-icon'></svg>")));
        var cut = Render<ShadcnToaster>();
        Assert.NotNull(cut.Find("[data-testid='custom-icon']"));
        var viewport = cut.Find("[data-slot='toast-viewport']");
        viewport.MouseEnter();
        Assert.Equal("true", viewport.GetAttribute("data-expanded"));
        var toast = cut.Find("[data-slot='toast']");
        toast.PointerDown(new PointerEventArgs { PointerId = 8, ClientX = 10, ClientY = 10 });
        toast.PointerMove(new PointerEventArgs { PointerId = 8, ClientX = 30, ClientY = 15 });
        Assert.Equal("move", toast.GetAttribute("data-swipe"));
        Assert.Contains("--shadcn-toast-swipe-x: 20px", toast.GetAttribute("style"), StringComparison.Ordinal);
    }

    [Fact]
    public void SemanticVariantsAndHoverStateExposeAStableBoundedPresentation()
    {
        var service = new ShadcnToastService(new ManualTimeProvider());
        Services.AddSingleton<IShadcnToastService>(service);
        foreach (var type in Enum.GetValues<ShadcnToastType>())
            service.Show(new ShadcnToastOptions(type.ToString(), Type: type, Duration: TimeSpan.Zero));

        var cut = Render<ShadcnToaster>(parameters => parameters.Add(component => component.MaximumVisible, 6));
        Assert.Equal(
            ["default", "success", "info", "warning", "error", "loading"],
            cut.FindAll("[data-slot='toast']").Select(toast => toast.GetAttribute("data-type")));
        Assert.Equal(["0", "1", "2", "3", "4", "5"], cut.FindAll("[data-slot='toast']").Select(toast => toast.GetAttribute("data-stack-index")));

        var viewport = cut.Find("[data-slot='toast-viewport']");
        Assert.Equal("manual", viewport.GetAttribute("popover"));
        viewport.FocusIn();
        Assert.Equal("true", viewport.GetAttribute("data-expanded"));
        viewport.FocusOut();
        Assert.Equal("false", viewport.GetAttribute("data-expanded"));

        var css = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-feedback-content.css"));
        foreach (var type in new[] { "success", "info", "warning", "error", "loading" })
            Assert.Contains($".shadcn-toast[data-type=\"{type}\"]", css, StringComparison.Ordinal);
        Assert.Contains("max-block-size: calc(100dvh - 2rem)", css, StringComparison.Ordinal);
        Assert.DoesNotContain("margin-block-end: -3rem", css, StringComparison.Ordinal);
    }

    [Fact]
    public void DocumentVisibilityPausesAndResumesEveryToast()
    {
        var time = new ManualTimeProvider();
        var service = new ShadcnToastService(time);
        Services.AddSingleton<IShadcnToastService>(service);
        service.Show(new ShadcnToastOptions("One", Duration: TimeSpan.FromSeconds(10)));
        service.Show(new ShadcnToastOptions("Two", Duration: TimeSpan.FromSeconds(10)));
        var cut = Render<ShadcnToaster>();

        time.Advance(TimeSpan.FromSeconds(3));
        cut.Instance.SetDocumentPaused(true);
        time.Advance(TimeSpan.FromSeconds(30));
        Assert.Equal(2, service.Items.Count);
        cut.Instance.SetDocumentPaused(false);
        time.Advance(TimeSpan.FromSeconds(7));
        Assert.Empty(service.Items);
    }

    [Fact]
    public void ToastRejectsInvalidDurationAndPlacement()
    {
        var service = new ShadcnToastService(new ManualTimeProvider());
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Show(new ShadcnToastOptions("Bad", Duration: TimeSpan.FromTicks(-1))));
        Services.AddSingleton<IShadcnToastService>(service);
        Assert.ThrowsAny<Exception>(() => Render<ShadcnToaster>(parameters => parameters.Add(component => component.Placement, (ShadcnToastPlacement)999)));
    }

    private sealed class ManualTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow = DateTimeOffset.UnixEpoch;
        private readonly List<ManualTimer> _timers = [];
        public override DateTimeOffset GetUtcNow() => _utcNow;
        public override long GetTimestamp() => _utcNow.UtcTicks;
        public override long TimestampFrequency => TimeSpan.TicksPerSecond;
        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            var timer = new ManualTimer(this, callback, state, dueTime, period);
            _timers.Add(timer);
            return timer;
        }
        public void Advance(TimeSpan amount)
        {
            _utcNow += amount;
            foreach (var timer in _timers.ToArray()) timer.FireDue(_utcNow);
        }
        private sealed class ManualTimer(ManualTimeProvider owner, TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period) : ITimer
        {
            private DateTimeOffset? _due = dueTime == Timeout.InfiniteTimeSpan ? null : owner._utcNow + dueTime;
            private bool _disposed;
            public bool Change(TimeSpan dueTime, TimeSpan newPeriod) { _due = dueTime == Timeout.InfiniteTimeSpan ? null : owner._utcNow + dueTime; period = newPeriod; return !_disposed; }
            public void FireDue(DateTimeOffset now) { if (!_disposed && _due is not null && now >= _due) { if (period == Timeout.InfiniteTimeSpan) _due = null; else _due = now + period; callback(state); } }
            public void Dispose() => _disposed = true;
            public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
        }
    }
}
