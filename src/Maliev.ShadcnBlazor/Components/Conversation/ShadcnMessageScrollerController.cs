namespace Maliev.ShadcnBlazor.Components.Conversation;

public sealed class ShadcnMessageScrollerController
{
    private ShadcnMessageScrollerOptions _options;
    private bool _opened;
    private bool _following;
    private string? _heldAnchorId;
    private IReadOnlyList<ShadcnMessageScrollerItemGeometry> _previousItems = [];

    public ShadcnMessageScrollerController(ShadcnMessageScrollerOptions options)
    {
        ValidateOptions(options);
        _options = options;
        _following = options.AutoScroll;
        State = new(new(false, false), null, Array.Empty<string>(), _following, false);
    }

    public ShadcnMessageScrollerState State { get; private set; }

    public void UpdateOptions(ShadcnMessageScrollerOptions options)
    {
        ValidateOptions(options);
        _options = options;
        if (!options.AutoScroll && _following) _following = false;
        State = State with { Following = _following };
    }

    public ShadcnMessageScrollResult OnContentChanged(ShadcnMessageScrollerMeasurement measurement, IReadOnlyList<ShadcnMessageScrollerItemGeometry> items, bool preserveScrollOnPrepend = true)
    {
        ShadcnMessageScrollerGeometry.ValidateItems(items);
        double? target = null;
        if (!_opened && items.Count > 0)
        {
            target = _options.DefaultScrollPosition switch
            {
                ShadcnMessageDefaultScrollPosition.Start => 0,
                ShadcnMessageDefaultScrollPosition.End => Bottom(measurement),
                ShadcnMessageDefaultScrollPosition.LastAnchor => LastAnchorTargetOrBottom(measurement, items),
                _ => throw new ArgumentOutOfRangeException()
            };
            _opened = true;
        }
        else
        {
            var previousFirst = _previousItems.FirstOrDefault();
            var shiftedFirst = string.IsNullOrEmpty(previousFirst.MessageId)
                ? default
                : items.FirstOrDefault(item => string.Equals(item.MessageId, previousFirst.MessageId, StringComparison.Ordinal));
            var prepended = !string.IsNullOrEmpty(shiftedFirst.MessageId) &&
                !string.Equals(items.FirstOrDefault().MessageId, previousFirst.MessageId, StringComparison.Ordinal);
            var previousIds = _previousItems.Select(item => item.MessageId).ToHashSet(StringComparer.Ordinal);
            var newAnchor = items.FirstOrDefault(item => item.ScrollAnchor && !previousIds.Contains(item.MessageId));
            if (prepended && preserveScrollOnPrepend)
            {
                target = ShadcnMessageScrollerGeometry.PreservePrependScrollTop(measurement.ScrollTop, previousFirst.Top, shiftedFirst.Top);
            }
            else if (_following && !string.IsNullOrEmpty(newAnchor.MessageId))
            {
                _heldAnchorId = newAnchor.MessageId;
                target = Math.Clamp(newAnchor.Top - _options.ScrollPreviousItemPeek - _options.ScrollMargin, 0, Bottom(measurement));
            }
            else if (_heldAnchorId is not null)
            {
                var held = items.FirstOrDefault(item => string.Equals(item.MessageId, _heldAnchorId, StringComparison.Ordinal));
                if (!string.IsNullOrEmpty(held.MessageId)) target = Math.Clamp(held.Top - _options.ScrollPreviousItemPeek - _options.ScrollMargin, 0, Bottom(measurement));
            }
            else if (_following)
            {
                target = Bottom(measurement);
            }
        }
        _previousItems = items.ToArray();
        UpdateState(target ?? measurement.ScrollTop, measurement with { ScrollTop = target ?? measurement.ScrollTop }, items);
        return new(true, target);
    }

    public void OnUserIntent()
    {
        _following = false;
        _heldAnchorId = null;
        State = State with { Following = false };
    }

    public ShadcnMessageScrollResult ScrollToEnd(ShadcnMessageScrollerMeasurement measurement)
    {
        _following = _options.AutoScroll;
        _heldAnchorId = null;
        return new(true, Bottom(measurement));
    }

    public ShadcnMessageScrollResult ScrollToStart(ShadcnMessageScrollBehavior behavior = ShadcnMessageScrollBehavior.Auto)
    {
        _following = false;
        _heldAnchorId = null;
        return new(true, 0, behavior);
    }

    public ShadcnMessageScrollResult ScrollToMessage(string messageId, ShadcnMessageScrollerMeasurement measurement, IReadOnlyList<ShadcnMessageScrollerItemGeometry> items, ShadcnMessageScrollOptions options)
    {
        ShadcnMessageScrollerGeometry.ValidateItems(items);
        var item = items.FirstOrDefault(row => string.Equals(row.MessageId, messageId, StringComparison.Ordinal));
        if (string.IsNullOrEmpty(item.MessageId)) return new(false, null, options.Behavior);
        _following = false;
        _heldAnchorId = null;
        var margin = _options.ScrollMargin + (options.ScrollMargin ?? 0);
        var target = ShadcnMessageScrollerGeometry.GetTargetScrollTop(item, measurement.ViewportHeight, measurement.ContentHeight, measurement.ScrollTop, options.Align, scrollMargin: margin);
        return new(true, target, options.Behavior);
    }

    private double LastAnchorTargetOrBottom(ShadcnMessageScrollerMeasurement measurement, IReadOnlyList<ShadcnMessageScrollerItemGeometry> items)
    {
        var anchor = items.LastOrDefault(item => item.ScrollAnchor);
        if (string.IsNullOrEmpty(anchor.MessageId) || measurement.ContentHeight - anchor.Top <= measurement.ViewportHeight) return Bottom(measurement);
        _heldAnchorId = anchor.MessageId;
        return Math.Clamp(anchor.Top - _options.ScrollPreviousItemPeek - _options.ScrollMargin, 0, Bottom(measurement));
    }

    private void UpdateState(double scrollTop, ShadcnMessageScrollerMeasurement measurement, IReadOnlyList<ShadcnMessageScrollerItemGeometry> items)
    {
        var scrollable = ShadcnMessageScrollerGeometry.GetScrollable(scrollTop, measurement.ViewportHeight, measurement.ContentHeight, _options.ScrollEdgeThreshold);
        var visibility = ShadcnMessageScrollerGeometry.GetVisibility(scrollTop, measurement.ViewportHeight, _options.ScrollPreviousItemPeek, items);
        State = new(scrollable, visibility.CurrentAnchorId, visibility.VisibleMessageIds, _following, scrollable.End && !_following);
    }

    private static double Bottom(ShadcnMessageScrollerMeasurement measurement) => Math.Max(0, measurement.ContentHeight - measurement.ViewportHeight);

    private static void ValidateOptions(ShadcnMessageScrollerOptions options)
    {
        if (!Enum.IsDefined(options.DefaultScrollPosition)) throw new ArgumentOutOfRangeException(nameof(options));
        if (!double.IsFinite(options.ScrollEdgeThreshold) || options.ScrollEdgeThreshold < 0) throw new ArgumentOutOfRangeException(nameof(options.ScrollEdgeThreshold));
        if (!double.IsFinite(options.ScrollPreviousItemPeek) || options.ScrollPreviousItemPeek < 0) throw new ArgumentOutOfRangeException(nameof(options.ScrollPreviousItemPeek));
        if (!double.IsFinite(options.ScrollMargin) || options.ScrollMargin < 0) throw new ArgumentOutOfRangeException(nameof(options.ScrollMargin));
    }
}
