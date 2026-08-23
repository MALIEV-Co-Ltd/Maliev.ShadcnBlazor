namespace Maliev.ShadcnBlazor.Showcase.Theming.Runway;

public enum ThemeRunwayTrack { Left, Right }

public sealed record ThemeUseCaseDefinition(
    string Id,
    int Order,
    ThemeRunwayTrack Track,
    string EnglishTitle,
    string ThaiTitle,
    IReadOnlyList<string> ComponentTypes);

public interface IThemeUseCaseRegistry
{
    IReadOnlyList<ThemeUseCaseDefinition> All { get; }
}

