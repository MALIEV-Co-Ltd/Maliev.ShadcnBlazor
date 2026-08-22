namespace Maliev.ShadcnBlazor.Theming.Internal;

internal static class ShadcnPaletteTokenCatalog
{
    internal static readonly IReadOnlyList<string> Names = Array.AsReadOnly(new[]
    {
        "background", "foreground", "card", "cardForeground", "popover", "popoverForeground",
        "primary", "primaryForeground", "secondary", "secondaryForeground", "muted", "mutedForeground",
        "accent", "accentForeground", "destructive", "destructiveForeground", "border", "input", "ring",
        "chart1", "chart2", "chart3", "chart4", "chart5", "sidebar", "sidebarForeground", "sidebarPrimary",
        "sidebarPrimaryForeground", "sidebarAccent", "sidebarAccentForeground", "sidebarBorder", "sidebarRing"
    });

    internal static bool IsPath(string path) =>
        TrySplit(path, out _, out var name) && Names.Contains(name, StringComparer.Ordinal);

    internal static string Get(ShadcnTheme theme, string path)
    {
        if (!TrySplit(path, out var dark, out var name) || !Names.Contains(name, StringComparer.Ordinal))
            throw new ArgumentException($"Unknown palette token path '{path}'.", nameof(path));
        return Get(dark ? theme.Dark : theme.Light, name);
    }

    internal static ShadcnTheme Set(ShadcnTheme theme, string path, string value)
    {
        if (!TrySplit(path, out var dark, out var name) || !Names.Contains(name, StringComparer.Ordinal))
            throw new ArgumentException($"Unknown palette token path '{path}'.", nameof(path));
        return dark
            ? theme with { Dark = Set(theme.Dark, name, value) }
            : theme with { Light = Set(theme.Light, name, value) };
    }

    private static bool TrySplit(string path, out bool dark, out string name)
    {
        dark = false;
        name = string.Empty;
        var separator = path.IndexOf('.', StringComparison.Ordinal);
        if (separator <= 0 || separator == path.Length - 1)
            return false;
        var scheme = path[..separator];
        dark = scheme == "dark";
        if (!dark && scheme != "light")
            return false;
        name = path[(separator + 1)..];
        return true;
    }

    private static string Get(ShadcnColorScheme scheme, string name) => name switch
    {
        "background" => scheme.Background,
        "foreground" => scheme.Foreground,
        "card" => scheme.Card,
        "cardForeground" => scheme.CardForeground,
        "popover" => scheme.Popover,
        "popoverForeground" => scheme.PopoverForeground,
        "primary" => scheme.Primary,
        "primaryForeground" => scheme.PrimaryForeground,
        "secondary" => scheme.Secondary,
        "secondaryForeground" => scheme.SecondaryForeground,
        "muted" => scheme.Muted,
        "mutedForeground" => scheme.MutedForeground,
        "accent" => scheme.Accent,
        "accentForeground" => scheme.AccentForeground,
        "destructive" => scheme.Destructive,
        "destructiveForeground" => scheme.DestructiveForeground,
        "border" => scheme.Border,
        "input" => scheme.Input,
        "ring" => scheme.Ring,
        "chart1" => scheme.Chart1,
        "chart2" => scheme.Chart2,
        "chart3" => scheme.Chart3,
        "chart4" => scheme.Chart4,
        "chart5" => scheme.Chart5,
        "sidebar" => scheme.Sidebar,
        "sidebarForeground" => scheme.SidebarForeground,
        "sidebarPrimary" => scheme.SidebarPrimary,
        "sidebarPrimaryForeground" => scheme.SidebarPrimaryForeground,
        "sidebarAccent" => scheme.SidebarAccent,
        "sidebarAccentForeground" => scheme.SidebarAccentForeground,
        "sidebarBorder" => scheme.SidebarBorder,
        "sidebarRing" => scheme.SidebarRing,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown palette token.")
    };

    private static ShadcnColorScheme Set(ShadcnColorScheme scheme, string name, string value) => name switch
    {
        "background" => scheme with { Background = value },
        "foreground" => scheme with { Foreground = value },
        "card" => scheme with { Card = value },
        "cardForeground" => scheme with { CardForeground = value },
        "popover" => scheme with { Popover = value },
        "popoverForeground" => scheme with { PopoverForeground = value },
        "primary" => scheme with { Primary = value },
        "primaryForeground" => scheme with { PrimaryForeground = value },
        "secondary" => scheme with { Secondary = value },
        "secondaryForeground" => scheme with { SecondaryForeground = value },
        "muted" => scheme with { Muted = value },
        "mutedForeground" => scheme with { MutedForeground = value },
        "accent" => scheme with { Accent = value },
        "accentForeground" => scheme with { AccentForeground = value },
        "destructive" => scheme with { Destructive = value },
        "destructiveForeground" => scheme with { DestructiveForeground = value },
        "border" => scheme with { Border = value },
        "input" => scheme with { Input = value },
        "ring" => scheme with { Ring = value },
        "chart1" => scheme with { Chart1 = value },
        "chart2" => scheme with { Chart2 = value },
        "chart3" => scheme with { Chart3 = value },
        "chart4" => scheme with { Chart4 = value },
        "chart5" => scheme with { Chart5 = value },
        "sidebar" => scheme with { Sidebar = value },
        "sidebarForeground" => scheme with { SidebarForeground = value },
        "sidebarPrimary" => scheme with { SidebarPrimary = value },
        "sidebarPrimaryForeground" => scheme with { SidebarPrimaryForeground = value },
        "sidebarAccent" => scheme with { SidebarAccent = value },
        "sidebarAccentForeground" => scheme with { SidebarAccentForeground = value },
        "sidebarBorder" => scheme with { SidebarBorder = value },
        "sidebarRing" => scheme with { SidebarRing = value },
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown palette token.")
    };
}
