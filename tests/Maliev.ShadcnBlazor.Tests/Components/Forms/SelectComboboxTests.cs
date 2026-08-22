using Bunit;
using Maliev.ShadcnBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace Maliev.ShadcnBlazor.Tests.Components.Forms;

public sealed class SelectComboboxTests : BunitContext
{
    public SelectComboboxTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-forms.js");
        module.SetupVoid("observePopupDismissal", _ => true);
        module.SetupVoid("disconnectPopupDismissal", _ => true);
        module.SetupVoid("focusElement", _ => true);
    }
    private static readonly IReadOnlyList<ShadcnSelectOption<int>> Priorities =
    [
        new(1, "Low", "Priority"),
        new(2, "High", "Priority"),
        new(3, "Unavailable", "Other", true)
    ];

    [Fact]
    public void SelectRendersControlledValueAndGroupedListbox()
    {
        var cut = Render<ShadcnSelect<int>>(parameters => parameters
            .Add(component => component.Value, 2)
            .Add(component => component.Options, Priorities)
            .Add(component => component.Name, "priority")
            .Add(component => component.Open, true)
            .Add(component => component.Placeholder, "Choose priority"));

        var trigger = cut.Find("button[data-slot='select-trigger']");
        Assert.Equal("combobox", trigger.GetAttribute("role"));
        Assert.Equal("listbox", trigger.GetAttribute("aria-haspopup"));
        Assert.Equal("true", trigger.GetAttribute("aria-expanded"));
        Assert.Equal("High", cut.Find("[data-slot='select-value']").TextContent);
        Assert.Equal(2, cut.FindAll("[role='group']").Count);
        Assert.Equal("true", cut.Find("[role='option'][data-value='2']").GetAttribute("aria-selected"));
        Assert.Equal("true", cut.Find("[role='option'][data-value='3']").GetAttribute("aria-disabled"));
        Assert.Equal("2", cut.Find("input[data-slot='select-form-control']").GetAttribute("value"));
        Assert.False(string.IsNullOrWhiteSpace(trigger.GetAttribute("aria-activedescendant")));
        Assert.All(cut.FindAll("[role='option']"), option => Assert.False(string.IsNullOrWhiteSpace(option.Id)));
    }

    [Fact]
    public void SelectRequiredProxyAndEditContextTrackTypedSelection()
    {
        var model = new SelectionModel();
        var editContext = new EditContext(model);
        var changed = 0;
        editContext.OnFieldChanged += (_, _) => changed++;
        var cut = Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(cascade => cascade.Value, editContext)
            .AddChildContent<ShadcnSelect<int?>>(select => select
                .Add(component => component.Value, model.Priority)
                .Add(component => component.ValueExpression, () => model.Priority)
                .Add(component => component.ValueChanged, value => model.Priority = value)
                .Add(component => component.Options, Priorities.Select(option => new ShadcnSelectOption<int?>(option.Value, option.Text, option.Group, option.Disabled)).ToArray())
                .Add(component => component.Name, "priority")
                .Add(component => component.Required, true)
                .Add(component => component.Open, true)));

        var proxy = cut.Find("input[data-slot='select-form-control']");
        Assert.True(proxy.HasAttribute("required"));
        Assert.Equal("priority", proxy.GetAttribute("name"));
        cut.Find("[role='option'][data-value='2']").Click();
        Assert.Equal(2, model.Priority);
        Assert.Equal(1, changed);
    }

    [Fact]
    public void UnselectedRequiredPopupProxiesAreEmptyAndClosedArrowDownOpensWithoutSelecting()
    {
        int? selected = null;
        var open = false;
        var cut = Render<ShadcnSelect<int?>>(parameters => parameters
            .Add(component => component.Value, selected)
            .Add(component => component.ValueChanged, value => selected = value)
            .Add(component => component.Options, Priorities.Select(option => new ShadcnSelectOption<int?>(option.Value, option.Text, option.Group, option.Disabled)).ToArray())
            .Add(component => component.Name, "priority")
            .Add(component => component.Required, true)
            .Add(component => component.Open, open)
            .Add(component => component.OpenChanged, value => open = value));

        Assert.Equal(string.Empty, cut.Find("[data-slot='select-form-control']").GetAttribute("value"));
        cut.Find("[data-slot='select-trigger']").KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        Assert.True(open);
        Assert.Null(selected);
    }

    [Fact]
    public void SelectKeyboardChoosesEnabledOptionAndCloses()
    {
        var selected = 1;
        var open = true;
        var cut = Render<ShadcnSelect<int>>(parameters => parameters
            .Add(component => component.Value, selected)
            .Add(component => component.ValueChanged, value => selected = value)
            .Add(component => component.Options, Priorities)
            .Add(component => component.Open, open)
            .Add(component => component.OpenChanged, value => open = value));

        var trigger = cut.Find("button");
        trigger.KeyDown(new KeyboardEventArgs { Key = "End" });
        trigger.KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.Equal(2, selected);
        Assert.False(open);
    }

    [Fact]
    public void SelectTypeaheadMovesToMatchingEnabledOption()
    {
        var selected = 1;
        var cut = Render<ShadcnSelect<int>>(parameters => parameters
            .Add(component => component.Value, selected)
            .Add(component => component.ValueChanged, value => selected = value)
            .Add(component => component.Options, Priorities)
            .Add(component => component.Open, true));

        var trigger = cut.Find("button");
        trigger.KeyDown(new KeyboardEventArgs { Key = "h" });
        trigger.KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.Equal(2, selected);
    }

    [Fact]
    public void SelectClearAndDisabledItemsHonorControlledSemantics()
    {
        int? selected = 2;
        var cut = Render<ShadcnSelect<int?>>(parameters => parameters
            .Add(component => component.Value, selected)
            .Add(component => component.ValueChanged, value => selected = value)
            .Add(component => component.Options, Priorities.Select(option => new ShadcnSelectOption<int?>(option.Value, option.Text, option.Group, option.Disabled)).ToArray())
            .Add(component => component.Clearable, true)
            .Add(component => component.Open, true));

        Assert.Equal("true", cut.Find("[data-slot='select']").GetAttribute("data-clearable"));
        cut.Find("[data-slot='select-clear']").Click();
        Assert.Null(selected);

        selected = 2;
        cut.Find("[role='option'][data-value='3']").Click();
        Assert.Equal(2, selected);
    }

    [Fact]
    public void SelectClearActionAndChevronReserveDistinctInTriggerSpace()
    {
        var cut = Render<ShadcnSelect<int>>(parameters => parameters
            .Add(component => component.Value, 2)
            .Add(component => component.Options, Priorities)
            .Add(component => component.Clearable, true));

        var root = cut.Find("[data-slot='select']");
        Assert.NotNull(root.QuerySelector(":scope > [data-slot='select-trigger']"));
        Assert.NotNull(root.QuerySelector(":scope > [data-slot='select-clear']"));
        Assert.NotNull(root.QuerySelector("[data-slot='select-trigger'] > [data-slot='select-trigger-icon']"));
        cut.Find("[data-slot='select-clear']").Click();
        Assert.Equal("false", cut.Find("[data-slot='select-trigger']").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void ReadOnlySelectStaysFocusableAndSuppressesChanges()
    {
        var calls = 0;
        var cut = Render<ShadcnSelect<int>>(parameters => parameters
            .Add(component => component.Value, 1)
            .Add(component => component.Options, Priorities)
            .Add(component => component.ReadOnly, true)
            .Add(component => component.ValueChanged, _ => calls++));

        var trigger = cut.Find("button");
        trigger.Click();
        trigger.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        Assert.Equal(0, calls);
        Assert.False(trigger.HasAttribute("disabled"));
        Assert.Equal("true", trigger.GetAttribute("aria-readonly"));
    }

    [Fact]
    public void SelectWithoutOpenBindingTogglesAndSelectsAnOption()
    {
        var cut = Render<ShadcnSelect<int>>(parameters => parameters
            .Add(component => component.Options, Priorities)
            .Add(component => component.Placeholder, "Choose priority"));

        cut.Find("[data-slot='select-trigger']").Click();

        Assert.Equal("true", cut.Find("[data-slot='select-trigger']").GetAttribute("aria-expanded"));
        cut.Find("[role='option'][data-value='2']").Click();

        Assert.Equal("High", cut.Find("[data-slot='select-value']").TextContent);
        Assert.Equal("false", cut.Find("[data-slot='select-trigger']").GetAttribute("aria-expanded"));
    }

    private static readonly IReadOnlyList<ShadcnComboboxOption<string>> Frameworks =
    [
        new("next", "Next.js", "Web"),
        new("nuxt", "Nuxt.js", "Web"),
        new("locked", "Locked", "Other", true)
    ];

    [Fact]
    public void ComboboxFiltersOptionsAndExposesTrueComboboxSemantics()
    {
        var cut = Render<ShadcnCombobox<string>>(parameters => parameters
            .Add(component => component.Options, Frameworks)
            .Add(component => component.Open, true)
            .Add(component => component.Query, "nu"));

        var input = cut.Find("input[role='combobox']");
        Assert.Equal("list", input.GetAttribute("aria-autocomplete"));
        Assert.Equal("listbox", input.GetAttribute("aria-haspopup"));
        Assert.Equal("true", input.GetAttribute("aria-expanded"));
        var options = cut.FindAll("[role='option']");
        Assert.Single(options);
        Assert.Equal("Nuxt.js", options[0].TextContent.Trim());
        Assert.Equal("Web", cut.Find("[role='group']").GetAttribute("aria-label"));
    }

    [Fact]
    public void ComboboxMergesDescriptionsAndSubmitsTypedValueInsteadOfDisplayText()
    {
        var field = new ShadcnFieldContext(false, false, "field-description", null);
        var cut = Render<CascadingValue<ShadcnFieldContext>>(parameters => parameters
            .Add(cascade => cascade.Value, field)
            .AddChildContent<ShadcnCombobox<string>>(combo => combo
                .Add(component => component.Value, "next")
                .Add(component => component.Options, Frameworks)
                .Add(component => component.Name, "framework")
                .Add(component => component.Required, true)
                .AddUnmatched("aria-describedby", "caller-description")));

        var input = cut.Find("input[role='combobox']");
        Assert.Equal("field-description caller-description", input.GetAttribute("aria-describedby"));
        Assert.False(input.HasAttribute("name"));
        Assert.Equal("next", cut.Find("input[data-slot='combobox-form-control']").GetAttribute("value"));
        Assert.True(cut.Find("input[data-slot='combobox-form-control']").HasAttribute("required"));
    }

    [Fact]
    public void EmptySingleComboboxRequiredProxyHasNoDefaultGenericValue()
    {
        var cut = Render<ShadcnCombobox<int>>(parameters => parameters
            .Add(component => component.Options, [])
            .Add(component => component.Name, "number")
            .Add(component => component.Required, true));

        Assert.Equal(string.Empty, cut.Find("[data-slot='combobox-form-control']").GetAttribute("value"));
    }

    [Fact]
    public void MultipleComboboxRequiredProxyAndClearUpdateEditContext()
    {
        var model = new SelectionModel { Frameworks = ["next"] };
        var editContext = new EditContext(model);
        var changed = 0;
        editContext.OnFieldChanged += (_, _) => changed++;
        var cut = Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(cascade => cascade.Value, editContext)
            .AddChildContent<ShadcnCombobox<string>>(combo => combo
                .Add(component => component.Multiple, true)
                .Add(component => component.Values, model.Frameworks)
                .Add(component => component.ValuesExpression, () => model.Frameworks)
                .Add(component => component.ValuesChanged, values => model.Frameworks = values)
                .Add(component => component.Options, Frameworks)
                .Add(component => component.Name, "frameworks")
                .Add(component => component.Required, true)
                .Add(component => component.ShowClear, true)));

        Assert.True(cut.Find("input[data-slot='combobox-required-control']").HasAttribute("required"));
        cut.Find("[data-slot='combobox-clear']").Click();
        Assert.Empty(model.Frameworks);
        Assert.Equal(1, changed);
    }

    [Fact]
    public void ComboboxKeyboardSelectsAndClearRequestsNull()
    {
        string? selected = null;
        var cut = Render<ShadcnCombobox<string>>(parameters => parameters
            .Add(component => component.Options, Frameworks)
            .Add(component => component.Value, selected)
            .Add(component => component.ValueChanged, value => selected = value)
            .Add(component => component.Open, true)
            .Add(component => component.ShowClear, true));

        var input = cut.Find("input[role='combobox']");
        input.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        input.KeyDown(new KeyboardEventArgs { Key = "Enter" });
        Assert.Equal("next", selected);

        cut.Render(parameters => parameters
            .Add(component => component.Options, Frameworks)
            .Add(component => component.Value, selected)
            .Add(component => component.ValueChanged, value => selected = value)
            .Add(component => component.ShowClear, true));
        cut.Find("[data-slot='combobox-clear']").Click();
        Assert.Null(selected);
    }

    [Fact]
    public void ComboboxWithoutBindingsOpensFiltersSelectsAndClearsItsValue()
    {
        var cut = Render<ShadcnCombobox<string>>(parameters => parameters
            .Add(component => component.Options, Frameworks)
            .Add(component => component.ShowClear, true)
            .Add(component => component.Placeholder, "Choose framework"));

        var input = cut.Find("input[role='combobox']");
        input.Focus();
        Assert.Equal("true", input.GetAttribute("aria-expanded"));

        input.Input("nu");
        Assert.Single(cut.FindAll("[role='option']"));
        input.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        input.KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.Equal("Nuxt.js", input.GetAttribute("value"));
        Assert.Equal("false", input.GetAttribute("aria-expanded"));
        cut.Find("[data-slot='combobox-clear']").Click();
        Assert.Equal(string.Empty, input.GetAttribute("value"));
    }

    [Fact]
    public void MultipleComboboxAddsAndRemovesChips()
    {
        IReadOnlyList<string> selected = ["next"];
        var cut = Render<ShadcnCombobox<string>>(parameters => parameters
            .Add(component => component.Options, Frameworks)
            .Add(component => component.Multiple, true)
            .Add(component => component.Values, selected)
            .Add(component => component.ValuesChanged, values => selected = values)
            .Add(component => component.Open, true));

        Assert.Equal("Next.js", cut.Find("[data-slot='combobox-chip']").TextContent.Trim().Trim('×').Trim());
        cut.Find("[role='option'][data-value='nuxt']").Click();
        Assert.Equal(["next", "nuxt"], selected);

        cut.Render(parameters => parameters
            .Add(component => component.Options, Frameworks)
            .Add(component => component.Multiple, true)
            .Add(component => component.Values, selected)
            .Add(component => component.ValuesChanged, values => selected = values));
        cut.Find("[data-slot='combobox-chip-remove']").Click();
        Assert.Equal(["nuxt"], selected);
    }

    [Fact]
    public void ComboboxShowsDeterministicEmptyState()
    {
        var cut = Render<ShadcnCombobox<string>>(parameters => parameters
            .Add(component => component.Options, Frameworks)
            .Add(component => component.Query, "missing")
            .Add(component => component.Open, true)
            .Add(component => component.EmptyText, "No items found."));

        Assert.Equal("No items found.", cut.Find("[data-slot='combobox-empty']").TextContent);
    }

    [Fact]
    public void ExternalValidationStateChangeAutomaticallyRerendersComboboxInvalidState()
    {
        var model = new SelectionModel();
        var editContext = new EditContext(model);
        var messages = new ValidationMessageStore(editContext);
        var cut = Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(cascade => cascade.Value, editContext)
            .AddChildContent<ShadcnCombobox<int?>>(combo => combo
                .Add(component => component.Value, model.Priority)
                .Add(component => component.ValueExpression, () => model.Priority)
                .Add(component => component.Options, [])));

        messages.Add(new FieldIdentifier(model, nameof(SelectionModel.Priority)), "External error");
        editContext.NotifyValidationStateChanged();

        cut.WaitForAssertion(() => Assert.Equal("true", cut.Find("[role='combobox']").GetAttribute("aria-invalid")));
    }

    private sealed class SelectionModel
    {
        public int? Priority { get; set; }
        public IReadOnlyList<string> Frameworks { get; set; } = [];
    }
}
