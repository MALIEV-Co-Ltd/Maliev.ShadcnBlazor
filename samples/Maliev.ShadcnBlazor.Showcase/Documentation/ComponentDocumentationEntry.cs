namespace Maliev.ShadcnBlazor.Showcase.Documentation;

public sealed record ComponentDocumentationEntry(
    string Name,
    string Slug,
    string Category,
    string Classification,
    ComponentDocumentationStatus Status,
    ComponentDocumentationEvidence Evidence,
    string Summary,
    string? Namespace,
    string? PrimaryType,
    IReadOnlyList<string> Aliases,
    IReadOnlyList<string> Capabilities,
    IReadOnlyList<string> RelatedSlugs,
    IReadOnlyList<string> TokenGroups)
{
    public string DocumentationUrl => $"docs/components/{Slug}";

    public int RoadmapPhase { get; init; }
}
