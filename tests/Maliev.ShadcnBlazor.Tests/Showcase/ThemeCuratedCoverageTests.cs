using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Theming.Runway;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeCuratedCoverageTests
{
    [Fact]
    public void EveryPublicComponentHasThreeRealCuratedUsages()
    {
        var workflows = new ThemeUseCaseRegistry().All.ToDictionary(value => value.Id, StringComparer.Ordinal);
        var coverage = new ThemeCuratedCoverageRegistry().All.ToDictionary(value => value.ComponentSlug, StringComparer.Ordinal);
        var insufficient = new List<string>();
        foreach (var component in new ComponentDocumentationCatalog().All)
        {
            var entry = Assert.Contains(component.Slug, coverage);
            if (entry.WorkflowIds.Distinct(StringComparer.Ordinal).Count() < 3)
                insufficient.Add($"{component.Slug}={entry.WorkflowIds.Count} [{string.Join(", ", entry.WorkflowIds)}]");
            Assert.All(entry.WorkflowIds, id => Assert.True(workflows.ContainsKey(id), $"Missing workflow {id}"));
        }
        Assert.True(insufficient.Count == 0, string.Join(Environment.NewLine, insufficient));
    }
}
