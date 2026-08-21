using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class DrawerShowcaseBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task DrawerSupportsTriggerEscapeOutsidePressAndRepeatedOpening()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/drawer").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var trigger = page.Locator("#preview [data-slot='drawer-trigger']");
        var content = page.Locator("[data-slot='drawer-content']");
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

        await trigger.ClickAsync();
        await Assertions.Expect(content).ToBeVisibleAsync();
        await Assertions.Expect(content).ToHaveAttributeAsync("aria-modal", "true");
        await Assertions.Expect(content.GetByText("Confirm dispatch", new() { Exact = true }).Last).ToBeFocusedAsync();

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(content).ToHaveCountAsync(0);
        await Assertions.Expect(trigger).ToBeFocusedAsync();

        await trigger.ClickAsync();
        await page.Locator("[data-slot='drawer-overlay']").ClickAsync(new() { Position = new() { X = 20, Y = 20 } });
        await Assertions.Expect(content).ToHaveCountAsync(0);
        await Assertions.Expect(trigger).ToBeFocusedAsync();

        await trigger.ClickAsync();
        await Assertions.Expect(content).ToBeVisibleAsync();
        await page.GetByTestId("control-drawer-modal-mode").EvaluateAsync("""
            select => {
                select.value = "NonModal";
                select.dispatchEvent(new Event("change", { bubbles: true }));
            }
            """);
        await Assertions.Expect(page.Locator("[data-slot='drawer-overlay']")).ToHaveCountAsync(0);
        await page.Locator("#overview").ClickAsync();
        await Assertions.Expect(content).ToHaveCountAsync(0);
        await Assertions.Expect(trigger).ToBeFocusedAsync();

        await trigger.ClickAsync();
        await Assertions.Expect(content).ToBeVisibleAsync();
        await Assertions.Expect(content.GetByText("Confirm dispatch", new() { Exact = true }).Last).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(content).ToHaveCountAsync(0);
        await Assertions.Expect(trigger).ToBeFocusedAsync();
    }

    [Fact]
    public async Task DrawerKeepsEdgeGeometryAndSwipeDismissalInsideANarrowRtlViewport()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 360, Height = 780 },
            DeviceScaleFactor = 1,
            ColorScheme = ColorScheme.Dark,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/drawer").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();
        await page.GetByTestId("documentation-direction-toggle").ClickAsync();
        await Assertions.Expect(page.Locator(".documentation-root")).ToHaveAttributeAsync("dir", "rtl");
        await page.GetByTestId("control-drawer-direction").SelectOptionAsync("Right");

        var trigger = page.Locator("#preview [data-slot='drawer-trigger']");
        var content = page.Locator("[data-slot='drawer-content']");
        await trigger.ClickAsync();
        await Assertions.Expect(content).ToHaveAttributeAsync("data-drawer-ready", "true");
        await Assertions.Expect(content).ToHaveAttributeAsync("data-edge", "right");
        await Assertions.Expect(content).ToHaveAttributeAsync("data-swipe-axis", "x");
        Assert.Equal("none", await content.EvaluateAsync<string>("element => getComputedStyle(element).animationName"));

        var box = await content.BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.InRange(box!.X, 0, 360 - box.Width + 1);
        Assert.InRange(box.X + box.Width, 359, 361);
        var handleLocator = content.Locator("[data-slot='drawer-swipe-handle']");
        var handle = await handleLocator.BoundingBoxAsync();
        Assert.NotNull(handle);
        Assert.True(float.IsFinite(handle!.X) && float.IsFinite(handle.Y) && float.IsFinite(handle.Width) && float.IsFinite(handle.Height), $"Invalid handle box: {handle.X}, {handle.Y}, {handle.Width}, {handle.Height}");
        var hitSlot = await handleLocator.EvaluateAsync<string?>("element => { const box = element.getBoundingClientRect(); return document.elementFromPoint(box.left + box.width / 2, box.top + box.height / 2)?.closest('[data-slot]')?.getAttribute('data-slot'); }");
        Assert.True(string.Equals("drawer-swipe-handle", hitSlot, StringComparison.Ordinal), $"Handle hit target was '{hitSlot ?? "null"}' at {handle.X}, {handle.Y}, {handle.Width}, {handle.Height}.");
        await page.Mouse.MoveAsync(handle!.X + handle.Width * .5f, handle.Y + handle.Height * .5f);
        await page.Mouse.DownAsync();
        await Assertions.Expect(content).ToHaveAttributeAsync("data-swiping", "");
        await page.Mouse.MoveAsync(box.X + box.Width - 2, handle.Y + handle.Height * .5f, new() { Steps = 8 });
        var drag = await content.EvaluateAsync<string>("element => getComputedStyle(element).getPropertyValue('--shadcn-drawer-drag').trim()");
        Assert.NotEqual("0px", drag);
        await page.Mouse.UpAsync();
        await Assertions.Expect(content).ToHaveCountAsync(0);
        await Assertions.Expect(trigger).ToBeFocusedAsync();
    }
}
