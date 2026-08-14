namespace Maliev.ShadcnBlazor.Components.Conversation;

public enum ShadcnMessageDefaultScrollPosition { Start, End, LastAnchor }
public enum ShadcnMessageScrollAlign { Start, Center, End, Nearest }
public enum ShadcnMessageScrollBehavior { Auto, Smooth }
public enum ShadcnMessageScrollDirection { Start, End }

public sealed record ShadcnMessageScrollerOptions(
    bool AutoScroll = false,
    ShadcnMessageDefaultScrollPosition DefaultScrollPosition = ShadcnMessageDefaultScrollPosition.End,
    double ScrollEdgeThreshold = 8,
    double ScrollPreviousItemPeek = 64,
    double ScrollMargin = 0);

public sealed record ShadcnMessageScrollOptions(
    ShadcnMessageScrollAlign Align = ShadcnMessageScrollAlign.Nearest,
    ShadcnMessageScrollBehavior Behavior = ShadcnMessageScrollBehavior.Auto,
    double? ScrollMargin = null,
    bool FocusViewport = false);

public readonly record struct ShadcnMessageScrollerMeasurement(double ScrollTop, double ViewportHeight, double ContentHeight);
public readonly record struct ShadcnMessageScrollerItemGeometry(string MessageId, double Top, double Height, bool ScrollAnchor);
public readonly record struct ShadcnMessageScrollerScrollable(bool Start, bool End);
public sealed record ShadcnMessageScrollerVisibility(string? CurrentAnchorId, IReadOnlyList<string> VisibleMessageIds);
public sealed record ShadcnMessageScrollerState(ShadcnMessageScrollerScrollable Scrollable, string? CurrentAnchorId, IReadOnlyList<string> VisibleMessageIds, bool Following, bool Unread);
public readonly record struct ShadcnMessageScrollResult(bool Handled, double? TargetScrollTop, ShadcnMessageScrollBehavior Behavior = ShadcnMessageScrollBehavior.Auto);
