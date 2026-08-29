using Bunit;
using Maliev.ShadcnBlazor.Components.Forms;
using Maliev.ShadcnBlazor.Components.Navigation.Sidebar;
using Maliev.ShadcnBlazor.Components.Overlays;
using Maliev.ShadcnBlazor.Components.Primitives;
using Maliev.ShadcnBlazor.Components.Selection;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Tests.Components.Forms;

public sealed class FocusApiTests : BunitContext
{
    public FocusApiTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task InputFocusAsyncTargetsTheRenderedInput()
    {
        var cut = Render<ShadcnInput<string>>();
        Assert.IsAssignableFrom<IShadcnFocusable>(cut.Instance);
        Assert.NotNull(cut.Find("input[data-slot='input']"));

        await AssertFocusAsync(() => cut.Instance.FocusAsync(preventScroll: true), true);
    }

    [Fact]
    public async Task TextareaFocusAsyncTargetsTheRenderedTextarea()
    {
        var cut = Render<ShadcnTextarea<string>>();
        Assert.IsAssignableFrom<IShadcnFocusable>(cut.Instance);
        Assert.NotNull(cut.Find("textarea[data-slot='textarea']"));

        await AssertFocusAsync(() => cut.Instance.FocusAsync(), false);
    }

    [Fact]
    public async Task NativeSelectFocusAsyncTargetsTheRenderedSelect()
    {
        var cut = Render<ShadcnNativeSelect<string>>();
        Assert.IsAssignableFrom<IShadcnFocusable>(cut.Instance);
        Assert.NotNull(cut.Find("select[data-slot='native-select']"));

        await AssertFocusAsync(() => cut.Instance.FocusAsync(preventScroll: true), true);
    }

    [Fact]
    public async Task CheckboxFocusAsyncTargetsTheRenderedInput()
    {
        var cut = Render<ShadcnCheckbox>();
        Assert.IsAssignableFrom<IShadcnFocusable>(cut.Instance);
        Assert.NotNull(cut.Find("input[data-slot='checkbox']"));

        await AssertFocusAsync(() => cut.Instance.FocusAsync(), false);
    }

    [Fact]
    public async Task SwitchFocusAsyncTargetsTheRenderedInput()
    {
        var cut = Render<ShadcnSwitch>();
        Assert.IsAssignableFrom<IShadcnFocusable>(cut.Instance);
        Assert.NotNull(cut.Find("input[data-slot='switch']"));

        await AssertFocusAsync(() => cut.Instance.FocusAsync(preventScroll: true), true);
    }

    [Fact]
    public async Task InputOtpFocusAsyncTargetsTheRenderedInput()
    {
        var cut = Render<ShadcnInputOtp>();
        Assert.IsAssignableFrom<IShadcnFocusable>(cut.Instance);
        Assert.NotNull(cut.Find("input[data-slot='input-otp']"));

        await AssertFocusAsync(() => cut.Instance.FocusAsync(), false);
    }

    [Fact]
    public async Task CommandInputFocusAsyncTargetsTheRenderedSearchInput()
    {
        var cut = Render<ShadcnCommand>(parameters => parameters
            .Add(component => component.Label, "Commands")
            .AddChildContent<ShadcnCommandInput>());
        var input = cut.FindComponent<ShadcnCommandInput>();
        Assert.IsAssignableFrom<IShadcnFocusable>(input.Instance);
        Assert.NotNull(cut.Find("input[data-slot='command-input']"));

        await AssertFocusAsync(() => input.Instance.FocusAsync(preventScroll: true), true);
    }

    [Fact]
    public async Task SidebarInputFocusAsyncTargetsTheRenderedInput()
    {
        var cut = Render<ShadcnSidebarInput>();
        Assert.IsAssignableFrom<IShadcnFocusable>(cut.Instance);
        Assert.NotNull(cut.Find("input[data-slot='sidebar-input']"));

        await AssertFocusAsync(() => cut.Instance.FocusAsync(), false);
    }

    private async Task AssertFocusAsync(Func<ValueTask> focus, bool preventScroll)
    {
        await focus();

        var invocation = Assert.Single(
            JSInterop.Invocations,
            candidate => candidate.Identifier == "Blazor._internal.domWrapper.focus");
        var actualElement = Assert.IsType<ElementReference>(invocation.Arguments[0]);
        Assert.False(string.IsNullOrWhiteSpace(actualElement.Id));
        Assert.Equal(preventScroll, invocation.Arguments[1]);
    }

}
