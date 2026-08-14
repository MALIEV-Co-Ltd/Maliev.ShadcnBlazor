using System.Collections.ObjectModel;
using System.Text.Json;
using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Showcase.Documentation;

public interface IComponentDocumentationCatalog
{
    IReadOnlyList<ComponentDocumentationEntry> All { get; }

    ComponentDocumentationEntry? FindBySlug(string slug);

    IReadOnlyList<ComponentDocumentationEntry> Search(string? query, DocumentationCatalogFilter filter);
}

public sealed class ComponentDocumentationCatalog : IComponentDocumentationCatalog
{
    private const string DocumentationMetadataResourceSuffix = ".Documentation.ComponentDocumentationCatalog.json";
    private const string LedgerResourceSuffix = ".Documentation.component-catalog.json";
    private readonly IReadOnlyDictionary<string, ComponentDocumentationEntry> entriesBySlug;

    public ComponentDocumentationCatalog()
        : this(
            ReadEmbeddedResource(DocumentationMetadataResourceSuffix),
            ReadEmbeddedResource(LedgerResourceSuffix))
    {
    }

    private ComponentDocumentationCatalog(string metadataJson, string ledgerJson)
    {
        var metadata = JsonSerializer.Deserialize<DocumentationMetadata>(metadataJson, SerializerOptions)
            ?? throw new InvalidOperationException("Documentation metadata is invalid.");
        var ledger = JsonSerializer.Deserialize<ComponentLedger>(ledgerJson, SerializerOptions)
            ?? throw new InvalidOperationException("Component ledger is invalid.");

        var metadataBySlug = ValidateMetadata(metadata.Components);
        var ledgerBySlug = ValidateLedger(ledger.Components);
        if (!metadataBySlug.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(ledgerBySlug.Keys))
            throw new InvalidOperationException("Documentation metadata must contain exactly one entry for every ledger component.");

        var entries = ledger.Components!
            .OrderBy(entry => entry.Name, StringComparer.Ordinal)
            .Select(entry => CreateEntry(entry, metadataBySlug))
            .ToArray();

        All = new ReadOnlyCollection<ComponentDocumentationEntry>(entries);
        entriesBySlug = new ReadOnlyDictionary<string, ComponentDocumentationEntry>(
            entries.ToDictionary(entry => entry.Slug, StringComparer.OrdinalIgnoreCase));
    }

    public IReadOnlyList<ComponentDocumentationEntry> All { get; }

    public ComponentDocumentationEntry? FindBySlug(string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        return entriesBySlug.TryGetValue(slug.Trim(), out var entry) ? entry : null;
    }

    public IReadOnlyList<ComponentDocumentationEntry> Search(string? query, DocumentationCatalogFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        var normalizedQuery = NormalizeWhitespace(query);
        var matches = All.Where(entry => MatchesFilter(entry, filter) && MatchesQuery(entry, normalizedQuery)).ToArray();
        return new ReadOnlyCollection<ComponentDocumentationEntry>(matches);
    }

    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };

    private static string ReadEmbeddedResource(string suffix)
    {
        var assembly = typeof(ComponentDocumentationCatalog).Assembly;
        var resourceName = assembly.GetManifestResourceNames().SingleOrDefault(name => name.EndsWith(suffix, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Missing embedded resource ending with '{suffix}'.");
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not read embedded resource '{resourceName}'.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static IReadOnlyDictionary<string, DocumentationMetadataEntry> ValidateMetadata(IReadOnlyList<DocumentationMetadataEntry>? components)
    {
        if (components is null || components.Count == 0)
            throw new InvalidOperationException("Documentation metadata must contain components.");

        var entries = new Dictionary<string, DocumentationMetadataEntry>(StringComparer.Ordinal);
        foreach (var component in components)
        {
            if (string.IsNullOrWhiteSpace(component.Slug) || !entries.TryAdd(component.Slug, component))
                throw new InvalidOperationException("Documentation metadata contains an empty or duplicate slug.");
        }

        return new ReadOnlyDictionary<string, DocumentationMetadataEntry>(entries);
    }

    private static IReadOnlyDictionary<string, LedgerComponent> ValidateLedger(IReadOnlyList<LedgerComponent>? components)
    {
        if (components is null || components.Count == 0)
            throw new InvalidOperationException("The component ledger must contain components.");

        var entries = new Dictionary<string, LedgerComponent>(StringComparer.Ordinal);
        foreach (var component in components)
        {
            if (string.IsNullOrWhiteSpace(component.Name) || string.IsNullOrWhiteSpace(component.Slug) ||
                string.IsNullOrWhiteSpace(component.Classification) || string.IsNullOrWhiteSpace(component.Status) ||
                component.Plan <= 0 || component.Evidence is null || !entries.TryAdd(component.Slug, component))
                throw new InvalidOperationException("The component ledger contains an invalid or duplicate component.");
        }

        return new ReadOnlyDictionary<string, LedgerComponent>(entries);
    }

    private static ComponentDocumentationEntry CreateEntry(
        LedgerComponent component,
        IReadOnlyDictionary<string, DocumentationMetadataEntry> metadataBySlug)
    {
        var name = component.Name!;
        var slug = component.Slug!;
        var classification = component.Classification!;
        var statusText = component.Status!;
        if (!metadataBySlug.TryGetValue(slug, out var metadata))
            throw new InvalidOperationException($"No documentation metadata exists for '{component.Slug}'.");
        if (!Enum.TryParse<ComponentDocumentationStatus>(statusText, true, out var status))
            throw new InvalidOperationException($"Unsupported ledger status '{component.Status}'.");
        var hasImplementationType = !string.IsNullOrWhiteSpace(metadata.Namespace) || !string.IsNullOrWhiteSpace(metadata.PrimaryType);
        if (status == ComponentDocumentationStatus.Complete &&
            (string.IsNullOrWhiteSpace(metadata.Namespace) || string.IsNullOrWhiteSpace(metadata.PrimaryType)))
            throw new InvalidOperationException($"Completed component '{slug}' requires authoritative API metadata.");
        if (status == ComponentDocumentationStatus.Complete &&
            typeof(ShadcnOptions).Assembly.GetType($"{metadata.Namespace}.{metadata.PrimaryType}") is null)
            throw new InvalidOperationException($"Completed component '{slug}' references an unknown implementation type.");

        return new ComponentDocumentationEntry(
            name,
            slug,
            GetCategory(component.Plan),
            classification,
            status,
            new ComponentDocumentationEvidence(
                component.Evidence!.Api,
                component.Evidence.ComponentTests,
                component.Evidence.Accessibility,
                component.Evidence.Interaction,
                component.Evidence.ComputedStyle,
                component.Evidence.Visual,
                component.Evidence.Integration),
            $"Documentation for the {name} component.",
            metadata.Namespace,
            metadata.PrimaryType,
            ToReadOnly(metadata.Aliases),
            ToReadOnly(metadata.Capabilities),
            Array.Empty<string>(),
            new ReadOnlyCollection<string>(["semantic"]))
        {
            RoadmapPhase = component.Plan
        };
    }

    private static IReadOnlyList<string> ToReadOnly(IReadOnlyList<string>? values) =>
        new ReadOnlyCollection<string>((values ?? []).Where(value => !string.IsNullOrWhiteSpace(value)).ToArray());

    private static string GetCategory(int plan) => plan switch
    {
        <= 2 => "Foundation",
        <= 4 => "Forms",
        <= 5 => "Feedback",
        <= 6 => "Layout",
        <= 7 => "Overlays",
        <= 8 => "Data",
        _ => "Composition"
    };

    private static bool MatchesFilter(ComponentDocumentationEntry entry, DocumentationCatalogFilter filter) =>
        (filter.Category is null || string.Equals(entry.Category, filter.Category.Trim(), StringComparison.OrdinalIgnoreCase)) &&
        (filter.Classification is null || string.Equals(entry.Classification, filter.Classification.Trim(), StringComparison.OrdinalIgnoreCase)) &&
        (filter.Status is null || entry.Status == filter.Status);

    private static bool MatchesQuery(ComponentDocumentationEntry entry, string normalizedQuery)
    {
        if (normalizedQuery.Length == 0)
            return true;

        return new[] { entry.Name, entry.Slug, entry.Namespace, entry.PrimaryType }
            .Concat(entry.Aliases)
            .Concat(entry.Capabilities)
            .Where(value => value is not null)
            .Any(value => value!.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeWhitespace(string? value) => string.Join(' ', (value ?? string.Empty)
        .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private sealed record DocumentationMetadata(IReadOnlyList<DocumentationMetadataEntry>? Components);

    private sealed record DocumentationMetadataEntry(
        string? Slug,
        string? Namespace,
        string? PrimaryType,
        IReadOnlyList<string>? Aliases,
        IReadOnlyList<string>? Capabilities);

    private sealed record ComponentLedger(IReadOnlyList<LedgerComponent>? Components);

    private sealed record LedgerComponent(
        string? Name,
        string? Slug,
        int Plan,
        string? Classification,
        string? Status,
        LedgerEvidence? Evidence);

    private sealed record LedgerEvidence(
        bool Api,
        bool ComponentTests,
        bool Accessibility,
        bool Interaction,
        bool ComputedStyle,
        bool Visual,
        bool Integration);
}
