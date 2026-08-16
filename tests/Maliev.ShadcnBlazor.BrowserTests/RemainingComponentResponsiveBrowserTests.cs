using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class RemainingComponentResponsiveBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    [Theory]
    [InlineData("button-group")]
    [InlineData("calendar")]
    [InlineData("direction")]
    [InlineData("tabs")]
    public async Task PreviewCanvasStaysWithinItsPhoneViewport(string component)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 320, Height = 568 },
            ColorScheme = ColorScheme.Dark,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync(new Uri(server.BaseUri, $"/docs/components/{component}?theme=dark&dir=rtl&locale=th").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var canvas = page.Locator("#preview .component-preview__canvas").First;
        await canvas.WaitForAsync();
        var overflow = await canvas.EvaluateAsync<double>("element => element.scrollWidth - element.clientWidth");
        Assert.InRange(overflow, 0, 1);

        var documentOverflow = await page.EvaluateAsync<double>(
            "Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)");
        Assert.InRange(documentOverflow, 0, 1);
    }
}
