namespace Maliev.ShadcnBlazor.Components.Styling;

/// <summary>Defines the perceived elevation of styled component surfaces.</summary>
public enum ShadcnDepthTreatment
{
    /// <summary>Inherits depth from the nearest containing scope.</summary>
    Inherit,

    /// <summary>Removes decorative elevation.</summary>
    Flat,

    /// <summary>Adds restrained surface elevation.</summary>
    Raised,

    /// <summary>Adds clear floating-surface elevation.</summary>
    Floating,

    /// <summary>Adds layered spatial depth.</summary>
    Spatial
}
