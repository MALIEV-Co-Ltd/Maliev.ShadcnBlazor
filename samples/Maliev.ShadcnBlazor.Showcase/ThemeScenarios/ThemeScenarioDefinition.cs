using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Showcase.ThemeScenarios;

public enum ThemeScenarioFamily
{
    SemanticFoundation,
    ActionsAndSelection,
    Forms,
    FeedbackContent,
    DisclosureNavigation,
    OverlayMenu,
    DataDisplay,
    ConversationWorkflow
}

public enum ThemeScenarioKind
{
    Default,
    Stress,
    Accessible
}

public sealed record ThemeScenarioCopy(string Title, string Description);

public sealed record ThemeScenarioDefinition
{
    public ThemeScenarioDefinition(string id, string componentSlug, ThemeScenarioFamily family, ThemeScenarioKind kind,
        ThemeScenarioCopy english, ThemeScenarioCopy thai, IEnumerable<string> tags)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(componentSlug);
        ArgumentNullException.ThrowIfNull(english);
        ArgumentNullException.ThrowIfNull(thai);
        ArgumentNullException.ThrowIfNull(tags);
        Id = id.Trim();
        ComponentSlug = componentSlug.Trim();
        Family = family;
        Kind = kind;
        English = english;
        Thai = thai;
        Tags = new ReadOnlyCollection<string>(tags.Select(tag => tag.Trim()).Where(tag => tag.Length != 0)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
        if (Tags.Count == 0)
            throw new ArgumentException("At least one scenario tag is required.", nameof(tags));
    }

    public string Id { get; }
    public string ComponentSlug { get; }
    public ThemeScenarioFamily Family { get; }
    public ThemeScenarioKind Kind { get; }
    public ThemeScenarioCopy English { get; }
    public ThemeScenarioCopy Thai { get; }
    public IReadOnlyList<string> Tags { get; }

    public ThemeScenarioCopy GetCopy(CultureInfo culture) =>
        string.Equals(culture.TwoLetterISOLanguageName, "th", StringComparison.OrdinalIgnoreCase) ? Thai : English;
}

public sealed record ThemeScenarioRenderContext(ThemeScenarioDefinition Scenario, CultureInfo Culture, RenderFragment Preview);
