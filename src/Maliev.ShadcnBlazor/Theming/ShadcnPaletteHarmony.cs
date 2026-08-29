namespace Maliev.ShadcnBlazor.Theming;

/// <summary>Describes the relationship among palette anchors.</summary>
public enum ShadcnPaletteHarmony
{
    /// <summary>Leaves anchor relationships unconstrained.</summary>
    Free,

    /// <summary>Uses neighbouring hues.</summary>
    Analogous,

    /// <summary>Uses opposing hues.</summary>
    Complementary,

    /// <summary>Uses three evenly spaced hues.</summary>
    Triadic
}
