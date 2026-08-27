namespace Maliev.ShadcnBlazor.Components.Layout;

/// <summary>Defines the visual entrance treatment applied by <see cref="ShadcnReveal"/>.</summary>
public enum ShadcnRevealEffect
{
    /// <summary>Fades the content into view.</summary>
    Fade,

    /// <summary>Fades and lifts the content into place.</summary>
    Rise,

    /// <summary>Fades and scales the content into place.</summary>
    Scale,

    /// <summary>Reveals the content through an animated clipping region.</summary>
    Clip,

    /// <summary>Disables the item-level entrance treatment.</summary>
    None
}
