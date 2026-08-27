using Bunit;
using Maliev.ShadcnBlazor.Components;
using Maliev.ShadcnBlazor.Components.Layout;

namespace Maliev.ShadcnBlazor.Tests.Components.Layout;

public sealed class BentoGridTests : BunitContext
{
    public BentoGridTests() => Services.AddMalievShadcn();

    [Fact]
    public void GridRendersAQueryContainerAndConfiguredTracks()
    {
        var cut = Render<ShadcnBentoGrid>(parameters => parameters
            .Add(component => component.Columns, 4)
            .Add(component => component.MediumColumns, 2)
            .Add(component => component.Gap, "1.25rem")
            .AddChildContent<ShadcnBentoItem>(item => item
                .Add(component => component.ColumnSpan, 2)
                .Add(component => component.RowSpan, 1)
                .AddChildContent("workflow")));

        var root = cut.Find("[data-slot='bento-grid']");
        var layout = cut.Find("[data-slot='bento-grid-layout']");
        var item = cut.Find("[data-slot='bento-item']");

        Assert.Contains("shadcn-bento-grid", root.ClassList);
        Assert.Contains("--shadcn-bento-columns: 4", root.GetAttribute("style"));
        Assert.Contains("--shadcn-bento-medium-columns: 2", root.GetAttribute("style"));
        Assert.Contains("--shadcn-bento-gap: 1.25rem", root.GetAttribute("style"));
        Assert.Contains("shadcn-bento-grid__layout", layout.ClassList);
        Assert.Equal("2", item.GetAttribute("data-column-span"));
        Assert.Equal("1", item.GetAttribute("data-row-span"));
        Assert.Equal("workflow", item.TextContent);
    }

    [Fact]
    public void GridCanOptIntoContentMeasuredMasonryPacking()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = Render<ShadcnBentoGrid>(parameters => parameters
            .Add(component => component.Masonry, true)
            .AddChildContent("workflow"));

        Assert.Equal("masonry", cut.Find("[data-slot='bento-grid']").GetAttribute("data-layout"));
    }

    [Fact]
    public void GridAndItemProtectOwnedAttributesWhileForwardingConsumerAttributes()
    {
        var cut = Render<ShadcnBentoGrid>(parameters => parameters
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object>
            {
                ["data-slot"] = "wrong",
                ["aria-label"] = "Production overview"
            })
            .AddChildContent<ShadcnBentoItem>(item => item
                .Add(component => component.ColumnSpan, 2)
                .Add(component => component.AdditionalAttributes, new Dictionary<string, object>
                {
                    ["data-slot"] = "wrong",
                    ["data-column-span"] = "4",
                    ["data-workflow"] = "capacity"
                })));

        Assert.Equal("bento-grid", cut.Find("[data-slot='bento-grid']").GetAttribute("data-slot"));
        Assert.Equal("Production overview", cut.Find("[data-slot='bento-grid']").GetAttribute("aria-label"));
        Assert.Equal("bento-item", cut.Find("[data-slot='bento-item']").GetAttribute("data-slot"));
        Assert.Equal("2", cut.Find("[data-slot='bento-item']").GetAttribute("data-column-span"));
        Assert.Equal("capacity", cut.Find("[data-slot='bento-item']").GetAttribute("data-workflow"));
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(5, 2)]
    [InlineData(4, 0)]
    [InlineData(4, 5)]
    public void GridRejectsColumnCountsOutsideTheSupportedRange(int columns, int mediumColumns)
    {
        var exception = Assert.ThrowsAny<Exception>(() => Render<ShadcnBentoGrid>(parameters => parameters
            .Add(component => component.Columns, columns)
            .Add(component => component.MediumColumns, mediumColumns)));

        Assert.Contains("column", exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(5, 1)]
    [InlineData(1, 0)]
    [InlineData(1, 5)]
    public void ItemRejectsSpansOutsideTheSupportedRange(int columnSpan, int rowSpan)
    {
        var exception = Assert.ThrowsAny<Exception>(() => Render<ShadcnBentoItem>(parameters => parameters
            .Add(component => component.ColumnSpan, columnSpan)
            .Add(component => component.RowSpan, rowSpan)));

        Assert.Contains("span", exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("")]
    [InlineData("normal")]
    [InlineData("1rem; color: red")]
    public void GridRejectsUnsafeGapValues(string gap)
    {
        var exception = Assert.ThrowsAny<Exception>(() => Render<ShadcnBentoGrid>(parameters => parameters
            .Add(component => component.Gap, gap)));

        Assert.Contains("gap", exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
