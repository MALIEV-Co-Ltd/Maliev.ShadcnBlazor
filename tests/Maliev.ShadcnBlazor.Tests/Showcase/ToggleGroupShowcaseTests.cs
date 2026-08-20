using Bunit;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ToggleGroupShowcaseTests : BunitContext
{
    private readonly ComponentDocumentationCatalog _catalog = new();

    public ToggleGroupShowcaseTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
    }

    [Fact]
    public void ToggleGroupDossierUsesARealInteractiveDrawingReviewToolbar()
    {
        var example = Assert.Single(new ComponentExampleRegistry(_catalog).GetBySlug("toggle-group"));
        var cut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, example));

        Assert.NotNull(cut.Find("[data-testid='toggle-group-dossier']"));
        Assert.Equal("Drawing review layers", cut.Find("[data-slot='toggle-group']").GetAttribute("aria-label"));
        Assert.Contains("Dimensions", cut.Find("[data-testid='toggle-group-selection']").TextContent, StringComparison.Ordinal);

        cut.Find("[data-testid='toggle-group-notes']").Click();

        Assert.Equal("true", cut.Find("[data-testid='toggle-group-notes']").GetAttribute("aria-pressed"));
        Assert.Contains("Notes", cut.Find("[data-testid='toggle-group-selection']").TextContent, StringComparison.Ordinal);

        cut.Find("[data-testid='control-toggle-group-multiple']").Change(false);
        cut.Find("[data-testid='toggle-group-notes']").Click();

        Assert.Equal("false", cut.Find("[data-testid='toggle-group-dimensions']").GetAttribute("aria-pressed"));
        Assert.Equal("true", cut.Find("[data-testid='toggle-group-notes']").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void EverySettingUpdatesTheRenderedGroupAndExactRazorSource()
    {
        var example = Assert.Single(new ComponentExampleRegistry(_catalog).GetBySlug("toggle-group"));
        var cut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, example));

        Assert.Equal(
            ["Multiple", "Orientation", "Spacing", "Variant", "Size", "Disabled", "Invalid"],
            cut.FindAll(".component-preview__control > label").Select(control => control.TextContent.Trim()));

        cut.Find("[data-testid='control-toggle-group-multiple']").Change(false);
        cut.SelectControl("toggle-group-orientation", "Vertical");
        cut.SelectControl("toggle-group-spacing", "0");
        cut.SelectControl("toggle-group-variant", "Default");
        cut.SelectControl("toggle-group-size", "Large");
        cut.Find("[data-testid='control-toggle-group-disabled']").Change(true);
        cut.Find("[data-testid='control-toggle-group-invalid']").Change(true);

        var group = cut.Find("[data-slot='toggle-group']");
        Assert.Equal("vertical", group.GetAttribute("data-orientation"));
        Assert.Equal("0", group.GetAttribute("data-spacing"));
        Assert.Equal("default", group.GetAttribute("data-variant"));
        Assert.Equal("lg", group.GetAttribute("data-size"));
        Assert.Equal("true", group.GetAttribute("aria-invalid"));
        Assert.All(cut.FindAll("[data-slot='toggle-group-item']"), item => Assert.True(item.HasAttribute("disabled")));

        var source = example.RazorSource;
        Assert.Contains("Multiple=\"false\"", source, StringComparison.Ordinal);
        Assert.Contains("Orientation=\"ShadcnToggleGroupOrientation.Vertical\"", source, StringComparison.Ordinal);
        Assert.Contains("Spacing=\"0\"", source, StringComparison.Ordinal);
        Assert.Contains("Variant=\"ShadcnToggleVariant.Default\"", source, StringComparison.Ordinal);
        Assert.Contains("Size=\"ShadcnToggleSize.Large\"", source, StringComparison.Ordinal);
        Assert.Contains("Disabled=\"true\"", source, StringComparison.Ordinal);
        Assert.Contains("aria-invalid=\"true\"", source, StringComparison.Ordinal);
        Assert.Contains("Final inspection note", source, StringComparison.Ordinal);
    }
}
