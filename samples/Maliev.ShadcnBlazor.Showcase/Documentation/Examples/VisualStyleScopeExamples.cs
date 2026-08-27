using Maliev.ShadcnBlazor.Components.Styling;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

internal static class VisualStyleScopeExamples
{
    public static IReadOnlyList<ComponentExampleDefinition> Create() =>
    [
        Approval(),
        Scheduling(),
        Operations()
    ];

    private static ComponentExampleDefinition Approval()
    {
        var style = ShadcnVisualStyle.Minimal;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<VisualStyleScopeDossierPreview>(0);
            builder.AddAttribute(1, nameof(VisualStyleScopeDossierPreview.Mode), "approval");
            builder.AddAttribute(2, nameof(VisualStyleScopeDossierPreview.VisualStyle), style);
            builder.CloseComponent();
        };
        var control = Select("approval-style", "Visual treatment", "Clean", ["Clean", "Bold frame"], value =>
            style = value == "Bold frame" ? ShadcnVisualStyle.NeoBrutalist : ShadcnVisualStyle.Minimal);
        string Source() => $$"""
<ShadcnVisualStyleScope VisualStyle="ShadcnVisualStyle.{{style}}"
                        Depth="ShadcnDepthTreatment.{{(style == ShadcnVisualStyle.NeoBrutalist ? "Raised" : "Flat")}}">
    <ShadcnCard>
        <ShadcnCardHeader><ShadcnCardTitle>Approve revision C</ShadcnCardTitle></ShadcnCardHeader>
        <ShadcnCardContent>WO-2418 · CNC enclosure · 24.982 mm verified</ShadcnCardContent>
        <ShadcnCardFooter><ShadcnButton>Approve release</ShadcnButton></ShadcnCardFooter>
    </ShadcnCard>
</ShadcnVisualStyleScope>
""";
        return Definition("visual-style-approval", "Production approval treatment", "Compare a quiet approval surface with a deliberately bold inspection treatment.", Source(), preview, [control], ["minimal", "neo-brutalist", "approval"]) with { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition Scheduling()
    {
        var style = ShadcnVisualStyle.Glass;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<VisualStyleScopeDossierPreview>(0);
            builder.AddAttribute(1, nameof(VisualStyleScopeDossierPreview.Mode), "scheduling");
            builder.AddAttribute(2, nameof(VisualStyleScopeDossierPreview.VisualStyle), style);
            builder.CloseComponent();
        };
        var control = Select("scheduling-style", "Glass treatment", "Frosted", ["Frosted", "Spatial glass"], value =>
            style = value == "Spatial glass" ? ShadcnVisualStyle.LiquidGlass : ShadcnVisualStyle.Glass);
        string Source() => $$"""
<ShadcnVisualStyleScope VisualStyle="ShadcnVisualStyle.{{style}}"
                        Depth="ShadcnDepthTreatment.{{(style == ShadcnVisualStyle.LiquidGlass ? "Spatial" : "Floating")}}"
                        Motion="ShadcnMotionTreatment.Calm">
    <ShadcnBentoGrid Columns="2" MediumColumns="2">
        <ShadcnBentoItem><ShadcnInput Value="Friday, 16:00" /></ShadcnBentoItem>
        <ShadcnBentoItem><ShadcnChart Title="Cell utilization" Config="@config" Categories="@days" Series="@series" /></ShadcnBentoItem>
    </ShadcnBentoGrid>
</ShadcnVisualStyleScope>
""";
        return Definition("visual-style-scheduling", "Scheduling and utilization", "Compare frosted and spatial glass around a real dispatch schedule and utilization chart.", Source(), preview, [control], ["glass", "liquid-glass", "bento", "chart"]) with { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition Operations()
    {
        var intensity = ShadcnStyleIntensity.Default;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<VisualStyleScopeDossierPreview>(0);
            builder.AddAttribute(1, nameof(VisualStyleScopeDossierPreview.Mode), "operations");
            builder.AddAttribute(2, nameof(VisualStyleScopeDossierPreview.VisualStyle), ShadcnVisualStyle.Minimal);
            builder.AddAttribute(3, nameof(VisualStyleScopeDossierPreview.ColorTreatment), ShadcnColorTreatment.VibrantDark);
            builder.AddAttribute(4, nameof(VisualStyleScopeDossierPreview.Intensity), intensity);
            builder.CloseComponent();
        };
        var control = Select("operations-intensity", "Accent strength", "Standard", ["Subtle", "Standard", "Strong"], value =>
            intensity = value switch { "Subtle" => ShadcnStyleIntensity.Subtle, "Strong" => ShadcnStyleIntensity.Strong, _ => ShadcnStyleIntensity.Default });
        string Source() => $$"""
<ShadcnVisualStyleScope VisualStyle="ShadcnVisualStyle.Minimal"
                        ColorTreatment="ShadcnColorTreatment.VibrantDark"
                        Depth="ShadcnDepthTreatment.Raised"
                        Motion="ShadcnMotionTreatment.Expressive"
                        Intensity="ShadcnStyleIntensity.{{intensity}}">
    <ShadcnInput Value="DMG MORI DMU 50" />
    <ShadcnDialog>
        <ShadcnDialogTrigger>Review cell status</ShadcnDialogTrigger>
        <ShadcnDialogContent><ShadcnDialogTitle>Cell status</ShadcnDialogTitle></ShadcnDialogContent>
    </ShadcnDialog>
</ShadcnVisualStyleScope>
""";
        return Definition("visual-style-operations", "Vibrant night operations", "Inspect focus, disabled controls, status feedback, and an overlay without changing the surrounding application theme.", Source(), preview, [control], ["vibrant-dark", "overlay", "focus", "reduced-motion"]) with { RazorSourceProvider = Source };
    }

    private static ComponentParameterControl Select(string id, string label, string value, IReadOnlyList<string> options, Action<string> apply) =>
        new(id, label, ComponentParameterControlKind.Select, value, options, apply);

    private static ComponentExampleDefinition Definition(
        string id,
        string title,
        string description,
        string source,
        RenderFragment preview,
        IReadOnlyList<ComponentParameterControl> controls,
        IReadOnlyList<string> tags) => new(id, title, description, source, preview, controls, tags);
}
