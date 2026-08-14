namespace Maliev.ShadcnBlazor.Components.Conversation;

public enum ShadcnLogicalAlign { Start, End }
public enum ShadcnBubbleVariant { Default, Secondary, Muted, Tinted, Outline, Ghost, Destructive }
public enum ShadcnReactionSide { Top, Bottom }
public enum ShadcnMarkerVariant { Default, Separator, Border }

internal static class ShadcnConversationValues
{
    internal static string Align(ShadcnLogicalAlign value) => value switch { ShadcnLogicalAlign.Start => "start", ShadcnLogicalAlign.End => "end", _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown logical alignment.") };
}
