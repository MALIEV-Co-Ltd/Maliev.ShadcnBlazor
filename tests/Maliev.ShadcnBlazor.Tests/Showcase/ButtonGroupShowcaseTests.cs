using Bunit;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ButtonGroupShowcaseTests : BunitContext
{
    public ButtonGroupShowcaseTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
    }

    [Fact]
    public void DossierUsesInteractiveProductionToolbarAndSynchronizedSource()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());
        var example = registry.GetBySlug("button-group").Single();
        var cut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, example));

        Assert.Contains("Production review actions", cut.Find("[data-testid='button-group-dossier-preview']").TextContent, StringComparison.Ordinal);
        cut.Find("[data-testid='button-group-archive']").Click();
        Assert.Contains("Quotation archived", cut.Find("[data-testid='button-group-last-action']").TextContent, StringComparison.Ordinal);

        cut.SelectControl("button-group-orientation", "Vertical");
        Assert.Equal("vertical", cut.Find("[data-testid='action-button-group']").GetAttribute("data-orientation"));
        Assert.Contains("Orientation=\"ShadcnButtonGroupOrientation.Vertical\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Orientation=\"ShadcnButtonGroupOrientation.Horizontal\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("OnClick=\"Archive\"", example.RazorSource, StringComparison.Ordinal);
    }
}
