using System.Collections.ObjectModel;
using System.Text.Json;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.ThemeScenarios;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeScenarioCatalogTests
{
    private static readonly IComponentDocumentationCatalog Documentation = new ComponentDocumentationCatalog();

    [Fact]
    public void CatalogContainsThreeStableBilingualScenariosForEveryDocumentedComponent()
    {
        var all = ThemeScenarioCatalog.Load(Documentation);
        Assert.Equal(201, all.Count);
        Assert.Equal(Documentation.All.Select(value => value.Slug).Order(), all.Select(value => value.ComponentSlug).Distinct().Order());
        Assert.Equal(all.OrderBy(value => value.Id, StringComparer.Ordinal).Select(value => value.Id), all.Select(value => value.Id));
        Assert.All(all.GroupBy(value => value.ComponentSlug), group =>
        {
            Assert.Equal(3, group.Count());
            Assert.Equal([ThemeScenarioKind.Default, ThemeScenarioKind.Stress, ThemeScenarioKind.Accessible],
                group.Select(value => value.Kind).Order());
            Assert.Equal(group.Select(value => $"{group.Key}-{value.Kind.ToString().ToLowerInvariant()}").Order(), group.Select(value => value.Id).Order());
        });
        Assert.Equal(201, all.Select(value => value.English.Title).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(201, all.Select(value => value.Thai.Title).Distinct(StringComparer.Ordinal).Count());
        Assert.All(all, value =>
        {
            Assert.NotEmpty(value.English.Description);
            Assert.NotEmpty(value.Thai.Description);
            Assert.Contains(value.Thai.Title, character => character is >= '\u0E00' and <= '\u0E7F');
            Assert.NotEmpty(value.Tags);
        });
        Assert.Equal(3, all.Count(value => value.ComponentSlug == "code-block"));
    }

    [Fact]
    public void CatalogAndSearchCollectionsAreImmutableAndBilingual()
    {
        var all = ThemeScenarioCatalog.Load(Documentation);
        var registry = CreateRegistry();
        Assert.IsType<ReadOnlyCollection<ThemeScenarioDefinition>>(all);
        Assert.Throws<NotSupportedException>(() => ((IList<ThemeScenarioDefinition>)all).Clear());
        Assert.IsType<ReadOnlyCollection<string>>(all[0].Tags);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)all[0].Tags).Add("mutation"));
        Assert.Contains(registry.Find("maintenance"), value => value.Id == "accordion-default");
        Assert.Contains(registry.Find("แป้นพิมพ์"), value => value.Kind == ThemeScenarioKind.Accessible);
        Assert.Equal(3, registry.ForComponent("toast").Count);
    }

    [Fact]
    public void ManifestAndFactoriesRemainIndependentFromDossierStateAndPackageApi()
    {
        var json = ThemeScenarioCatalog.ReadEmbeddedJson();
        using var document = JsonDocument.Parse(json);
        Assert.Equal(1, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.DoesNotContain("factory", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("assembly", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(typeof(Maliev.ShadcnBlazor.Theming.ShadcnOptions).Assembly.GetExportedTypes(),
            type => type.Namespace?.Contains("ThemeScenario", StringComparison.Ordinal) == true);
        var root = FindRoot();
        var source = string.Join('\n', Directory.GetFiles(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "ThemeScenarios"), "*.cs")
            .Concat(Directory.GetFiles(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Scenarios"), "*.cs"))
            .Select(File.ReadAllText));
        Assert.DoesNotContain("ComponentExample", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ComponentDossier", source, StringComparison.Ordinal);
    }

    [Fact]
    public void RegistryRequiresExactlyOneValidIndependentFactoryForEveryScenario()
    {
        var all = ThemeScenarioCatalog.Load(Documentation);
        var complete = ThemeScenarioFactoryCatalog.Create(Documentation, all);
        var first = complete[0];

        var missing = Assert.Throws<InvalidOperationException>(() => ThemeScenarioRegistry.Create(all, complete.Skip(1)));
        var unknown = Assert.Throws<InvalidOperationException>(() => ThemeScenarioRegistry.Create(all,
            complete.Append(first with { ScenarioId = "unknown-default" })));
        var duplicate = Assert.Throws<InvalidOperationException>(() => ThemeScenarioRegistry.Create(all,
            complete.Append(first)));
        var invalidFactory = complete.ToArray();
        invalidFactory[0] = first with { FactoryType = typeof(string) };
        var invalid = Assert.Throws<ArgumentException>(() => ThemeScenarioRegistry.Create(all, invalidFactory));

        Assert.Contains("missing", missing.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("unknown", unknown.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("duplicate", duplicate.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(nameof(IThemeScenarioFactory), invalid.Message, StringComparison.Ordinal);
    }

    internal static ThemeScenarioRegistry CreateRegistry()
    {
        var all = ThemeScenarioCatalog.Load(Documentation);
        return ThemeScenarioRegistry.Create(all, ThemeScenarioFactoryCatalog.Create(Documentation, all));
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
