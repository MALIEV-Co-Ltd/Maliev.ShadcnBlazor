using Bunit;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class FormActionSourceSynchronizationTests : BunitContext
{
    public FormActionSourceSynchronizationTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
    }

    [Fact]
    public void FormSourcesFollowPreviewControls()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());

        var textarea = registry.GetBySlug("textarea").Single();
        var textareaCut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, textarea));
        textareaCut.Find("[data-testid='control-textarea-rows']").Change("5");
        Assert.Contains("Rows=\"5\"", textarea.RazorSource, StringComparison.Ordinal);

        var datePicker = registry.GetBySlug("date-picker").Single();
        Assert.DoesNotContain("AllowTextInput=\"true\"", datePicker.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Mode=\"ShadcnCalendarSelectionMode.Range\"", datePicker.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void ActionSourcesFollowPreviewControls()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());

        var slider = registry.GetBySlug("slider").Single();
        var sliderCut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, slider));
        sliderCut.Find("[data-testid='control-slider-values']").Change("Single");
        Assert.Contains("Values=\"new[] { 40d }\"", slider.RazorSource, StringComparison.Ordinal);

        var toggle = registry.GetBySlug("toggle").Single();
        var toggleCut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, toggle));
        toggleCut.Find("[data-testid='control-toggle-pressed']").Change(false);
        Assert.Contains("Pressed=\"false\"", toggle.RazorSource, StringComparison.Ordinal);
    }
}
