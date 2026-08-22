namespace Maliev.ShadcnBlazor.Theming;

/// <summary>Describes application defaults that accompany a portable theme.</summary>
/// <param name="Preset">The source preset identifier.</param>
/// <param name="Style">The component style identifier.</param>
/// <param name="BaseColor">The neutral base-color identifier.</param>
/// <param name="IconLibrary">The application icon-library identifier.</param>
/// <param name="MenuAccent">The navigation accent identifier.</param>
/// <param name="MenuColor">The navigation surface identifier.</param>
/// <param name="DefaultDarkMode">Whether dark mode is the default.</param>
/// <param name="DefaultDirection">The default logical direction.</param>
/// <param name="DefaultLocale">The default BCP 47 locale.</param>
/// <param name="ReducedMotionBehavior">The default reduced-motion policy.</param>
public sealed record ShadcnThemeApplication(
    string Preset,
    string Style,
    string BaseColor,
    string IconLibrary,
    string MenuAccent,
    string MenuColor,
    bool DefaultDarkMode,
    ShadcnDirection DefaultDirection,
    string DefaultLocale,
    ShadcnReducedMotionBehavior ReducedMotionBehavior);
