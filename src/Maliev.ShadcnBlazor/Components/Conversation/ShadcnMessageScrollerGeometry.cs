namespace Maliev.ShadcnBlazor.Components.Conversation;

public static class ShadcnMessageScrollerGeometry
{
    public static ShadcnMessageScrollerScrollable GetScrollable(double scrollTop, double viewportHeight, double contentHeight, double threshold)
    {
        ValidateFinite(scrollTop, viewportHeight, contentHeight, threshold);
        if (viewportHeight <= 0 || contentHeight <= viewportHeight) return new(false, false);
        return new(scrollTop > threshold, contentHeight - viewportHeight - scrollTop > threshold);
    }

    public static double GetTargetScrollTop(ShadcnMessageScrollerItemGeometry item, double viewportHeight, double contentHeight, double currentScrollTop, ShadcnMessageScrollAlign align, double paddingStart = 0, double paddingEnd = 0, double scrollMargin = 0)
    {
        ValidateFinite(item.Top, item.Height, viewportHeight, contentHeight, currentScrollTop, paddingStart, paddingEnd, scrollMargin);
        var start = item.Top - paddingStart - scrollMargin;
        var end = item.Top + item.Height + paddingEnd + scrollMargin - viewportHeight;
        var target = align switch
        {
            ShadcnMessageScrollAlign.Start => start,
            ShadcnMessageScrollAlign.Center => item.Top - ((viewportHeight - item.Height) / 2) - paddingStart - scrollMargin,
            ShadcnMessageScrollAlign.End => end,
            ShadcnMessageScrollAlign.Nearest when item.Top >= currentScrollTop && item.Top + item.Height <= currentScrollTop + viewportHeight => currentScrollTop,
            ShadcnMessageScrollAlign.Nearest when item.Top < currentScrollTop => start,
            ShadcnMessageScrollAlign.Nearest => end,
            _ => throw new ArgumentOutOfRangeException(nameof(align), align, "Unknown message scroll alignment.")
        };
        return Math.Clamp(target, 0, Math.Max(0, contentHeight - viewportHeight));
    }

    public static double PreservePrependScrollTop(double scrollTop, double oldAnchorTop, double newAnchorTop)
    {
        ValidateFinite(scrollTop, oldAnchorTop, newAnchorTop);
        return Math.Max(0, scrollTop + newAnchorTop - oldAnchorTop);
    }

    public static ShadcnMessageScrollerVisibility GetVisibility(double scrollTop, double viewportHeight, double peek, IReadOnlyList<ShadcnMessageScrollerItemGeometry> items)
    {
        ValidateFinite(scrollTop, viewportHeight, peek);
        var visible = items.Where(item => item.Top + item.Height > scrollTop && item.Top < scrollTop + viewportHeight).Select(item => item.MessageId).ToArray();
        var readingLine = scrollTop + peek;
        var anchor = items.Where(item => item.ScrollAnchor && item.Top <= readingLine).LastOrDefault();
        return new(string.IsNullOrEmpty(anchor.MessageId) ? null : anchor.MessageId, visible);
    }

    internal static void ValidateItems(IReadOnlyList<ShadcnMessageScrollerItemGeometry> items)
    {
        if (items.Any(item => string.IsNullOrWhiteSpace(item.MessageId))) throw new InvalidOperationException("Message scroller item ids must be non-empty.");
        if (items.Select(item => item.MessageId).Distinct(StringComparer.Ordinal).Count() != items.Count) throw new InvalidOperationException("Message scroller item ids must be unique.");
        foreach (var item in items) ValidateFinite(item.Top, item.Height);
    }

    private static void ValidateFinite(params double[] values)
    {
        if (values.Any(value => !double.IsFinite(value))) throw new ArgumentOutOfRangeException(nameof(values), "Scroller geometry must be finite.");
    }
}
