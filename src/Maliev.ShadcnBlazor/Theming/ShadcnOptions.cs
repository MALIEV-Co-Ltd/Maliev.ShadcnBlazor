namespace Maliev.ShadcnBlazor.Theming;

public sealed class ShadcnOptions
{
    /// <summary>Gets or sets the optional application-wide theme used when a provider has no explicit theme.</summary>
    public ShadcnTheme? Theme { get; set; }

    public string FontFamily { get; set; } = "'Geist', 'Noto Sans Thai', ui-sans-serif, system-ui, sans-serif";
    public bool DefaultDarkMode { get; set; }
    public ShadcnDirection DefaultDirection { get; set; } = ShadcnDirection.LeftToRight;
    public TimeSpan ToastDuration { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan ToastExitDuration { get; set; } = TimeSpan.FromMilliseconds(180);
}
