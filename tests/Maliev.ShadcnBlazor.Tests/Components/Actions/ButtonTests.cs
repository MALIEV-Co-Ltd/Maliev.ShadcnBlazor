using Bunit;
using Maliev.ShadcnBlazor.Components.Actions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Maliev.ShadcnBlazor.Tests.Components.Actions;

public sealed class ButtonTests : BunitContext
{
    [Theory]
    [InlineData(ShadcnButtonVariant.Default, "default")]
    [InlineData(ShadcnButtonVariant.Destructive, "destructive")]
    [InlineData(ShadcnButtonVariant.Outline, "outline")]
    [InlineData(ShadcnButtonVariant.Secondary, "secondary")]
    [InlineData(ShadcnButtonVariant.Ghost, "ghost")]
    [InlineData(ShadcnButtonVariant.Link, "link")]
    public void RendersEveryVariant(ShadcnButtonVariant variant, string expected)
    {
        var cut = Render<ShadcnButton>(p => p.Add(x => x.Variant, variant).AddChildContent("Action"));

        Assert.Equal(expected, cut.Find("[data-slot='button']").GetAttribute("data-variant"));
    }

    [Theory]
    [InlineData(ShadcnButtonSize.Default, "default")]
    [InlineData(ShadcnButtonSize.ExtraSmall, "xs")]
    [InlineData(ShadcnButtonSize.Small, "sm")]
    [InlineData(ShadcnButtonSize.Large, "lg")]
    [InlineData(ShadcnButtonSize.Icon, "icon")]
    [InlineData(ShadcnButtonSize.IconExtraSmall, "icon-xs")]
    [InlineData(ShadcnButtonSize.IconSmall, "icon-sm")]
    [InlineData(ShadcnButtonSize.IconLarge, "icon-lg")]
    public void RendersEverySize(ShadcnButtonSize size, string expected)
    {
        var cut = Render<ShadcnButton>(p => p.Add(x => x.Size, size).AddChildContent("Action"));

        Assert.Equal(expected, cut.Find("[data-slot='button']").GetAttribute("data-size"));
    }

    [Theory]
    [InlineData(ShadcnButtonType.Button, "button")]
    [InlineData(ShadcnButtonType.Submit, "submit")]
    [InlineData(ShadcnButtonType.Reset, "reset")]
    public void NativeButtonProtectsOwnedAttributes(ShadcnButtonType type, string expected)
    {
        var cut = Render<ShadcnButton>(p => p
            .Add(x => x.ButtonType, type)
            .Add(x => x.Class, "consumer")
            .Add(x => x.AdditionalAttributes, new Dictionary<string, object>
            {
                ["type"] = "wrong",
                ["data-slot"] = "wrong",
                ["aria-label"] = "Save"
            })
            .AddChildContent("Action"));

        var button = cut.Find("button[data-slot='button']");
        Assert.Equal(expected, button.GetAttribute("type"));
        Assert.Equal("Save", button.GetAttribute("aria-label"));
        Assert.Contains("shadcn-button", button.ClassList);
        Assert.Contains("consumer", button.ClassList);
    }

    [Fact]
    public void RendersIconsAndInvokesEnabledButton()
    {
        var clicked = 0;
        var cut = Render<ShadcnButton>(p => p
            .Add(x => x.LeadingIcon, builder => builder.AddMarkupContent(0, "<svg data-icon='leading'></svg>"))
            .Add(x => x.TrailingIcon, builder => builder.AddMarkupContent(0, "<svg data-icon='trailing'></svg>"))
            .Add(x => x.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => clicked++))
            .AddChildContent("Save"));

        cut.Find("button").Click();

        Assert.Equal(1, clicked);
        Assert.NotNull(cut.Find("[data-slot='button-leading-icon'] svg"));
        Assert.NotNull(cut.Find("[data-slot='button-trailing-icon'] svg"));
    }

    [Fact]
    public void DisabledButtonAndLinkSuppressClicks()
    {
        var clicked = 0;
        var callback = EventCallback.Factory.Create<MouseEventArgs>(this, () => clicked++);
        var button = Render<ShadcnButton>(p => p.Add(x => x.Disabled, true).Add(x => x.OnClick, callback).AddChildContent("Button"));
        var link = Render<ShadcnButton>(p => p.Add(x => x.Href, "/target").Add(x => x.Disabled, true).Add(x => x.OnClick, callback).AddChildContent("Link"));

        Assert.True(button.Find("button").HasAttribute("disabled"));
        button.Find("button").Click();
        link.Find("a").Click();

        Assert.Equal(0, clicked);
        Assert.Equal("true", link.Find("a").GetAttribute("aria-disabled"));
        Assert.Equal("-1", link.Find("a").GetAttribute("tabindex"));
        Assert.Null(link.Find("a").GetAttribute("href"));
    }

    [Fact]
    public void EnabledLinkForwardsNavigationAndClick()
    {
        var clicked = 0;
        var cut = Render<ShadcnButton>(p => p
            .Add(x => x.Href, "/customers")
            .Add(x => x.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => clicked++))
            .Add(x => x.AdditionalAttributes, new Dictionary<string, object> { ["target"] = "_blank", ["href"] = "/wrong" })
            .AddChildContent("Customers"));

        cut.Find("a").Click();

        Assert.Equal(1, clicked);
        Assert.Equal("/customers", cut.Find("a").GetAttribute("href"));
        Assert.Equal("_blank", cut.Find("a").GetAttribute("target"));
    }

    [Fact]
    public void ButtonGroupCompositionUsesExpectedSemantics()
    {
        var cut = Render<ShadcnButtonGroup>(p => p
            .Add(x => x.Orientation, ShadcnButtonGroupOrientation.Vertical)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnButtonGroupText>(0);
                builder.AddAttribute(1, nameof(ShadcnButtonGroupText.ChildContent), (RenderFragment)(b => b.AddContent(0, "Total")));
                builder.CloseComponent();
                builder.OpenComponent<ShadcnButtonGroupSeparator>(2);
                builder.CloseComponent();
            }));

        var group = cut.Find("[data-slot='button-group']");
        Assert.Equal("group", group.GetAttribute("role"));
        Assert.Equal("vertical", group.GetAttribute("data-orientation"));
        Assert.Equal("Total", group.QuerySelector("[data-slot='button-group-text']")!.TextContent);
        Assert.Equal("vertical", group.QuerySelector("[data-slot='button-group-separator']")!.GetAttribute("aria-orientation"));
    }

    [Fact]
    public void RejectsUnknownEnums()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnButton>(p => p.Add(x => x.Variant, (ShadcnButtonVariant)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnButton>(p => p.Add(x => x.Size, (ShadcnButtonSize)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnButton>(p => p.Add(x => x.ButtonType, (ShadcnButtonType)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnButtonGroup>(p => p.Add(x => x.Orientation, (ShadcnButtonGroupOrientation)999)));
    }
}
