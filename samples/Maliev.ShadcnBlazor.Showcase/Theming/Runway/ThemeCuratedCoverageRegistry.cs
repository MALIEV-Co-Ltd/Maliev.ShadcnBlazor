using System.Collections.ObjectModel;
using Maliev.ShadcnBlazor.Showcase.Documentation;

namespace Maliev.ShadcnBlazor.Showcase.Theming.Runway;

public sealed record ThemeCuratedCoverage(string ComponentSlug, IReadOnlyList<string> WorkflowIds);

public interface IThemeCuratedCoverageRegistry
{
    IReadOnlyList<ThemeCuratedCoverage> All { get; }
}

public sealed class ThemeCuratedCoverageRegistry : IThemeCuratedCoverageRegistry
{
    public ThemeCuratedCoverageRegistry()
        : this(new ComponentDocumentationCatalog(), new ThemeUseCaseRegistry())
    {
    }

    public ThemeCuratedCoverageRegistry(IComponentDocumentationCatalog catalog, IThemeUseCaseRegistry workflows)
    {
        All = new ReadOnlyCollection<ThemeCuratedCoverage>(catalog.All
            .Select(component => new ThemeCuratedCoverage(component.Slug,
                new ReadOnlyCollection<string>(workflows.All
                    .Where(workflow => workflow.ComponentTypes.Contains(Normalize(component.PrimaryType), StringComparer.Ordinal))
                    .Select(workflow => workflow.Id)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray())))
            .ToArray());
    }

    public IReadOnlyList<ThemeCuratedCoverage> All { get; }

    private static string Normalize(string? primaryType)
    {
        var value = primaryType ?? string.Empty;
        var generic = value.IndexOf('`');
        return generic < 0 ? value : value[..generic];
    }
}
