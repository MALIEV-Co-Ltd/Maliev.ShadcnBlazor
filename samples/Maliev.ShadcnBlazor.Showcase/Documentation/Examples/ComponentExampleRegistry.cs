namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

public sealed class ComponentExampleRegistry(IComponentDocumentationCatalog documentation) : IComponentExampleRegistry
{
    public IReadOnlyList<ComponentExampleDefinition> GetBySlug(string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        var entry = documentation.FindBySlug(slug);
        if (entry is null)
            return [];

        var semanticExamples = SemanticFoundationExamples.Create(entry.Slug);
        if (semanticExamples.Count > 0)
            return entry.Status == ComponentDocumentationStatus.Complete ? semanticExamples : [];

        var formExamples = FormDateExamples.Create(entry.Slug);
        if (formExamples.Count > 0)
            return formExamples;

        var feedbackExamples = FeedbackContentExamples.Create(entry.Slug);
        if (feedbackExamples.Count > 0)
            return feedbackExamples;

        var disclosureNavigationExamples = DisclosureNavigationExamples.Create(entry.Slug);
        if (disclosureNavigationExamples.Count > 0)
            return disclosureNavigationExamples;

        var overlayExamples = OverlayMenuExamples.Create(entry.Slug);
        if (overlayExamples.Count > 0)
            return overlayExamples;

        var dataDisplayExamples = DataDisplayExamples.Create(entry.Slug);
        if (dataDisplayExamples.Count > 0)
            return dataDisplayExamples;

        var conversationExamples = ConversationWorkflowExamples.Create(entry.Slug);
        if (conversationExamples.Count > 0)
            return conversationExamples;

        return ActionSelectionExamples.Create(entry.Slug);
    }
}
