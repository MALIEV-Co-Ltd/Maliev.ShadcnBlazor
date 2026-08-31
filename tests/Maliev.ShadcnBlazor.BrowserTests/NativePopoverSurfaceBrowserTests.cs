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
        var theme = colorScheme == ColorScheme.Dark ? "dark" : "light";
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ColorScheme = colorScheme,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();

        var datePickerContent = await OpenDatePickerAsync(page, theme);
        await AssertTokenizedSurfaceAsync(datePickerContent);

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/select").ToString());
        await SetDocumentationThemeAsync(page, theme);
        var selectTrigger = page.GetByTestId("forms-dossier-select");
        await selectTrigger.ClickAsync();
        var selectContent = page.Locator("#preview [data-slot='select-content']");
        await Assertions.Expect(selectContent).ToBeVisibleAsync();
        await AssertTokenizedSurfaceAsync(selectContent);
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

        var content = await OpenDatePickerAsync(page, "dark");
        await AssertBorderAsync(content, "1px", "solid");
        Assert.True(await content.EvaluateAsync<bool>("""
            element => {
                const probe = document.createElement('span');
                probe.style.color = 'CanvasText';
                document.body.append(probe);
                const matches = getComputedStyle(element).borderTopColor === getComputedStyle(probe).color;
                probe.remove();
                return matches;
            }
            """), "Expected the forced-colors popup border to resolve to CanvasText.");
    }

    private async Task<ILocator> OpenDatePickerAsync(IPage page, string theme)
    {
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/date-picker").ToString());
        await SetDocumentationThemeAsync(page, theme);
        var trigger = page.GetByTestId("forms-dossier-date-picker");
        await trigger.ClickAsync();
        var content = page.Locator("#preview [data-slot='date-picker-content']");
        await Assertions.Expect(content).ToBeVisibleAsync();
        return content;
    }

    private static async Task SetDocumentationThemeAsync(IPage page, string theme)
    {
        var scope = page.Locator("[data-shadcn-scope]").First;
        await scope.WaitForAsync();
        if (theme == "dark")
            await page.GetByTestId("documentation-theme-toggle").ClickAsync();
        await Assertions.Expect(scope).ToHaveAttributeAsync("data-shadcn-theme", theme);
    }

    private static async Task AssertTokenizedSurfaceAsync(ILocator content)
    {
        await AssertBorderAsync(content, "0px", "none");
        Assert.NotEqual("0px", await content.EvaluateAsync<string>("element => getComputedStyle(element).borderTopLeftRadius"));
        Assert.NotEqual("none", await content.EvaluateAsync<string>("element => getComputedStyle(element).boxShadow"));
    }

    private static async Task AssertBorderAsync(ILocator content, string width, string style)
    {
        await Assertions.Expect(content).ToHaveCSSAsync("border-top-width", width);
        await Assertions.Expect(content).ToHaveCSSAsync("border-top-style", style);
    }
}
