using Maliev.ShadcnBlazor.Showcase.ThemeScenarios;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

public static class ThemeScenarioPreviewFactory
{
    public static RenderFragment Create(IComponentExampleRegistry examples, ThemeScenarioDefinition scenario)
    {
        ArgumentNullException.ThrowIfNull(examples);
        ArgumentNullException.ThrowIfNull(scenario);

        var example = examples.GetBySlug(scenario.ComponentSlug).FirstOrDefault()
            ?? throw new InvalidOperationException($"Component '{scenario.ComponentSlug}' has no showcase example.");
        ApplyScenarioState(example.Controls, scenario.Kind);
        return example.Preview;
    }

    private static void ApplyScenarioState(IReadOnlyList<ComponentParameterControl> controls, ThemeScenarioKind kind)
    {
        foreach (var control in controls)
        {
            if (kind == ThemeScenarioKind.Stress)
            {
                if (control.Kind == ComponentParameterControlKind.Select && control.Options.Count > 1)
                    control.Apply(control.Options[^1]);
                else if (control.Kind == ComponentParameterControlKind.Toggle && IsStressControl(control.Id))
                    control.Apply(bool.TrueString);
            }
            else if (kind == ThemeScenarioKind.Accessible &&
                     control.Kind == ComponentParameterControlKind.Toggle &&
                     IsAccessibilityControl(control.Id))
            {
                control.Apply(bool.TrueString);
            }
        }
    }

    private static bool IsStressControl(string id) => ContainsAny(id,
        "compact", "dense", "group", "indeterminate", "loading", "multiple", "stacked", "vertical");

    private static bool IsAccessibilityControl(string id) => ContainsAny(id,
        "disabled", "footer", "invalid", "label", "required", "show-value");

    private static bool ContainsAny(string value, params string[] fragments) =>
        fragments.Any(fragment => value.Contains(fragment, StringComparison.OrdinalIgnoreCase));
}
