using Bunit;
using Maliev.ShadcnBlazor.Components.Content;
using Maliev.ShadcnBlazor.Components.Feedback;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Tests.Components.FeedbackContent;

public sealed class AlertBadgeCardTests : BunitContext
{
    [Theory]
    [InlineData(ShadcnAlertVariant.Default, "default")]
    [InlineData(ShadcnAlertVariant.Destructive, "destructive")]
    public void AlertRendersPinnedVariantsAndForwardsPresentation(ShadcnAlertVariant variant, string expected)
    {
        var cut = Render<ShadcnAlert>(parameters => parameters
            .Add(component => component.Variant, variant)
            .Add(component => component.Class, "consumer-layout")
            .Add(component => component.Style, "max-width: 30rem")
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object>
            {
                ["data-slot"] = "wrong",
                ["data-testid"] = "notice"
            })
            .AddChildContent("ชำระเงินสำเร็จ"));

        var alert = cut.Find("[data-slot='alert']");
        Assert.Equal(expected, alert.GetAttribute("data-variant"));
        Assert.Equal("alert", alert.GetAttribute("role"));
        Assert.Equal("notice", alert.GetAttribute("data-testid"));
        Assert.Contains("shadcn-alert", alert.ClassList);
        Assert.Contains("consumer-layout", alert.ClassList);
        Assert.Contains("max-width: 30rem", alert.GetAttribute("style"));
        Assert.Contains("ชำระเงินสำเร็จ", alert.TextContent);
    }

    [Theory]
    [InlineData(ShadcnAlertRole.Alert, "alert", "assertive")]
    [InlineData(ShadcnAlertRole.Status, "status", "polite")]
    [InlineData(ShadcnAlertRole.None, null, null)]
    public void AlertMakesAnnouncementUrgencyExplicit(
        ShadcnAlertRole alertRole,
        string? expectedRole,
        string? expectedLive)
    {
        var cut = Render<ShadcnAlert>(parameters => parameters
            .Add(component => component.AlertRole, alertRole)
            .AddChildContent("Update"));

        var alert = cut.Find("[data-slot='alert']");
        Assert.Equal(expectedRole, alert.GetAttribute("role"));
        Assert.Equal(expectedLive, alert.GetAttribute("aria-live"));
    }

    [Fact]
    public void AlertCompositionExposesIconTitleDescriptionAndActionSlots()
    {
        var cut = Render<ShadcnAlert>(parameters => parameters.AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnAlertIcon>(0);
            builder.AddAttribute(1, nameof(ShadcnAlertIcon.ChildContent), SvgIcon());
            builder.CloseComponent();
            builder.OpenComponent<ShadcnAlertTitle>(2);
            builder.AddAttribute(3, nameof(ShadcnAlertTitle.ChildContent), Text("Heads up"));
            builder.CloseComponent();
            builder.OpenComponent<ShadcnAlertDescription>(4);
            builder.AddAttribute(5, nameof(ShadcnAlertDescription.ChildContent), Text("Long localized description"));
            builder.CloseComponent();
            builder.OpenComponent<ShadcnAlertAction>(6);
            builder.AddAttribute(7, nameof(ShadcnAlertAction.ChildContent), Text("Enable"));
            builder.CloseComponent();
        }));

        Assert.NotNull(cut.Find("[data-slot='alert-icon'] svg[aria-hidden='true']"));
        Assert.Equal("Heads up", cut.Find("[data-slot='alert-title']").TextContent);
        Assert.Equal("Long localized description", cut.Find("[data-slot='alert-description']").TextContent);
        Assert.Equal("Enable", cut.Find("[data-slot='alert-action']").TextContent);
    }

    [Theory]
    [InlineData(ShadcnBadgeVariant.Default, "default")]
    [InlineData(ShadcnBadgeVariant.Secondary, "secondary")]
    [InlineData(ShadcnBadgeVariant.Destructive, "destructive")]
    [InlineData(ShadcnBadgeVariant.Outline, "outline")]
    [InlineData(ShadcnBadgeVariant.Ghost, "ghost")]
    [InlineData(ShadcnBadgeVariant.Link, "link")]
    public void BadgeRendersEveryPinnedVariant(ShadcnBadgeVariant variant, string expected)
    {
        var cut = Render<ShadcnBadge>(parameters => parameters
            .Add(component => component.Variant, variant)
            .AddChildContent("Ready"));

        Assert.Equal(expected, cut.Find("[data-slot='badge']").GetAttribute("data-variant"));
    }

    [Fact]
    public void BadgeUsesAnchorOnlyWhenHrefIsProvidedAndProtectsOwnedAttributes()
    {
        var inline = Render<ShadcnBadge>(parameters => parameters.AddChildContent("New"));
        var link = Render<ShadcnBadge>(parameters => parameters
            .Add(component => component.Href, "/orders/42")
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object>
            {
                ["href"] = "/wrong",
                ["data-slot"] = "wrong",
                ["aria-label"] = "Open order 42"
            })
            .AddChildContent("Order"));

        Assert.Equal("span", inline.Find("[data-slot='badge']").TagName, ignoreCase: true);
        var anchor = link.Find("a[data-slot='badge']");
        Assert.Equal("/orders/42", anchor.GetAttribute("href"));
        Assert.Equal("Open order 42", anchor.GetAttribute("aria-label"));
    }

    [Theory]
    [InlineData(ShadcnCardSize.Default, "default")]
    [InlineData(ShadcnCardSize.Small, "sm")]
    public void CardRendersSizeAndCustomSpacing(ShadcnCardSize size, string expected)
    {
        var cut = Render<ShadcnCard>(parameters => parameters
            .Add(component => component.Size, size)
            .Add(component => component.Spacing, "1.25rem")
            .AddChildContent("Card"));

        var card = cut.Find("[data-slot='card']");
        Assert.Equal(expected, card.GetAttribute("data-size"));
        Assert.Contains("--shadcn-card-spacing: 1.25rem", card.GetAttribute("style"));
    }

    [Fact]
    public void CardCompositionExposesEveryOfficialSlot()
    {
        var cut = Render<ShadcnCard>(parameters => parameters.AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnCardHeader>(0);
            builder.AddAttribute(1, nameof(ShadcnCardHeader.ChildContent), (RenderFragment)(header =>
            {
                header.OpenComponent<ShadcnCardTitle>(0);
                header.AddAttribute(1, nameof(ShadcnCardTitle.ChildContent), Text("Machine status"));
                header.CloseComponent();
                header.OpenComponent<ShadcnCardDescription>(2);
                header.AddAttribute(3, nameof(ShadcnCardDescription.ChildContent), Text("Updated now"));
                header.CloseComponent();
                header.OpenComponent<ShadcnCardAction>(4);
                header.AddAttribute(5, nameof(ShadcnCardAction.ChildContent), Text("Open"));
                header.CloseComponent();
            }));
            builder.CloseComponent();
            builder.OpenComponent<ShadcnCardContent>(2);
            builder.AddAttribute(3, nameof(ShadcnCardContent.ChildContent), Text("Running"));
            builder.CloseComponent();
            builder.OpenComponent<ShadcnCardFooter>(4);
            builder.AddAttribute(5, nameof(ShadcnCardFooter.ChildContent), Text("Footer"));
            builder.CloseComponent();
        }));

        Assert.Equal("Machine status", cut.Find("[data-slot='card-title']").TextContent);
        Assert.Equal("Updated now", cut.Find("[data-slot='card-description']").TextContent);
        Assert.Equal("Open", cut.Find("[data-slot='card-action']").TextContent);
        Assert.Equal("Running", cut.Find("[data-slot='card-content']").TextContent);
        Assert.Equal("Footer", cut.Find("[data-slot='card-footer']").TextContent);
    }

    [Fact]
    public void ComponentsRejectUnknownEnumsAndUnsafeSpacing()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnAlert>(parameters =>
            parameters.Add(component => component.Variant, (ShadcnAlertVariant)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnAlert>(parameters =>
            parameters.Add(component => component.AlertRole, (ShadcnAlertRole)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnBadge>(parameters =>
            parameters.Add(component => component.Variant, (ShadcnBadgeVariant)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnCard>(parameters =>
            parameters.Add(component => component.Size, (ShadcnCardSize)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnCard>(parameters =>
            parameters.Add(component => component.Spacing, "1rem; color: red")));
    }

    [Fact]
    public void PinnedStructuralSelectorsCoverDirectIconsLinksInvalidStateAndCardEdges()
    {
        var css = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-feedback-content.css"));

        Assert.Contains(".shadcn-alert:has(> svg)", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-alert-description a", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-alert-description p + p", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-alert-description p + p { margin-block-start: calc(1rem", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-alert-title a", css, StringComparison.Ordinal);
        Assert.Contains("[data-icon=\"inline-start\"]", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-badge[aria-invalid=\"true\"]", css, StringComparison.Ordinal);
        Assert.Contains("data-variant=\"outline\"]", css, StringComparison.Ordinal);
        Assert.Contains("data-variant=\"default\"]:hover { background: color-mix(in oklch, var(--shadcn-primary) 80%", css, StringComparison.Ordinal);
        Assert.Contains("data-variant=\"secondary\"]:hover { background: color-mix(in oklch, var(--shadcn-secondary) 80%, transparent); }", css, StringComparison.Ordinal);
        Assert.Contains("data-variant=\"outline\"]:hover { background: var(--shadcn-muted); color: var(--shadcn-muted-foreground); }", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-card > img:first-child", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-card-header.border-b", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-card-footer.border-t", css, StringComparison.Ordinal);
    }

    private static RenderFragment Text(string value) => builder => builder.AddContent(0, value);

    private static RenderFragment SvgIcon() => builder =>
        builder.AddMarkupContent(0, "<svg aria-hidden='true' viewBox='0 0 24 24'></svg>");
}
