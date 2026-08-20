using Bunit;
using Maliev.ShadcnBlazor.Components.Actions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Maliev.ShadcnBlazor.Tests.Components.Actions;

public sealed class ToggleTests : BunitContext
{
    public ToggleTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-selection.js");
        module.SetupVoid("attachRovingGroup", _ => true);
        module.SetupVoid("detach", _ => true);
    }

    [Theory]
    [InlineData(ShadcnToggleVariant.Default, "default")]
    [InlineData(ShadcnToggleVariant.Outline, "outline")]
    public void ToggleRendersVariantAndControlledState(ShadcnToggleVariant variant, string expected)
    {
        var cut = Render<ShadcnToggle>(p => p.Add(x => x.Variant, variant).Add(x => x.Pressed, true).AddChildContent("Bold"));
        var toggle = cut.Find("button[data-slot='toggle']");

        Assert.Equal(expected, toggle.GetAttribute("data-variant"));
        Assert.Equal("true", toggle.GetAttribute("aria-pressed"));
        Assert.Equal("on", toggle.GetAttribute("data-state"));
        Assert.Equal("button", toggle.GetAttribute("type"));
    }

    [Theory]
    [InlineData(ShadcnToggleSize.Default, "default")]
    [InlineData(ShadcnToggleSize.Small, "sm")]
    [InlineData(ShadcnToggleSize.Large, "lg")]
    public void ToggleRendersEverySize(ShadcnToggleSize size, string expected)
    {
        var cut = Render<ShadcnToggle>(p => p.Add(x => x.Size, size).AddChildContent("Bold"));
        Assert.Equal(expected, cut.Find("button").GetAttribute("data-size"));
    }

    [Fact]
    public void ToggleRendersTypedLogicalIconSlots()
    {
        Assert.NotNull(typeof(ShadcnToggle).GetProperty("LeadingIcon"));
        Assert.NotNull(typeof(ShadcnToggle).GetProperty("TrailingIcon"));
        var cut = Render((RenderFragment)(builder =>
        {
            builder.OpenComponent<ShadcnToggle>(0);
            builder.AddAttribute(1, "LeadingIcon", (RenderFragment)(icon => icon.AddContent(0, "L")));
            builder.AddAttribute(2, "TrailingIcon", (RenderFragment)(icon => icon.AddContent(0, "T")));
            builder.AddAttribute(3, nameof(ShadcnToggle.ChildContent), (RenderFragment)(content => content.AddContent(0, "Format")));
            builder.CloseComponent();
        }));

        Assert.Equal("inline-start", cut.Find("[data-slot='toggle-leading-icon']").GetAttribute("data-icon"));
        Assert.Equal("inline-end", cut.Find("[data-slot='toggle-trailing-icon']").GetAttribute("data-icon"));
    }

    [Fact]
    public void ToggleGroupItemsRenderTypedIconsWhileInheritingGroupPresentation()
    {
        Assert.NotNull(typeof(ShadcnToggleGroupItem<string>).GetProperty("LeadingIcon"));
        Assert.NotNull(typeof(ShadcnToggleGroupItem<string>).GetProperty("TrailingIcon"));
        var cut = Render((RenderFragment)(builder =>
        {
            builder.OpenComponent<ShadcnToggleGroup<string>>(0);
            builder.AddAttribute(1, nameof(ShadcnToggleGroup<string>.Variant), ShadcnToggleVariant.Outline);
            builder.AddAttribute(2, nameof(ShadcnToggleGroup<string>.Size), ShadcnToggleSize.Small);
            builder.AddAttribute(3, nameof(ShadcnToggleGroup<string>.ChildContent), (RenderFragment)(content =>
            {
                content.OpenComponent<ShadcnToggleGroupItem<string>>(0);
                content.AddAttribute(1, nameof(ShadcnToggleGroupItem<string>.Value), "align");
                content.AddAttribute(2, "LeadingIcon", (RenderFragment)(icon => icon.AddContent(0, "L")));
                content.AddAttribute(3, "TrailingIcon", (RenderFragment)(icon => icon.AddContent(0, "T")));
                content.AddAttribute(4, nameof(ShadcnToggleGroupItem<string>.ChildContent), (RenderFragment)(itemContent => itemContent.AddContent(0, "Align")));
                content.CloseComponent();
            }));
            builder.CloseComponent();
        }));

        var item = cut.Find("[data-slot='toggle-group-item']");
        Assert.Equal("outline", item.GetAttribute("data-variant"));
        Assert.Equal("sm", item.GetAttribute("data-size"));
        Assert.Equal("inline-start", cut.Find("[data-slot='toggle-group-item-leading-icon']").GetAttribute("data-icon"));
        Assert.Equal("inline-end", cut.Find("[data-slot='toggle-group-item-trailing-icon']").GetAttribute("data-icon"));
    }

    [Fact]
    public void ToggleRequestsStateBeforeClickCallbackAndSuppressesWhenDisabled()
    {
        var calls = new List<string>();
        var cut = Render<ShadcnToggle>(p => p
            .Add(x => x.PressedChanged, EventCallback.Factory.Create<bool>(this, value => calls.Add($"state:{value}")))
            .Add(x => x.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => calls.Add("click")))
            .AddChildContent("Bold"));

        cut.Find("button").Click();
        Assert.Equal(new[] { "state:True", "click" }, calls);

        var disabled = Render<ShadcnToggle>(p => p
            .Add(x => x.Disabled, true)
            .Add(x => x.PressedChanged, EventCallback.Factory.Create<bool>(this, value => calls.Add($"state:{value}")))
            .Add(x => x.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => calls.Add("click"))));
        disabled.Find("button").Click();
        Assert.Equal(2, calls.Count);
    }

    [Fact]
    public void ToggleWithoutPressedBindingOwnsItsInteractiveState()
    {
        var cut = Render<ShadcnToggle>(parameters => parameters.AddChildContent("Bold"));
        var toggle = cut.Find("button[data-slot='toggle']");

        toggle.Click();

        Assert.Equal("true", toggle.GetAttribute("aria-pressed"));
        Assert.Equal("on", toggle.GetAttribute("data-state"));

        toggle.Click();

        Assert.Equal("false", toggle.GetAttribute("aria-pressed"));
        Assert.Equal("off", toggle.GetAttribute("data-state"));
    }

    [Fact]
    public void ToggleWithPressedBindingWaitsForTheOwnerAndReconcilesParameterChanges()
    {
        bool? requested = null;
        var cut = Render<ShadcnToggle>(parameters => parameters
            .Add(component => component.Pressed, false)
            .Add(component => component.PressedChanged, EventCallback.Factory.Create<bool>(this, value => requested = value))
            .AddChildContent("Bold"));
        var toggle = cut.Find("button[data-slot='toggle']");

        toggle.Click();

        Assert.True(requested);
        Assert.Equal("false", toggle.GetAttribute("aria-pressed"));

        cut.Render(parameters => parameters
            .Add(component => component.Pressed, true)
            .Add(component => component.PressedChanged, EventCallback.Factory.Create<bool>(this, value => requested = value))
            .AddChildContent("Bold"));

        Assert.Equal("true", toggle.GetAttribute("aria-pressed"));
        Assert.Equal("on", toggle.GetAttribute("data-state"));
    }

    [Fact]
    public void SingleToggleGroupSelectsOneValueAndInheritsPresentation()
    {
        IReadOnlyCollection<string>? requested = null;
        var cut = Render<ShadcnToggleGroup<string>>(p => p
            .Add(x => x.Values, new[] { "bold" })
            .Add(x => x.ValuesChanged, EventCallback.Factory.Create<IReadOnlyCollection<string>>(this, value => requested = value))
            .Add(x => x.Variant, ShadcnToggleVariant.Outline)
            .Add(x => x.Size, ShadcnToggleSize.Large)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnToggleGroupItem<string>>(0);
                builder.AddAttribute(1, nameof(ShadcnToggleGroupItem<string>.Value), "italic");
                builder.AddAttribute(2, nameof(ShadcnToggleGroupItem<string>.ChildContent), (RenderFragment)(b => b.AddContent(0, "Italic")));
                builder.CloseComponent();
            }));

        var item = cut.Find("button[data-slot='toggle-group-item']");
        Assert.Equal("outline", item.GetAttribute("data-variant"));
        Assert.Equal("lg", item.GetAttribute("data-size"));
        item.Click();
        Assert.Equal(new[] { "italic" }, requested);
    }

    [Fact]
    public void MultipleToggleGroupAddsAndRemovesValues()
    {
        IReadOnlyCollection<int>? requested = null;
        var cut = Render<ShadcnToggleGroup<int>>(p => p
            .Add(x => x.Multiple, true)
            .Add(x => x.Values, new[] { 1 })
            .Add(x => x.ValuesChanged, EventCallback.Factory.Create<IReadOnlyCollection<int>>(this, value => requested = value))
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnToggleGroupItem<int>>(0);
                builder.AddAttribute(1, nameof(ShadcnToggleGroupItem<int>.Value), 2);
                builder.AddAttribute(2, nameof(ShadcnToggleGroupItem<int>.ChildContent), (RenderFragment)(b => b.AddContent(0, "Two")));
                builder.CloseComponent();
            }));

        cut.Find("button").Click();
        Assert.Equal(new[] { 1, 2 }, requested);
    }

    [Fact]
    public void DisabledGroupSuppressesItemChanges()
    {
        var calls = 0;
        var cut = Render<ShadcnToggleGroup<string>>(p => p
            .Add(x => x.Disabled, true)
            .Add(x => x.ValuesChanged, EventCallback.Factory.Create<IReadOnlyCollection<string>>(this, _ => calls++))
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnToggleGroupItem<string>>(0);
                builder.AddAttribute(1, nameof(ShadcnToggleGroupItem<string>.Value), "x");
                builder.CloseComponent();
            }));

        Assert.True(cut.Find("button").HasAttribute("disabled"));
        Assert.Equal(0, calls);
    }

    [Fact]
    public void RejectsUnknownEnumsAndNegativeSpacing()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnToggle>(p => p.Add(x => x.Variant, (ShadcnToggleVariant)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnToggle>(p => p.Add(x => x.Size, (ShadcnToggleSize)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnToggleGroup<string>>(p => p.Add(x => x.Orientation, (ShadcnToggleGroupOrientation)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnToggleGroup<string>>(p => p.Add(x => x.Spacing, -1)));
    }
}
