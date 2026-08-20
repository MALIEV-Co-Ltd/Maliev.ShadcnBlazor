using Bunit;
using Maliev.ShadcnBlazor.Components.Forms;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class CalendarShowcaseTests : BunitContext
{
    public CalendarShowcaseTests()
    {
        Services.AddMalievShadcn();
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-forms.js");
        module.SetupVoid("focusCalendarDay", _ => true);
        module.SetupVoid("observePopupDismissal", _ => true);
        module.SetupVoid("disconnectPopupDismissal", _ => true);
        module.SetupVoid("focusElement", _ => true);
    }

    [Fact]
    public void CalendarDossierUsesARealInteractiveScheduleAndExactDynamicSource()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());
        var example = registry.GetBySlug("calendar").Single();
        var cut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, example));

        Assert.NotEmpty(cut.FindAll("[data-slot='card']"));
        Assert.Contains("กำหนดวันตรวจรับ", cut.Markup, StringComparison.Ordinal);
        cut.Find("[data-day='2026-08-18']").Click();
        Assert.Contains("18", cut.Find("[data-testid='calendar-selection']").TextContent, StringComparison.Ordinal);

        Change(cut, "calendar-mode", "Range");
        Change(cut, "calendar-week-numbers", true);
        Change(cut, "calendar-caption-layout", "Dropdown");

        Assert.Equal("true", cut.Find("[data-slot='calendar-grid']").GetAttribute("aria-multiselectable"));
        Assert.NotEmpty(cut.FindAll("[data-slot='calendar-week-number-header']"));
        Assert.NotEmpty(cut.FindAll("[data-slot='calendar-month-select']"));
        Assert.Contains("Mode=\"ShadcnCalendarSelectionMode.Range\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("CaptionLayout=\"ShadcnCalendarCaptionLayout.Dropdown\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShowWeekNumbers=\"true\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("@bind-Range=\"InspectionWindow\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("CultureInfo.GetCultureInfo(\"th-TH\")", example.RazorSource, StringComparison.Ordinal);
    }

    private static void Change(IRenderedComponent<ComponentPreview> cut, string controlId, object value) =>
        cut.ChangeControl(controlId, value);
}
