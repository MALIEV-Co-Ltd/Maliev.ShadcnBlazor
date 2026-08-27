namespace Maliev.ShadcnBlazor.Showcase.Theming.Runway;

public enum ThemeBentoSize { Standard, Wide, Tall }

public enum ThemeUseCaseCategory
{
    Overview,
    Forms,
    Data,
    Communication,
    Overlays,
    Security,
    Media
}

public sealed record ThemeUseCaseDefinition(
    string Id,
    int Order,
    ThemeBentoSize Size,
    ThemeUseCaseCategory Category,
    string EnglishTitle,
    string ThaiTitle,
    IReadOnlyList<string> ComponentTypes);

public interface IThemeUseCaseRegistry
{
    IReadOnlyList<ThemeUseCaseDefinition> All { get; }
}

