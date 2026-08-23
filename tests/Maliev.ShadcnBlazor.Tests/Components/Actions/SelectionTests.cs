using Bunit;
using Maliev.ShadcnBlazor.Components.Forms;
using Maliev.ShadcnBlazor.Components.Selection;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Tests.Components.Actions;

public sealed class SelectionTests : BunitContext
{
    public SelectionTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-selection.js");
        module.SetupVoid("setIndeterminate", _ => true);
        module.SetupVoid("attachRovingGroup", _ => true);
        module.SetupVoid("detach", _ => true);
    }

    [Theory]
    [InlineData(true, "checked", "true")]
    [InlineData(false, "unchecked", "false")]
    [InlineData(null, "indeterminate", "mixed")]
    public void CheckboxRendersAllStates(bool? value, string state, string ariaChecked)
    {
        var cut = Render<ShadcnCheckbox>(p => p.Add(x => x.Value, value).Add(x => x.Name, "terms"));
        var input = cut.Find("input[data-slot='checkbox']");

        Assert.Equal("checkbox", input.GetAttribute("type"));
        Assert.Equal("terms", input.GetAttribute("name"));
        Assert.Equal(state, input.GetAttribute("data-state"));
        Assert.Equal(ariaChecked, input.GetAttribute("aria-checked"));
        Assert.Equal(value == true, input.HasAttribute("checked"));
    }

    [Fact]
    public void CheckboxChangesValueAndHonorsReadOnly()
    {
        bool? requested = null;
        var cut = Render<ShadcnCheckbox>(p => p
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<bool?>(this, value => requested = value)));
        cut.Find("input").Change(true);
        Assert.True(requested);

        requested = null;
        var readOnly = Render<ShadcnCheckbox>(p => p
            .Add(x => x.ReadOnly, true)
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<bool?>(this, value => requested = value)));
        readOnly.Find("input").Change(true);
        Assert.Null(requested);
    }

    [Fact]
    public void UnboundCheckboxOwnsItsInteractiveState()
    {
        var cut = Render<ShadcnCheckbox>();
        var input = cut.Find("input[data-slot='checkbox']");

        input.Change(true);

        Assert.True(input.HasAttribute("checked"));
        Assert.Equal("true", input.GetAttribute("aria-checked"));
        Assert.Equal("checked", input.GetAttribute("data-state"));

        input.Change(false);

        Assert.False(input.HasAttribute("checked"));
        Assert.Equal("false", input.GetAttribute("aria-checked"));
        Assert.Equal("unchecked", input.GetAttribute("data-state"));
    }

    [Fact]
    public void CheckboxConsumesFieldAccessibilityState()
    {
        var cut = Render<ShadcnField>(p => p
            .Add(x => x.Invalid, true)
            .Add(x => x.Disabled, true)
            .Add(x => x.DescriptionId, "help")
            .Add(x => x.ErrorId, "error")
            .AddChildContent<ShadcnCheckbox>());
        var input = cut.Find("input");

        Assert.True(input.HasAttribute("disabled"));
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Equal("help error", input.GetAttribute("aria-describedby"));
    }

    [Theory]
    [InlineData(ShadcnSwitchSize.Default, "default")]
    [InlineData(ShadcnSwitchSize.Small, "sm")]
    public void SwitchRendersSizesAndNativeSwitchSemantics(ShadcnSwitchSize size, string expected)
    {
        var cut = Render<ShadcnSwitch>(p => p.Add(x => x.Size, size).Add(x => x.Value, true).Add(x => x.Name, "alerts"));
        var input = cut.Find("input[data-slot='switch']");

        Assert.Equal("switch", input.GetAttribute("role"));
        Assert.Equal("checked", input.GetAttribute("data-state"));
        Assert.Equal(expected, input.GetAttribute("data-size"));
        Assert.Equal("alerts", input.GetAttribute("name"));
    }

    [Fact]
    public void SwitchRequestsControlledValue()
    {
        var requested = false;
        var cut = Render<ShadcnSwitch>(p => p
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<bool>(this, value => requested = value)));

        cut.Find("input").Change(true);
        Assert.True(requested);
    }

    [Fact]
    public void UncontrolledSwitchOwnsItsChangedState()
    {
        var cut = Render<ShadcnSwitch>();

        cut.Find("input").Change(true);

        var input = cut.Find("input[data-slot='switch']");
        Assert.True(input.HasAttribute("checked"));
        Assert.Equal("true", input.GetAttribute("aria-checked"));
        Assert.Equal("checked", input.GetAttribute("data-state"));
        Assert.Equal("checked", cut.Find("[data-slot='switch-root']").GetAttribute("data-state"));
        Assert.Equal("checked", cut.Find("[data-slot='switch-thumb']").GetAttribute("data-state"));
    }

    [Fact]
    public void ControlledSwitchKeepsCallerOwnedValueUntilParametersChange()
    {
        var requested = false;
        var cut = Render<ShadcnSwitch>(parameters => parameters
            .Add(component => component.Value, false)
            .Add(component => component.ValueChanged, EventCallback.Factory.Create<bool>(this, value => requested = value)));

        cut.Find("input").Change(true);

        Assert.True(requested);
        Assert.Equal("false", cut.Find("input").GetAttribute("aria-checked"));
        cut.Render(parameters => parameters
            .Add(component => component.Value, true)
            .Add(component => component.ValueChanged, EventCallback.Factory.Create<bool>(this, value => requested = value)));
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-checked"));
    }

    [Fact]
    public void RadioGroupUsesSharedNameAndRequestsTypedValue()
    {
        int requested = 0;
        var cut = Render<ShadcnRadioGroup<int>>(p => p
            .Add(x => x.Name, "priority")
            .Add(x => x.Value, 1)
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<int>(this, value => requested = value))
            .Add(x => x.Orientation, ShadcnRadioGroupOrientation.Horizontal)
            .AddChildContent(builder =>
            {
                for (var value = 1; value <= 2; value++)
                {
                    builder.OpenComponent<ShadcnRadioGroupItem<int>>(value * 10);
                    builder.AddAttribute(value * 10 + 1, nameof(ShadcnRadioGroupItem<int>.Value), value);
                    builder.AddAttribute(value * 10 + 2, nameof(ShadcnRadioGroupItem<int>.ChildContent), (RenderFragment)(b => b.AddContent(0, $"P{value}")));
                    builder.CloseComponent();
                }
            }));

        var inputs = cut.FindAll("input[type='radio']");
        Assert.All(inputs, input => Assert.Equal("priority", input.GetAttribute("name")));
        Assert.True(inputs[0].HasAttribute("checked"));
        inputs[1].Change(true);
        Assert.Equal(2, requested);
        Assert.Equal("horizontal", cut.Find("[data-slot='radio-group']").GetAttribute("data-orientation"));
    }

    [Fact]
    public void DisabledRadioItemSuppressesChange()
    {
        var calls = 0;
        var cut = Render<ShadcnRadioGroup<string>>(p => p
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<string>(this, _ => calls++))
            .AddChildContent<ShadcnRadioGroupItem<string>>(item => item.Add(x => x.Value, "blocked").Add(x => x.Disabled, true)));

        Assert.True(cut.Find("input").HasAttribute("disabled"));
        Assert.Equal(0, calls);
    }

    [Fact]
    public void CardRadioPresentationMarksTheWholeNativeLabelSurface()
    {
        var cut = Render<ShadcnRadioGroup<string>>(parameters => parameters
            .Add(component => component.Value, "priority")
            .Add(component => component.Presentation, ShadcnRadioGroupPresentation.Card)
            .AddChildContent<ShadcnRadioGroupItem<string>>(item => item
                .Add(component => component.Value, "priority")
                .Add(component => component.ChildContent, "Priority review")));

        Assert.Equal("card", cut.Find("[data-slot='radio-group']").GetAttribute("data-presentation"));
        var card = cut.Find("label[data-slot='radio-group-item-root']");
        Assert.Equal("card", card.GetAttribute("data-presentation"));
        Assert.Equal("checked", card.GetAttribute("data-state"));
        Assert.NotNull(card.QuerySelector("input[type='radio']"));
    }

    [Fact]
    public void SelectionControlsRejectUnknownEnums()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSwitch>(p => p.Add(x => x.Size, (ShadcnSwitchSize)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnRadioGroup<string>>(p => p.Add(x => x.Orientation, (ShadcnRadioGroupOrientation)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnRadioGroup<string>>(p => p.Add(x => x.Presentation, (ShadcnRadioGroupPresentation)999)));
    }
}
