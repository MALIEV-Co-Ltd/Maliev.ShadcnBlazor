namespace Maliev.ShadcnBlazor.Components.Styling;

/// <summary>Defines decorative motion applied inside a visual style scope.</summary>
public enum ShadcnMotionTreatment
{
    /// <summary>Inherits motion from the nearest containing scope.</summary>
    Inherit,

    /// <summary>Uses restrained transitions.</summary>
    Calm,

    /// <summary>Uses more expressive but bounded transitions.</summary>
    Expressive,

    /// <summary>Disables decorative motion.</summary>
    None
}
