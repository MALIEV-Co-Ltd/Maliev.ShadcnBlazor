using System.Globalization;
using Bunit;
using Maliev.ShadcnBlazor.Showcase.Components.Theming;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.ThemeScenarios;
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
    public void ThemeStudioUsesOnlyTheCuratedRunwayWhileKeepingScenarioQaInternal()
    {
        var root = FindRoot();
        var page = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "ThemeStudio.razor"));
        var program = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Program.cs"));
        var css = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css"));

        Assert.Contains("<ThemeRunway", page, StringComparison.Ordinal);
        Assert.DoesNotContain("<ThemeScenarioBrowser", page, StringComparison.Ordinal);
        Assert.DoesNotContain("<MockSiteHost", page, StringComparison.Ordinal);
        Assert.DoesNotContain("SupplyParameterFromQuery", page, StringComparison.Ordinal);
        Assert.Contains("IThemeScenarioRegistry", program, StringComparison.Ordinal);
        Assert.DoesNotContain(".theme-scenario-browser", css, StringComparison.Ordinal);
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
