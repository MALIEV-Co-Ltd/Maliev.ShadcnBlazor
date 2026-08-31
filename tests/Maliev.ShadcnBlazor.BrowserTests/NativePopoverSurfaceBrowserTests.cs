using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class NativePopoverSurfaceBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    [Theory]
    [InlineData(ColorScheme.Light)]
    [InlineData(ColorScheme.Dark)]
    public async Task NativeFormPopoversResetTheUserAgentBorder(ColorScheme colorScheme)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ColorScheme = colorScheme,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();

        var datePickerContent = await OpenDatePickerAsync(page);
        await AssertBorderAsync(datePickerContent, "0px", "none");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/select").ToString());
        var selectTrigger = page.GetByTestId("forms-dossier-select");
        await selectTrigger.ClickAsync();
        var selectContent = page.Locator("#preview [data-slot='select-content']");
        await Assertions.Expect(selectContent).ToBeVisibleAsync();
        await AssertBorderAsync(selectContent, "0px", "none");
    }

    [Fact]
    public async Task DatePickerPopoverRetainsAVisibleSystemBoundaryInForcedColors()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ColorScheme = ColorScheme.Dark,
            ForcedColors = ForcedColors.Active,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();

        var content = await OpenDatePickerAsync(page);
        await AssertBorderAsync(content, "1px", "solid");
    }

    private async Task<ILocator> OpenDatePickerAsync(IPage page)
    {
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/date-picker").ToString());
        var trigger = page.GetByTestId("forms-dossier-date-picker");
        await trigger.ClickAsync();
        var content = page.Locator("#preview [data-slot='date-picker-content']");
        await Assertions.Expect(content).ToBeVisibleAsync();
        return content;
    }

    private static async Task AssertBorderAsync(ILocator content, string width, string style)
    {
        await Assertions.Expect(content).ToHaveCSSAsync("border-top-width", width);
        await Assertions.Expect(content).ToHaveCSSAsync("border-top-style", style);
    }
}
