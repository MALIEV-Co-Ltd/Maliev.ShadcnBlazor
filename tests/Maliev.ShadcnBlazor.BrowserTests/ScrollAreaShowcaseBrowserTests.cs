using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ScrollAreaShowcaseBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task DossierScrollsByKeyboardAndPointerWithContainedVerticalAndHorizontalTracks()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 900, Height = 900 },
            ColorScheme = ColorScheme.Dark,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/scroll-area").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var root = page.Locator("#preview [data-slot='scroll-area']");
        var viewport = root.Locator("[data-slot='scroll-area-viewport']");
        var vertical = root.Locator("[data-slot='scroll-area-scrollbar'][data-orientation='vertical']");
        await Assertions.Expect(viewport).ToHaveAttributeAsync("aria-label", "Production activity");
        await Assertions.Expect(root).ToHaveAttributeAsync("data-overflow-y", "true");

        await viewport.FocusAsync();
        await page.Keyboard.PressAsync("End");
        await page.WaitForFunctionAsync("element => element.scrollTop > 0", await viewport.ElementHandleAsync());
        Assert.True(await viewport.EvaluateAsync<double>("element => element.scrollTop") > 0);
        await AssertContainedAsync(root, vertical);

        await page.GetByTestId("control-scroll-horizontal").CheckAsync();
        await Assertions.Expect(viewport).ToHaveAttributeAsync("aria-label", "Weekly machine schedule");
        await Assertions.Expect(root).ToHaveAttributeAsync("data-overflow-x", "true");
        var horizontal = root.Locator("[data-slot='scroll-area-scrollbar'][data-orientation='horizontal']");
        var thumb = horizontal.Locator("[data-slot='scroll-area-thumb']");
        await Assertions.Expect(thumb).ToBeVisibleAsync();
        await page.EvaluateAsync("() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)))");
        var thumbBox = await thumb.BoundingBoxAsync();
        Assert.NotNull(thumbBox);
        await page.Mouse.MoveAsync(thumbBox!.X + (thumbBox.Width / 2), thumbBox.Y + (thumbBox.Height / 2));
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(thumbBox.X + thumbBox.Width + 140, thumbBox.Y + (thumbBox.Height / 2));
        await page.Mouse.UpAsync();
        Assert.True(await viewport.EvaluateAsync<double>("element => Math.abs(element.scrollLeft)") > 0);
        await AssertContainedAsync(root, horizontal);

        await page.GetByTestId("control-scroll-always").UncheckAsync();
        await Assertions.Expect(page.Locator("#preview .component-code pre").First).ToContainTextAsync("ShadcnScrollAreaType.Auto");
        await Assertions.Expect(page.Locator("#preview .component-code pre").First).ToContainTextAsync("ShadcnScrollAreaOrientation.Horizontal");
    }

    private static async Task AssertContainedAsync(ILocator root, ILocator scrollbar)
    {
        var rootBox = await root.BoundingBoxAsync();
        var barBox = await scrollbar.BoundingBoxAsync();
        Assert.NotNull(rootBox);
        Assert.NotNull(barBox);
        Assert.InRange(barBox!.X, rootBox!.X, rootBox.X + rootBox.Width);
        Assert.InRange(barBox.Y, rootBox.Y, rootBox.Y + rootBox.Height);
        Assert.InRange(barBox.X + barBox.Width, rootBox.X, rootBox.X + rootBox.Width + 0.5);
        Assert.InRange(barBox.Y + barBox.Height, rootBox.Y, rootBox.Y + rootBox.Height + 0.5);
    }
}
