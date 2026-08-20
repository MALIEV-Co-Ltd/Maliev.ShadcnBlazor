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
        var datePickerCut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, datePicker));
        datePickerCut.Find("[data-testid='control-date-picker-mode']").Change("Single");
        Assert.Contains("Mode=\"ShadcnCalendarSelectionMode.Single\"", datePicker.RazorSource, StringComparison.Ordinal);
        Assert.Contains("@bind-Value=\"SelectedDate\"", datePicker.RazorSource, StringComparison.Ordinal);
        Assert.Contains("@bind-Open=\"DatePickerOpen\"", datePicker.RazorSource, StringComparison.Ordinal);
        Assert.Contains("private DateOnly? SelectedDate", datePicker.RazorSource, StringComparison.Ordinal);
        datePickerCut.Find("[data-testid='control-date-picker-invalid']").Change(true);
        datePickerCut.Find("[data-testid='control-date-picker-clearable']").Change(false);
        Assert.Contains("Invalid=\"true\"", datePicker.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Clearable=\"false\"", datePicker.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void ActionSourcesFollowPreviewControls()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());

        var slider = registry.GetBySlug("slider").Single();
        var sliderCut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, slider));
        sliderCut.Find("[data-testid='control-slider-values']").Change("Single");
        Assert.Contains("@bind-Values=\"SliderValues\"", slider.RazorSource, StringComparison.Ordinal);
        Assert.Contains("private IReadOnlyList<double> SliderValues { get; set; } = [40d];", slider.RazorSource, StringComparison.Ordinal);

        sliderCut.Find("[data-testid='action-slider'] input[type='range']").Input("65");
        sliderCut.WaitForAssertion(() =>
            Assert.Contains("private IReadOnlyList<double> SliderValues { get; set; } = [65d];", slider.RazorSource, StringComparison.Ordinal));

        sliderCut.Find("[data-testid='control-slider-orientation']").Change("Vertical");
        sliderCut.Find("[data-testid='control-slider-readonly']").Change(true);
        Assert.Contains("Orientation=\"ShadcnSliderOrientation.Vertical\"", slider.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ReadOnly=\"true\"", slider.RazorSource, StringComparison.Ordinal);

        var toggle = registry.GetBySlug("toggle").Single();
        var toggleCut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, toggle));
        toggleCut.Find("[data-testid='control-toggle-pressed']").Change(false);
        Assert.Contains("Pressed=\"false\"", toggle.RazorSource, StringComparison.Ordinal);

        var radioGroup = registry.GetBySlug("radio-group").Single();
        var radioCut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, radioGroup));
        radioCut.Find("[data-testid='control-radio-orientation']").Change("Horizontal");
        radioCut.Find("[data-testid='control-radio-readonly']").Change(true);

        Assert.Contains("@bind-Value=\"ReviewSpeed\"", radioGroup.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Orientation=\"ShadcnRadioGroupOrientation.Horizontal\"", radioGroup.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ReadOnly=\"true\"", radioGroup.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Priority review", radioGroup.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Same-day review", radioGroup.RazorSource, StringComparison.Ordinal);
    }
}
