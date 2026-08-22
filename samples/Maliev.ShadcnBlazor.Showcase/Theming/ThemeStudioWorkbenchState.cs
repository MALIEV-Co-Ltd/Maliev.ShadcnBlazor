using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Showcase.Theming;

public sealed class ThemeStudioWorkbenchState
{
    private static readonly IReadOnlySet<string> Sections = new HashSet<string>(StringComparer.Ordinal)
    {
        "colors",
        "typography",
        "generation",
        "transfer"
    };

    public bool SidebarOpen { get; private set; }
    public string ActiveSection { get; private set; } = "colors";
    public ThemeStudioViewport Viewport { get; private set; } = ThemeStudioViewport.Desktop;
    public ThemeStudioMode Mode { get; private set; } = ThemeStudioMode.Light;
    public ShadcnDirection Direction { get; private set; } = ShadcnDirection.LeftToRight;
    public ThemeStudioLocale Locale { get; private set; } = ThemeStudioLocale.English;
    public bool ReducedMotion { get; private set; }
    public bool HighContrastPreview { get; private set; }
    public bool SystemDarkMode { get; private set; }
    public bool EffectiveDarkMode => Mode == ThemeStudioMode.Dark || Mode == ThemeStudioMode.System && SystemDarkMode;

    public event EventHandler? Changed;

    public void OpenSidebar() => SetSidebarOpen(true);
    public void CloseSidebar() => SetSidebarOpen(false);
    public void ToggleSidebar() => SetSidebarOpen(!SidebarOpen);

    public void SetSidebarOpen(bool open)
    {
        if (SidebarOpen == open) return;
        SidebarOpen = open;
        RaiseChanged();
    }

    public void SetActiveSection(string section)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(section);
        if (!Sections.Contains(section))
            throw new ArgumentOutOfRangeException(nameof(section), section, "Unknown Theme Studio settings section.");
        if (string.Equals(ActiveSection, section, StringComparison.Ordinal)) return;
        ActiveSection = section;
        RaiseChanged();
    }

    public void SetViewport(ThemeStudioViewport viewport)
    {
        ArgumentNullException.ThrowIfNull(viewport);
        if (!ThemeStudioViewport.All.Contains(viewport))
            throw new ArgumentOutOfRangeException(nameof(viewport), viewport, "Unknown Theme Studio viewport.");
        if (Viewport == viewport) return;
        Viewport = viewport;
        RaiseChanged();
    }

    public void SetMode(ThemeStudioMode mode)
    {
        ValidateEnum(mode, nameof(mode));
        if (Mode == mode) return;
        Mode = mode;
        RaiseChanged();
    }

    public void SetSystemDarkMode(bool isDarkMode)
    {
        if (SystemDarkMode == isDarkMode) return;
        SystemDarkMode = isDarkMode;
        if (Mode == ThemeStudioMode.System)
            RaiseChanged();
    }

    public void SetDirection(ShadcnDirection direction)
    {
        ValidateEnum(direction, nameof(direction));
        if (Direction == direction) return;
        Direction = direction;
        RaiseChanged();
    }

    public void SetLocale(ThemeStudioLocale locale)
    {
        ValidateEnum(locale, nameof(locale));
        if (Locale == locale) return;
        Locale = locale;
        RaiseChanged();
    }

    public void SetReducedMotion(bool reduce)
    {
        if (ReducedMotion == reduce) return;
        ReducedMotion = reduce;
        RaiseChanged();
    }

    public void SetHighContrastPreview(bool enabled)
    {
        if (HighContrastPreview == enabled) return;
        HighContrastPreview = enabled;
        RaiseChanged();
    }

    private static void ValidateEnum<T>(T value, string name) where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new ArgumentOutOfRangeException(name, value, $"Unknown Theme Studio {name}.");
    }

    private void RaiseChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
