using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ResizableShowcaseBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task ResizableDossierSupportsBothPointerAxesKeyboardAndDynamicSource()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        var browserMessages = new System.Collections.Concurrent.ConcurrentQueue<string>();
        page.Console += (_, message) => browserMessages.Enqueue($"{message.Type}: {message.Text}");
        page.PageError += (_, error) => browserMessages.Enqueue($"page-error: {error}");
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/resizable").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var group = page.Locator("#preview [data-slot='resizable-group']");
        var handle = group.Locator("[data-slot='resizable-handle']");
        var grip = handle.Locator("[data-slot='resizable-handle-grip']");
        await Assertions.Expect(group).ToHaveAttributeAsync("data-direction", "horizontal");

        var beforeHorizontal = double.Parse((await handle.GetAttributeAsync("aria-valuenow"))!, System.Globalization.CultureInfo.InvariantCulture);
        var handleBox = await handle.BoundingBoxAsync();
        Assert.NotNull(handleBox);
        await page.Mouse.MoveAsync(handleBox!.X + handleBox.Width / 2, handleBox.Y + handleBox.Height / 2);
        await page.Mouse.DownAsync();
        await Assertions.Expect(handle).ToHaveAttributeAsync("data-resize-active", "true");
        await page.Mouse.MoveAsync(handleBox.X + handleBox.Width / 2 + 72, handleBox.Y + handleBox.Height / 2 + 36, new() { Steps = 4 });
        await page.Mouse.UpAsync();
        await page.WaitForTimeoutAsync(250);
        var afterHorizontal = double.Parse((await handle.GetAttributeAsync("aria-valuenow"))!, System.Globalization.CultureInfo.InvariantCulture);
        Assert.True(afterHorizontal > beforeHorizontal, $"Horizontal drag should increase the first panel from {beforeHorizontal}, but ended at {afterHorizontal}. Browser messages: {string.Join(" | ", browserMessages)}");

        var gripBox = await grip.BoundingBoxAsync();
        Assert.NotNull(gripBox);
        Assert.True(gripBox!.Width <= 16, $"Resizable grip should stay compact but was {gripBox.Width}px wide.");
        Assert.True(gripBox.Height <= 22, $"Resizable grip should stay compact but was {gripBox.Height}px tall.");

        await page.GetByTestId("control-resizable-vertical").CheckAsync();
        await Assertions.Expect(group).ToHaveAttributeAsync("data-direction", "vertical");
        await Assertions.Expect(page.Locator("#preview [data-slot='code-block']").First).ToContainTextAsync("ShadcnResizableDirection.Vertical");

        handle = group.Locator("[data-slot='resizable-handle']");
        var beforeVertical = double.Parse((await handle.GetAttributeAsync("aria-valuenow"))!, System.Globalization.CultureInfo.InvariantCulture);
        handleBox = await handle.BoundingBoxAsync();
        Assert.NotNull(handleBox);
        await page.Mouse.MoveAsync(handleBox!.X + handleBox.Width / 2, handleBox.Y + handleBox.Height / 2);
        await page.Mouse.DownAsync();
        await Assertions.Expect(handle).ToHaveAttributeAsync("data-resize-active", "true");
        await page.Mouse.MoveAsync(handleBox.X + handleBox.Width / 2 + 36, handleBox.Y + handleBox.Height / 2 + 54, new() { Steps = 4 });
        await page.Mouse.UpAsync();
        await Assertions.Expect(handle).Not.ToHaveAttributeAsync("aria-valuenow", beforeVertical.ToString(System.Globalization.CultureInfo.InvariantCulture));
        var afterVertical = double.Parse((await handle.GetAttributeAsync("aria-valuenow"))!, System.Globalization.CultureInfo.InvariantCulture);
        Assert.True(afterVertical > beforeVertical, $"Vertical drag should increase the first panel from {beforeVertical}, but ended at {afterVertical}.");

        await handle.FocusAsync();
        var beforeKeyboard = await handle.GetAttributeAsync("aria-valuenow");
        await page.Keyboard.PressAsync("ArrowUp");
        await Assertions.Expect(handle).Not.ToHaveAttributeAsync("aria-valuenow", beforeKeyboard!);

        var firstPanel = group.Locator("[data-slot='resizable-panel']").First;
        var padding = await firstPanel.Locator(".showcase-resizable-panel-content").EvaluateAsync<double>("element => parseFloat(getComputedStyle(element).paddingInlineStart)");
        Assert.True(padding >= 16, $"Panel content should remain inset from the resize edge, but padding was {padding}px.");
    }
}
