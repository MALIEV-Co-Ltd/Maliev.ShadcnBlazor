using Bunit;
using Maliev.ShadcnBlazor.Components.DataDisplay;
using Maliev.ShadcnBlazor.Theming;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.ShadcnBlazor.Tests.Components.DataDisplay;

public sealed class ChartTests : BunitContext
{
    public ChartTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddScoped<IShadcnIdAllocator, ShadcnIdAllocator>();
    }

    private static readonly ShadcnChartConfig Config = new()
    {
        ["desktop"] = new("เดสก์ท็อป") { Color = "var(--shadcn-chart-1)" },
        ["mobile"] = new("มือถือ") { Theme = new("#2563eb", "#60a5fa") }
    };
    private static readonly ShadcnChartSeries[] Series =
    [new("desktop", [186, 305, 237]), new("mobile", [80, 200, 120])];

    [Fact]
    public void ChartHasDeterministicSsrSvgAccessibleNameDescriptionAndDataTable()
    {
        var cut = RenderChart();
        var figure = cut.Find("figure[data-slot='chart']");
        Assert.Equal("chart-sales", figure.GetAttribute("data-chart"));
        Assert.NotNull(cut.Find("svg[data-slot='chart-surface'][viewBox='0 0 320 200']"));
        Assert.Equal("none", cut.Find("svg[data-slot='chart-surface']").GetAttribute("preserveAspectRatio"));
        Assert.Equal("ยอดผู้เข้าชม", cut.Find("[data-slot='chart-title']").TextContent);
        Assert.Equal("สามเดือนล่าสุด", cut.Find("[data-slot='chart-description']").TextContent);
        Assert.Equal(6, cut.FindAll("[data-slot='chart-accessible-value']").Count);
        Assert.Contains("--color-desktop", cut.Find("style[data-slot='chart-style']").TextContent);
        Assert.Contains("[data-shadcn-theme=\"dark\"] [data-chart=\"chart-sales\"]", cut.Find("style[data-slot='chart-style']").TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void BarGeometryUsesSquareCornersByDefault()
    {
        var cut = RenderChart();

        Assert.All(cut.FindAll("rect[data-series]"), bar => Assert.Equal("0", bar.GetAttribute("rx")));
    }

    [Theory]
    [InlineData(ShadcnChartIndicator.Dot, "dot")]
    [InlineData(ShadcnChartIndicator.Line, "line")]
    [InlineData(ShadcnChartIndicator.Dashed, "dashed")]
    public void KeyboardTraversalSynchronizesCustomizableTooltip(ShadcnChartIndicator indicator, string expected)
    {
        var cut = RenderChart(indicator: indicator);
        var surface = cut.Find("svg[data-slot='chart-surface']");
        surface.KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        surface.KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        var tooltip = cut.Find("[data-slot='chart-tooltip-content']");
        Assert.Equal(expected, tooltip.GetAttribute("data-indicator"));
        Assert.Contains("Feb", tooltip.TextContent);
        Assert.Contains("305", tooltip.TextContent);
        surface.KeyDown(new KeyboardEventArgs { Key = "End" });
        Assert.Contains("Mar", cut.Find("[data-slot='chart-tooltip-content']").TextContent);
        surface.KeyDown(new KeyboardEventArgs { Key = "Escape" });
        Assert.Empty(cut.FindAll("[data-slot='chart-tooltip-content']"));
    }

    [Fact]
    public void TooltipAndLegendHonorHideAndNameOptions()
    {
        var cut = RenderChart(hideLabel: true, hideIndicator: true, nameKey: "mobile");
        cut.Find("svg").KeyDown(new KeyboardEventArgs { Key = "Home" });
        var tooltip = cut.Find("[data-slot='chart-tooltip-content']");
        Assert.Null(tooltip.QuerySelector("[data-slot='chart-tooltip-label']"));
        Assert.Null(tooltip.QuerySelector("[data-slot='chart-tooltip-indicator']"));
        Assert.Contains("มือถือ", tooltip.TextContent);
        Assert.Equal(2, cut.FindAll("[data-slot='chart-legend-item']").Count);
    }

    [Fact]
    public void LoadingEmptyAndErrorAreExplicit()
    {
        var loading = RenderChart(loading: true);
        Assert.Equal("status", loading.Find("[data-slot='chart-loading']").GetAttribute("role"));
        var error = RenderChart(error: "โหลดแผนภูมิไม่สำเร็จ");
        Assert.Equal("alert", error.Find("[data-slot='chart-error']").GetAttribute("role"));
        var empty = Render<ShadcnChart>(parameters => parameters
            .Add(component => component.Config, Config)
            .Add(component => component.Categories, new[] { "Jan" })
            .Add(component => component.Series, new[] { new ShadcnChartSeries("desktop", new double?[] { null }) })
            .Add(component => component.Title, "Empty"));
        Assert.Equal("No chart data.", empty.Find("[data-slot='chart-empty']").TextContent);
    }

    [Fact]
    public void ResizeInteropContractIsSsrSafeAndDisposable()
    {
        var source = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Maliev.ShadcnBlazor", "wwwroot", "js", "shadcn-chart.js"));
        Assert.Contains("ResizeObserver", source, StringComparison.Ordinal);
        Assert.Contains("disconnect", source, StringComparison.Ordinal);
        Assert.Contains("isConnected", source, StringComparison.Ordinal);
        Assert.Contains("requestAnimationFrame", source, StringComparison.Ordinal);
        Assert.Contains("invokeMethodAsync", source, StringComparison.Ordinal);
        Assert.Contains("OnChartResize", source, StringComparison.Ordinal);
        Assert.Contains("[data-slot=\"chart-surface\"]", source, StringComparison.Ordinal);
        Assert.Contains("(surface ?? element).getBoundingClientRect()", source, StringComparison.Ordinal);
        Assert.Contains("observer.observe(surface ?? element)", source, StringComparison.Ordinal);
    }

    [Fact]
    public void GeneratedIdsAreStablePerInstanceUniqueAcrossEquivalentInstancesAndDescriptionIdrefIsConditional()
    {
        var first = RenderChart(id: null, description: null);
        var second = RenderChart(id: null, description: null);
        var firstId = first.Find("figure").GetAttribute("data-chart");
        Assert.NotEqual(firstId, second.Find("figure").GetAttribute("data-chart"));
        first.Render(parameters => parameters.Add(component => component.Title, "ยอดผู้เข้าชม"));
        Assert.Equal(firstId, first.Find("figure").GetAttribute("data-chart"));
        Assert.Null(first.Find("figure").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void ScopedAllocatorReplaysPrerenderIdsAndKeepsSiblingSelectorsUnique()
    {
        var prerender = RenderChartPair();
        var interactive = RenderChartPair();
        Assert.Equal(prerender, interactive);
        Assert.Equal(2, prerender.Select(item => item.Chart).Distinct(StringComparer.Ordinal).Count());
        Assert.All(prerender, item =>
        {
            Assert.Equal($"{item.Chart}-title", item.Title);
            Assert.Equal($"{item.Chart}-description", item.Description);
            Assert.Contains($"[data-chart=\"{item.Chart}\"]", item.Style, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void ExplicitIdCanTransitionToStableFallbackAndBackWithoutBreakingSelectors()
    {
        var cut = RenderChart(id: "sales");
        Assert.Equal("chart-sales", cut.Find("figure").GetAttribute("data-chart"));
        cut.Render(parameters => parameters.Add(component => component.Id, (string?)null));
        var fallback = cut.Find("figure").GetAttribute("data-chart")!;
        Assert.StartsWith("chart-", fallback, StringComparison.Ordinal);
        Assert.NotEqual("chart-sales", fallback);
        Assert.Equal($"{fallback}-title", cut.Find("figure").GetAttribute("aria-labelledby"));
        Assert.Contains($"[data-chart=\"{fallback}\"]", cut.Find("style[data-slot='chart-style']").TextContent, StringComparison.Ordinal);
        cut.Render(parameters => parameters.Add(component => component.Id, (string?)null));
        Assert.Equal(fallback, cut.Find("figure").GetAttribute("data-chart"));
        cut.Render(parameters => parameters.Add(component => component.Id, "sales"));
        Assert.Equal("chart-sales", cut.Find("figure").GetAttribute("data-chart"));
    }

    [Fact]
    public void HiddenSeriesAreExcludedFromGeometryLegendAndAccessibleTable()
    {
        var hidden = new[] { Series[0], Series[1] with { Visible = false } };
        var cut = RenderChart(series: hidden);
        Assert.Empty(cut.FindAll("[data-series='mobile']"));
        Assert.Single(cut.FindAll("[data-slot='chart-legend-item']"));
        Assert.Equal(3, cut.FindAll("[data-slot='chart-accessible-value']").Count);
    }

    [Fact]
    public async Task ResizeUpdatesViewBoxAndRecomputedGeometry()
    {
        var cut = RenderChart();
        var before = cut.Find("rect[data-series='desktop']").GetAttribute("x");
        await cut.Instance.OnChartResize(640, 300);
        Assert.Equal("0 0 640 300", cut.Find("svg").GetAttribute("viewBox"));
        Assert.NotEqual(before, cut.Find("rect[data-series='desktop']").GetAttribute("x"));
    }

    [Fact]
    public void AreaGeometryCreatesSeparateNullGapsAndStackedPositiveNegativeBands()
    {
        var gap = ShadcnChartGeometry.Create(ShadcnChartType.Area, ["A", "B", "C", "D"], [new("desktop", [2, null, 4, 5])], 320, 200);
        Assert.Equal(2, gap.Shapes.Count(shape => shape.Kind == "area"));
        Assert.All(gap.Shapes.SelectMany(shape => shape.Values), value => Assert.True(double.IsFinite(value)));

        var stacked = ShadcnChartGeometry.Create(ShadcnChartType.Area, ["A", "B"], [new("desktop", [4, -3]), new("mobile", [2, -2])], 320, 200, stacked: true);
        var bands = stacked.Shapes.Where(shape => shape.Kind == "area").ToArray();
        Assert.Equal(2, bands.Length);
        Assert.NotEqual(bands[0].Values, bands[1].Values);
        Assert.True(stacked.Minimum <= -5);
        Assert.True(stacked.Maximum >= 6);
    }

    [Fact]
    public void LineAndAreaUseTheFullCartesianPlotWidthLikeThePinnedScale()
    {
        var geometry = ShadcnChartGeometry.Create(ShadcnChartType.Area, ["Jan", "Feb", "Mar"], [new("desktop", [1, 2, 1])], 656, 369);
        var points = Assert.Single(geometry.Shapes, shape => shape.Kind == "area").Values;
        Assert.Equal(36, points[0], 3);
        Assert.Equal(644, points[4], 3);
    }

    [Theory]
    [InlineData(ShadcnChartType.Pie)]
    [InlineData(ShadcnChartType.Donut)]
    public void RadialGeometryIncludesEveryPositiveCategoryValue(ShadcnChartType type)
    {
        var geometry = ShadcnChartGeometry.Create(type, ["Jan", "Feb", "Mar"], [new("desktop", [10, 20, 30])], 320, 200);
        var arcs = geometry.Shapes.Where(shape => shape.Kind == "arc").ToArray();
        Assert.Equal(3, arcs.Length);
        Assert.Equal([0, 1, 2], arcs.Select(shape => shape.PointIndex));
    }

    [Fact]
    public void DonutUsesRadialCompositionWithoutCartesianChromeAndListsEverySlice()
    {
        var cut = Render<ShadcnChart>(parameters => parameters
            .Add(component => component.Id, "donut")
            .Add(component => component.Type, ShadcnChartType.Donut)
            .Add(component => component.Config, Config)
            .Add(component => component.Categories, new[] { "Jan", "Feb", "Mar" })
            .Add(component => component.Series, Series)
            .Add(component => component.Title, "Visitors")
            .Add(component => component.ShowGrid, true)
            .Add(component => component.ShowTicks, true)
            .Add(component => component.ShowLegend, true));
        Assert.Empty(cut.FindAll("[data-slot='chart-grid-line']"));
        Assert.Empty(cut.FindAll("[data-slot='chart-tick']"));
        Assert.Equal(6, cut.FindAll("[data-slot='chart-legend-item']").Count);
        Assert.Contains("Jan เดสก์ท็อป", cut.Find("[data-slot='chart-legend']").TextContent);
        Assert.Contains("Mar มือถือ", cut.Find("[data-slot='chart-legend']").TextContent);
        Assert.Equal("xMidYMid meet", cut.Find("svg[data-slot='chart-surface']").GetAttribute("preserveAspectRatio"));
    }

    [Fact]
    public void RadialCategoriesCanOwnDistinctThemeColors()
    {
        var config = new ShadcnChartConfig
        {
            ["orders"] = new("Scheduled jobs") { Color = "red" },
            ["orders-0"] = new("Aluminum") { Color = "blue" },
            ["orders-1"] = new("Stainless") { Color = "green" },
            ["orders-2"] = new("Polymer") { Color = "orange" }
        };
        var cut = Render<ShadcnChart>(parameters => parameters
            .Add(component => component.Type, ShadcnChartType.Donut)
            .Add(component => component.Config, config)
            .Add(component => component.Categories, new[] { "Aluminum", "Stainless", "Polymer" })
            .Add(component => component.Series, new[] { new ShadcnChartSeries("orders", new double?[] { 12, 8, 5 }) })
            .Add(component => component.Title, "Order mix")
            .Add(component => component.ShowLegend, true));

        Assert.Equal(new[] { "var(--color-orders-0)", "var(--color-orders-1)", "var(--color-orders-2)" },
            cut.FindAll("path[data-series='orders']").Select(path => path.GetAttribute("fill")));
        Assert.Equal(3, cut.FindAll("[data-slot='chart-legend-item']").Count);
    }

    [Fact]
    public void PointerAndPerBarFocusExposeVisibleActiveTargetWithoutSyntheticMarker()
    {
        var cut = RenderChart();
        var bar = cut.Find("rect[data-series='desktop'][data-point='1']");
        Assert.Equal("0", bar.GetAttribute("tabindex"));
        Assert.Contains("Feb", bar.GetAttribute("aria-label"));
        bar.PointerEnter(new PointerEventArgs());
        Assert.Empty(cut.FindAll("[data-slot='chart-point']"));
        Assert.Contains("Feb", cut.Find("[data-slot='chart-tooltip-content']").TextContent);
        Assert.NotNull(cut.Find("[data-slot='chart-tooltip-cursor']"));
        cut.Find("figure").MouseLeave(new MouseEventArgs());
        Assert.Empty(cut.FindAll("[data-slot='chart-tooltip-content']"));
    }

    [Fact]
    public void PointerTooltipIsAnchoredToTheActiveDatum()
    {
        var cut = RenderChart();
        var firstBar = cut.Find("rect[data-series='desktop'][data-point='0']");

        firstBar.MouseEnter();

        var tooltip = cut.Find("[data-slot='chart-tooltip-content']");
        Assert.Equal("0", tooltip.GetAttribute("data-active-point"));
        Assert.Contains("--shadcn-chart-tooltip-x:", tooltip.GetAttribute("style"), StringComparison.Ordinal);
        Assert.Contains("--shadcn-chart-tooltip-y:", tooltip.GetAttribute("style"), StringComparison.Ordinal);
        Assert.Contains("Jan", tooltip.TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void BarHoverDirectlyExposesTheCategoryAndSeriesValues()
    {
        var cut = RenderChart();
        var bar = cut.Find("rect[data-series='desktop'][data-point='1']");

        bar.PointerEnter(new PointerEventArgs());

        var tooltip = cut.Find("[data-slot='chart-tooltip-content']");
        Assert.Contains("Feb", tooltip.TextContent);
        Assert.Contains("305", tooltip.TextContent);
        cut.Find("figure").MouseLeave(new MouseEventArgs());
        Assert.Empty(cut.FindAll("[data-slot='chart-tooltip-content']"));
    }

    [Fact]
    public void BarChartsDoNotRenderSyntheticPointMarkers()
    {
        var cut = RenderChart();

        Assert.Empty(cut.FindAll("[data-slot='chart-point']"));
    }

    [Fact]
    public void ActiveDonutSliceMovesOutwardWithoutRenderingAPointMarker()
    {
        var cut = Render<ShadcnChart>(parameters => parameters
            .Add(component => component.Type, ShadcnChartType.Donut)
            .Add(component => component.Config, Config)
            .Add(component => component.Categories, new[] { "Jan", "Feb", "Mar" })
            .Add(component => component.Series, new[] { Series[0] })
            .Add(component => component.Title, "Visitors"));

        var slice = cut.Find("path[data-point='1']");
        slice.PointerEnter(new PointerEventArgs());

        Assert.Equal("true", cut.Find("path[data-point='1']").GetAttribute("data-active"));
        Assert.Contains("translate(", cut.Find("path[data-point='1']").GetAttribute("transform"), StringComparison.Ordinal);
        Assert.Empty(cut.FindAll("[data-slot='chart-point']"));
    }

    [Fact]
    public void PointerMovementAcrossThePlotExposesTheHoveredCategory()
    {
        var cut = RenderChart();
        cut.Find("svg").MouseMove(new MouseEventArgs { OffsetX = 90 });

        Assert.Contains("Jan", cut.Find("[data-slot='chart-tooltip-content']").TextContent);
    }

    [Fact]
    public void RtlKeyboardTraversalUsesLogicalHorizontalDirection()
    {
        var cut = RenderChart(direction: ShadcnDirection.RightToLeft);
        var surface = cut.Find("svg");
        surface.KeyDown(new KeyboardEventArgs { Key = "Home" });
        surface.KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        Assert.Contains("Mar", cut.Find("[data-slot='chart-tooltip-content']").TextContent);
        Assert.DoesNotContain("role=\"application\"", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void GridTicksLegendPlacementIconsAndAnimationAreExplicit()
    {
        var iconConfig = new ShadcnChartConfig
        {
            ["desktop"] = Config["desktop"] with { Icon = builder => builder.AddMarkupContent(0, "<svg data-testid='desktop-icon'></svg>") },
            ["mobile"] = Config["mobile"]
        };
        var cut = Render<ShadcnChart>(parameters => parameters
            .Add(component => component.Id, "details")
            .Add(component => component.Config, iconConfig)
            .Add(component => component.Categories, new[] { "Jan", "Feb", "Mar" })
            .Add(component => component.Series, Series)
            .Add(component => component.Title, "Details")
            .Add(component => component.ShowGrid, true)
            .Add(component => component.ShowTicks, true)
            .Add(component => component.LegendPlacement, ShadcnChartLegendPlacement.Top)
            .Add(component => component.LegendInteractive, true)
            .Add(component => component.Animated, false));
        Assert.NotEmpty(cut.FindAll("[data-slot='chart-grid-line']"));
        Assert.Equal(3, cut.FindAll("[data-slot='chart-tick']").Count);
        Assert.True(cut.Find("[data-slot='chart-legend']").PreviousElementSibling?.Matches("[data-slot='chart-description'], [data-slot='chart-title']") ?? false);
        Assert.NotNull(cut.Find("[data-testid='desktop-icon']"));
        Assert.Equal("false", cut.Find("figure").GetAttribute("data-animated"));
        cut.Find("button[data-legend-series='mobile']").Click();
        Assert.Empty(cut.FindAll("[data-series='mobile']"));
    }

    [Fact]
    public void CartesianAxesAndMajorMinorGridLinesAreIndependentlyConfigurableAndLogical()
    {
        var cut = Render<ShadcnChart>(parameters => parameters
            .Add(component => component.Id, "axes")
            .Add(component => component.Config, Config)
            .Add(component => component.Categories, new[] { "Jan", "Feb", "Mar" })
            .Add(component => component.Series, Series)
            .Add(component => component.Title, "Axes")
            .Add(component => component.ShowAxis, false)
            .Add(component => component.ShowPrimaryYAxis, true)
            .Add(component => component.ShowSecondaryYAxis, false)
            .Add(component => component.ShowMajorGrid, true)
            .Add(component => component.ShowMinorGrid, true));

        Assert.Single(cut.FindAll("[data-slot='chart-axis'][data-axis='primary']"));
        Assert.Empty(cut.FindAll("[data-slot='chart-axis'][data-axis='secondary']"));
        Assert.Equal(5, cut.FindAll("[data-slot='chart-grid-line'][data-grid-level='major']").Count);
        Assert.Equal(4, cut.FindAll("[data-slot='chart-grid-line'][data-grid-level='minor']").Count);
        Assert.Equal("start", cut.Find("[data-axis='primary']").GetAttribute("data-side"));

        cut.Render(parameters => parameters
            .Add(component => component.Direction, ShadcnDirection.RightToLeft)
            .Add(component => component.ShowPrimaryYAxis, false)
            .Add(component => component.ShowSecondaryYAxis, true)
            .Add(component => component.ShowMajorGrid, false)
            .Add(component => component.ShowMinorGrid, true));

        Assert.Empty(cut.FindAll("[data-grid-level='major']"));
        Assert.Equal(4, cut.FindAll("[data-grid-level='minor']").Count);
        Assert.Equal("end", cut.Find("[data-axis='secondary']").GetAttribute("data-side"));
        Assert.Equal("36", cut.Find("[data-axis='secondary']").GetAttribute("x1"));
    }

    [Fact]
    public void LabelKeyAndTemplatesOwnTooltipAndLegendPresentation()
    {
        var config = new ShadcnChartConfig { ["desktop"] = Config["desktop"], ["mobile"] = Config["mobile"], ["visitors"] = new("ผู้เข้าชม") { Color = "red" } };
        var cut = Render<ShadcnChart>(parameters => parameters
            .Add(component => component.Id, "templates")
            .Add(component => component.Config, config)
            .Add(component => component.Categories, new[] { "Jan", "Feb", "Mar" })
            .Add(component => component.Series, Series)
            .Add(component => component.Title, "Templates")
            .Add(component => component.LabelKey, "visitors")
            .Add(component => component.TooltipTemplate, (RenderFragment<ShadcnChartTooltipContext>)(context => builder => builder.AddContent(0, $"TIP-{context.Category}")))
            .Add(component => component.LegendTemplate, (RenderFragment<IReadOnlyList<ShadcnChartSeries>>)(items => builder => builder.AddContent(0, $"LEGEND-{items.Count}"))));
        cut.Find("svg").KeyDown(new KeyboardEventArgs { Key = "Home" });
        Assert.Contains("TIP-Jan", cut.Markup);
        Assert.Contains("LEGEND-2", cut.Markup);
    }

    [Fact]
    public void NameKeyResolvesEachSeriesMetadataWithoutCollapsingLabels()
    {
        var config = new ShadcnChartConfig
        {
            ["desktop"] = Config["desktop"],
            ["mobile"] = Config["mobile"],
            ["web"] = new("เว็บ") { Color = "red" },
            ["app"] = new("แอป") { Color = "blue" }
        };
        var named = new[]
        {
            Series[0] with { Names = new Dictionary<string, string> { ["channel"] = "web" } },
            Series[1] with { Names = new Dictionary<string, string> { ["channel"] = "app" } }
        };
        var cut = Render<ShadcnChart>(parameters => parameters.Add(component => component.Id, "names").Add(component => component.Config, config).Add(component => component.Categories, new[] { "Jan", "Feb", "Mar" }).Add(component => component.Series, named).Add(component => component.Title, "Names").Add(component => component.NameKey, "channel").Add(component => component.LegendInteractive, true));
        Assert.Contains("เว็บ", cut.Find("[data-slot='chart-legend']").TextContent);
        Assert.Contains("แอป", cut.Find("[data-slot='chart-legend']").TextContent);
        cut.Find("svg").KeyDown(new KeyboardEventArgs { Key = "Home" });
        Assert.Contains("เว็บ", cut.Find("[data-slot='chart-tooltip-content']").TextContent);
        Assert.Contains("แอป", cut.Find("[data-slot='chart-tooltip-content']").TextContent);
    }

    [Fact]
    public void InteractiveBottomLegendPersistsAfterAllSeriesHiddenAndReconcilesParameters()
    {
        var cut = Render<ShadcnChart>(parameters => parameters.Add(component => component.Id, "legend").Add(component => component.Config, Config).Add(component => component.Categories, new[] { "Jan", "Feb", "Mar" }).Add(component => component.Series, Series).Add(component => component.Title, "Legend").Add(component => component.LegendInteractive, true));
        cut.Find("button[data-legend-series='desktop']").Click();
        cut.Find("button[data-legend-series='mobile']").Click();
        Assert.Equal(2, cut.FindAll("button[data-legend-series]").Count);
        Assert.All(cut.FindAll("button[data-legend-series]"), button => Assert.Equal("false", button.GetAttribute("aria-pressed")));
        Assert.Empty(cut.FindAll("[data-series]"));
        cut.Render(parameters => parameters.Add(component => component.Categories, new[] { "Jan" }).Add(component => component.Series, new[] { Series[0] }));
        Assert.Single(cut.FindAll("button[data-legend-series]"));
    }

    private IRenderedComponent<ShadcnChart> RenderChart(
        ShadcnChartIndicator indicator = ShadcnChartIndicator.Dot,
        bool hideLabel = false,
        bool hideIndicator = false,
        string? nameKey = null,
        bool loading = false,
        string? error = null,
        string? id = "sales",
        string? description = "สามเดือนล่าสุด",
        IReadOnlyList<ShadcnChartSeries>? series = null,
        ShadcnDirection direction = ShadcnDirection.LeftToRight) => Render<ShadcnChart>(parameters => parameters
            .Add(component => component.Id, id)
            .Add(component => component.Type, ShadcnChartType.Bar)
            .Add(component => component.Config, Config)
            .Add(component => component.Categories, new[] { "Jan", "Feb", "Mar" })
            .Add(component => component.Series, series ?? Series)
            .Add(component => component.Title, "ยอดผู้เข้าชม")
            .Add(component => component.Description, description)
            .Add(component => component.Indicator, indicator)
            .Add(component => component.HideTooltipLabel, hideLabel)
            .Add(component => component.HideTooltipIndicator, hideIndicator)
            .Add(component => component.NameKey, nameKey)
            .Add(component => component.Direction, direction)
            .Add(component => component.Loading, loading)
            .Add(component => component.Error, error));

    private static IReadOnlyList<(string Chart, string Title, string Description, string Style)> RenderChartPair()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddScoped<IShadcnIdAllocator, ShadcnIdAllocator>();
        RenderFragment pair = builder =>
        {
            for (var index = 0; index < 2; index++)
            {
                builder.OpenComponent<ShadcnChart>(index * 10);
                builder.AddAttribute(index * 10 + 1, nameof(ShadcnChart.Config), new ShadcnChartConfig { ["desktop"] = new("Desktop") { Color = "red" } });
                builder.AddAttribute(index * 10 + 2, nameof(ShadcnChart.Categories), new[] { "Jan" });
                builder.AddAttribute(index * 10 + 3, nameof(ShadcnChart.Series), new[] { new ShadcnChartSeries("desktop", [1]) });
                builder.AddAttribute(index * 10 + 4, nameof(ShadcnChart.Title), "Sales");
                builder.AddAttribute(index * 10 + 5, nameof(ShadcnChart.Description), "Monthly");
                builder.CloseComponent();
            }
        };
        var cut = context.Render(pair);
        return cut.FindAll("figure[data-slot='chart']").Select(figure =>
        {
            var chart = figure.GetAttribute("data-chart")!;
            return (chart, figure.GetAttribute("aria-labelledby")!, figure.GetAttribute("aria-describedby")!, figure.QuerySelector("style[data-slot='chart-style']")!.TextContent);
        }).ToList();
    }
}
