using Maliev.ShadcnBlazor.Components.Conversation;

namespace Maliev.ShadcnBlazor.Tests.Components.Conversation;

public sealed class MessageScrollerControllerTests
{
    [Fact]
    public void ScrollableUsesConfiguredEdgeThresholdAndSubpixelValues()
    {
        Assert.Equal(new(false, true), ShadcnMessageScrollerGeometry.GetScrollable(7.75, 200, 500, 8));
        Assert.Equal(new(true, false), ShadcnMessageScrollerGeometry.GetScrollable(292.25, 200, 500, 8));
        Assert.Equal(new(true, true), ShadcnMessageScrollerGeometry.GetScrollable(50, 200, 500, 8));
        Assert.Equal(new(false, false), ShadcnMessageScrollerGeometry.GetScrollable(0, 500, 200, 8));
    }

    [Theory]
    [InlineData(ShadcnMessageScrollAlign.Start, 136)]
    [InlineData(ShadcnMessageScrollAlign.Center, 66)]
    [InlineData(ShadcnMessageScrollAlign.End, 48)]
    public void TargetCalculationSupportsEveryAlignment(ShadcnMessageScrollAlign align, double expected)
    {
        var target = ShadcnMessageScrollerGeometry.GetTargetScrollTop(
            new("target", Top: 160, Height: 40, ScrollAnchor: false), viewportHeight: 180, contentHeight: 600,
            currentScrollTop: 20, align, paddingStart: 8, paddingEnd: 12, scrollMargin: 16);

        Assert.Equal(expected, target);
    }

    [Fact]
    public void NearestLeavesVisibleRowsInPlaceAndRevealsRowsOutsideViewport()
    {
        Assert.Equal(100, ShadcnMessageScrollerGeometry.GetTargetScrollTop(new("target", 130, 30, false), 200, 600, 100, ShadcnMessageScrollAlign.Nearest));
        Assert.Equal(40, ShadcnMessageScrollerGeometry.GetTargetScrollTop(new("target", 40, 30, false), 200, 600, 100, ShadcnMessageScrollAlign.Nearest));
        Assert.Equal(190, ShadcnMessageScrollerGeometry.GetTargetScrollTop(new("target", 360, 30, false), 200, 600, 100, ShadcnMessageScrollAlign.Nearest));
    }

    [Fact]
    public void ControllerOpensOnceAndLastAnchorFallsBackToEnd()
    {
        var controller = new ShadcnMessageScrollerController(new(DefaultScrollPosition: ShadcnMessageDefaultScrollPosition.LastAnchor));
        var first = controller.OnContentChanged(new(0, 200, 900), [new("a", 0, 200, false), new("b", 600, 300, false)]);
        var second = controller.OnContentChanged(new(first.TargetScrollTop ?? 0, 200, 1000), [new("a", 0, 200, false), new("b", 600, 400, false)]);

        Assert.Equal(700, first.TargetScrollTop);
        Assert.Null(second.TargetScrollTop);
    }

    [Fact]
    public void AutoFollowStopsOnUserIntentAndRearmsAfterExplicitEndCommand()
    {
        var controller = new ShadcnMessageScrollerController(new(AutoScroll: true));
        _ = controller.OnContentChanged(new(0, 200, 400), [new("a", 0, 400, false)]);
        controller.OnUserIntent();
        var held = controller.OnContentChanged(new(200, 200, 500), [new("a", 0, 500, false)]);
        var command = controller.ScrollToEnd(new(200, 200, 500));
        var followed = controller.OnContentChanged(new(command.TargetScrollTop!.Value, 200, 600), [new("a", 0, 600, false)]);

        Assert.Null(held.TargetScrollTop);
        Assert.Equal(300, command.TargetScrollTop);
        Assert.Equal(400, followed.TargetScrollTop);
    }

    [Fact]
    public void UpdatingOptionsPreservesUserIntentAndMeasuredState()
    {
        var controller = new ShadcnMessageScrollerController(new(AutoScroll: true));
        _ = controller.OnContentChanged(new(200, 200, 400), [new("row", 0, 400, false)]);
        controller.OnUserIntent();
        var before = controller.State;

        controller.UpdateOptions(new(AutoScroll: true, ScrollEdgeThreshold: 24));

        Assert.False(controller.State.Following);
        Assert.Equal(before.VisibleMessageIds, controller.State.VisibleMessageIds);
        Assert.Equal(before.CurrentAnchorId, controller.State.CurrentAnchorId);
    }

    [Fact]
    public void UserIntentSuppressesNewAnchorJumpAndPublishesUnread()
    {
        var controller = new ShadcnMessageScrollerController(new(AutoScroll: true));
        _ = controller.OnContentChanged(new(200, 200, 400), [new("old", 0, 400, false)]);
        controller.OnUserIntent();

        var result = controller.OnContentChanged(new(0, 200, 500), [new("old", 0, 400, false), new("new", 400, 100, true)]);

        Assert.Null(result.TargetScrollTop);
        Assert.True(controller.State.Unread);
        Assert.False(controller.State.Following);
    }

    [Fact]
    public void PrependPreservesTheFirstPreviouslyRenderedRowsViewportPosition()
    {
        var controller = new ShadcnMessageScrollerController(new());
        _ = controller.OnContentChanged(new(100, 200, 600), [new("first", 40, 80, false), new("second", 120, 80, false)]);

        var result = controller.OnContentChanged(new(100, 200, 720), [new("before", 0, 120, false), new("first", 160, 80, false), new("second", 240, 80, false)]);

        Assert.Equal(220, result.TargetScrollTop);
    }

    [Fact]
    public void PrependDoesNotPreserveWhenViewportOptsOut()
    {
        var controller = new ShadcnMessageScrollerController(new());
        _ = controller.OnContentChanged(new(100, 200, 600), [new("first", 40, 80, false)]);
        var result = controller.OnContentChanged(new(100, 200, 720), [new("before", 0, 120, false), new("first", 160, 80, false)], preserveScrollOnPrepend: false);
        Assert.Null(result.TargetScrollTop);
    }

    [Fact]
    public void AutoFollowTracksTheBottomAcrossNewAnchorsAndStreamingGrowth()
    {
        var controller = new ShadcnMessageScrollerController(new(AutoScroll: true, ScrollPreviousItemPeek: 64));
        _ = controller.OnContentChanged(new(0, 300, 300), [new("old", 0, 300, false)]);
        var anchored = controller.OnContentChanged(new(0, 300, 700), [new("old", 0, 300, false), new("turn", 400, 100, true), new("reply", 500, 200, false)]);
        var streamed = controller.OnContentChanged(new(anchored.TargetScrollTop!.Value, 300, 900), [new("old", 0, 300, false), new("turn", 400, 100, true), new("reply", 500, 400, false)]);

        Assert.Equal(400, anchored.TargetScrollTop);
        Assert.Equal(600, streamed.TargetScrollTop);
        Assert.True(controller.State.Following);
    }

    [Fact]
    public void MeasuredUserScrollPausesAwayFromTheEndAndResumesAtTheEdge()
    {
        var controller = new ShadcnMessageScrollerController(new(AutoScroll: true, ScrollEdgeThreshold: 8));
        _ = controller.OnContentChanged(new(400, 200, 600), [new("row", 0, 600, false)]);

        controller.OnUserScroll(new(250, 200, 600));
        var paused = controller.OnContentChanged(new(250, 200, 700), [new("row", 0, 700, false)]);
        controller.OnUserScroll(new(500, 200, 700));
        var resumed = controller.OnContentChanged(new(500, 200, 760), [new("row", 0, 760, false)]);

        Assert.Null(paused.TargetScrollTop);
        Assert.Equal(560, resumed.TargetScrollTop);
        Assert.True(controller.State.Following);
    }

    [Fact]
    public void PrependPreservesStableVisibleRowAndVisibilityIsDocumentOrdered()
    {
        Assert.Equal(220, ShadcnMessageScrollerGeometry.PreservePrependScrollTop(100, oldAnchorTop: 40, newAnchorTop: 160));
        var state = ShadcnMessageScrollerGeometry.GetVisibility(
            scrollTop: 110, viewportHeight: 180, peek: 64,
            [new("first", 0, 100, true), new("second", 100, 100, false), new("third", 200, 100, true)]);

        Assert.Equal(["second", "third"], state.VisibleMessageIds);
        Assert.Equal("first", state.CurrentAnchorId);
    }

    [Fact]
    public void ScrollToMessageRejectsUnknownIdsAndHonorsMargin()
    {
        var controller = new ShadcnMessageScrollerController(new(ScrollMargin: 12));
        var rows = new[] { new ShadcnMessageScrollerItemGeometry("known", 200, 40, false) };
        Assert.False(controller.ScrollToMessage("missing", new(0, 100, 500), rows, new()).Handled);
        var result = controller.ScrollToMessage("known", new(0, 100, 500), rows, new(Align: ShadcnMessageScrollAlign.Start, ScrollMargin: 8));
        Assert.True(result.Handled);
        Assert.Equal(180, result.TargetScrollTop);
    }

    [Fact]
    public void InvalidOptionsAndDuplicateMessageIdsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ShadcnMessageScrollerController(new(ScrollEdgeThreshold: -1)));
        var controller = new ShadcnMessageScrollerController(new());
        Assert.Throws<InvalidOperationException>(() => controller.OnContentChanged(new(0, 100, 200), [new("same", 0, 50, false), new("same", 50, 50, false)]));
    }
}
