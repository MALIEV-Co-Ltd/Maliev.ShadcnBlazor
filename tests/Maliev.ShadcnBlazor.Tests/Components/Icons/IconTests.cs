using Bunit;
using Maliev.ShadcnBlazor.Components.Icons;

namespace Maliev.ShadcnBlazor.Tests.Components.Icons;

public sealed class IconTests : BunitContext
{
    private static readonly ShadcnIconData TestIcon = new(
        "test",
        "arrow",
        "0 0 24 24",
        "<path d=\"M4 12h16\" />");

    [Fact]
    public void DecorativeIconIsHiddenAndUsesCurrentColor()
    {
        var cut = Render<ShadcnIcon>(parameters => parameters
            .Add(component => component.Icon, TestIcon));

        var svg = cut.Find("svg");
        Assert.Equal("true", svg.GetAttribute("aria-hidden"));
        Assert.Null(svg.GetAttribute("role"));
        Assert.Equal("test", svg.GetAttribute("data-library"));
        Assert.Equal("arrow", svg.GetAttribute("data-icon"));
        Assert.Equal("currentColor", svg.GetAttribute("stroke"));
        Assert.Equal("false", svg.GetAttribute("focusable"));
    }

    [Fact]
    public void NamedIconUsesImageSemantics()
    {
        var cut = Render<ShadcnIcon>(parameters => parameters
            .Add(component => component.Icon, TestIcon)
            .Add(component => component.Label, "Change direction"));

        var svg = cut.Find("svg");
        Assert.Equal("img", svg.GetAttribute("role"));
        Assert.Equal("Change direction", svg.GetAttribute("aria-label"));
        Assert.Null(svg.GetAttribute("aria-hidden"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void NonPositiveOrNonFiniteSizeIsRejected(double size)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Render<ShadcnIcon>(parameters => parameters
            .Add(component => component.Icon, TestIcon)
            .Add(component => component.Size, size)));

        Assert.Equal("Size", exception.ParamName);
    }

    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("<path onload=\"alert(1)\" d=\"M0 0\" />")]
    [InlineData("<use href=\"https://example.invalid/icon.svg\" />")]
    [InlineData("plain text is not icon geometry")]
    public void UnsafeSvgContentIsRejected(string svgContent)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new ShadcnIconData("test", "unsafe", "0 0 24 24", svgContent));

        Assert.Equal("svgContent", exception.ParamName);
    }

    [Theory]
    [InlineData("0 0 0 24")]
    [InlineData("0 0 24")]
    [InlineData("0 0 NaN 24")]
    public void InvalidViewBoxIsRejectedWithItsPublicParameterName(string viewBox)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new ShadcnIconData("test", "unsafe", viewBox, "<path d=\"M0 0\" />"));

        Assert.Equal("viewBox", exception.ParamName);
    }

    [Fact]
    public void FreeCatalogPresentationAttributesRemainRenderable()
    {
        var icon = new ShadcnIconData(
            "test",
            "mitered-shape",
            "0 0 24 24",
            "<path d=\"M2 2h20v20z\" stroke=\"currentColor\" stroke-miterlimit=\"10\" />");

        Assert.Contains("stroke-miterlimit", icon.SvgContent, StringComparison.Ordinal);
    }
}
