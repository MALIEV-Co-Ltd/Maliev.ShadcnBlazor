using Bunit;
using Maliev.ShadcnBlazor.Components.Actions;
using Maliev.ShadcnBlazor.Components.Selection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Api;

namespace Maliev.ShadcnBlazor.Tests.Components.Actions;

public sealed class ActionsSelectionParityTests : BunitContext
{
    public ActionsSelectionParityTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-selection.js");
        module.SetupVoid("setIndeterminate", _ => true);
        module.SetupVoid("attachRovingGroup", _ => true);
        module.SetupVoid("attachSlider", _ => true);
        module.SetupVoid("detach", _ => true);
    }

    [Fact]
    public void ToggleGroupUsesCurrentSpacingAndExposesInheritedPresentation()
    {
        var cut = Render<ShadcnToggleGroup<string>>(parameters => parameters
            .Add(group => group.Variant, ShadcnToggleVariant.Outline)
            .Add(group => group.Size, ShadcnToggleSize.Large)
            .AddChildContent<ShadcnToggleGroupItem<string>>(item => item
                .Add(control => control.Value, "bold")
                .AddChildContent("Bold")));

        var group = cut.Find("[data-slot='toggle-group']");
        Assert.Equal("2", group.GetAttribute("data-spacing"));
        Assert.Equal("outline", group.GetAttribute("data-variant"));
        Assert.Equal("lg", group.GetAttribute("data-size"));
    }

    [Fact]
    public void ToggleGroupRendersOneRovingTabStop()
    {
        var cut = Render<ShadcnToggleGroup<string>>(parameters => parameters
            .Add(group => group.Values, ["italic"])
            .AddChildContent(builder =>
            {
                AddToggleItem(builder, 0, "bold", "Bold");
                AddToggleItem(builder, 10, "italic", "Italic");
                AddToggleItem(builder, 20, "underline", "Underline", disabled: true);
            }));

        var items = cut.FindAll("[data-slot='toggle-group-item']");
        Assert.Equal("-1", items[0].GetAttribute("tabindex"));
        Assert.Equal("0", items[1].GetAttribute("tabindex"));
        Assert.Equal("-1", items[2].GetAttribute("tabindex"));
    }

    [Fact]
    public void RadioGroupRendersStableIdsAndOneRovingTabStop()
    {
        var cut = Render<ShadcnRadioGroup<string>>(parameters => parameters
            .Add(group => group.Value, "comfortable")
            .AddChildContent(builder =>
            {
                AddRadioItem(builder, 0, "default", "Default");
                AddRadioItem(builder, 10, "comfortable", "Comfortable");
                AddRadioItem(builder, 20, "compact", "Compact", disabled: true);
            }));

        var items = cut.FindAll("input[data-slot='radio-group-item']");
        Assert.All(items, item => Assert.False(string.IsNullOrWhiteSpace(item.Id)));
        Assert.Equal(items.Count, items.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal("-1", items[0].GetAttribute("tabindex"));
        Assert.Equal("0", items[1].GetAttribute("tabindex"));
        Assert.Equal("-1", items[2].GetAttribute("tabindex"));
    }

    [Fact]
    public void BooleanControlsRenderStableIds()
    {
        var checkbox = Render<ShadcnCheckbox>();
        var @switch = Render<ShadcnSwitch>();

        Assert.False(string.IsNullOrWhiteSpace(checkbox.Find("input").Id));
        Assert.False(string.IsNullOrWhiteSpace(@switch.Find("input").Id));
    }

    [Fact]
    public void CheckboxNotifiesEditContextAfterRequestingValueChange()
    {
        var model = new BooleanModel();
        var editContext = new EditContext(model);
        FieldIdentifier? notified = null;
        editContext.OnFieldChanged += (_, args) => notified = args.FieldIdentifier;

        var cut = Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(cascade => cascade.Value, editContext)
            .AddChildContent<ShadcnCheckbox>(checkbox => checkbox
                .Add(control => control.ValueExpression, () => model.Accepted)
                .Add(control => control.ValueChanged, value => model.Accepted = value)));

        cut.Find("input").Change(true);

        Assert.Equal(nameof(BooleanModel.Accepted), notified?.FieldName);
    }

    [Fact]
    public void RadioSwitchAndSliderNotifyTheirEditContextFields()
    {
        var model = new FormModel();
        var editContext = new EditContext(model);
        var notified = new List<string>();
        editContext.OnFieldChanged += (_, args) => notified.Add(args.FieldIdentifier.FieldName);

        var radio = Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(cascade => cascade.Value, editContext)
            .AddChildContent<ShadcnRadioGroup<string>>(group => group
                .Add(control => control.Value, model.Priority)
                .Add(control => control.ValueExpression, () => model.Priority)
                .Add(control => control.ValueChanged, value => model.Priority = value)
                .AddChildContent<ShadcnRadioGroupItem<string>>(item => item.Add(control => control.Value, "high"))));
        radio.Find("input").Change(true);

        var @switch = Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(cascade => cascade.Value, editContext)
            .AddChildContent<ShadcnSwitch>(control => control
                .Add(input => input.ValueExpression, () => model.Enabled)
                .Add(input => input.ValueChanged, value => model.Enabled = value)));
        @switch.Find("input").Change(true);

        var slider = Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(cascade => cascade.Value, editContext)
            .AddChildContent<ShadcnSlider>(control => control
                .Add(input => input.Values, model.Range)
                .Add(input => input.ValueExpression, () => model.Range)
                .Add(input => input.ValuesChanged, value => model.Range = value)));
        slider.Find("input").Input("25");

        Assert.Equal([nameof(FormModel.Priority), nameof(FormModel.Enabled), nameof(FormModel.Range)], notified);
    }

    [Fact]
    public void SliderSingleValueFillsFromMinimumAndSeparatorUsesOfficialAxis()
    {
        var slider = Render<ShadcnSlider>(parameters => parameters.Add(control => control.Values, [25d]));
        var group = Render<ShadcnButtonGroup>(parameters => parameters
            .AddChildContent<ShadcnButtonGroupSeparator>());

        Assert.Contains("--shadcn-slider-start: 0%", slider.Find("[data-slot='slider']").GetAttribute("style"), StringComparison.Ordinal);
        Assert.Contains("--shadcn-slider-end: 25%", slider.Find("[data-slot='slider']").GetAttribute("style"), StringComparison.Ordinal);
        Assert.Equal("vertical", group.Find("[data-slot='button-group-separator']").GetAttribute("aria-orientation"));
    }

    [Fact]
    public void SelectionInteropContainsKeyboardRtlPointerAndReadOnlyGuards()
    {
        var script = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "js", "shadcn-selection.js"));

        Assert.Contains("event.key === \"Home\"", script, StringComparison.Ordinal);
        Assert.Contains("event.key === \"End\"", script, StringComparison.Ordinal);
        Assert.Contains("event.key === \"ArrowRight\"", script, StringComparison.Ordinal);
        Assert.Contains("getComputedStyle(root).direction === \"rtl\"", script, StringComparison.Ordinal);
        Assert.Contains("root.addEventListener(\"pointerdown\"", script, StringComparison.Ordinal);
        Assert.Contains("if (readOnly", script, StringComparison.Ordinal);
        Assert.Contains("Math.abs(Number(input.value) - raw)", script, StringComparison.Ordinal);
    }

    [Fact]
    public void SliderUsesLiveInputAndExplicitRangeAria()
    {
        IReadOnlyList<double>? requested = null;
        var cut = Render<ShadcnSlider>(parameters => parameters
            .Add(slider => slider.Values, [25d])
            .Add(slider => slider.Minimum, 0)
            .Add(slider => slider.Maximum, 100)
            .Add(slider => slider.Step, 5)
            .Add(slider => slider.ValuesChanged, value => requested = value));

        var thumb = cut.Find("input[data-slot='slider-thumb']");
        Assert.Equal("0", thumb.GetAttribute("aria-valuemin"));
        Assert.Equal("100", thumb.GetAttribute("aria-valuemax"));
        Assert.Equal("25", thumb.GetAttribute("aria-valuenow"));

        thumb.Input("30");
        Assert.Equal([30d], requested);
    }

    [Fact]
    public void ActionStylesMatchPinnedVegaGeometryAndStateTreatment()
    {
        var css = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-actions.css"));

        Assert.Contains("background: color-mix(in oklch, var(--shadcn-destructive) 10%, transparent)", css, StringComparison.Ordinal);
        Assert.Contains("color: var(--shadcn-destructive)", css, StringComparison.Ordinal);
        Assert.Contains("background: color-mix(in oklch, var(--shadcn-primary) 80%, transparent)", css, StringComparison.Ordinal);
        Assert.Contains("height: 1.5rem", css, StringComparison.Ordinal);
        Assert.Contains("height: var(--shadcn-control-height)", css, StringComparison.Ordinal);
        Assert.Contains("height: 0.375rem", css, StringComparison.Ordinal);
        Assert.Contains("@media (pointer: coarse)", css, StringComparison.Ordinal);
        Assert.Contains("@media (forced-colors: active)", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-toggle {\n        border-width: 1px;", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-toggle {", css, StringComparison.Ordinal);
        Assert.Contains("border: 0 solid transparent", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-toggle[data-variant=\"outline\"] { border-width: 1px", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-toggle-group[data-orientation=\"vertical\"] { flex-direction: column; align-items: stretch; }", css, StringComparison.Ordinal);
        Assert.True(
            css.LastIndexOf(".shadcn-button:focus-visible", StringComparison.Ordinal) >
            css.LastIndexOf(".shadcn-button[data-variant=\"outline\"]", StringComparison.Ordinal),
            "Focus-visible treatment must follow the outline shadow so the variant cannot erase the ring.");
        Assert.True(
            css.LastIndexOf(".shadcn-button[aria-invalid=\"true\"]", StringComparison.Ordinal) >
            css.LastIndexOf(".shadcn-button:focus-visible", StringComparison.Ordinal),
            "Invalid treatment must remain the final state override.");
        Assert.True(
            css.LastIndexOf(".shadcn-toggle[data-state=\"on\"]", StringComparison.Ordinal) >
            css.LastIndexOf(".shadcn-toggle[data-variant=\"outline\"]", StringComparison.Ordinal),
            "Pressed state must win over the outline variant's transparent base.");
    }

    [Fact]
    public void SliderExposesStableThumbFormAndAccessibleNameContracts()
    {
        var cut = Render<ShadcnSlider>(parameters => parameters.Add(slider => slider.Values, [20d, 80d]));
        var thumbs = cut.FindAll("input[data-slot='slider-thumb']");
        var initialIds = thumbs.Select(thumb => thumb.Id).ToArray();

        Assert.All(initialIds, id => Assert.False(string.IsNullOrWhiteSpace(id)));
        cut.Render();
        Assert.Equal(initialIds, cut.FindAll("input[data-slot='slider-thumb']").Select(thumb => thumb.Id));

        var parameters = typeof(ShadcnSlider).GetProperties().Select(property => property.Name).ToHashSet(StringComparer.Ordinal);
        Assert.Contains("Form", parameters);
        Assert.Contains("Required", parameters);
        Assert.Contains("ThumbAttributes", parameters);
        var typedThumbParameters = typeof(ShadcnSliderThumbAttributes).GetProperties().Select(property => property.Name).ToHashSet(StringComparer.Ordinal);
        Assert.True(new[] { "Id", "Name", "Form", "Required", "AriaLabel", "AriaLabelledBy", "AdditionalAttributes" }.ToHashSet(StringComparer.Ordinal).SetEquals(typedThumbParameters));
    }

    [Fact]
    public void SliderInteropAndCssProtectReadOnlyAndImplementARealVerticalAxis()
    {
        var root = FindRoot();
        var script = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "js", "shadcn-selection.js"));
        var css = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-actions.css"));

        Assert.Contains("if (readOnly) {", script, StringComparison.Ordinal);
        Assert.Contains("event.preventDefault()", script[script.IndexOf("if (readOnly) {", StringComparison.Ordinal)..], StringComparison.Ordinal);
        Assert.Contains("restoreReadOnlyValues", script, StringComparison.Ordinal);
        Assert.Contains("writing-mode: vertical-lr", css, StringComparison.Ordinal);
        Assert.Contains("direction: rtl", css, StringComparison.Ordinal);
    }

    [Fact]
    public void VegaActionCssIncludesPinnedInteractiveIconAndConnectedGroupStates()
    {
        var css = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-actions.css"));

        Assert.Contains(":active:not([aria-haspopup])", css, StringComparison.Ordinal);
        Assert.Contains("aria-invalid=\"true\"", css, StringComparison.Ordinal);
        Assert.Contains("aria-expanded=\"true\"", css, StringComparison.Ordinal);
        Assert.Contains("[data-icon=\"inline-start\"]", css, StringComparison.Ordinal);
        Assert.Contains("[data-icon=\"inline-end\"]", css, StringComparison.Ordinal);
        Assert.Contains("data-spacing=\"0\"", css, StringComparison.Ordinal);
        Assert.Contains("border-inline-start-width: 0", css, StringComparison.Ordinal);
        Assert.Contains("cursor: default", css, StringComparison.Ordinal);
        Assert.Contains("[data-pointer-cursor=\"true\"]", css, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("button", "ShadcnButtonGroup")]
    [InlineData("toggle", "ShadcnToggleGroup")]
    public void ApiCatalogDoesNotLeakSiblingCompositionTypes(string slug, string excludedType)
    {
        var documented = new ComponentDocumentationCatalog().FindBySlug(slug)!;
        var entry = documented with { Status = ComponentDocumentationStatus.Complete };
        var descriptors = new ComponentApiCatalog().GetByEntry(entry);

        Assert.DoesNotContain(descriptors, descriptor => descriptor.Name.StartsWith(excludedType, StringComparison.Ordinal));
    }

    private static void AddToggleItem(RenderTreeBuilder builder, int sequence, string value, string label, bool disabled = false)
    {
        builder.OpenComponent<ShadcnToggleGroupItem<string>>(sequence);
        builder.AddAttribute(sequence + 1, nameof(ShadcnToggleGroupItem<string>.Value), value);
        builder.AddAttribute(sequence + 2, nameof(ShadcnToggleGroupItem<string>.Disabled), disabled);
        builder.AddAttribute(sequence + 3, nameof(ShadcnToggleGroupItem<string>.ChildContent), (RenderFragment)(content => content.AddContent(0, label)));
        builder.CloseComponent();
    }

    private static void AddRadioItem(RenderTreeBuilder builder, int sequence, string value, string label, bool disabled = false)
    {
        builder.OpenComponent<ShadcnRadioGroupItem<string>>(sequence);
        builder.AddAttribute(sequence + 1, nameof(ShadcnRadioGroupItem<string>.Value), value);
        builder.AddAttribute(sequence + 2, nameof(ShadcnRadioGroupItem<string>.Disabled), disabled);
        builder.AddAttribute(sequence + 3, nameof(ShadcnRadioGroupItem<string>.ChildContent), (RenderFragment)(content => content.AddContent(0, label)));
        builder.CloseComponent();
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }

    private sealed class BooleanModel
    {
        public bool? Accepted { get; set; }
    }

    private sealed class FormModel
    {
        public string Priority { get; set; } = "normal";
        public bool Enabled { get; set; }
        public IReadOnlyList<double> Range { get; set; } = [10d];
    }
}
