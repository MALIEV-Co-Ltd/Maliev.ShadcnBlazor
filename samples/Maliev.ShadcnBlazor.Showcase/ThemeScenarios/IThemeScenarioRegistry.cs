using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Showcase.ThemeScenarios;

public interface IThemeScenarioFactory
{
    RenderFragment Create(ThemeScenarioRenderContext context);
}

public sealed record ThemeScenarioFactoryDescriptor(
    string ScenarioId,
    Type FactoryType,
    Type RootComponentType,
    Type PackageComponentType);

public interface IThemeScenarioRegistry
{
    IReadOnlyList<ThemeScenarioDefinition> All { get; }
    IReadOnlyList<ThemeScenarioDefinition> ForComponent(string slug);
    IReadOnlyList<ThemeScenarioDefinition> Find(string? query);
    ThemeScenarioFactoryDescriptor GetFactory(string scenarioId);
}
