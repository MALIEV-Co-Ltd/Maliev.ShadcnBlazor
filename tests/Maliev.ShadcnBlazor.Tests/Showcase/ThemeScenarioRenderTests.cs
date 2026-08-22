using System.Globalization;
using Bunit;
using Maliev.ShadcnBlazor.Showcase.Components.Theming.Scenarios;
using Maliev.ShadcnBlazor.Showcase.ThemeScenarios;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeScenarioRenderTests : BunitContext
{
    private static readonly ThemeScenarioRegistry Registry = ThemeScenarioCatalogTests.CreateRegistry();

    public ThemeScenarioRenderTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
    }

    public static IEnumerable<object[]> ScenarioIds() => Registry.All.Select(value => new object[] { value.Id });

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void EveryScenarioOwnsIndependentStateAndDisposesCleanly(string scenarioId)
    {
        var scenario = Registry.All.Single(value => value.Id == scenarioId);
        var descriptor = Registry.GetFactory(scenarioId);
        var factory = (IThemeScenarioFactory)Activator.CreateInstance(descriptor.FactoryType, descriptor.PackageComponentType)!;
        var context = new ThemeScenarioRenderContext(scenario, CultureInfo.GetCultureInfo("th-TH"));

        var first = Render(factory.Create(context));
        var second = Render(factory.Create(context));
        var firstRoot = first.Find($"[data-theme-scenario-id='{scenarioId}']");
        var secondRoot = second.Find($"[data-theme-scenario-id='{scenarioId}']");
        ThemeScenarioFamilyRootBase firstInstance = scenario.Family switch
        {
            ThemeScenarioFamily.SemanticFoundation => first.FindComponent<SemanticFoundationScenarioRoot>().Instance,
            ThemeScenarioFamily.ActionsAndSelection => first.FindComponent<ActionsAndSelectionScenarioRoot>().Instance,
            ThemeScenarioFamily.Forms => first.FindComponent<FormsScenarioRoot>().Instance,
            ThemeScenarioFamily.FeedbackContent => first.FindComponent<FeedbackContentScenarioRoot>().Instance,
            ThemeScenarioFamily.DisclosureNavigation => first.FindComponent<DisclosureNavigationScenarioRoot>().Instance,
            ThemeScenarioFamily.OverlayMenu => first.FindComponent<OverlayMenuScenarioRoot>().Instance,
            ThemeScenarioFamily.DataDisplay => first.FindComponent<DataDisplayScenarioRoot>().Instance,
            ThemeScenarioFamily.ConversationWorkflow => first.FindComponent<ConversationWorkflowScenarioRoot>().Instance,
            _ => throw new ArgumentOutOfRangeException(nameof(scenario.Family))
        };
        ThemeScenarioFamilyRootBase secondInstance = scenario.Family switch
        {
            ThemeScenarioFamily.SemanticFoundation => second.FindComponent<SemanticFoundationScenarioRoot>().Instance,
            ThemeScenarioFamily.ActionsAndSelection => second.FindComponent<ActionsAndSelectionScenarioRoot>().Instance,
            ThemeScenarioFamily.Forms => second.FindComponent<FormsScenarioRoot>().Instance,
            ThemeScenarioFamily.FeedbackContent => second.FindComponent<FeedbackContentScenarioRoot>().Instance,
            ThemeScenarioFamily.DisclosureNavigation => second.FindComponent<DisclosureNavigationScenarioRoot>().Instance,
            ThemeScenarioFamily.OverlayMenu => second.FindComponent<OverlayMenuScenarioRoot>().Instance,
            ThemeScenarioFamily.DataDisplay => second.FindComponent<DataDisplayScenarioRoot>().Instance,
            ThemeScenarioFamily.ConversationWorkflow => second.FindComponent<ConversationWorkflowScenarioRoot>().Instance,
            _ => throw new ArgumentOutOfRangeException(nameof(scenario.Family))
        };

        Assert.Equal(descriptor.PackageComponentType.FullName, firstRoot.GetAttribute("data-package-component"));
        Assert.Equal(scenario.Family.ToString(), firstRoot.GetAttribute("data-theme-scenario-root"));
        Assert.NotEqual(firstRoot.GetAttribute("data-lifecycle-id"), secondRoot.GetAttribute("data-lifecycle-id"));
        Assert.NotEqual(firstInstance.LifecycleId, secondInstance.LifecycleId);
        Assert.Equal(1, firstInstance.ActivationCount);
        Assert.Equal(1, secondInstance.ActivationCount);
        Assert.False(string.IsNullOrWhiteSpace(firstRoot.InnerHtml));

        firstInstance.Dispose();
        secondInstance.Dispose();
        Assert.True(firstInstance.IsDisposed);
        Assert.True(secondInstance.IsDisposed);
        first.Dispose();
        second.Dispose();
    }
}
