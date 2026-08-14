using Bunit;
using Maliev.ShadcnBlazor.Components.Typography;

namespace Maliev.ShadcnBlazor.Tests.Components.SemanticFoundations;

public sealed class TypographyTests : BunitContext
{
    public static TheoryData<ShadcnTypographyVariant, string, string> VariantCases => new()
    {
        { ShadcnTypographyVariant.H1, "h1", "h1" },
        { ShadcnTypographyVariant.H2, "h2", "h2" },
        { ShadcnTypographyVariant.H3, "h3", "h3" },
        { ShadcnTypographyVariant.H4, "h4", "h4" },
        { ShadcnTypographyVariant.Paragraph, "p", "paragraph" },
        { ShadcnTypographyVariant.Blockquote, "blockquote", "blockquote" },
        { ShadcnTypographyVariant.InlineCode, "code", "inline-code" },
        { ShadcnTypographyVariant.Lead, "p", "lead" },
        { ShadcnTypographyVariant.Large, "div", "large" },
        { ShadcnTypographyVariant.Small, "small", "small" },
        { ShadcnTypographyVariant.Muted, "p", "muted" },
        { ShadcnTypographyVariant.UnorderedList, "ul", "unordered-list" },
        { ShadcnTypographyVariant.OrderedList, "ol", "ordered-list" }
    };

    [Theory]
    [MemberData(nameof(VariantCases))]
    public void TypographyVariantRendersNativeSemanticElement(
        ShadcnTypographyVariant variant,
        string expectedTag,
        string expectedValue)
    {
        var cut = Render<ShadcnTypography>(parameters => parameters
            .Add(x => x.Variant, variant)
            .Add(x => x.Class, "consumer-type")
            .Add(x => x.AdditionalAttributes, new Dictionary<string, object>
            {
                ["data-slot"] = "wrong",
                ["data-variant"] = "wrong",
                ["id"] = "type-fixture"
            })
            .AddChildContent("Typography content"));

        var element = cut.Find("[data-slot='typography']");
        Assert.Equal(expectedTag, element.TagName, ignoreCase: true);
        Assert.Equal(expectedValue, element.GetAttribute("data-variant"));
        Assert.Equal("type-fixture", element.Id);
        Assert.Contains("shadcn-typography", element.ClassList);
        Assert.Contains($"shadcn-typography--{expectedValue}", element.ClassList);
        Assert.Contains("consumer-type", element.ClassList);
        Assert.Equal("Typography content", element.TextContent);
    }

    [Fact]
    public void TypographyPreservesChildMarkupAndCallerStyle()
    {
        var cut = Render<ShadcnTypography>(parameters => parameters
            .Add(x => x.Variant, ShadcnTypographyVariant.Paragraph)
            .Add(x => x.Style, "max-inline-size: 65ch")
            .AddChildContent(builder => builder.AddMarkupContent(0, "Text <strong>with emphasis</strong>")));

        var element = cut.Find("p");
        Assert.Equal("max-inline-size: 65ch", element.GetAttribute("style"));
        Assert.Equal("with emphasis", element.QuerySelector("strong")!.TextContent);
    }

    [Fact]
    public void TypographyRejectsUnknownVariant()
    {
        var exception = Assert.ThrowsAny<Exception>(() => Render<ShadcnTypography>(parameters =>
            parameters.Add(x => x.Variant, (ShadcnTypographyVariant)999)));

        Assert.Contains("variant", exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(null, "div")]
    [InlineData("article", "article")]
    [InlineData("section", "section")]
    public void TypesetRendersAllowedContainer(string? tag, string expectedTag)
    {
        var cut = Render<ShadcnTypeset>(parameters =>
        {
            if (tag is not null)
                parameters.Add(x => x.Tag, tag);
            parameters.AddChildContent(builder => builder.AddMarkupContent(0, "<h2>Heading</h2><p>Body</p>"));
        });

        var element = cut.Find("[data-slot='typeset']");
        Assert.Equal(expectedTag, element.TagName, ignoreCase: true);
        Assert.Contains("shadcn-typeset", element.ClassList);
        Assert.Null(element.GetAttribute("style"));
        Assert.Equal("Heading", element.QuerySelector("h2")!.TextContent);
        Assert.Equal("Body", element.QuerySelector("p")!.TextContent);
    }

    [Fact]
    public void TypesetExposesRhythmAndMeasureVariables()
    {
        var cut = Render<ShadcnTypeset>(parameters => parameters
            .Add(x => x.Size, "15px")
            .Add(x => x.Leading, "1.7")
            .Add(x => x.Flow, "1.5em")
            .Add(x => x.MaxWidth, "72ch")
            .Add(x => x.Style, "text-wrap: pretty"));

        Assert.Equal(
            "--shadcn-typeset-size: 15px; --shadcn-typeset-leading: 1.7; --shadcn-typeset-flow: 1.5em; max-width: 72ch; text-wrap: pretty",
            cut.Find("[data-slot='typeset']").GetAttribute("style"));
    }

    [Theory]
    [InlineData("span")]
    [InlineData("main")]
    [InlineData("")]
    public void TypesetRejectsUnsupportedTags(string tag)
    {
        var exception = Assert.ThrowsAny<Exception>(() => Render<ShadcnTypeset>(parameters =>
            parameters.Add(x => x.Tag, tag)));

        Assert.Contains("tag", exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
