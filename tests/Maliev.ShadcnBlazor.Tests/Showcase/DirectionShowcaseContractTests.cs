using Bunit;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class DirectionShowcaseContractTests : BunitContext
{
    public DirectionShowcaseContractTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
    }

    [Fact]
    public void DirectionDossierUsesLibraryControlsInARealLocalizedForm()
    {
        var example = GetExample();
        var cut = Render(example.Preview);

        var direction = cut.Find("[data-testid='direction-example']");
        Assert.Equal("rtl", direction.GetAttribute("dir"));
        Assert.Equal("ar", direction.GetAttribute("lang"));
        Assert.NotNull(direction.QuerySelector("form[aria-labelledby='direction-form-title']"));
        Assert.Equal(2, direction.QuerySelectorAll("[data-slot='input']").Length);
        Assert.Equal(2, direction.QuerySelectorAll("[data-slot='label']").Length);
        Assert.Equal("direction-email-help", direction.QuerySelector("#direction-email")!.GetAttribute("aria-describedby"));
        Assert.NotNull(direction.QuerySelector("#direction-email-help"));
        Assert.NotNull(direction.QuerySelector("[data-slot='button']"));
    }

    [Fact]
    public void DirectionControlUpdatesThePreviewAndExactRazorSource()
    {
        var example = GetExample();

        Assert.Contains("Direction=\"null\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("lang=\"ar\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnInput<string>", example.RazorSource, StringComparison.Ordinal);

        example.Controls.Single(control => control.Id == "direction").Apply("Left to right (LTR)");

        var cut = Render(example.Preview);
        var direction = cut.Find("[data-testid='direction-example']");
        Assert.Equal("ltr", direction.GetAttribute("dir"));
        Assert.Equal("en", direction.GetAttribute("lang"));
        Assert.Contains("Direction=\"ShadcnDirection.LeftToRight\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Create a production workspace", example.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("inherited RTL preview", example.RazorSource, StringComparison.Ordinal);
    }

    private static ComponentExampleDefinition GetExample()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());
        return Assert.Single(registry.GetBySlug("direction"));
    }
}
