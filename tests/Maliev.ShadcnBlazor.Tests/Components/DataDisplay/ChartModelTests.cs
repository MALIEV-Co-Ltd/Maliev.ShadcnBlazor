using Maliev.ShadcnBlazor.Components.DataDisplay;

namespace Maliev.ShadcnBlazor.Tests.Components.DataDisplay;

public sealed class ChartModelTests
{
    [Fact]
    public void ConfigProducesSafeScopedLightAndDarkVariables()
    {
        var config = new ShadcnChartConfig
        {
            ["desktop"] = new("Desktop") { Color = "var(--shadcn-chart-1)" },
            ["mobile"] = new("Mobile") { Theme = new("#2563eb", "oklch(0.7 0.2 240)") }
        };

        var css = config.ToScopedCss("chart-sales");
        Assert.Contains("[data-chart=\"chart-sales\"]", css, StringComparison.Ordinal);
        Assert.Contains("--color-desktop: var(--shadcn-chart-1)", css, StringComparison.Ordinal);
        Assert.Contains(".dark [data-chart=\"chart-sales\"]", css, StringComparison.Ordinal);
        Assert.Contains("--color-mobile: oklch(0.7 0.2 240)", css, StringComparison.Ordinal);
    }

    [Fact]
    public void ConfigRejectsUnsafeKeysColorsAndConflictingColorModes()
    {
        Assert.Throws<ArgumentException>(() => new ShadcnChartConfig { ["bad key"] = new("Bad") { Color = "red" } }.Validate());
        Assert.Throws<ArgumentException>(() => new ShadcnChartConfig { ["bad"] = new("Bad") { Color = "red;display:none" } }.Validate());
        Assert.Throws<ArgumentException>(() => new ShadcnChartConfig { ["bad"] = new("Bad") { Color = "red", Theme = new("red", "blue") } }.Validate());
    }

    [Theory]
    [InlineData(ShadcnChartType.Bar)]
    [InlineData(ShadcnChartType.Line)]
    [InlineData(ShadcnChartType.Area)]
    [InlineData(ShadcnChartType.Donut)]
    public void GeometryIsFiniteForEveryCertifiedChartType(ShadcnChartType type)
    {
        var series = new[]
        {
            new ShadcnChartSeries("desktop", [10, -5, 20]),
            new ShadcnChartSeries("mobile", [4, null, 12])
        };
        var geometry = ShadcnChartGeometry.Create(type, ["Jan", "Feb", "Mar"], series, 320, 200, stacked: type == ShadcnChartType.Area);

        Assert.NotEmpty(geometry.Shapes);
        Assert.All(geometry.Shapes, shape => Assert.All(shape.Values, value => Assert.True(double.IsFinite(value))));
        Assert.Equal(320, geometry.Width);
        Assert.Equal(200, geometry.Height);
    }

    [Fact]
    public void GeometryRejectsMismatchedDuplicateAndNonFiniteData()
    {
        Assert.Throws<ArgumentException>(() => ShadcnChartGeometry.Create(ShadcnChartType.Bar, ["Jan"], [new("a", [1, 2])], 320, 200));
        Assert.Throws<ArgumentException>(() => ShadcnChartGeometry.Create(ShadcnChartType.Bar, ["Jan"], [new("a", [1]), new("a", [2])], 320, 200));
        Assert.Throws<ArgumentException>(() => ShadcnChartGeometry.Create(ShadcnChartType.Bar, ["Jan"], [new("a", [double.NaN])], 320, 200));
        Assert.Throws<ArgumentOutOfRangeException>(() => ShadcnChartGeometry.Create(ShadcnChartType.Bar, ["Jan"], [new("a", [1])], 0, 200));
    }
}
