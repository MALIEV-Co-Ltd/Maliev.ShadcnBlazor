namespace Maliev.ShadcnBlazor.Showcase.Documentation;

public sealed record DocumentationCatalogFilter(
    string? Category = null,
    string? Classification = null,
    ComponentDocumentationStatus? Status = null);
