using Maliev.ShadcnBlazor.Components.Actions;
using Maliev.ShadcnBlazor.Components.Selection;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

internal static class ActionSelectionExamples
{
    public static IReadOnlyList<ComponentExampleDefinition> Create(string slug) => slug switch
    {
        "button" => [Button()],
        "button-group" => [ButtonGroup()],
        "checkbox" => [Checkbox()],
        "radio-group" => [RadioGroup()],
        "slider" => [Slider()],
        "switch" => [Switch()],
        "toggle" => [Toggle()],
        "toggle-group" => [ToggleGroup()],
        _ => []
    };

    private static ComponentExampleDefinition Button()
    {
        var disabled = false;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<ButtonDossierPreview>(0);
            builder.AddAttribute(1, nameof(ButtonDossierPreview.Disabled), disabled);
            builder.CloseComponent();
        };
        const string source = """
@using Maliev.ShadcnBlazor.Components.Actions

<section aria-label="Button variants and sizes">
    <p>Production order #4189 · Revision C · 3 files ready</p>

    <div>
        <ShadcnButton Variant="ShadcnButtonVariant.Default" Disabled="@disabled" OnClick="@(() => Announce(\"Save changes\"))">Save changes</ShadcnButton>
        <ShadcnButton Variant="ShadcnButtonVariant.Destructive" Disabled="@disabled" OnClick="@(() => Announce(\"Delete\"))">Delete</ShadcnButton>
        <ShadcnButton Variant="ShadcnButtonVariant.Outline" Disabled="@disabled" OnClick="@(() => Announce(\"Save changes\"))">Save changes</ShadcnButton>
        <ShadcnButton Variant="ShadcnButtonVariant.Secondary" Disabled="@disabled" OnClick="@(() => Announce(\"Save changes\"))">Save changes</ShadcnButton>
        <ShadcnButton Variant="ShadcnButtonVariant.Ghost" Disabled="@disabled" OnClick="@(() => Announce(\"More actions\"))">More actions</ShadcnButton>
        <ShadcnButton Variant="ShadcnButtonVariant.Link" Disabled="@disabled" OnClick="@(() => Announce(\"View details\"))">View details</ShadcnButton>
    </div>

    <div aria-label="Button sizes">
        <ShadcnButton Size="ShadcnButtonSize.ExtraSmall" Variant="ShadcnButtonVariant.Outline" Disabled="@disabled">Save changes</ShadcnButton>
        <ShadcnButton Size="ShadcnButtonSize.Small" Variant="ShadcnButtonVariant.Outline" Disabled="@disabled">Save changes</ShadcnButton>
        <ShadcnButton Size="ShadcnButtonSize.Default" Variant="ShadcnButtonVariant.Outline" Disabled="@disabled">Save changes</ShadcnButton>
        <ShadcnButton Size="ShadcnButtonSize.Large" Variant="ShadcnButtonVariant.Outline" Disabled="@disabled">Save changes</ShadcnButton>
    </div>

    <label><input type="checkbox" @bind="disabled" /> Disable actions</label>
    <p role="status" aria-live="polite">@(_lastAction ?? "Choose an enabled action to try it")</p>
</section>

@code {
    private bool disabled;
    private string? _lastAction;

    private void Announce(string action) => _lastAction = $"{action} pressed";
}
""";
        return Example("button", "Button variants and sizes", "Compare every supported treatment at once, scan the size scale, and try each action in a production-order context.",
            source, preview,
            [Toggle("button-disabled", "Disabled", value => disabled = value)],
            ["variants", "sizes", "disabled"]);
    }

    private static ComponentExampleDefinition ButtonGroup()
    {
        var orientation = ShadcnButtonGroupOrientation.Horizontal;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<ShadcnButtonGroup>(0);
            builder.AddAttribute(1, nameof(ShadcnButtonGroup.Orientation), orientation);
            builder.AddAttribute(2, nameof(ShadcnButtonGroup.AdditionalAttributes), Attributes("action-button-group", "Drawing actions"));
            builder.AddAttribute(3, nameof(ShadcnButtonGroup.ChildContent), (RenderFragment)(content =>
            {
                content.OpenComponent<ShadcnButtonGroupText>(0);
                content.AddAttribute(1, nameof(ShadcnButtonGroupText.ChildContent), Text("Status"));
                content.CloseComponent();
                AddButton(content, 10, "Archive", ShadcnButtonVariant.Outline);
                content.OpenComponent<ShadcnButtonGroupSeparator>(20);
                content.CloseComponent();
                content.OpenComponent<ShadcnButtonGroup>(30);
                content.AddAttribute(31, nameof(ShadcnButtonGroup.ChildContent), (RenderFragment)(nested =>
                {
                    AddButton(nested, 0, "Preview", ShadcnButtonVariant.Outline);
                    AddButton(nested, 10, "Report", ShadcnButtonVariant.Outline);
                }));
                content.CloseComponent();
            }));
            builder.CloseComponent();
        };
        return Example("button-group", "Grouped actions", "Compose related action buttons with logical horizontal or vertical geometry.",
            "<ShadcnButtonGroup aria-label=\"Drawing actions\">\n    <ShadcnButton Variant=\"ShadcnButtonVariant.Outline\">Archive</ShadcnButton>\n    <ShadcnButtonGroupSeparator />\n    <ShadcnButton Variant=\"ShadcnButtonVariant.Outline\">Report</ShadcnButton>\n</ShadcnButtonGroup>", preview,
            [Select("button-group-orientation", "Orientation", orientation, value => orientation = value)],
            ["horizontal", "vertical", "separator", "nested", "text"]);
    }

    private static ComponentExampleDefinition Checkbox()
    {
        bool? value = null;
        var disabled = false;
        var readOnly = false;
        var invalid = false;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<CheckboxDossierPreview>(0);
            builder.AddAttribute(1, nameof(CheckboxDossierPreview.Value), value);
            builder.AddAttribute(2, nameof(CheckboxDossierPreview.Disabled), disabled);
            builder.AddAttribute(3, nameof(CheckboxDossierPreview.ReadOnly), readOnly);
            builder.AddAttribute(4, nameof(CheckboxDossierPreview.Invalid), invalid);
            builder.CloseComponent();
        };
        return Example("checkbox", "Three-state checkbox", "Preview checked, unchecked, indeterminate, disabled, read-only, and invalid states.",
            "<ShadcnCheckbox Value=\"null\" Name=\"terms\" aria-label=\"Accept terms\" />", preview,
            [new("checkbox-state", "State", ComponentParameterControlKind.Select, "Indeterminate", ["Unchecked", "Checked", "Indeterminate"], state => value = state == "Indeterminate" ? null : state == "Checked"), Toggle("checkbox-disabled", "Disabled", v => disabled = v), Toggle("checkbox-readonly", "Read only", v => readOnly = v), Toggle("checkbox-invalid", "Invalid", v => invalid = v)],
            ["unchecked", "checked", "indeterminate", "disabled", "read-only", "invalid", "form"]);
    }

    private static ComponentExampleDefinition RadioGroup()
    {
        var selected = "comfortable";
        var orientation = ShadcnRadioGroupOrientation.Vertical;
        var disabled = false;
        var readOnly = false;
        var invalid = false;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<ShadcnRadioGroup<string>>(0);
            builder.AddAttribute(1, nameof(ShadcnRadioGroup<string>.Value), selected);
            builder.AddAttribute(2, nameof(ShadcnRadioGroup<string>.Orientation), orientation);
            builder.AddAttribute(3, nameof(ShadcnRadioGroup<string>.Disabled), disabled);
            builder.AddAttribute(4, nameof(ShadcnRadioGroup<string>.ReadOnly), readOnly);
            builder.AddAttribute(5, nameof(ShadcnRadioGroup<string>.AdditionalAttributes), Attributes("action-radio-group", "Density"));
            builder.AddAttribute(6, nameof(ShadcnRadioGroup<string>.Name), "density");
            builder.AddAttribute(7, nameof(ShadcnRadioGroup<string>.Invalid), invalid);
            builder.AddAttribute(8, nameof(ShadcnRadioGroup<string>.ChildContent), (RenderFragment)(content =>
            {
                AddRadio(content, 0, "default", "Default");
                AddRadio(content, 10, "comfortable", "Comfortable");
                AddRadio(content, 20, "compact", "Compact", true);
            }));
            builder.CloseComponent();
        };
        return Example("radio-group", "Roving radio choices", "Use native same-name radios with orientation-aware roving focus and typed values.",
            "<ShadcnRadioGroup TValue=\"string\" Value=\"comfortable\" Name=\"density\">\n    <ShadcnRadioGroupItem Value=\"default\">Default</ShadcnRadioGroupItem>\n    <ShadcnRadioGroupItem Value=\"comfortable\">Comfortable</ShadcnRadioGroupItem>\n</ShadcnRadioGroup>", preview,
            [Select("radio-orientation", "Orientation", orientation, value => orientation = value), Toggle("radio-disabled", "Disabled", v => disabled = v), Toggle("radio-readonly", "Read only", v => readOnly = v), Toggle("radio-invalid", "Invalid", v => invalid = v)],
            ["selected", "unselected", "disabled-item", "horizontal", "vertical", "roving-focus", "read-only", "invalid", "form"]);
    }

    private static ComponentExampleDefinition Slider()
    {
        IReadOnlyList<double> values = [20d, 80d];
        var orientation = ShadcnSliderOrientation.Horizontal;
        var disabled = false;
        var readOnly = false;
        var invalid = false;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<SliderDossierPreview>(0);
            builder.AddAttribute(1, nameof(SliderDossierPreview.Values), values);
            builder.AddAttribute(2, nameof(SliderDossierPreview.Orientation), orientation);
            builder.AddAttribute(3, nameof(SliderDossierPreview.Disabled), disabled);
            builder.AddAttribute(4, nameof(SliderDossierPreview.ReadOnly), readOnly);
            builder.AddAttribute(5, nameof(SliderDossierPreview.Invalid), invalid);
            builder.CloseComponent();
        };
        return Example("slider", "Single and range values", "A snapped range with native keyboard input, nearest-thumb pointer targeting, RTL, and vertical support.",
            "<ShadcnSlider Values=\"new[] { 20d, 80d }\" Minimum=\"0\" Maximum=\"100\" Step=\"5\" />", preview,
            [new("slider-values", "Values", ComponentParameterControlKind.Select, "Range", ["Single", "Range", "Multiple"], mode => values = mode switch { "Single" => [40d], "Multiple" => [20d, 50d, 80d], _ => [20d, 80d] }), Select("slider-orientation", "Orientation", orientation, value => orientation = value), Toggle("slider-disabled", "Disabled", v => disabled = v), Toggle("slider-readonly", "Read only", v => readOnly = v), Toggle("slider-invalid", "Invalid", v => invalid = v)],
            ["single", "range", "multiple", "horizontal", "vertical", "keyboard", "pointer", "disabled", "read-only", "invalid", "form"]);
    }

    private static ComponentExampleDefinition Switch()
    {
        var value = true;
        var size = ShadcnSwitchSize.Default;
        var disabled = false;
        var readOnly = false;
        var invalid = false;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<SwitchDossierPreview>(0);
            builder.AddAttribute(1, nameof(SwitchDossierPreview.Value), value);
            builder.AddAttribute(2, nameof(SwitchDossierPreview.Size), size);
            builder.AddAttribute(3, nameof(SwitchDossierPreview.Disabled), disabled);
            builder.AddAttribute(4, nameof(SwitchDossierPreview.ReadOnly), readOnly);
            builder.AddAttribute(5, nameof(SwitchDossierPreview.Invalid), invalid);
            builder.CloseComponent();
        };
        return Example("switch", "Boolean switch", "Preview checked state, both sizes, disabled, read-only, invalid, and RTL thumb motion.",
            "<ShadcnSwitch Value=\"true\" aria-label=\"Enable notifications\" />", preview,
            [Toggle("switch-value", "On", v => value = v, true), Select("switch-size", "Size", size, v => size = v), Toggle("switch-disabled", "Disabled", v => disabled = v), Toggle("switch-readonly", "Read only", v => readOnly = v), Toggle("switch-invalid", "Invalid", v => invalid = v)],
            ["checked", "unchecked", "default", "sm", "disabled", "read-only", "invalid", "form"]);
    }

    private static ComponentExampleDefinition Toggle()
    {
        var pressed = true;
        var variant = ShadcnToggleVariant.Outline;
        var size = ShadcnToggleSize.Default;
        var disabled = false;
        var invalid = false;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<ShadcnToggle>(0);
            builder.AddAttribute(1, nameof(ShadcnToggle.Pressed), pressed);
            builder.AddAttribute(2, nameof(ShadcnToggle.Variant), variant);
            builder.AddAttribute(3, nameof(ShadcnToggle.Size), size);
            builder.AddAttribute(4, nameof(ShadcnToggle.AdditionalAttributes), invalid
                ? new Dictionary<string, object> { ["data-testid"] = "action-toggle", ["aria-label"] = "Bold", ["aria-invalid"] = "true" }
                : Attributes("action-toggle", "Bold"));
            builder.AddAttribute(5, nameof(ShadcnToggle.ChildContent), Text("Bold"));
            builder.AddAttribute(6, nameof(ShadcnToggle.Disabled), disabled);
            builder.CloseComponent();
        };
        return Example("toggle", "Two-state action", "Control pressed state, outline treatment, and every supported size.",
            "<ShadcnToggle Pressed=\"true\" Variant=\"ShadcnToggleVariant.Outline\">Bold</ShadcnToggle>", preview,
            [Toggle("toggle-pressed", "Pressed", v => pressed = v, true), Select("toggle-variant", "Variant", variant, v => variant = v), Select("toggle-size", "Size", size, v => size = v), Toggle("toggle-disabled", "Disabled", v => disabled = v), Toggle("toggle-invalid", "Invalid", v => invalid = v)],
            ["on", "off", "default", "outline", "sm", "lg", "disabled", "invalid"]);
    }

    private static ComponentExampleDefinition ToggleGroup()
    {
        var orientation = ShadcnToggleGroupOrientation.Horizontal;
        var spacing = 2d;
        var multiple = true;
        var variant = ShadcnToggleVariant.Outline;
        var size = ShadcnToggleSize.Default;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<ShadcnToggleGroup<string>>(0);
            builder.AddAttribute(1, nameof(ShadcnToggleGroup<string>.Values), new[] { "bold" });
            builder.AddAttribute(2, nameof(ShadcnToggleGroup<string>.Multiple), multiple);
            builder.AddAttribute(3, nameof(ShadcnToggleGroup<string>.Orientation), orientation);
            builder.AddAttribute(4, nameof(ShadcnToggleGroup<string>.Spacing), spacing);
            builder.AddAttribute(5, nameof(ShadcnToggleGroup<string>.Variant), variant);
            builder.AddAttribute(6, nameof(ShadcnToggleGroup<string>.Size), size);
            builder.AddAttribute(7, nameof(ShadcnToggleGroup<string>.AdditionalAttributes), new Dictionary<string, object>
            {
                ["data-testid"] = "action-toggle-group",
                ["aria-label"] = "Text formatting",
                ["data-fixture-multiple"] = multiple.ToString().ToLowerInvariant()
            });
            builder.AddAttribute(8, nameof(ShadcnToggleGroup<string>.ChildContent), (RenderFragment)(content =>
            {
                AddToggleItem(content, 0, "bold", "Bold");
                AddToggleItem(content, 10, "italic", "Italic");
                AddToggleItem(content, 20, "underline", "Underline", true);
            }));
            builder.CloseComponent();
        };
        return Example("toggle-group", "Roving toggle choices", "Multiple selection with inherited presentation, spacing, disabled items, orientation, and RTL arrows.",
            "<ShadcnToggleGroup TValue=\"string\" Values=\"new[] { \"bold\" }\" Multiple=\"true\" Variant=\"ShadcnToggleVariant.Outline\">\n    <ShadcnToggleGroupItem Value=\"bold\">Bold</ShadcnToggleGroupItem>\n    <ShadcnToggleGroupItem Value=\"italic\">Italic</ShadcnToggleGroupItem>\n</ShadcnToggleGroup>", preview,
            [Toggle("toggle-group-multiple", "Multiple", value => multiple = value, true), Select("toggle-group-orientation", "Orientation", orientation, value => orientation = value), new("toggle-group-spacing", "Spacing", ComponentParameterControlKind.Select, "2", ["0", "1", "2", "4"], value => spacing = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture)), Select("toggle-group-variant", "Variant", variant, value => variant = value), Select("toggle-group-size", "Size", size, value => size = value)],
            ["single", "multiple", "spacing", "connected", "horizontal", "vertical", "roving-focus", "disabled-item", "outline", "sizes"]);
    }

    private static ComponentParameterControl Select<TEnum>(string id, string label, TEnum value, Action<TEnum> apply) where TEnum : struct, Enum =>
        new(id, label, ComponentParameterControlKind.Select, value.ToString(), Enum.GetNames<TEnum>(), text => apply(Enum.Parse<TEnum>(text)));

    private static ComponentParameterControl Toggle(string id, string label, Action<bool> apply, bool initial = false) =>
        new(id, label, ComponentParameterControlKind.Toggle, initial.ToString(), [], text => apply(bool.Parse(text)));

    private static ComponentExampleDefinition Example(string id, string title, string description, string source, RenderFragment preview, IReadOnlyList<ComponentParameterControl> controls, IReadOnlyList<string> tags) =>
        new($"{id}-primary", title, description, source, preview, controls, tags);

    private static IReadOnlyDictionary<string, object> Attributes(string testId, string ariaLabel) =>
        new Dictionary<string, object> { ["data-testid"] = testId, ["aria-label"] = ariaLabel };

    private static RenderFragment Text(string text) => builder => builder.AddContent(0, text);

    private static void AddButton(RenderTreeBuilder builder, int sequence, string text, ShadcnButtonVariant variant)
    {
        builder.OpenComponent<ShadcnButton>(sequence);
        builder.AddAttribute(sequence + 1, nameof(ShadcnButton.Variant), variant);
        builder.AddAttribute(sequence + 2, nameof(ShadcnButton.ChildContent), Text(text));
        builder.CloseComponent();
    }

    private static void AddRadio(RenderTreeBuilder builder, int sequence, string value, string text, bool disabled = false)
    {
        builder.OpenComponent<ShadcnRadioGroupItem<string>>(sequence);
        builder.AddAttribute(sequence + 1, nameof(ShadcnRadioGroupItem<string>.Value), value);
        builder.AddAttribute(sequence + 2, nameof(ShadcnRadioGroupItem<string>.Disabled), disabled);
        builder.AddAttribute(sequence + 3, nameof(ShadcnRadioGroupItem<string>.ChildContent), Text(text));
        builder.CloseComponent();
    }

    private static void AddToggleItem(RenderTreeBuilder builder, int sequence, string value, string text, bool disabled = false)
    {
        builder.OpenComponent<ShadcnToggleGroupItem<string>>(sequence);
        builder.AddAttribute(sequence + 1, nameof(ShadcnToggleGroupItem<string>.Value), value);
        builder.AddAttribute(sequence + 2, nameof(ShadcnToggleGroupItem<string>.Disabled), disabled);
        builder.AddAttribute(sequence + 3, nameof(ShadcnToggleGroupItem<string>.ChildContent), Text(text));
        builder.CloseComponent();
    }
}
