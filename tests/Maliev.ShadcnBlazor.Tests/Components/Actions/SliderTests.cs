using Bunit;
using Maliev.ShadcnBlazor.Components.Selection;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Tests.Components.Actions;

public sealed class SliderTests : BunitContext
{
    public SliderTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-selection.js");
        module.SetupVoid("attachSlider", _ => true);
        module.SetupVoid("detach", _ => true);
    }

    [Fact]
    public void SingleSliderRendersNativeRangeSemantics()
    {
        var cut = Render<ShadcnSlider>(p => p
            .Add(x => x.Values, new[] { 25d })
            .Add(x => x.Minimum, 0)
            .Add(x => x.Maximum, 100)
            .Add(x => x.Step, 5)
            .Add(x => x.Name, "volume"));
        var input = cut.Find("input[data-slot='slider-thumb']");

        Assert.Equal("range", input.GetAttribute("type"));
        Assert.Equal("25", input.GetAttribute("value"));
        Assert.Equal("0", input.GetAttribute("min"));
        Assert.Equal("100", input.GetAttribute("max"));
        Assert.Equal("5", input.GetAttribute("step"));
        Assert.Equal("volume", input.GetAttribute("name"));
        Assert.Equal("horizontal", input.GetAttribute("aria-orientation"));
    }

    [Fact]
    public void RootAccessibleLabelIsAppliedToEveryNativeThumb()
    {
        var labelled = Render<ShadcnSlider>(parameters => parameters
            .Add(component => component.Values, [20d, 80d])
            .AddUnmatched("aria-labelledby", "budget-label"));
        Assert.All(labelled.FindAll("input[type='range']"), thumb =>
            Assert.Equal("budget-label", thumb.GetAttribute("aria-labelledby")));

        var named = Render<ShadcnSlider>(parameters => parameters
            .Add(component => component.Values, [20d, 80d])
            .AddUnmatched("aria-label", "Budget"));
        Assert.All(named.FindAll("input[type='range']"), thumb =>
            Assert.StartsWith("Budget", thumb.GetAttribute("aria-label"), StringComparison.Ordinal));
    }

    [Fact]
    public void RangeSliderRendersOrderedThumbsAndRequestsChangedValue()
    {
        IReadOnlyList<double>? requested = null;
        var cut = Render<ShadcnSlider>(p => p
            .Add(x => x.Values, new[] { 20d, 80d })
            .Add(x => x.ValuesChanged, EventCallback.Factory.Create<IReadOnlyList<double>>(this, value => requested = value)));

        var inputs = cut.FindAll("input[type='range']");
        Assert.Equal(2, inputs.Count);
        inputs[0].Input("40");
        Assert.Equal(new[] { 40d, 80d }, requested);
    }

    [Fact]
    public void RangeSliderKeepsEveryNativeThumbOnTheGlobalVisualAxisAndClampsChangesToNeighbours()
    {
        IReadOnlyList<double>? requested = null;
        var cut = Render<ShadcnSlider>(parameters => parameters
            .Add(component => component.Values, [20d, 80d])
            .Add(component => component.Minimum, 0)
            .Add(component => component.Maximum, 100)
            .Add(component => component.ValuesChanged, value => requested = value));

        var thumbs = cut.FindAll("input[type='range']");
        Assert.All(thumbs, thumb =>
        {
            Assert.Equal("0", thumb.GetAttribute("min"));
            Assert.Equal("100", thumb.GetAttribute("max"));
        });

        thumbs[0].Input("90");
        Assert.Equal([80d, 80d], requested);
    }

    [Fact]
    public void SliderAssociatesEveryThumbWithExternalFormAndTypedOverrides()
    {
        var cut = Render<ShadcnSlider>(p => p
            .Add(x => x.Values, [20d, 80d])
            .Add(x => x.Name, "budget")
            .Add(x => x.Form, "quote-form")
            .Add(x => x.Required, true)
            .Add(x => x.ThumbAttributes,
            [
                new() { Id = "budget-min", AriaLabel = "Minimum budget" },
                new() { Id = "budget-max", Name = "budget-maximum", Form = "other-form", AriaLabelledBy = "maximum-label", AdditionalAttributes = new Dictionary<string, object> { ["data-currency"] = "THB" } }
            ]));

        var thumbs = cut.FindAll("input[type='range']");
        Assert.Equal("budget-min", thumbs[0].Id);
        Assert.Equal("budget", thumbs[0].GetAttribute("name"));
        Assert.Equal("quote-form", thumbs[0].GetAttribute("form"));
        Assert.True(thumbs[0].HasAttribute("required"));
        Assert.Equal("Minimum budget", thumbs[0].GetAttribute("aria-label"));
        Assert.Equal("budget-max", thumbs[1].Id);
        Assert.Equal("budget-maximum", thumbs[1].GetAttribute("name"));
        Assert.Equal("other-form", thumbs[1].GetAttribute("form"));
        Assert.Equal("maximum-label", thumbs[1].GetAttribute("aria-labelledby"));
        Assert.False(thumbs[1].HasAttribute("aria-label"));
        Assert.Equal("THB", thumbs[1].GetAttribute("data-currency"));
    }

    [Fact]
    public void ThumbAttributesMustBeNullOrMatchEverySliderValue()
    {
        var empty = Assert.Throws<ArgumentException>(() => Render<ShadcnSlider>(parameters => parameters
            .Add(component => component.Values, [20d])
            .Add(component => component.ThumbAttributes, [])));
        Assert.Equal("ThumbAttributes", empty.ParamName);

        var mismatched = Assert.Throws<ArgumentException>(() => Render<ShadcnSlider>(parameters => parameters
            .Add(component => component.Values, [20d, 80d])
            .Add(component => component.ThumbAttributes, [new()])));
        Assert.Equal("ThumbAttributes", mismatched.ParamName);

        var exact = Render<ShadcnSlider>(parameters => parameters
            .Add(component => component.Values, [20d, 80d])
            .Add(component => component.ThumbAttributes, [new(), new()]));
        Assert.Equal(2, exact.FindAll("input[type='range']").Count);
    }

    [Fact]
    public void VerticalSliderExposesOrientationAndState()
    {
        var cut = Render<ShadcnSlider>(p => p
            .Add(x => x.Values, new[] { 50d })
            .Add(x => x.Orientation, ShadcnSliderOrientation.Vertical)
            .Add(x => x.Disabled, true)
            .Add(x => x.Invalid, true));

        var root = cut.Find("[data-slot='slider']");
        var input = cut.Find("input");
        Assert.Equal("vertical", root.GetAttribute("data-orientation"));
        Assert.True(input.HasAttribute("disabled"));
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
    }

    [Fact]
    public void ReadOnlySliderSuppressesChangesWithoutRemovingFocus()
    {
        var calls = 0;
        var cut = Render<ShadcnSlider>(p => p
            .Add(x => x.Values, new[] { 10d })
            .Add(x => x.ReadOnly, true)
            .Add(x => x.ValuesChanged, EventCallback.Factory.Create<IReadOnlyList<double>>(this, _ => calls++)));

        Assert.False(cut.Find("input").HasAttribute("disabled"));
        Assert.Equal("true", cut.Find("[data-slot='slider']").GetAttribute("data-readonly"));
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-readonly"));
        cut.Find("input").Input("20");
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(0, 0, 1)]
    [InlineData(10, 0, 1)]
    [InlineData(0, 10, 0)]
    [InlineData(0, 10, -1)]
    public void RejectsInvalidRanges(double minimum, double maximum, double step)
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSlider>(p => p
            .Add(x => x.Values, new[] { minimum })
            .Add(x => x.Minimum, minimum)
            .Add(x => x.Maximum, maximum)
            .Add(x => x.Step, step)));
    }

    [Fact]
    public void RejectsMissingUnorderedAndOutOfRangeValues()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSlider>(p => p.Add(x => x.Values, Array.Empty<double>())));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSlider>(p => p.Add(x => x.Values, new[] { 80d, 20d })));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSlider>(p => p.Add(x => x.Values, new[] { 101d })));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSlider>(p => p.Add(x => x.Orientation, (ShadcnSliderOrientation)999)));
    }
}
