using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class PopoverShowcaseBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task PopoverOpensWithoutAPlacementFlashAndRestoresTriggerFocus()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/popover").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var trigger = page.Locator("#preview [data-slot='popover-trigger']");
        var content = page.Locator("#preview [data-slot='popover-content']");
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        await Assertions.Expect(content).ToHaveCountAsync(0);

        await page.EvaluateAsync("""
            () => {
                window.__popoverPlacementFlash = false;
                const observer = new MutationObserver(() => {
                    const content = document.querySelector("#preview [data-slot='popover-content']");
                    if (content && content.dataset.positioned === "false" && getComputedStyle(content).visibility !== "hidden")
                        window.__popoverPlacementFlash = true;
                });
                observer.observe(document.body, { childList: true, subtree: true });
                window.__popoverPlacementObserver = observer;
            }
            """);

        await trigger.ClickAsync();
        await Assertions.Expect(content).ToBeVisibleAsync();
        await Assertions.Expect(content).ToHaveAttributeAsync("data-positioned", "true");
        await Assertions.Expect(content).ToContainTextAsync("Part dimensions");
        await Assertions.Expect(content.Locator("[data-slot='input']")).ToHaveCountAsync(3);
        Assert.False(await page.EvaluateAsync<bool>("window.__popoverPlacementFlash"));

        await content.Locator("#part-width").FillAsync("145");
        await page.Locator("#overview").ClickAsync();
        await Assertions.Expect(content).ToHaveCountAsync(0);

        await page.GetByTestId("control-popover-top").CheckAsync();
        await trigger.ClickAsync();
        await Assertions.Expect(content).ToHaveAttributeAsync("data-side", "top");
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(content).ToHaveCountAsync(0);
        await Assertions.Expect(trigger).ToBeFocusedAsync();

        var source = page.Locator("#preview [data-slot='code-block']").First;
        await Assertions.Expect(source).ToContainTextAsync("Side=\"ShadcnOverlaySide.Top\"");
        await Assertions.Expect(source).ToContainTextAsync("<ShadcnInput");
    }

    [Fact]
    public async Task PopoverRemainsInsideANarrowRtlViewport()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 360, Height = 780 },
            DeviceScaleFactor = 1,
            ColorScheme = ColorScheme.Dark,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/popover").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();
        await page.GetByTestId("documentation-direction-toggle").ClickAsync();

        var trigger = page.Locator("#preview [data-slot='popover-trigger']");
        await trigger.ClickAsync();
        var content = page.Locator("#preview [data-slot='popover-content']");
        await Assertions.Expect(content).ToBeVisibleAsync();
        var box = await content.BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.InRange(box!.X, 0, 360 - box.Width);
        Assert.InRange(box.X + box.Width, box.Width, 360);
        await Assertions.Expect(page.Locator(".documentation-root")).ToHaveAttributeAsync("dir", "rtl");
    }
}
