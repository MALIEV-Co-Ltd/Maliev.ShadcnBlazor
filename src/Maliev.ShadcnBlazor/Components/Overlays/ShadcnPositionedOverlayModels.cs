namespace Maliev.ShadcnBlazor.Components.Overlays;

/// <summary>Controls the preferred physical side of a positioned overlay.</summary>
public enum ShadcnOverlaySide { Top, Right, Bottom, Left }
/// <summary>Controls alignment along the positioned overlay's cross axis.</summary>
public enum ShadcnOverlayAlign { Start, Center, End }

internal static class ShadcnPositionedOverlayValues
{
    internal static string Side(ShadcnOverlaySide value) => value switch { ShadcnOverlaySide.Top => "top", ShadcnOverlaySide.Right => "right", ShadcnOverlaySide.Bottom => "bottom", ShadcnOverlaySide.Left => "left", _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown overlay side.") };
    internal static string Align(ShadcnOverlayAlign value) => value switch { ShadcnOverlayAlign.Start => "start", ShadcnOverlayAlign.Center => "center", ShadcnOverlayAlign.End => "end", _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown overlay alignment.") };
    internal static void ValidateOffset(double value, string name) { if (!double.IsFinite(value) || value is < -1024 or > 1024) throw new ArgumentOutOfRangeException(name); }
    internal static int Milliseconds(TimeSpan value, string name) { if (value < TimeSpan.Zero || value > TimeSpan.FromMinutes(1)) throw new ArgumentOutOfRangeException(name); return checked((int)value.TotalMilliseconds); }
}

internal sealed class ShadcnPopoverContext(ShadcnPopover owner, string triggerId, string contentId) { internal ShadcnPopover Owner { get; } = owner; internal string TriggerId { get; } = triggerId; internal string ContentId { get; } = contentId; internal string? TitleId { get; set; } internal string? DescriptionId { get; set; } }
internal sealed class ShadcnHoverCardContext(ShadcnHoverCard owner, string triggerId, string contentId) { internal ShadcnHoverCard Owner { get; } = owner; internal string TriggerId { get; } = triggerId; internal string ContentId { get; } = contentId; }
internal sealed class ShadcnTooltipContext(ShadcnTooltip owner, string anchorId, string triggerId, string contentId) { internal ShadcnTooltip Owner { get; } = owner; internal string AnchorId { get; } = anchorId; internal string TriggerId { get; } = triggerId; internal string ContentId { get; } = contentId; internal bool TriggerDisabled { get; set; } }
internal sealed record ShadcnTooltipProviderContext(TimeSpan OpenDelay, TimeSpan CloseDelay);
