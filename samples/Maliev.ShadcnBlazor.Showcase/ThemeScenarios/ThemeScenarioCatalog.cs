using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using Maliev.ShadcnBlazor.Showcase.Documentation;

namespace Maliev.ShadcnBlazor.Showcase.ThemeScenarios;

public static partial class ThemeScenarioCatalog
{
    public const int SchemaVersion = 1;
    public const int ScenarioCount = 192;
    public const int ComponentCount = 64;
    private const string ResourceSuffix = ".ThemeScenarios.ThemeScenarioCatalog.json";
    private static readonly Lazy<IReadOnlyList<ThemeScenarioDefinition>> Embedded = new(LoadCore);

    public static IReadOnlyList<ThemeScenarioDefinition> Load(IComponentDocumentationCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var definitions = Embedded.Value;
        var expected = catalog.All.ToDictionary(entry => entry.Slug, entry => FamilyForPlan(entry.RoadmapPhase), StringComparer.Ordinal);
        if (expected.Count != ComponentCount)
            throw new InvalidOperationException($"Documentation catalog must contain exactly {ComponentCount} components.");
        var groups = definitions.GroupBy(value => value.ComponentSlug, StringComparer.Ordinal).ToArray();
        if (!groups.Select(group => group.Key).ToHashSet(StringComparer.Ordinal).SetEquals(expected.Keys))
            throw new InvalidOperationException("Scenario slugs must exactly match the documentation catalog.");
        var mismatch = definitions.FirstOrDefault(value => expected[value.ComponentSlug] != value.Family);
        if (mismatch is not null)
            throw new InvalidOperationException($"Scenario '{mismatch.Id}' has the wrong documentation family.");
        return definitions;
    }

    public static string ReadEmbeddedJson()
    {
        var assembly = typeof(ThemeScenarioCatalog).Assembly;
        var name = assembly.GetManifestResourceNames().Single(value => value.EndsWith(ResourceSuffix, StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(name) ?? throw new InvalidOperationException("Scenario resource is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static IReadOnlyList<ThemeScenarioDefinition> LoadCore()
    {
        var document = JsonSerializer.Deserialize<CatalogDocument>(ReadEmbeddedJson(), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow
        }) ?? throw new InvalidOperationException("Scenario catalog is invalid.");
        if (document.SchemaVersion != SchemaVersion || document.Scenarios is null || document.Scenarios.Count != ScenarioCount)
            throw new InvalidOperationException("Scenario catalog version or record count is invalid.");
        var definitions = document.Scenarios.Select(Create).OrderBy(value => value.Id, StringComparer.Ordinal).ToArray();
        if (definitions.Select(value => value.Id).Distinct(StringComparer.Ordinal).Count() != ScenarioCount)
            throw new InvalidOperationException("Scenario ids must be unique.");
        var groups = definitions.GroupBy(value => value.ComponentSlug, StringComparer.Ordinal).ToArray();
        if (groups.Length != ComponentCount || groups.Any(group => group.Count() != 3 ||
            !group.Select(value => value.Kind).ToHashSet().SetEquals(Enum.GetValues<ThemeScenarioKind>())))
            throw new InvalidOperationException("Every component must contain default, stress, and accessible scenarios.");
        return new ReadOnlyCollection<ThemeScenarioDefinition>(definitions);
    }

    private static ThemeScenarioDefinition Create(CatalogScenario value)
    {
        if (string.IsNullOrWhiteSpace(value.Id) || string.IsNullOrWhiteSpace(value.ComponentSlug) ||
            !IdPattern().IsMatch(value.Id) || !value.Id.StartsWith(value.ComponentSlug + '-', StringComparison.Ordinal) ||
            !Enum.TryParse<ThemeScenarioFamily>(value.Family, out var family) ||
            !Enum.TryParse<ThemeScenarioKind>(value.Kind, out var kind) || value.English is null || value.Thai is null || value.Tags is null)
            throw new InvalidOperationException("Scenario record is invalid.");
        if (!value.Id.EndsWith('-' + value.Kind!.ToLowerInvariant(), StringComparison.Ordinal))
            throw new InvalidOperationException($"Scenario '{value.Id}' has an unstable kind suffix.");
        return new(value.Id, value.ComponentSlug, family, kind,
            new(value.English.Title ?? string.Empty, value.English.Description ?? string.Empty),
            new(value.Thai.Title ?? string.Empty, value.Thai.Description ?? string.Empty), value.Tags);
    }

    private static ThemeScenarioFamily FamilyForPlan(int plan) => plan switch
    {
        2 => ThemeScenarioFamily.SemanticFoundation,
        3 => ThemeScenarioFamily.ActionsAndSelection,
        4 => ThemeScenarioFamily.Forms,
        5 => ThemeScenarioFamily.FeedbackContent,
        6 => ThemeScenarioFamily.DisclosureNavigation,
        7 => ThemeScenarioFamily.OverlayMenu,
        8 => ThemeScenarioFamily.DataDisplay,
        9 => ThemeScenarioFamily.ConversationWorkflow,
        _ => throw new InvalidOperationException($"Unsupported documentation plan '{plan}'.")
    };

    [GeneratedRegex("^[a-z0-9-]+-(default|stress|accessible)$", RegexOptions.CultureInvariant)]
    private static partial Regex IdPattern();

    private sealed record CatalogDocument(int SchemaVersion, IReadOnlyList<CatalogScenario>? Scenarios);
    private sealed record CatalogScenario(string? Id, string? ComponentSlug, string? Family, string? Kind,
        CatalogCopy? English, CatalogCopy? Thai, IReadOnlyList<string>? Tags);
    private sealed record CatalogCopy(string? Title, string? Description);
}
