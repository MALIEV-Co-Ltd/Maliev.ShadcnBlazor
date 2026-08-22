using System.Globalization;
using Maliev.ShadcnBlazor.Showcase.Components.Theming.Scenarios;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Showcase.ThemeScenarios;

public static class ThemeScenarioFactoryCatalog
{
    public static IReadOnlyList<ThemeScenarioFactoryDescriptor> Create(
        IComponentDocumentationCatalog catalog,
        IReadOnlyList<ThemeScenarioDefinition> definitions)
    {
        var entries = catalog.All.ToDictionary(entry => entry.Slug, StringComparer.Ordinal);
        return definitions.Select(definition =>
        {
            var entry = entries[definition.ComponentSlug];
            var componentType = typeof(Maliev.ShadcnBlazor.Theming.ShadcnOptions).Assembly.GetType($"{entry.Namespace}.{entry.PrimaryType}")
                ?? throw new InvalidOperationException($"Package component '{entry.Namespace}.{entry.PrimaryType}' was not found.");
            if (componentType.IsGenericTypeDefinition)
                componentType = componentType.MakeGenericType(Enumerable.Repeat(typeof(string), componentType.GetGenericArguments().Length).ToArray());
            var root = RootFor(definition.Family);
            var factory = typeof(ThemeScenarioFactory<>).MakeGenericType(root);
            return new ThemeScenarioFactoryDescriptor(definition.Id, factory, root, componentType);
        }).ToArray();
    }

    private static Type RootFor(ThemeScenarioFamily family) => family switch
    {
        ThemeScenarioFamily.SemanticFoundation => typeof(SemanticFoundationScenarioRoot),
        ThemeScenarioFamily.ActionsAndSelection => typeof(ActionsAndSelectionScenarioRoot),
        ThemeScenarioFamily.Forms => typeof(FormsScenarioRoot),
        ThemeScenarioFamily.FeedbackContent => typeof(FeedbackContentScenarioRoot),
        ThemeScenarioFamily.DisclosureNavigation => typeof(DisclosureNavigationScenarioRoot),
        ThemeScenarioFamily.OverlayMenu => typeof(OverlayMenuScenarioRoot),
        ThemeScenarioFamily.DataDisplay => typeof(DataDisplayScenarioRoot),
        ThemeScenarioFamily.ConversationWorkflow => typeof(ConversationWorkflowScenarioRoot),
        _ => throw new InvalidOperationException($"Unsupported scenario family '{family}'.")
    };
}

public sealed class ThemeScenarioFactory<TRoot> : IThemeScenarioFactory where TRoot : ThemeScenarioFamilyRootBase
{
    private readonly Type packageComponentType;

    public ThemeScenarioFactory(Type packageComponentType) => this.packageComponentType = packageComponentType;

    public RenderFragment Create(ThemeScenarioRenderContext context) => builder =>
    {
        builder.OpenComponent<TRoot>(0);
        builder.AddAttribute(1, nameof(ThemeScenarioFamilyRootBase.Scenario), context.Scenario);
        builder.AddAttribute(2, nameof(ThemeScenarioFamilyRootBase.PackageComponentType), packageComponentType);
        builder.AddAttribute(3, nameof(ThemeScenarioFamilyRootBase.Culture), context.Culture);
        builder.CloseComponent();
    };
}
