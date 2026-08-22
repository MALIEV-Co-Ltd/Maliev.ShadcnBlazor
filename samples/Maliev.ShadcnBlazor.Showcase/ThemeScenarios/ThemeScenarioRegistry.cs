using System.Collections.ObjectModel;

namespace Maliev.ShadcnBlazor.Showcase.ThemeScenarios;

public sealed class ThemeScenarioRegistry : IThemeScenarioRegistry
{
    private readonly IReadOnlyDictionary<string, ThemeScenarioFactoryDescriptor> factories;

    private ThemeScenarioRegistry(IReadOnlyList<ThemeScenarioDefinition> definitions,
        IReadOnlyDictionary<string, ThemeScenarioFactoryDescriptor> factories)
    {
        All = definitions;
        this.factories = factories;
    }

    public IReadOnlyList<ThemeScenarioDefinition> All { get; }

    public static ThemeScenarioRegistry Create(IEnumerable<ThemeScenarioDefinition> definitions,
        IEnumerable<ThemeScenarioFactoryDescriptor> descriptors)
    {
        var all = definitions.ToArray();
        var supplied = descriptors.ToArray();
        var duplicate = supplied.GroupBy(value => value.ScenarioId, StringComparer.Ordinal).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
            throw new InvalidOperationException($"Duplicate factory descriptor for scenario '{duplicate.Key}'.");
        var byId = supplied.ToDictionary(value => value.ScenarioId, StringComparer.Ordinal);
        var ids = all.Select(value => value.Id).ToHashSet(StringComparer.Ordinal);
        var missing = ids.Except(byId.Keys, StringComparer.Ordinal).Order().ToArray();
        if (missing.Length != 0)
            throw new InvalidOperationException($"Factory descriptors are missing scenarios: {string.Join(", ", missing)}.");
        var unknown = byId.Keys.Except(ids, StringComparer.Ordinal).Order().ToArray();
        if (unknown.Length != 0)
            throw new InvalidOperationException($"Factory descriptors reference unknown scenarios: {string.Join(", ", unknown)}.");
        foreach (var descriptor in supplied)
        {
            if (!typeof(IThemeScenarioFactory).IsAssignableFrom(descriptor.FactoryType) || descriptor.FactoryType.IsAbstract)
                throw new ArgumentException($"Factory type must implement {nameof(IThemeScenarioFactory)}.", nameof(descriptors));
            if (!typeof(Microsoft.AspNetCore.Components.IComponent).IsAssignableFrom(descriptor.RootComponentType) ||
                !typeof(Microsoft.AspNetCore.Components.IComponent).IsAssignableFrom(descriptor.PackageComponentType))
                throw new ArgumentException("Scenario roots and package component types must implement IComponent.", nameof(descriptors));
        }
        return new(new ReadOnlyCollection<ThemeScenarioDefinition>(all),
            new ReadOnlyDictionary<string, ThemeScenarioFactoryDescriptor>(byId));
    }

    public IReadOnlyList<ThemeScenarioDefinition> ForComponent(string slug) => new ReadOnlyCollection<ThemeScenarioDefinition>(
        All.Where(value => string.Equals(value.ComponentSlug, slug?.Trim(), StringComparison.OrdinalIgnoreCase)).ToArray());

    public IReadOnlyList<ThemeScenarioDefinition> Find(string? query)
    {
        var value = string.Join(' ', (query ?? string.Empty).Split((char[]?)null,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        if (value.Length == 0)
            return All;
        return new ReadOnlyCollection<ThemeScenarioDefinition>(All.Where(definition =>
            new[] { definition.Id, definition.ComponentSlug, definition.English.Title, definition.English.Description,
                    definition.Thai.Title, definition.Thai.Description }.Concat(definition.Tags)
                .Any(candidate => candidate.Contains(value, StringComparison.OrdinalIgnoreCase))).ToArray());
    }

    public ThemeScenarioFactoryDescriptor GetFactory(string scenarioId) =>
        factories.TryGetValue(scenarioId, out var descriptor) ? descriptor : throw new KeyNotFoundException(scenarioId);
}
