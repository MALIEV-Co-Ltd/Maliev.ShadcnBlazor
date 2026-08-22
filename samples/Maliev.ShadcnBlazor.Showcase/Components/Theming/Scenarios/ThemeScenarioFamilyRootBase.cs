using System.Globalization;
using Maliev.ShadcnBlazor.Components.Conversation;
using Maliev.ShadcnBlazor.Components.DataDisplay;
using Maliev.ShadcnBlazor.Components.Navigation.Sidebar;
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
        var copy = Scenario.GetCopy(Culture);
        builder.OpenElement(0, "section");
        builder.AddAttribute(1, "data-theme-scenario-root", Family.ToString());
        builder.AddAttribute(2, "data-theme-scenario-id", Scenario.Id);
        builder.AddAttribute(3, "data-package-component", PackageComponentType.FullName);
        builder.AddAttribute(4, "data-lifecycle-id", LifecycleId.ToString("N", CultureInfo.InvariantCulture));
        RenderPackageComponent(builder, 5, copy);
        builder.CloseElement();
    }

    private void RenderPackageComponent(RenderTreeBuilder builder, int sequence, ThemeScenarioCopy copy)
    {
        if (PackageComponentType == typeof(ShadcnMessageScroller))
        {
            builder.OpenComponent<ShadcnMessageScrollerProvider>(sequence);
            builder.AddAttribute(sequence + 1, nameof(ShadcnMessageScrollerProvider.ChildContent),
                (RenderFragment)(content => RenderDynamicComponent(content, 0, copy)));
            builder.CloseComponent();
            return;
        }

        if (PackageComponentType == typeof(ShadcnSidebar))
        {
            builder.OpenComponent<ShadcnSidebarProvider>(sequence);
            builder.AddAttribute(sequence + 1, nameof(ShadcnSidebarProvider.ChildContent),
                (RenderFragment)(content => RenderDynamicComponent(content, 0, copy)));
            builder.CloseComponent();
            return;
        }

        RenderDynamicComponent(builder, sequence, copy);
    }

    private void RenderDynamicComponent(RenderTreeBuilder builder, int sequence, ThemeScenarioCopy copy)
    {
        builder.OpenComponent<DynamicComponent>(sequence);
        builder.AddAttribute(sequence + 1, nameof(DynamicComponent.Type), PackageComponentType);
        builder.AddAttribute(sequence + 2, nameof(DynamicComponent.Parameters), CreateParameters(copy));
        builder.CloseComponent();
    }

    private IReadOnlyDictionary<string, object> CreateParameters(ThemeScenarioCopy copy)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);
        var properties = PackageComponentType.GetProperties();
        if (properties.Any(property => property.Name == "ChildContent" && property.PropertyType == typeof(RenderFragment)))
            result["ChildContent"] = (RenderFragment)(content => content.AddContent(0, copy.Title));
        if (properties.Any(property => property.Name == "AccessibleName" && property.PropertyType == typeof(string)))
            result["AccessibleName"] = copy.Title;
        if (properties.Any(property => property.Name == "Label" && property.PropertyType == typeof(string)))
            result["Label"] = copy.Title;

        switch (PackageComponentType.Name)
        {
            case "ShadcnAspectRatio":
                result["Ratio"] = 16d / 9d;
                break;
            case "ShadcnChart":
                result["Title"] = copy.Title;
                result["Config"] = new ShadcnChartConfig
                {
                    ["value"] = new(copy.Title) { Color = "currentColor" }
                };
                break;
            case "ShadcnDataTable`1":
                result["RowKey"] = (Func<string, string>)(value => value);
                result["Columns"] = new[]
                {
                    new ShadcnDataTableColumn<string>("value", copy.Title, value => value)
                };
                break;
            case "ShadcnQuestionnaire":
                result["Items"] = new[] { new ShadcnQuestionnaireItemDefinition("response", AllowsFreeform: true) };
                break;
            case "ShadcnTabs":
                result["Value"] = "overview";
                break;
        }

        return result;
    }

    public void Dispose() => IsDisposed = true;
}
