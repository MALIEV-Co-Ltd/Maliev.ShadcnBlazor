using System.Globalization;
using Bunit;
using Maliev.ShadcnBlazor.Showcase.Components.Theming;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.ThemeScenarios;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeScenarioIntegrationTests : BunitContext
{
    private static readonly IComponentDocumentationCatalog Documentation = new ComponentDocumentationCatalog();
    private static readonly ThemeScenarioRegistry Registry = ThemeScenarioRegistry.Create(
        ThemeScenarioCatalog.Load(Documentation),
        ThemeScenarioFactoryCatalog.Create(Documentation, ThemeScenarioCatalog.Load(Documentation)));

    public ThemeScenarioIntegrationTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
        Services.AddSingleton(Documentation);
        Services.AddSingleton<IThemeScenarioRegistry>(Registry);
        Services.AddTransient<IComponentExampleRegistry>(_ => new ComponentExampleRegistry(Documentation));
    }

    [Fact]
    public void BrowserMountsOnlyTheRequestedScenarioAndExposesStableDirectNavigation()
    {
        var selections = new List<ThemeScenarioSelection>();
        using var cut = Render<ThemeScenarioBrowser>(parameters => parameters
            .Add(component => component.ComponentSlug, "toast")
            .Add(component => component.ScenarioId, "toast-stress")
            .Add(component => component.Culture, CultureInfo.GetCultureInfo("th-TH"))
            .Add(component => component.SelectionChanged,
                EventCallback.Factory.Create<ThemeScenarioSelection>(this, selections.Add)));

        Assert.Single(cut.FindAll("[data-theme-scenario-host]"));
        Assert.NotNull(cut.Find("[data-theme-scenario-id='toast-stress']"));
        Assert.Contains("component=toast", cut.Find("[data-testid='theme-scenario-direct-link']").GetAttribute("href"), StringComparison.Ordinal);
        Assert.Contains("scenario=toast-stress", cut.Find("[data-testid='theme-scenario-direct-link']").GetAttribute("href"), StringComparison.Ordinal);
        Assert.Equal("th", cut.Find("[data-testid='theme-scenario-browser']").GetAttribute("lang"));

        cut.Find("[data-testid='theme-scenario-kind-accessible']").Click();
        Assert.Equal(new("toast", "toast-accessible"), Assert.Single(selections));
    }

    [Fact]
    public void SearchAndSequentialNavigationUseTheRegistryWithoutDossierState()
    {
        ThemeScenarioSelection? selected = null;
        using var cut = Render<ThemeScenarioBrowser>(parameters => parameters
            .Add(component => component.ComponentSlug, "accordion")
            .Add(component => component.ScenarioId, "accordion-default")
            .Add(component => component.Culture, CultureInfo.GetCultureInfo("en-US"))
            .Add(component => component.SelectionChanged,
                EventCallback.Factory.Create<ThemeScenarioSelection>(this, value => selected = value)));

        cut.Find("[data-testid='theme-scenario-search']").Input("invoice");
        Assert.Single(cut.FindAll("[data-theme-scenario-component]"));
        cut.Find("[data-theme-scenario-component='table']").Click();
        Assert.Equal(new("table", "table-default"), selected);

        cut.Find("[data-testid='theme-scenario-next']").Click();
        Assert.Equal(new("alert", "alert-default"), selected);
    }

    [Fact]
    public void KeyedHostCreatesIndependentLifecycleWhenTheActiveScenarioChanges()
    {
        using var cut = Render<ThemeScenarioHost>(parameters => parameters
            .Add(component => component.Scenario, Registry.All.Single(value => value.Id == "button-default"))
            .Add(component => component.Culture, CultureInfo.GetCultureInfo("en-US")));
        var firstLifecycle = cut.Find("[data-theme-scenario-root]").GetAttribute("data-lifecycle-id");

        cut.Render(parameters => parameters
            .Add(component => component.Scenario, Registry.All.Single(value => value.Id == "button-stress"))
            .Add(component => component.Culture, CultureInfo.GetCultureInfo("en-US")));

        Assert.NotEqual(firstLifecycle, cut.Find("[data-theme-scenario-root]").GetAttribute("data-lifecycle-id"));
        Assert.Single(cut.FindAll("[data-theme-scenario-root]"));
    }

    [Fact]
    public void ThemeStudioOwnsScenarioQueryModeWithoutReplacingTheExistingMockSiteDefault()
    {
        var root = FindRoot();
        var page = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "ThemeStudio.razor"));
        var program = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Program.cs"));
        var css = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css"));

        Assert.Contains("<ThemeScenarioBrowser", page, StringComparison.Ordinal);
        Assert.Contains("<MockSiteHost", page, StringComparison.Ordinal);
        Assert.Contains("SupplyParameterFromQuery(Name = \"component\")", page, StringComparison.Ordinal);
        Assert.Contains("SupplyParameterFromQuery(Name = \"scenario\")", page, StringComparison.Ordinal);
        Assert.Contains("IThemeScenarioRegistry", program, StringComparison.Ordinal);
        Assert.Contains(".theme-scenario-browser__layout", css, StringComparison.Ordinal);
        Assert.Contains("grid-template-columns: minmax(12rem, 16rem) minmax(0, 1fr)", css, StringComparison.Ordinal);
        Assert.Contains("@container (max-width: 42rem)", css, StringComparison.Ordinal);
        Assert.Contains("@media (forced-colors: active)", css, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
