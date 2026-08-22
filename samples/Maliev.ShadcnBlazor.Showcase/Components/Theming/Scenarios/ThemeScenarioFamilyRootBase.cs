using System.Globalization;
using Maliev.ShadcnBlazor.Showcase.ThemeScenarios;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Maliev.ShadcnBlazor.Showcase.Components.Theming.Scenarios;

public abstract class ThemeScenarioFamilyRootBase : ComponentBase, IDisposable
{
    private string? activeScenarioId;

    [Parameter, EditorRequired] public required ThemeScenarioDefinition Scenario { get; set; }
    [Parameter, EditorRequired] public required Type PackageComponentType { get; set; }
    [Parameter] public CultureInfo Culture { get; set; } = CultureInfo.GetCultureInfo("en-US");
    [Parameter, EditorRequired] public RenderFragment Preview { get; set; } = _ => { };

    public Guid LifecycleId { get; } = Guid.NewGuid();
    public int ActivationCount { get; private set; }
    public bool IsDisposed { get; private set; }
    protected abstract ThemeScenarioFamily Family { get; }

    protected override void OnParametersSet()
    {
        if (Scenario.Family != Family)
            throw new InvalidOperationException($"{GetType().Name} cannot render {Scenario.Family} scenarios.");
        if (!typeof(IComponent).IsAssignableFrom(PackageComponentType))
            throw new InvalidOperationException("The scenario package type must implement IComponent.");
        if (!string.Equals(activeScenarioId, Scenario.Id, StringComparison.Ordinal))
        {
            activeScenarioId = Scenario.Id;
            ActivationCount++;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "section");
        builder.AddAttribute(1, "data-theme-scenario-root", Family.ToString());
        builder.AddAttribute(2, "data-theme-scenario-id", Scenario.Id);
        builder.AddAttribute(3, "data-package-component", PackageComponentType.FullName);
        builder.AddAttribute(4, "data-lifecycle-id", LifecycleId.ToString("N", CultureInfo.InvariantCulture));
        builder.AddContent(5, Preview);
        builder.CloseElement();
    }

    public void Dispose() => IsDisposed = true;
}
