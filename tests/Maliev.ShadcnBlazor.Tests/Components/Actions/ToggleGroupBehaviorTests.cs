using Bunit;
using Maliev.ShadcnBlazor.Components.Actions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Maliev.ShadcnBlazor.Tests.Components.Actions;

public sealed class ToggleGroupBehaviorTests : BunitContext
{
    public ToggleGroupBehaviorTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-selection.js");
        module.SetupVoid("attachRovingGroup", _ => true);
        module.SetupVoid("detach", _ => true);
    }

    [Fact]
    public void UncontrolledSingleGroupRetainsTheSelectedItem()
    {
        var cut = RenderGroup(multiple: false);
        var items = cut.FindAll("[data-slot='toggle-group-item']");

        items[1].Click();

        items = cut.FindAll("[data-slot='toggle-group-item']");
        Assert.Equal("false", items[0].GetAttribute("aria-pressed"));
        Assert.Equal("true", items[1].GetAttribute("aria-pressed"));
    }

    [Fact]
    public void UncontrolledMultipleGroupAddsAndRemovesItems()
    {
        var cut = RenderGroup(multiple: true);
        var items = cut.FindAll("[data-slot='toggle-group-item']");

        items[1].Click();
        cut.FindAll("[data-slot='toggle-group-item']")[0].Click();

        items = cut.FindAll("[data-slot='toggle-group-item']");
        Assert.Equal("false", items[0].GetAttribute("aria-pressed"));
        Assert.Equal("true", items[1].GetAttribute("aria-pressed"));
    }

    [Fact]
    public void ControlledGroupRequestsTheNextSelectionAndFollowsTheSuppliedValue()
    {
        IReadOnlyCollection<string> requested = [];
        var cut = Render<ShadcnToggleGroup<string>>(parameters => parameters
            .Add(group => group.Values, ["dimensions"])
            .Add(group => group.ValuesChanged, EventCallback.Factory.Create<IReadOnlyCollection<string>>(this, next => requested = next))
            .AddChildContent(builder =>
            {
                AddItem(builder, 0, "dimensions", "Dimensions");
                AddItem(builder, 10, "notes", "Notes");
            }));

        cut.FindAll("button")[1].Click();
        Assert.Equal(["notes"], requested);

        cut.Render(parameters => parameters
            .Add(group => group.Values, requested)
            .Add(group => group.ValuesChanged, EventCallback.Factory.Create<IReadOnlyCollection<string>>(this, next => requested = next))
            .AddChildContent(builder =>
            {
                AddItem(builder, 0, "dimensions", "Dimensions");
                AddItem(builder, 10, "notes", "Notes");
            }));

        Assert.Equal("true", cut.FindAll("button")[1].GetAttribute("aria-pressed"));
    }

    [Fact]
    public void GroupOwnsOrientationAndDisabledSemanticsWhileForwardingInvalidState()
    {
        var cut = Render<ShadcnToggleGroup<string>>(parameters => parameters
            .Add(group => group.Orientation, ShadcnToggleGroupOrientation.Vertical)
            .Add(group => group.Disabled, true)
            .Add(group => group.AdditionalAttributes, new Dictionary<string, object>
            {
                ["aria-invalid"] = "true",
                ["aria-orientation"] = "horizontal"
            })
            .AddChildContent(builder => AddItem(builder, 0, "dimensions", "Dimensions")));

        var group = cut.Find("[data-slot='toggle-group']");
        Assert.Equal("vertical", group.GetAttribute("aria-orientation"));
        Assert.Equal("true", group.GetAttribute("aria-disabled"));
        Assert.Equal("true", group.GetAttribute("aria-invalid"));
        Assert.True(cut.Find("button").HasAttribute("disabled"));
    }

    private IRenderedComponent<ShadcnToggleGroup<string>> RenderGroup(bool multiple) =>
        Render<ShadcnToggleGroup<string>>(parameters => parameters
            .Add(group => group.Multiple, multiple)
            .Add(group => group.Values, ["dimensions"])
            .AddChildContent(builder =>
            {
                AddItem(builder, 0, "dimensions", "Dimensions");
                AddItem(builder, 10, "notes", "Notes");
            }));

    private static void AddItem(RenderTreeBuilder builder, int sequence, string value, string label)
    {
        builder.OpenComponent<ShadcnToggleGroupItem<string>>(sequence);
        builder.AddAttribute(sequence + 1, nameof(ShadcnToggleGroupItem<string>.Value), value);
        builder.AddAttribute(sequence + 2, nameof(ShadcnToggleGroupItem<string>.ChildContent), (RenderFragment)(content => content.AddContent(0, label)));
        builder.CloseComponent();
    }
}
