namespace Maliev.ShadcnBlazor.Components.Overlays;

/// <summary>Controls the edge from which a Sheet enters.</summary>
public enum ShadcnSheetSide { Top, Right, Bottom, Left }

internal sealed class ShadcnSheetContext(ShadcnSheet owner, string contentId)
{
    internal ShadcnSheet Owner { get; } = owner;
    internal string ContentId { get; } = contentId;
    internal string? TitleId { get; set; }
    internal string? DescriptionId { get; set; }
}
