using Bunit;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

internal static class ComponentPreviewTestExtensions
{
    internal static void ChangeControl(this IRenderedComponent<ComponentPreview> cut, string controlId, object value)
    {
        var control = cut.Find($"[data-testid='control-{controlId}']");
        if (control.GetAttribute("data-slot") == "select-trigger")
        {
            cut.SelectControl(controlId, value.ToString()!);
            return;
        }

        control.Change(value);
    }

    internal static void SelectControl(this IRenderedComponent<ComponentPreview> cut, string controlId, string value)
    {
        cut.Find($"[data-testid='control-{controlId}']").Click();
        cut.Find($"[role='option'][data-value='{value}']").Click();
    }

    internal static string[] SelectControlOptions(this IRenderedComponent<ComponentPreview> cut, string controlId)
    {
        var trigger = cut.Find($"[data-testid='control-{controlId}']");
        trigger.Click();
        var options = cut.FindAll("[role='option']").Select(option => option.GetAttribute("data-value")!).ToArray();
        trigger.Click();
        return options;
    }
}
