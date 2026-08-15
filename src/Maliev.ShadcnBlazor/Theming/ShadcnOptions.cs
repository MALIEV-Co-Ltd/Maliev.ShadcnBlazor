namespace Maliev.ShadcnBlazor.Theming;

public sealed class ShadcnOptions
{
    public string FontFamily { get; set; } = "'IBM Plex Sans', 'IBM Plex Sans Thai', ui-sans-serif, system-ui, sans-serif";
    public bool DefaultDarkMode { get; set; }
    public ShadcnDirection DefaultDirection { get; set; } = ShadcnDirection.LeftToRight;
    public TimeSpan ToastDuration { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan ToastExitDuration { get; set; } = TimeSpan.FromMilliseconds(180);
}
