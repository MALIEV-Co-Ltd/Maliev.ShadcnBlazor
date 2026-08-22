using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class CalendarParityBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    [Fact]
    public async Task RangeSelectionCaptionControlsAndExactSourceOperateInBrowser()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            Locale = "th-TH",
            TimezoneId = "Asia/Bangkok",
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/calendar").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var calendar = page.Locator("#preview [data-slot='calendar']");
        await page.ChooseOptionAsync("control-calendar-mode", "Range");
        await calendar.Locator("[data-day='2026-08-18']").ClickAsync();
        await calendar.Locator("[data-day='2026-08-20']").ClickAsync();

        await Assertions.Expect(calendar.Locator("[data-day='2026-08-18']")).ToHaveAttributeAsync("data-range-start", "true");
        await Assertions.Expect(calendar.Locator("[data-day='2026-08-19']")).ToHaveAttributeAsync("data-range-middle", "true");
        await Assertions.Expect(calendar.Locator("[data-day='2026-08-20']")).ToHaveAttributeAsync("data-range-end", "true");
        await Assertions.Expect(calendar.Locator("[data-day='2026-08-18']")).ToHaveAttributeAsync("data-range-complete", "true");
        await Assertions.Expect(page.GetByTestId("calendar-selection")).ToContainTextAsync("18");
        await Assertions.Expect(page.GetByTestId("calendar-selection")).ToContainTextAsync("20");

        await page.ChooseOptionAsync("control-calendar-caption-layout", "Dropdown");
        var monthSelect = calendar.Locator("[data-slot='calendar-month-select']");
        var yearSelect = calendar.Locator("[data-slot='calendar-year-select']");
        await Assertions.Expect(monthSelect.Locator("[data-slot='select-trigger']")).ToHaveAttributeAsync("aria-label", "เลือกเดือน");
        await Assertions.Expect(yearSelect.Locator("[data-slot='select-trigger']")).ToHaveAttributeAsync("aria-label", "เลือกปี");
        await calendar.Locator("[data-slot='calendar-next']").ClickAsync();
        await Assertions.Expect(monthSelect.Locator("[data-slot='select-value']")).ToHaveTextAsync("กันยายน");

        await page.GetByTestId("control-calendar-week-numbers").CheckAsync();
        await Assertions.Expect(calendar.Locator("[data-slot='calendar-week-number-header']")).ToBeVisibleAsync();
        var source = page.Locator("#preview pre");
        await Assertions.Expect(source).ToContainTextAsync("Mode=\"ShadcnCalendarSelectionMode.Range\"");
        await Assertions.Expect(source).ToContainTextAsync("CaptionLayout=\"ShadcnCalendarCaptionLayout.Dropdown\"");
        await Assertions.Expect(source).ToContainTextAsync("ShowWeekNumbers=\"true\"");
        await Assertions.Expect(source).ToContainTextAsync("@bind-Range=\"InspectionWindow\"");
    }

    [Fact]
    public async Task MobileDarkForcedColorsCalendarRemainsCenteredAndKeyboardUsable()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 320, Height = 568 },
            ColorScheme = ColorScheme.Dark,
            Locale = "th-TH",
            ReducedMotion = ReducedMotion.Reduce,
            HasTouch = true
        });
        var page = await context.NewPageAsync();
        await page.EmulateMediaAsync(new() { ForcedColors = ForcedColors.Active, ReducedMotion = ReducedMotion.Reduce });
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/calendar").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var canvas = page.GetByTestId("component-preview-canvas");
        var calendar = canvas.Locator("[data-slot='calendar']");
        var card = canvas.Locator(".showcase-calendar-panel");
        var canvasBox = await canvas.BoundingBoxAsync();
        var cardBox = await card.BoundingBoxAsync();
        Assert.NotNull(canvasBox);
        Assert.NotNull(cardBox);
        Assert.True(cardBox.X >= canvasBox.X && cardBox.X + cardBox.Width <= canvasBox.X + canvasBox.Width + 1);

        var focused = calendar.Locator("[data-slot='calendar-day'][tabindex='0']");
        await focused.FocusAsync();
        await focused.PressAsync("ArrowRight");
        await Assertions.Expect(calendar.Locator("[data-slot='calendar-day'][tabindex='0']")).ToBeFocusedAsync();
        await Assertions.Expect(calendar.Locator("[data-day='2026-08-13']")).ToHaveAttributeAsync("aria-current", "date");
        Assert.Equal("reduce", await page.EvaluateAsync<string>("matchMedia('(prefers-reduced-motion: reduce)').matches ? 'reduce' : 'motion'"));
        Assert.Equal("active", await page.EvaluateAsync<string>("matchMedia('(forced-colors: active)').matches ? 'active' : 'none'"));
    }
}
