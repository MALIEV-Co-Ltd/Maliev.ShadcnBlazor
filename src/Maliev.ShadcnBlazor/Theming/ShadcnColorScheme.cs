namespace Maliev.ShadcnBlazor.Theming;

public sealed record ShadcnColorScheme
{
    public required string Background { get; init; }
    public required string Foreground { get; init; }
    public required string Card { get; init; }
    public required string CardForeground { get; init; }
    public required string Popover { get; init; }
    public required string PopoverForeground { get; init; }
    public required string Primary { get; init; }
    public required string PrimaryForeground { get; init; }
    public required string Secondary { get; init; }
    public required string SecondaryForeground { get; init; }
    public required string Muted { get; init; }
    public required string MutedForeground { get; init; }
    public required string Accent { get; init; }
    public required string AccentForeground { get; init; }
    public required string Destructive { get; init; }
    public required string DestructiveForeground { get; init; }
    public required string Border { get; init; }
    public required string Input { get; init; }
    public required string Ring { get; init; }
    public required string Chart1 { get; init; }
    public required string Chart2 { get; init; }
    public required string Chart3 { get; init; }
    public required string Chart4 { get; init; }
    public required string Chart5 { get; init; }
    public required string Sidebar { get; init; }
    public required string SidebarForeground { get; init; }
    public required string SidebarPrimary { get; init; }
    public required string SidebarPrimaryForeground { get; init; }
    public required string SidebarAccent { get; init; }
    public required string SidebarAccentForeground { get; init; }
    public required string SidebarBorder { get; init; }
    public required string SidebarRing { get; init; }
    public required string ShadowExtraSmall { get; init; }
    public required string ShadowSmall { get; init; }
    public required string ShadowMedium { get; init; }
}
