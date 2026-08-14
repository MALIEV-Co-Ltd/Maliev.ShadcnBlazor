namespace Maliev.ShadcnBlazor.Showcase.Documentation;

public sealed record ComponentDocumentationEvidence(
    bool Api,
    bool ComponentTests,
    bool Accessibility,
    bool Interaction,
    bool ComputedStyle,
    bool Visual,
    bool Integration);
