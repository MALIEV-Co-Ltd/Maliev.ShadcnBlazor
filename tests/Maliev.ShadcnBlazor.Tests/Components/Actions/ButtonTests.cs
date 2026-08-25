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
    public async Task SubmitFeedbackPreventsReentryAndTransitionsThroughBusyAndSuccessStates()
    {
        var calls = 0;
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cut = Render<ShadcnButton>(parameters => parameters
            .Add(component => component.ButtonType, ShadcnButtonType.Submit)
            .Add(component => component.BusyText, "Saving")
            .Add(component => component.SuccessText, "Saved")
            .Add(component => component.SuccessDuration, TimeSpan.FromMilliseconds(40))
            .Add(component => component.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, async () =>
            {
                calls++;
                await completion.Task;
            }))
            .AddChildContent("Save profile"));

        var firstClick = cut.Find("button").ClickAsync(new MouseEventArgs());
        cut.WaitForAssertion(() =>
        {
            Assert.Equal("busy", cut.Find("button").GetAttribute("data-operation-state"));
            Assert.Equal("true", cut.Find("button").GetAttribute("aria-busy"));
            Assert.Equal("Saving", cut.Find("button").TextContent.Trim());
            Assert.True(cut.Find("button").HasAttribute("disabled"));
        });

        cut.Find("button").Click();
        Assert.Equal(1, calls);
        completion.SetResult();
        cut.WaitForAssertion(() => Assert.Equal("Saved", cut.Find("button").TextContent.Trim()));
        await firstClick;
        cut.WaitForAssertion(() => Assert.Equal("Save profile", cut.Find("button").TextContent.Trim()), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void BadgeStylesUseDefaultCursorUnlessTheBadgeIsALink()
    {
        var root = FindRoot();
        var css = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-feedback-content.css"));

        Assert.Contains(".shadcn-badge {", css, StringComparison.Ordinal);
        Assert.Contains("cursor: default", css, StringComparison.Ordinal);
        Assert.Contains("a.shadcn-badge", css, StringComparison.Ordinal);
        Assert.Contains("cursor: pointer", css, StringComparison.Ordinal);
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
        var groupText = group.QuerySelector("[data-slot='button-group-text']")!;
        Assert.Equal("DIV", groupText.TagName);
        Assert.Equal("Total", groupText.TextContent);
        Assert.Equal("vertical", group.QuerySelector("[data-slot='button-group-separator']")!.GetAttribute("aria-orientation"));
    }

    [Fact]
    public void ButtonGroupStylesUseConnectedLogicalGeometry()
    {
        var root = FindRoot();
        var css = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-actions.css"));
        var groupRules = string.Join('\n', css.Split('\n').Where(line => line.Contains("shadcn-button-group[data-orientation", StringComparison.Ordinal)));

        Assert.Contains("border-start-end-radius: 0", groupRules, StringComparison.Ordinal);
        Assert.Contains("border-end-end-radius: 0", groupRules, StringComparison.Ordinal);
        Assert.Contains("border-inline-start-width: 0", groupRules, StringComparison.Ordinal);
        Assert.Contains("border-block-start-width: 0", groupRules, StringComparison.Ordinal);
        Assert.DoesNotContain("border-left-width", groupRules, StringComparison.Ordinal);
        Assert.DoesNotContain(":has(> [data-slot=\"button-group\"])", css, StringComparison.Ordinal);
    }

    [Fact]
    public void BusySpinnerUsesCalmComponentScopedTiming()
    {
        var root = FindRoot();
        var css = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-actions.css"));

        Assert.Contains("--shadcn-button-spinner-duration, var(--shadcn-motion-duration-slow)", css, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsUnknownEnums()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnButton>(p => p.Add(x => x.Variant, (ShadcnButtonVariant)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnButton>(p => p.Add(x => x.Size, (ShadcnButtonSize)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnButton>(p => p.Add(x => x.ButtonType, (ShadcnButtonType)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnButtonGroup>(p => p.Add(x => x.Orientation, (ShadcnButtonGroupOrientation)999)));
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
