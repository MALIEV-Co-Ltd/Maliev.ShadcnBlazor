namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

public interface IComponentExampleRegistry
{
    IReadOnlyList<ComponentExampleDefinition> GetBySlug(string slug);
}
