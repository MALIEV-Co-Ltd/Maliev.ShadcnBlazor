using Bunit;
using Maliev.ShadcnBlazor.Components.Content;

namespace Maliev.ShadcnBlazor.Tests.Components.SemanticFoundations;

public sealed class ContentFoundationTests : BunitContext
{
    [Theory]
    [InlineData(ShadcnItemVariant.Default, ShadcnItemSize.Default, "default", "default")]
    [InlineData(ShadcnItemVariant.Outline, ShadcnItemSize.Small, "outline", "sm")]
    [InlineData(ShadcnItemVariant.Muted, ShadcnItemSize.Default, "muted", "default")]
    public void ItemRendersVariantsAndSizes(
        ShadcnItemVariant variant,
        ShadcnItemSize size,
        string variantValue,
        string sizeValue)
    {
        var cut = Render<ShadcnItem>(parameters => parameters
            .Add(x => x.Variant, variant)
            .Add(x => x.Size, size)
            .AddChildContent("Item"));

        var item = cut.Find("div[data-slot='item']");
        Assert.Equal(variantValue, item.GetAttribute("data-variant"));
        Assert.Equal(sizeValue, item.GetAttribute("data-size"));
        Assert.Equal("Item", item.TextContent);
    }

    [Fact]
    public void ItemUsesAnchorWhenHrefIsProvidedAndProtectsHref()
    {
        var cut = Render<ShadcnItem>(parameters => parameters
            .Add(x => x.Href, "/customers/42")
            .Add(x => x.AdditionalAttributes, new Dictionary<string, object>
            {
                ["href"] = "/wrong",
                ["target"] = "_blank",
                ["rel"] = "noreferrer"
            })
            .AddChildContent("Customer"));

        var item = cut.Find("a[data-slot='item']");
        Assert.Equal("/customers/42", item.GetAttribute("href"));
        Assert.Equal("_blank", item.GetAttribute("target"));
        Assert.Equal("noreferrer", item.GetAttribute("rel"));
    }

    [Fact]
    public void ItemGroupAndCompositionExposeExpectedSemanticsAndSlots()
    {
        var group = Render<ShadcnItemGroup>(parameters => parameters.AddChildContent("items"));
        var media = Render<ShadcnItemMedia>(parameters => parameters
            .Add(x => x.Variant, ShadcnItemMediaVariant.Icon)
            .AddChildContent("icon"));
        var content = Render<ShadcnItemContent>(parameters => parameters.AddChildContent("content"));
        var title = Render<ShadcnItemTitle>(parameters => parameters.AddChildContent("title"));
        var description = Render<ShadcnItemDescription>(parameters => parameters.AddChildContent("description"));
        var actions = Render<ShadcnItemActions>(parameters => parameters.AddChildContent("actions"));
        var header = Render<ShadcnItemHeader>(parameters => parameters.AddChildContent("header"));
        var footer = Render<ShadcnItemFooter>(parameters => parameters.AddChildContent("footer"));

        Assert.Equal("list", group.Find("[data-slot='item-group']").GetAttribute("role"));
        Assert.Equal("icon", media.Find("[data-slot='item-media']").GetAttribute("data-variant"));
        Assert.Equal("content", content.Find("[data-slot='item-content']").TextContent);
        Assert.Equal("title", title.Find("[data-slot='item-title']").TextContent);
        Assert.Equal("p", description.Find("[data-slot='item-description']").TagName, ignoreCase: true);
        Assert.Equal("actions", actions.Find("[data-slot='item-actions']").TextContent);
        Assert.Equal("header", header.Find("[data-slot='item-header']").TextContent);
        Assert.Equal("footer", footer.Find("[data-slot='item-footer']").TextContent);
    }

    [Fact]
    public void ItemSeparatorIsHorizontalAndSemantic()
    {
        var cut = Render<ShadcnItemSeparator>();
        var separator = cut.Find("[data-slot='item-separator'] [data-slot='separator']");

        Assert.Equal("separator", separator.GetAttribute("role"));
        Assert.Equal("horizontal", separator.GetAttribute("aria-orientation"));
    }

    [Fact]
    public void KbdAndKbdGroupUseNativeKeyboardSemantics()
    {
        var key = Render<ShadcnKbd>(parameters => parameters.AddChildContent("Ctrl"));
        var group = Render<ShadcnKbdGroup>(parameters => parameters
            .AddUnmatched("role", "presentation")
            .AddUnmatched("data-slot", "consumer-slot")
            .AddChildContent("Ctrl K"));

        Assert.Equal("kbd", key.Find("[data-slot='kbd']").TagName, ignoreCase: true);
        var groupElement = group.Find("[data-slot='kbd-group']");
        Assert.Equal("kbd", groupElement.TagName, ignoreCase: true);
        Assert.Null(groupElement.GetAttribute("role"));
        Assert.DoesNotContain("consumer-slot", group.Markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(ShadcnSeparatorOrientation.Horizontal, true, "none", null)]
    [InlineData(ShadcnSeparatorOrientation.Horizontal, false, "separator", "horizontal")]
    [InlineData(ShadcnSeparatorOrientation.Vertical, false, "separator", "vertical")]
    public void SeparatorSupportsDecorativeAndSemanticModes(
        ShadcnSeparatorOrientation orientation,
        bool decorative,
        string expectedRole,
        string? expectedOrientation)
    {
        var cut = Render<ShadcnSeparator>(parameters => parameters
            .Add(x => x.Orientation, orientation)
            .Add(x => x.Decorative, decorative));

        var separator = cut.Find("[data-slot='separator']");
        Assert.Equal(expectedRole, separator.GetAttribute("role"));
        Assert.Equal(expectedOrientation, separator.GetAttribute("aria-orientation"));
        Assert.Equal(decorative ? "true" : null, separator.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void SeparatorProtectsOwnedSemanticsAndPreservesAnAccessibleName()
    {
        var cut = Render<ShadcnSeparator>(parameters => parameters
            .Add(x => x.Orientation, ShadcnSeparatorOrientation.Vertical)
            .Add(x => x.Decorative, false)
            .Add(x => x.AdditionalAttributes, new Dictionary<string, object>
            {
                ["role"] = "presentation",
                ["aria-hidden"] = "true",
                ["aria-orientation"] = "horizontal",
                ["data-orientation"] = "horizontal",
                ["aria-label"] = "Quotation sections"
            }));

        var separator = cut.Find("[data-slot='separator']");
        Assert.Equal("separator", separator.GetAttribute("role"));
        Assert.Equal("vertical", separator.GetAttribute("aria-orientation"));
        Assert.Equal("vertical", separator.GetAttribute("data-orientation"));
        Assert.False(separator.HasAttribute("aria-hidden"));
        Assert.Equal("Quotation sections", separator.GetAttribute("aria-label"));
    }

    [Fact]
    public void SeparatorStylesMatchPinnedGeometryAndForcedColors()
    {
        var root = FindRoot();
        var css = File.ReadAllText(Path.Combine(
            root,
            "src",
            "Maliev.ShadcnBlazor",
            "wwwroot",
            "css",
            "shadcn-semantic-foundations.css"));

        Assert.Contains(".shadcn-separator[data-orientation=\"horizontal\"]", css, StringComparison.Ordinal);
        Assert.Contains("inline-size: 100%;", css, StringComparison.Ordinal);
        Assert.Contains("block-size: 1px;", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-separator[data-orientation=\"vertical\"]", css, StringComparison.Ordinal);
        Assert.Contains("inline-size: 1px;", css, StringComparison.Ordinal);
        Assert.Contains("block-size: 100%;", css, StringComparison.Ordinal);
        Assert.Contains("@media (forced-colors: active)", css, StringComparison.Ordinal);
        Assert.Contains("background: CanvasText;", css, StringComparison.Ordinal);
    }

    [Fact]
    public void EmptyCompositionExposesAllSlotsAndIconVariant()
    {
        var root = Render<ShadcnEmpty>(parameters => parameters.AddChildContent("empty"));
        var header = Render<ShadcnEmptyHeader>(parameters => parameters.AddChildContent("header"));
        var media = Render<ShadcnEmptyMedia>(parameters => parameters
            .Add(x => x.Variant, ShadcnEmptyMediaVariant.Icon)
            .AddChildContent("media"));
        var title = Render<ShadcnEmptyTitle>(parameters => parameters.AddChildContent("title"));
        var description = Render<ShadcnEmptyDescription>(parameters => parameters.AddChildContent("description"));
        var content = Render<ShadcnEmptyContent>(parameters => parameters.AddChildContent("content"));

        Assert.Equal("empty", root.Find("[data-slot='empty']").TextContent);
        Assert.Equal("header", header.Find("[data-slot='empty-header']").TextContent);
        Assert.Equal("icon", media.Find("[data-slot='empty-icon']").GetAttribute("data-variant"));
        Assert.Equal("title", title.Find("[data-slot='empty-title']").TextContent);
        Assert.Equal("div", description.Find("[data-slot='empty-description']").TagName, ignoreCase: true);
        Assert.Equal("content", content.Find("[data-slot='empty-content']").TextContent);
    }

    [Fact]
    public void ContentComponentsRejectUnknownEnums()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnItem>(parameters =>
            parameters.Add(x => x.Variant, (ShadcnItemVariant)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnItem>(parameters =>
            parameters.Add(x => x.Size, (ShadcnItemSize)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnItemMedia>(parameters =>
            parameters.Add(x => x.Variant, (ShadcnItemMediaVariant)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSeparator>(parameters =>
            parameters.Add(x => x.Orientation, (ShadcnSeparatorOrientation)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnEmptyMedia>(parameters =>
            parameters.Add(x => x.Variant, (ShadcnEmptyMediaVariant)999)));
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
