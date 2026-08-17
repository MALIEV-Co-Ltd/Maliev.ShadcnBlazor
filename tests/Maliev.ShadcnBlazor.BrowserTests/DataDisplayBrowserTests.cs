using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class DataDisplayBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task DataTableSupportsSortFilterSelectVisibilityPageAndStateControls()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 390, Height = 844 }, ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/data-table").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();
        var table = page.Locator("[data-slot='data-table']");
        await Assertions.Expect(table.Locator("tbody tr[data-row-key]")).ToHaveCountAsync(5);
        await Assertions.Expect(table.Locator(".showcase-data-table-row-action svg")).ToHaveCountAsync(5);
        await table.Locator(".showcase-data-table-row-action").First.ClickAsync();
        await Assertions.Expect(page.Locator("[data-slot='dropdown-menu-content']")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-slot='dropdown-menu-item']")).ToHaveCountAsync(2);
        await page.Locator("[data-slot='dropdown-menu-item']").First.ClickAsync();
        await Assertions.Expect(page.Locator("[data-slot='dropdown-menu-content']")).ToHaveCountAsync(0);
        await Assertions.Expect(table.Locator("input[data-row-key='3']")).ToHaveCSSAsync("appearance", "none");
        var headerBackground = await table.Locator("thead th").Nth(1).EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");
        Assert.Equal("rgba(0, 0, 0, 0)", headerBackground);
        Assert.False(await page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
        await table.Locator("button[data-column='email']").ClickAsync();
        await Assertions.Expect(table.Locator("th[data-column='email']")).ToHaveAttributeAsync("aria-sort", "ascending");
        await table.Locator("input[data-slot='data-table-filter']").FillAsync("niran");
        await Assertions.Expect(table.Locator("tbody tr[data-row-key='8']")).ToHaveCountAsync(1);
        await table.Locator("input[data-slot='data-table-filter']").FillAsync("");
        await table.Locator("input[data-column-filter='status']").FillAsync("success");
        await Assertions.Expect(table.Locator("tbody tr[data-row-key='2']")).ToHaveCountAsync(1);
        await table.Locator("input[data-column-filter='status']").FillAsync("");
        await page.GetByTestId("control-data-table-manual").CheckAsync();
        await Assertions.Expect(table.Locator("button[data-slot='data-table-next']")).ToBeEnabledAsync();
        await table.Locator("button[data-slot='data-table-next']").ClickAsync();
        await Assertions.Expect(table.Locator("[data-slot='data-table-page-summary']")).ToContainTextAsync("2");
        await table.Locator("select[data-slot='data-table-page-size']").SelectOptionAsync("25");
        await Assertions.Expect(table.Locator("select[data-slot='data-table-page-size']")).ToHaveValueAsync("25");
        await Assertions.Expect(table.Locator("button[data-slot='data-table-first']")).ToBeDisabledAsync();
        await table.Locator("input[data-row-key='3']").CheckAsync();
        await Assertions.Expect(table.Locator("[data-slot='data-table-selection-summary']")).ToContainTextAsync("1");
        await page.GetByTestId("control-data-table-loading").CheckAsync();
        await Assertions.Expect(table.Locator("[data-slot='data-table-loading']")).ToBeVisibleAsync();
        await page.GetByTestId("control-data-table-loading").UncheckAsync();
        await page.GetByTestId("control-data-table-error").CheckAsync();
        await Assertions.Expect(table.Locator("[role='alert']")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task DataTableRemainsBalancedInDesktopDarkRtl()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 1440, Height = 900 }, ColorScheme = ColorScheme.Dark });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/data-table").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();
        await page.GetByTestId("documentation-direction-toggle").ClickAsync();
        await page.Locator("html").EvaluateAsync("element => element.classList.add('dark')");
        var table = page.Locator("[data-slot='data-table']");

        await Assertions.Expect(table).ToHaveCSSAsync("direction", "rtl");
        var geometry = await table.Locator(".shadcn-data-table-frame").EvaluateAsync<double[]>("element => { const table = element.querySelector('table'); return [element.clientWidth, table.getBoundingClientRect().width]; }");
        Assert.InRange(Math.Abs(geometry[0] - geometry[1]), 0, 1);
        var actionAlignment = await table.Locator(".shadcn-data-table-action-cell").First.EvaluateAsync<string>("element => getComputedStyle(element).textAlign");
        Assert.Contains(actionAlignment, new[] { "end", "right" });
        Assert.False(await page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Fact]
    public async Task ChartKeyboardTooltipThemeDirectionAndResizeRemainUsable()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 1280, Height = 900 }, ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/chart").ToString());
        var chart = page.Locator("[data-slot='chart']");
        await chart.WaitForAsync();
        await Assertions.Expect(chart).ToHaveAttributeAsync("data-chart-measured", "true");
        var surface = chart.Locator("svg[data-slot='chart-surface']");
        var bars = surface.Locator("rect[data-series]");
        Assert.True(await bars.CountAsync() > 0);
        await Assertions.Expect(bars.First).ToHaveAttributeAsync("rx", "0");
        await surface.FocusAsync(); await page.Keyboard.PressAsync("End");
        await Assertions.Expect(chart.Locator("[data-slot='chart-tooltip-content']")).ToContainTextAsync("Jun");
        await page.GetByTestId("control-chart-line").CheckAsync();
        await Assertions.Expect(surface.Locator("polyline[data-series='desktop']")).ToHaveCountAsync(1);
        await chart.EvaluateAsync("el => { const scope = el.closest('[dir]') ?? document.documentElement; scope.classList.add('dark'); scope.dir='rtl'; }");
        await Assertions.Expect(chart).ToHaveCSSAsync("direction", "rtl");
        await page.SetViewportSizeAsync(390, 844);
        var box = await surface.BoundingBoxAsync(); Assert.NotNull(box); Assert.True(box.Width <= 390);
        await Assertions.Expect(surface).ToHaveAttributeAsync("viewBox", new Regex("^0 0 3[0-9]{2} "));
        var point = surface.Locator("[data-slot='chart-point'][data-point='1']");
        await point.HoverAsync();
        await Assertions.Expect(point).ToHaveAttributeAsync("data-active", "true");
        await Assertions.Expect(chart.Locator("[data-slot='chart-tooltip-content']")).ToContainTextAsync("Feb");
        await chart.Locator("button[data-legend-series='mobile']").ClickAsync();
        await Assertions.Expect(surface.Locator("[data-series='mobile']")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task TableControlsExposeSelectedExpandedAndResponsiveOverflow()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 1280, Height = 900 }, ForcedColors = ForcedColors.Active, ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/table").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();
        var table = page.Locator("#preview table[data-slot='table']");
        await Assertions.Expect(table).ToHaveAttributeAsync("data-expected-columns", "4");
        await Assertions.Expect(table.Locator("thead th")).ToHaveCountAsync(4);
        await Assertions.Expect(table.Locator("tbody tr").First.Locator("td")).ToHaveCountAsync(4);
        await Assertions.Expect(table.Locator("tfoot td")).ToHaveCountAsync(2);
        await Assertions.Expect(table.Locator("tfoot td").First).ToHaveAttributeAsync("colspan", "3");
        await Assertions.Expect(table.Locator("tfoot td").Last).ToContainTextAsync("37,800");
        var container = page.Locator("#preview [data-slot='table-container']");
        var centering = await container.EvaluateAsync<double[]>("element => { const box = element.getBoundingClientRect(); const parent = element.parentElement.getBoundingClientRect(); return [box.left - parent.left, parent.right - box.right]; }");
        Assert.InRange(Math.Abs(centering[0] - centering[1]), 0, 2);
        await page.GetByTestId("control-table-borders").UncheckAsync();
        await Assertions.Expect(table).ToHaveAttributeAsync("data-borders", "false");
        await Assertions.Expect(page.Locator("section.component-code").First).ToContainTextAsync("Borders=\"false\"");
        await page.GetByTestId("control-table-selected").CheckAsync();
        await page.GetByTestId("control-table-expanded").CheckAsync();
        var row = page.Locator("#preview tbody [data-slot='table-row']").First;
        await Assertions.Expect(row).ToHaveAttributeAsync("data-state", "selected");
        await Assertions.Expect(row).ToHaveAttributeAsync("data-expanded", "true");
        await page.GetByTestId("control-table-disabled").CheckAsync();
        await Assertions.Expect(row).ToHaveAttributeAsync("aria-disabled", "true");
        await Assertions.Expect(row).Not.ToHaveAttributeAsync("data-state", "selected");
        await page.SetViewportSizeAsync(320, 700);
        await Assertions.Expect(container).ToHaveCSSAsync("overflow-x", "auto");
        Assert.True(await container.EvaluateAsync<bool>("element => element.scrollWidth > element.clientWidth"));
        Assert.True(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= document.documentElement.clientWidth"));
    }
}
