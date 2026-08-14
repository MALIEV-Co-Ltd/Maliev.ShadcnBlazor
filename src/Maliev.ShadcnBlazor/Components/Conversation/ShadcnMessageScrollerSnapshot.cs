namespace Maliev.ShadcnBlazor.Components.Conversation;

public sealed record ShadcnMessageScrollerSnapshot(double ScrollTop, double ViewportHeight, double ContentHeight, IReadOnlyList<ShadcnMessageScrollerItemGeometry> Items, long Sequence = 0, bool PreserveScrollOnPrepend = true);
