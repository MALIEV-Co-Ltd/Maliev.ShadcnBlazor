namespace Maliev.ShadcnBlazor.Components.Styling;

/// <summary>Defines the surface and control treatment applied by a visual style scope.</summary>
public enum ShadcnVisualStyle
{
    /// <summary>Inherits the visual style from the nearest containing scope.</summary>
    Inherit,

    /// <summary>Uses quiet, flat surfaces with restrained boundaries.</summary>
    Minimal,

    /// <summary>Uses translucent surfaces with bounded backdrop effects.</summary>
    Glass,

    /// <summary>Uses strong outlines, offset shadows, and direct interaction feedback.</summary>
    NeoBrutalist,

    /// <summary>Uses layered translucent surfaces with spatial highlights.</summary>
    LiquidGlass
}
