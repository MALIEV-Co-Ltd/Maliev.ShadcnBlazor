namespace Maliev.ShadcnBlazor.Components.Overlays;

internal sealed class ShadcnDialogContext(ShadcnDialog owner, string contentId)
{
    internal ShadcnDialog Owner { get; } = owner;
    internal string ContentId { get; } = contentId;
    internal string? TitleId { get; set; }
    internal string? DescriptionId { get; set; }
}

internal sealed class ShadcnAlertDialogContext(ShadcnAlertDialog owner, string contentId)
{
    internal ShadcnAlertDialog Owner { get; } = owner;
    internal string ContentId { get; } = contentId;
    internal string? TitleId { get; set; }
    internal string? DescriptionId { get; set; }
}

/// <summary>Controls the Alert Dialog surface size.</summary>
public enum ShadcnAlertDialogSize
{
    /// <summary>Uses the standard confirmation layout.</summary>
    Default,
    /// <summary>Uses the compact confirmation layout.</summary>
    Small
}
