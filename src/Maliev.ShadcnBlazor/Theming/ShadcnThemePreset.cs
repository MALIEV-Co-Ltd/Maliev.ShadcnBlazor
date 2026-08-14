namespace Maliev.ShadcnBlazor.Theming;

public sealed record ShadcnThemePreset(string Id, string DisplayName, ShadcnTheme Theme)
{
    public ShadcnTheme CreateTheme() => Theme.DeepClone();
}
