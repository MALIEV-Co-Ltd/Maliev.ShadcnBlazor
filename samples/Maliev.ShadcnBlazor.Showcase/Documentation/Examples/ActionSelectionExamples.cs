using System.Globalization;
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
        string Source()
        {
            var disabledValue = disabled.ToString().ToLowerInvariant();
            return $$"""
@using Maliev.ShadcnBlazor.Components.Actions

<section aria-label="Button variants and sizes">
    <header>
        <h3>Production order #4189</h3>
        <p>Revision C · 3 files ready</p>
    </header>

    <section aria-labelledby="variants-title">
        <h4 id="variants-title">Variants</h4>
        <div dir="auto">
            <ShadcnButton Variant="ShadcnButtonVariant.Default" Disabled="{{disabledValue}}" PointerCursor="true" OnClick="@(() => Announce("Save changes"))">Save changes</ShadcnButton>
            <ShadcnButton Variant="ShadcnButtonVariant.Destructive" Disabled="{{disabledValue}}" PointerCursor="true" OnClick="@(() => Announce("Delete"))">Delete</ShadcnButton>
            <ShadcnButton Variant="ShadcnButtonVariant.Outline" Disabled="{{disabledValue}}" PointerCursor="true" OnClick="@(() => Announce("Save changes"))">Save changes</ShadcnButton>
            <ShadcnButton Variant="ShadcnButtonVariant.Secondary" Disabled="{{disabledValue}}" PointerCursor="true" OnClick="@(() => Announce("Save changes"))">Save changes</ShadcnButton>
            <ShadcnButton Variant="ShadcnButtonVariant.Ghost" Disabled="{{disabledValue}}" PointerCursor="true" OnClick="@(() => Announce("More actions"))">More actions</ShadcnButton>
            <ShadcnButton Variant="ShadcnButtonVariant.Link" Href="#usage" Disabled="{{disabledValue}}" PointerCursor="true" OnClick="@(() => Announce("View details"))">View details</ShadcnButton>
        </div>
    </section>

    <section aria-labelledby="sizes-title">
        <h4 id="sizes-title">Sizes</h4>
        <div>
            <ShadcnButton Size="ShadcnButtonSize.ExtraSmall" Variant="ShadcnButtonVariant.Outline" Disabled="{{disabledValue}}" PointerCursor="true">Save changes</ShadcnButton>
            <ShadcnButton Size="ShadcnButtonSize.Small" Variant="ShadcnButtonVariant.Outline" Disabled="{{disabledValue}}" PointerCursor="true">Save changes</ShadcnButton>
            <ShadcnButton Size="ShadcnButtonSize.Default" Variant="ShadcnButtonVariant.Outline" Disabled="{{disabledValue}}" PointerCursor="true">Save changes</ShadcnButton>
            <ShadcnButton Size="ShadcnButtonSize.Large" Variant="ShadcnButtonVariant.Outline" Disabled="{{disabledValue}}" PointerCursor="true">Save changes</ShadcnButton>
        </div>
    </section>

    <section aria-labelledby="icon-sizes-title">
        <h4 id="icon-sizes-title">Icon sizes</h4>
        <div>
            <ShadcnButton Size="ShadcnButtonSize.IconExtraSmall" Variant="ShadcnButtonVariant.Outline" Disabled="{{disabledValue}}" PointerCursor="true" aria-label="Save drawing (Icon extra small)">
                <LeadingIcon>
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                        <path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2Z" />
                        <path d="M17 21v-8H7v8" />
                        <path d="M7 3v5h8" />
                    </svg>
                </LeadingIcon>
            </ShadcnButton>
            <ShadcnButton Size="ShadcnButtonSize.IconSmall" Variant="ShadcnButtonVariant.Outline" Disabled="{{disabledValue}}" PointerCursor="true" aria-label="Save drawing (Icon small)">
                <LeadingIcon>
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                        <path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2Z" />
                        <path d="M17 21v-8H7v8" />
                        <path d="M7 3v5h8" />
                    </svg>
                </LeadingIcon>
            </ShadcnButton>
            <ShadcnButton Size="ShadcnButtonSize.Icon" Variant="ShadcnButtonVariant.Outline" Disabled="{{disabledValue}}" PointerCursor="true" aria-label="Save drawing (Icon default)">
                <LeadingIcon>
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                        <path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2Z" />
                        <path d="M17 21v-8H7v8" />
                        <path d="M7 3v5h8" />
                    </svg>
                </LeadingIcon>
            </ShadcnButton>
            <ShadcnButton Size="ShadcnButtonSize.IconLarge" Variant="ShadcnButtonVariant.Outline" Disabled="{{disabledValue}}" PointerCursor="true" aria-label="Save drawing (Icon large)">
                <LeadingIcon>
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                        <path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2Z" />
                        <path d="M17 21v-8H7v8" />
                        <path d="M7 3v5h8" />
                    </svg>
                </LeadingIcon>
            </ShadcnButton>
        </div>
    </section>

    <p role="status" aria-live="polite">@(_lastAction ?? "Choose an enabled action to try it")</p>
</section>

@code {
    private string? _lastAction;

    private void Announce(string action) => _lastAction = $"{action} pressed";
}
""";
        }
        return Example("button", "Button variants and sizes", "Compare every supported treatment at once, scan the size scale, and try each action in a production-order context.",
            Source(), preview,
            [Toggle("button-disabled", "Disabled", value => disabled = value)],
            ["variants", "sizes", "icons", "link", "disabled"]) with
        { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition ButtonGroup()
    {
        var orientation = ShadcnButtonGroupOrientation.Horizontal;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<ButtonGroupDossierPreview>(0);
            builder.AddAttribute(1, nameof(ButtonGroupDossierPreview.Orientation), orientation);
            builder.CloseComponent();
        };
        string Source()
        {
            var separatorOrientation = orientation == ShadcnButtonGroupOrientation.Horizontal
                ? ShadcnButtonGroupOrientation.Vertical
                : ShadcnButtonGroupOrientation.Horizontal;
            return $$"""
@using Maliev.ShadcnBlazor.Components.Actions

<ShadcnButtonGroup Orientation="ShadcnButtonGroupOrientation.{{orientation}}" aria-label="Production review actions">
    <ShadcnButtonGroupText>Revision C</ShadcnButtonGroupText>
    <ShadcnButton Variant="ShadcnButtonVariant.Outline" OnClick="Archive">Archive</ShadcnButton>
    <ShadcnButtonGroupSeparator Orientation="ShadcnButtonGroupOrientation.{{separatorOrientation}}" />
    <ShadcnButton Variant="ShadcnButtonVariant.Outline" OnClick="Preview">Preview</ShadcnButton>
    <ShadcnButton Variant="ShadcnButtonVariant.Outline" OnClick="Report">Report</ShadcnButton>
</ShadcnButtonGroup>

<p role="status" aria-live="polite">@lastAction</p>

@code {
    private string lastAction = "Choose an action to continue the production review";
    private void Archive() => lastAction = "Quotation archived";
    private void Preview() => lastAction = "Opening production preview";
    private void Report() => lastAction = "Production report requested";
}
""";
        }
        return Example("button-group", "Connected production actions", "Try a realistic review toolbar with pinned shadcn/ui geometry, logical orientation, and an announced result for every action.",
            Source(), preview,
            [Select("button-group-orientation", "Orientation", orientation, value => orientation = value)],
            ["horizontal", "vertical", "separator", "nested", "text"]) with
        { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition Checkbox()
    {
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<CheckboxDossierPreview>(0);
            builder.CloseComponent();
        };
        const string source = """
@using Maliev.ShadcnBlazor.Components.Selection

<section class="showcase-checkbox-dossier" aria-labelledby="checkbox-dossier-title">
    <header class="showcase-checkbox-dossier__header">
        <div>
            <h3 id="checkbox-dossier-title">Notification preferences</h3>
            <p>Choose how the production team can contact you.</p>
        </div>
        <span dir="auto">Workspace settings</span>
    </header>

    <div class="showcase-checkbox-dossier__list">
        <label class="showcase-checkbox-option">
            <ShadcnCheckbox @bind-Value="AcceptTerms" Name="terms" />
            <span dir="auto"><strong>Accept terms and conditions</strong><small>Required before production files can be released.</small></span>
        </label>
        <label class="showcase-checkbox-option">
            <ShadcnCheckbox @bind-Value="ProductionUpdates" Name="production-updates" />
            <span dir="auto"><strong>Production updates</strong><small>Receive status changes for active manufacturing orders.</small></span>
        </label>
        <label class="showcase-checkbox-option">
            <ShadcnCheckbox @bind-Value="InspectionRecipients" Name="inspection-recipients" />
            <span dir="auto"><strong>Inspection recipients</strong><small>Some quality-team recipients are selected.</small></span>
        </label>
        <label class="showcase-checkbox-option showcase-checkbox-option--invalid">
            <ShadcnCheckbox @bind-Value="QualityApproval" Name="quality-approval" Invalid="true" aria-describedby="checkbox-quality-error" />
            <span dir="auto"><strong>Quality approval</strong><small id="checkbox-quality-error">Select this before submitting the inspection report.</small></span>
        </label>
        <label class="showcase-checkbox-option">
            <ShadcnCheckbox Value="true" Name="archived-reports" ReadOnly="true" />
            <span dir="auto"><strong>Archive completed reports</strong><small>This workspace policy is read-only.</small></span>
        </label>
        <label class="showcase-checkbox-option">
            <ShadcnCheckbox Value="false" Name="legacy-alerts" Disabled="true" />
            <span dir="auto"><strong>Legacy machine alerts</strong><small>Unavailable for this workspace.</small></span>
        </label>
    </div>
</section>

@code {
    private bool? AcceptTerms { get; set; } = false;
    private bool? ProductionUpdates { get; set; } = true;
    private bool? InspectionRecipients { get; set; }
    private bool? QualityApproval { get; set; } = false;
}
""";
        return Example("checkbox", "Notification preferences", "Test checked, unchecked, mixed, invalid, read-only, and disabled states in a realistic settings group.",
            source, preview, [],
            ["unchecked", "checked", "indeterminate", "disabled", "read-only", "invalid", "form"]);
    }

    private static ComponentExampleDefinition RadioGroup()
    {
        var selected = "priority";
        var orientation = ShadcnRadioGroupOrientation.Vertical;
        var disabled = false;
        var readOnly = false;
        var invalid = false;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<RadioGroupDossierPreview>(0);
            builder.AddAttribute(1, nameof(RadioGroupDossierPreview.Value), selected);
            builder.AddAttribute(2, nameof(RadioGroupDossierPreview.Orientation), orientation);
            builder.AddAttribute(3, nameof(RadioGroupDossierPreview.Disabled), disabled);
            builder.AddAttribute(4, nameof(RadioGroupDossierPreview.ReadOnly), readOnly);
            builder.AddAttribute(5, nameof(RadioGroupDossierPreview.Invalid), invalid);
            builder.CloseComponent();
        };
        string Source() => $$"""
@using Maliev.ShadcnBlazor.Components.Selection

<section aria-labelledby="review-speed-title">
    <h3 id="review-speed-title">Inspection turnaround</h3>
    <p>Choose how quickly the production drawing should be reviewed.</p>

    <ShadcnRadioGroup TValue="string" @bind-Value="ReviewSpeed" Orientation="ShadcnRadioGroupOrientation.{{orientation}}" Presentation="ShadcnRadioGroupPresentation.Card" Disabled="{{disabled.ToString().ToLowerInvariant()}}" ReadOnly="{{readOnly.ToString().ToLowerInvariant()}}" Invalid="{{invalid.ToString().ToLowerInvariant()}}" Name="review-speed" aria-label="Inspection turnaround">
        <ShadcnRadioGroupItem Value="standard">Standard review · Within 2 business days</ShadcnRadioGroupItem>
        <ShadcnRadioGroupItem Value="priority">Priority review · By the next business day</ShadcnRadioGroupItem>
        <ShadcnRadioGroupItem Value="same-day" Disabled="true">Same-day review · Unavailable after 2:00 PM</ShadcnRadioGroupItem>
    </ShadcnRadioGroup>

    <p role="status" aria-live="polite">Selected: @ReviewSpeed</p>
</section>

@code {
    private string ReviewSpeed { get; set; } = "{{selected}}";
}
""";
        return Example("radio-group", "Inspection turnaround", "Choose a production-drawing review speed with native pointer input and orientation-aware keyboard navigation.",
            Source(), preview,
            [
                Select("radio-orientation", "Orientation", orientation, value => orientation = value),
                Toggle("radio-disabled", "Disabled", v => disabled = v),
                Toggle("radio-readonly", "Read only", v => readOnly = v),
                Toggle("radio-invalid", "Invalid", v => invalid = v)
            ],
            ["selected", "unselected", "disabled-item", "horizontal", "vertical", "roving-focus", "read-only", "invalid", "form"]) with
        { RazorSourceProvider = Source };
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
            builder.AddAttribute(6, nameof(SliderDossierPreview.ValuesChanged), EventCallback.Factory.Create<IReadOnlyList<double>>(new object(), next => values = next));
            builder.CloseComponent();
        };
        string Source() => $$"""
@using Maliev.ShadcnBlazor.Components.Selection

<ShadcnSlider @bind-Values="SliderValues"
              Minimum="0"
              Maximum="100"
              Step="5"
              Orientation="ShadcnSliderOrientation.{{orientation}}"
              Disabled="{{disabled.ToString().ToLowerInvariant()}}"
              ReadOnly="{{readOnly.ToString().ToLowerInvariant()}}"
              Invalid="{{invalid.ToString().ToLowerInvariant()}}"
              Name="budget"
              Form="slider-form"
              Required="true"
              aria-label="Budget range" />

<form id="slider-form"></form>
<output aria-live="polite">@string.Join(" – ", SliderValues)</output>

@code {
    private IReadOnlyList<double> SliderValues { get; set; } = [{{string.Join(", ", values.Select(value => $"{value.ToString(CultureInfo.InvariantCulture)}d"))}}];
}
""";
        return Example("slider", "Single and range values", "A snapped range with native keyboard input, nearest-thumb pointer targeting, RTL, and vertical support.",
            Source(), preview,
            [
                new("slider-values", "Values", ComponentParameterControlKind.Select, "Range", ["Single", "Range", "Multiple"], mode => values = mode switch { "Single" => [40d], "Multiple" => [20d, 50d, 80d], _ => [20d, 80d] }),
                Select("slider-orientation", "Orientation", orientation, value => orientation = value),
                Toggle("slider-disabled", "Disabled", v => disabled = v),
                Toggle("slider-readonly", "Read only", v => readOnly = v),
                Toggle("slider-invalid", "Invalid", v => invalid = v)
            ],
            ["single", "range", "multiple", "horizontal", "vertical", "keyboard", "pointer", "disabled", "read-only", "invalid", "form"]) with
        { RazorSourceProvider = Source };
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
            builder.AddAttribute(2, nameof(SwitchDossierPreview.ValueChanged), EventCallback.Factory.Create<bool>(new object(), next => value = next));
            builder.AddAttribute(3, nameof(SwitchDossierPreview.Size), size);
            builder.AddAttribute(4, nameof(SwitchDossierPreview.Disabled), disabled);
            builder.AddAttribute(5, nameof(SwitchDossierPreview.ReadOnly), readOnly);
            builder.AddAttribute(6, nameof(SwitchDossierPreview.Invalid), invalid);
            builder.CloseComponent();
        };
        string Source() => $$"""
@using Maliev.ShadcnBlazor.Components.Selection

<section aria-labelledby="notification-preferences-title">
    <header>
        <div dir="auto">
            <h3 id="notification-preferences-title">Notification preferences</h3>
            <p>Choose which production updates reach your team.</p>
        </div>
        <span dir="auto">Workspace</span>
    </header>

    <div>
        <label for="production-updates-switch" dir="auto">
            <strong>Production updates</strong>
            <span>Receive alerts when drawings are approved or a job needs attention.</span>
        </label>
        <ShadcnSwitch id="production-updates-switch"
                      @bind-Value="ProductionUpdates"
                      Size="ShadcnSwitchSize.{{size}}"
                      Disabled="{{disabled.ToString().ToLowerInvariant()}}"
                      ReadOnly="{{readOnly.ToString().ToLowerInvariant()}}"
                      Invalid="{{invalid.ToString().ToLowerInvariant()}}"
                      Name="production-updates"
                      aria-label="Production updates" />
    </div>

    <p dir="auto" role="status" aria-live="polite">
        @(ProductionUpdates ? "Production updates are enabled." : "Production updates are paused.")
    </p>
</section>

@code {
    private bool ProductionUpdates = {{value.ToString().ToLowerInvariant()}};
}
""";
        return Example("switch", "Notification preference", "Try a real workspace notification setting while comparing size, availability, validation, and RTL thumb motion.",
            Source(), preview,
            [
                Toggle("switch-value", "On", v => value = v, true),
                Select("switch-size", "Size", size, v => size = v),
                Toggle("switch-disabled", "Disabled", v => disabled = v),
                Toggle("switch-readonly", "Read only", v => readOnly = v),
                Toggle("switch-invalid", "Invalid", v => invalid = v)
            ],
            ["checked", "unchecked", "default", "sm", "disabled", "read-only", "invalid", "form"]) with
        { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition Toggle()
    {
        var variant = ShadcnToggleVariant.Outline;
        var size = ShadcnToggleSize.Default;
        var disabled = false;
        var invalid = false;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<ToggleDossierPreview>(0);
            builder.AddAttribute(1, nameof(ToggleDossierPreview.Pressed), true);
            builder.AddAttribute(2, nameof(ToggleDossierPreview.Variant), variant);
            builder.AddAttribute(3, nameof(ToggleDossierPreview.Size), size);
            builder.AddAttribute(4, nameof(ToggleDossierPreview.Invalid), invalid);
            builder.AddAttribute(5, nameof(ToggleDossierPreview.Disabled), disabled);
            builder.CloseComponent();
        };
        string Source() => $$"""
            <section class="inspection-note" aria-label="Inspection note editor">
                <header>
                    <div>
                        <strong dir="auto">Inspection note</strong>
                        <span dir="auto">Revision C · autosaved</span>
                    </div>
                    <span dir="auto" aria-live="polite">@(Bold ? "Bold enabled" : "Bold disabled")</span>
                </header>

                <div role="toolbar" aria-label="Text formatting">
                    <ShadcnToggle @bind-Pressed="Bold"
                                  Variant="ShadcnToggleVariant.{{variant}}"
                                  Size="ShadcnToggleSize.{{size}}"
                                  Disabled="{{disabled.ToString().ToLowerInvariant()}}"
                                  PointerCursor="true"
                                  aria-invalid="{{invalid.ToString().ToLowerInvariant()}}"
                                  aria-label="Toggle bold emphasis">
                        <LeadingIcon>
                            <svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <path d="M6 4h8a4 4 0 0 1 0 8H6z" />
                                <path d="M6 12h9a4 4 0 0 1 0 8H6z" />
                            </svg>
                        </LeadingIcon>
                        <ChildContent>Bold</ChildContent>
                    </ShadcnToggle>
                </div>

                <p dir="auto" data-emphasis="@(Bold ? "bold" : "regular")">
                    Confirm the enclosure edge is deburred before final inspection.
                </p>
            </section>

            @code {
                private bool Bold { get; set; } = true;
            }
            """;
        return Example("toggle", "Inspection note emphasis", "Use the button itself to test pressed state, then compare every supported visual and validation state.",
            Source(), preview,
            [
                Select("toggle-variant", "Variant", variant, v => variant = v),
                Select("toggle-size", "Size", size, v => size = v),
                Toggle("toggle-disabled", "Disabled", v => disabled = v),
                Toggle("toggle-invalid", "Invalid", v => invalid = v)
            ],
            ["on", "off", "default", "outline", "sm", "lg", "disabled", "invalid"]) with
        { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition ToggleGroup()
    {
        var orientation = ShadcnToggleGroupOrientation.Horizontal;
        var spacing = 0d;
        var multiple = true;
        var variant = ShadcnToggleVariant.Outline;
        var size = ShadcnToggleSize.Default;
        var disabled = false;
        var invalid = false;
        IReadOnlyCollection<string> values = ["dimensions"];
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<ToggleGroupDossierPreview>(0);
            builder.AddAttribute(1, nameof(ToggleGroupDossierPreview.Values), values);
            builder.AddAttribute(2, nameof(ToggleGroupDossierPreview.ValuesChanged), EventCallback.Factory.Create<IReadOnlyCollection<string>>(new object(), next => values = next));
            builder.AddAttribute(3, nameof(ToggleGroupDossierPreview.Multiple), multiple);
            builder.AddAttribute(4, nameof(ToggleGroupDossierPreview.Orientation), orientation);
            builder.AddAttribute(5, nameof(ToggleGroupDossierPreview.Spacing), spacing);
            builder.AddAttribute(6, nameof(ToggleGroupDossierPreview.Variant), variant);
            builder.AddAttribute(7, nameof(ToggleGroupDossierPreview.Size), size);
            builder.AddAttribute(8, nameof(ToggleGroupDossierPreview.Disabled), disabled);
            builder.AddAttribute(9, nameof(ToggleGroupDossierPreview.Invalid), invalid);
            builder.CloseComponent();
        };
        string Source()
        {
            var selected = string.Join(", ", values.Select(value => $"\"{value}\""));
            var invalidAttributes = invalid ? " aria-invalid=\"true\" aria-describedby=\"overlay-error\"" : string.Empty;
            return $$"""
@using Maliev.ShadcnBlazor.Components.Actions

<section class="drawing-review" aria-labelledby="drawing-review-title">
    <header>
        <h3 id="drawing-review-title">Drawing review layers</h3>
        <p>Choose which annotations are visible while checking revision C.</p>
    </header>

    <ShadcnToggleGroup TValue="string"
                       @bind-Values="VisibleLayers"
                       Multiple="{{multiple.ToString().ToLowerInvariant()}}"
                       Orientation="ShadcnToggleGroupOrientation.{{orientation}}"
                       Spacing="{{spacing.ToString(CultureInfo.InvariantCulture)}}"
                       Variant="ShadcnToggleVariant.{{variant}}"
                       Size="ShadcnToggleSize.{{size}}"
                       Disabled="{{disabled.ToString().ToLowerInvariant()}}"
                       aria-label="Drawing review layers"{{invalidAttributes}}>
        <ShadcnToggleGroupItem Value="dimensions">Dimensions</ShadcnToggleGroupItem>
        <ShadcnToggleGroupItem Value="notes">Notes</ShadcnToggleGroupItem>
        <ShadcnToggleGroupItem Value="inspection" Disabled="true">Inspection</ShadcnToggleGroupItem>
    </ShadcnToggleGroup>

    <article aria-label="Final inspection note">
        <h4>Final inspection note</h4>
        <p>WI-2418 · CNC enclosure · Revision C</p>
        @if (VisibleLayers.Contains("dimensions"))
        {
            <p><strong>Overall dimensions:</strong> 126 × 84 × 32 mm · ±0.10 mm</p>
        }
        @if (VisibleLayers.Contains("notes"))
        {
            <p><strong>Machining note N4:</strong> Deburr all edges 0.2–0.5 mm before anodizing.</p>
        }
        <p><strong>Visible:</strong> @string.Join(", ", VisibleLayers)</p>
    </article>
    {{(invalid ? "<p id=\"overlay-error\">Select at least one editable review layer.</p>" : string.Empty)}}
</section>

@code {
    private IReadOnlyCollection<string> VisibleLayers { get; set; } = [{{selected}}];
}
""";
        }

        return Example("toggle-group", "Drawing review layers", "Switch a production drawing between single- and multi-layer review with connected, orientation-aware controls.",
            Source(), preview,
            [
                Toggle("toggle-group-multiple", "Multiple", value => { multiple = value; if (!value && values.Count > 1) values = [values.First()]; }, true),
                Select("toggle-group-orientation", "Orientation", orientation, value => orientation = value),
                new("toggle-group-spacing", "Spacing", ComponentParameterControlKind.Select, "0", ["0", "1", "2", "4"], value => spacing = double.Parse(value, CultureInfo.InvariantCulture)),
                Select("toggle-group-variant", "Variant", variant, value => variant = value),
                Select("toggle-group-size", "Size", size, value => size = value),
                Toggle("toggle-group-disabled", "Disabled", value => disabled = value),
                Toggle("toggle-group-invalid", "Invalid", value => invalid = value)
            ],
            ["single", "multiple", "spacing", "connected", "horizontal", "vertical", "roving-focus", "disabled-item", "outline", "sizes", "disabled", "invalid"]) with
        { RazorSourceProvider = Source };
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

    private static void AddToggleItem(RenderTreeBuilder builder, int sequence, string value, string text, bool disabled = false)
    {
        builder.OpenComponent<ShadcnToggleGroupItem<string>>(sequence);
        builder.AddAttribute(sequence + 1, nameof(ShadcnToggleGroupItem<string>.Value), value);
        builder.AddAttribute(sequence + 2, nameof(ShadcnToggleGroupItem<string>.Disabled), disabled);
        builder.AddAttribute(sequence + 3, nameof(ShadcnToggleGroupItem<string>.ChildContent), Text(text));
        builder.CloseComponent();
    }
}
