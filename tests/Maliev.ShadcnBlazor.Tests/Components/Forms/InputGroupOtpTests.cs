using Bunit;
using Maliev.ShadcnBlazor.Components.Actions;
using Maliev.ShadcnBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Tests.Components.Forms;

public sealed class InputGroupOtpTests : BunitContext
{
    public InputGroupOtpTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-forms.js");
        module.SetupVoid("wireGroupAddon", _ => true);
        module.SetupVoid("unwireGroupAddon", _ => true);
        module.SetupVoid("observeOtpSelection", _ => true);
        module.SetupVoid("disconnectOtpSelection", _ => true);
    }

    [Fact]
    public void OtpOnlyPaintsActiveCaretWhileItsNativeInputOwnsFocus()
    {
        var cut = Render<ShadcnInputOtp>(parameters => parameters
            .Add(component => component.Value, "123")
            .Add(component => component.ChildContent, builder =>
            {
                builder.OpenComponent<ShadcnInputOtpGroup>(0);
                builder.AddAttribute(1, nameof(ShadcnInputOtpGroup.ChildContent), (RenderFragment)(slots =>
                {
                    for (var index = 0; index < 3; index++)
                    {
                        slots.OpenComponent<ShadcnInputOtpSlot>(index * 2);
                        slots.AddAttribute(index * 2 + 1, nameof(ShadcnInputOtpSlot.Index), index);
                        slots.CloseComponent();
                    }
                }));
                builder.CloseComponent();
            }));

        Assert.Empty(cut.FindAll("[data-slot='input-otp-slot'][data-active='true']"));
        cut.Instance.UpdateOtpSelection(1, true);
        Assert.Single(cut.FindAll("[data-slot='input-otp-slot'][data-active='true']"));
        cut.Instance.UpdateOtpSelection(1, false);
        Assert.Empty(cut.FindAll("[data-slot='input-otp-slot'][data-active='true']"));
    }

    [Theory]
    [InlineData(ShadcnInputGroupAlignment.InlineStart, "inline-start")]
    [InlineData(ShadcnInputGroupAlignment.InlineEnd, "inline-end")]
    [InlineData(ShadcnInputGroupAlignment.BlockStart, "block-start")]
    [InlineData(ShadcnInputGroupAlignment.BlockEnd, "block-end")]
    public void InputGroupRendersLogicalAddonAlignment(ShadcnInputGroupAlignment alignment, string expected)
    {
        var cut = Render<ShadcnInputGroup>(parameters => parameters
            .AddChildContent<ShadcnInputGroupAddon>(addon => addon
                .Add(component => component.Alignment, alignment)
                .AddChildContent("Addon")));

        Assert.Equal("group", cut.Find("[data-slot='input-group']").GetAttribute("role"));
        Assert.Equal(expected, cut.Find("[data-slot='input-group-addon']").GetAttribute("data-align"));
    }

    [Theory]
    [InlineData(ShadcnInputGroupButtonSize.ExtraSmall, "xs")]
    [InlineData(ShadcnInputGroupButtonSize.Small, "sm")]
    [InlineData(ShadcnInputGroupButtonSize.IconExtraSmall, "icon-xs")]
    [InlineData(ShadcnInputGroupButtonSize.IconSmall, "icon-sm")]
    public void InputGroupButtonUsesPinnedSizesAndSafeDefaultType(ShadcnInputGroupButtonSize size, string expected)
    {
        var cut = Render<ShadcnInputGroupButton>(parameters => parameters
            .Add(component => component.Size, size)
            .AddChildContent("Action"));

        var button = cut.Find("button");
        Assert.Equal("button", button.GetAttribute("type"));
        Assert.Equal(expected, button.GetAttribute("data-size"));
        Assert.Equal("ghost", button.GetAttribute("data-variant"));
    }

    [Theory]
    [InlineData(ShadcnButtonVariant.Default, "default")]
    [InlineData(ShadcnButtonVariant.Destructive, "destructive")]
    [InlineData(ShadcnButtonVariant.Outline, "outline")]
    [InlineData(ShadcnButtonVariant.Secondary, "secondary")]
    [InlineData(ShadcnButtonVariant.Ghost, "ghost")]
    [InlineData(ShadcnButtonVariant.Link, "link")]
    public void InputGroupButtonExposesPinnedButtonVariants(ShadcnButtonVariant variant, string expected)
    {
        var cut = Render<ShadcnInputGroupButton>(parameters => parameters
            .Add(component => component.Variant, variant)
            .AddChildContent("Action"));

        var button = cut.Find("button");
        Assert.Contains("shadcn-button", button.ClassList);
        Assert.Contains("shadcn-input-group-button", button.ClassList);
        Assert.Equal(expected, button.GetAttribute("data-variant"));
    }

    [Fact]
    public void InputGroupTextAndInvalidControlExposePinnedStylingHooks()
    {
        var cut = Render<ShadcnInputGroup>(parameters => parameters.AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnInput<string>>(0);
            builder.AddAttribute(1, nameof(ShadcnInput<string>.Invalid), true);
            builder.CloseComponent();
            builder.OpenComponent<ShadcnInputGroupAddon>(2);
            builder.AddAttribute(3, nameof(ShadcnInputGroupAddon.ChildContent), (RenderFragment)(addon =>
            {
                addon.OpenComponent<ShadcnInputGroupText>(0);
                addon.AddAttribute(1, nameof(ShadcnInputGroupText.ChildContent), (RenderFragment)(text => text.AddContent(0, "THB / part")));
                addon.CloseComponent();
            }));
            builder.CloseComponent();
        }));

        Assert.Equal("true", cut.Find("[data-slot='input-group-control']").GetAttribute("aria-invalid"));
        Assert.Equal("THB / part", cut.Find("[data-slot='input-group-text']").TextContent);
    }

    [Fact]
    public void InputGroupMarksOfficialControlSlotAndDoesNotStealInteractiveAddonFocus()
    {
        var cut = Render<ShadcnInputGroup>(parameters => parameters.AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnInput<string>>(0);
            builder.CloseComponent();
            builder.OpenComponent<ShadcnInputGroupAddon>(1);
            builder.AddAttribute(2, nameof(ShadcnInputGroupAddon.ChildContent), (RenderFragment)(content =>
            {
                content.OpenElement(0, "button");
                content.AddAttribute(1, "type", "button");
                content.AddContent(2, "Action");
                content.CloseElement();
            }));
            builder.CloseComponent();
        }));

        Assert.Equal("input-group-control", cut.Find("input").GetAttribute("data-slot"));
        var script = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "js", "shadcn-forms.js"));
        Assert.Contains("eventTarget?.closest", script, StringComparison.Ordinal);
    }

    [Fact]
    public void InputGroupStylesPinCompactButtonDarkInvalidAndLogicalAddonContracts()
    {
        var css = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-forms.css"));

        Assert.Contains("[data-shadcn-theme=\"dark\"] .shadcn-input-group", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-input-group-button[data-size=\"icon-xs\"] { width: 1.5rem; height: 1.5rem;", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-input-group-addon[data-align=\"inline-end\"]:has(> button) { margin-inline-end: -.4rem;", css, StringComparison.Ordinal);
        Assert.Contains(":has(> [data-slot=\"input-group-control\"][aria-invalid=\"true\"])", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-input-group > :where(.shadcn-input, .shadcn-textarea):focus-visible { outline: none; box-shadow: none; }", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-input-group:focus-within { border-color: var(--shadcn-ring); box-shadow: 0 0 0 1px", css, StringComparison.Ordinal);
    }

    [Fact]
    public void InputOtpRendersOneAccessibleInputAndPresentationalSlots()
    {
        var cut = Render<ShadcnInputOtp>(parameters => parameters
            .Add(component => component.Value, "12")
            .Add(component => component.MaxLength, 4)
            .Add(component => component.Pattern, "[0-9]")
            .Add(component => component.Name, "code")
            .Add(component => component.InputMode, "numeric")
            .AddUnmatched("aria-label", "Verification code")
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnInputOtpGroup>(0);
                builder.AddAttribute(1, nameof(ShadcnInputOtpGroup.ChildContent), (RenderFragment)(group =>
                {
                    for (var index = 0; index < 4; index++)
                    {
                        group.OpenComponent<ShadcnInputOtpSlot>(index * 2);
                        group.AddAttribute(index * 2 + 1, nameof(ShadcnInputOtpSlot.Index), index);
                        group.CloseComponent();
                    }
                }));
                builder.CloseComponent();
            }));

        var input = Assert.Single(cut.FindAll("input[data-slot='input-otp']"));
        Assert.Equal("Verification code", input.GetAttribute("aria-label"));
        Assert.Equal("numeric", input.GetAttribute("inputmode"));
        Assert.Equal("code", input.GetAttribute("name"));
        Assert.Equal("12", input.GetAttribute("value"));
        Assert.False(input.HasAttribute("pattern"));
        Assert.Equal("[0-9]", input.GetAttribute("data-pattern"));
        var slots = cut.FindAll("[data-slot='input-otp-slot']");
        Assert.Equal(4, slots.Count);
        Assert.Equal("1", slots[0].TextContent);
        Assert.Equal("2", slots[1].TextContent);
    }

    [Fact]
    public void InputOtpNormalizesPasteLikeInputAndSuppressesLockedChanges()
    {
        var requested = string.Empty;
        var cut = Render<ShadcnInputOtp>(parameters => parameters
            .Add(component => component.Value, string.Empty)
            .Add(component => component.MaxLength, 6)
            .Add(component => component.Pattern, "[0-9]")
            .Add(component => component.ValueChanged, value => requested = value));
        cut.Find("input").Input("12 a3-4567");
        Assert.Equal("123456", requested);

        var calls = 0;
        var locked = Render<ShadcnInputOtp>(parameters => parameters
            .Add(component => component.Value, "123")
            .Add(component => component.ReadOnly, true)
            .Add(component => component.ValueChanged, _ => calls++));
        locked.Find("input").Input("999");
        Assert.Equal(0, calls);
        Assert.True(locked.Find("input").HasAttribute("readonly"));
    }

    [Fact]
    public void OtpReplacementPreservesTheSelectionReportedByTheNativeInputObserver()
    {
        var cut = Render<ShadcnInputOtp>(parameters => parameters
            .Add(component => component.Value, "2241")
            .Add(component => component.MaxLength, 4)
            .Add(component => component.ValueChanged, _ => { })
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnInputOtpGroup>(0);
                builder.AddAttribute(1, nameof(ShadcnInputOtpGroup.ChildContent), (RenderFragment)(group =>
                {
                    for (var index = 0; index < 4; index++)
                    {
                        group.OpenComponent<ShadcnInputOtpSlot>(index * 2);
                        group.AddAttribute(index * 2 + 1, nameof(ShadcnInputOtpSlot.Index), index);
                        group.CloseComponent();
                    }
                }));
                builder.CloseComponent();
            }));

        cut.Instance.UpdateOtpSelection(0, true);
        cut.Find("input").Input("3241");
        cut.Instance.UpdateOtpSelection(1, true);

        Assert.Equal("true", cut.FindAll("[data-slot='input-otp-slot']")[1].GetAttribute("data-active"));
    }

    [Fact]
    public void InputOtpDoesNotLetNativeMaxlengthTruncatePasteBeforePatternNormalization()
    {
        var cut = Render<ShadcnInputOtp>(parameters => parameters
            .Add(component => component.MaxLength, 6)
            .Add(component => component.Pattern, "[0-9]"));

        Assert.False(cut.Find("input").HasAttribute("maxlength"));
    }

    [Fact]
    public void InputOtpPreservesThaiCharactersWithoutNumericPattern()
    {
        var requested = string.Empty;
        var cut = Render<ShadcnInputOtp>(parameters => parameters
            .Add(component => component.MaxLength, 4)
            .Add(component => component.ValueChanged, value => requested = value));

        cut.Find("input").Input("กขคงจ");

        Assert.Equal("กขคง", requested);
    }

    [Fact]
    public void InputOtpExposesCompletionInvalidAndDisabledStateOnItsVisibleRoot()
    {
        var cut = Render<ShadcnInputOtp>(parameters => parameters
            .Add(component => component.Value, "123456")
            .Add(component => component.MaxLength, 6)
            .Add(component => component.Invalid, true)
            .Add(component => component.Disabled, true)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnInputOtpGroup>(0);
                builder.AddAttribute(1, nameof(ShadcnInputOtpGroup.ChildContent), (RenderFragment)(slots =>
                {
                    for (var index = 0; index < 6; index++)
                    {
                        slots.OpenComponent<ShadcnInputOtpSlot>(index * 2);
                        slots.AddAttribute(index * 2 + 1, nameof(ShadcnInputOtpSlot.Index), index);
                        slots.CloseComponent();
                    }
                }));
                builder.CloseComponent();
            }));

        var root = cut.Find("[data-slot='input-otp-root']");
        Assert.Equal("true", root.GetAttribute("data-complete"));
        Assert.Equal("true", root.GetAttribute("data-invalid"));
        Assert.Equal("true", root.GetAttribute("data-disabled"));
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid"));
    }

    [Fact]
    public void OtpSeparatorUsesNativeSeparatorSemantics()
    {
        var cut = Render<ShadcnInputOtpSeparator>();
        Assert.Equal("separator", cut.Find("[data-slot='input-otp-separator']").GetAttribute("role"));
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
}
