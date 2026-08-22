using Deque.AxeCore.Playwright;
using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class DropdownMenuShowcaseBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task DropdownMenuSupportsSelectionSubmenusKeyboardAndExactSource()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/dropdown-menu").ToString());

        var dossier = page.GetByTestId("dropdown-menu-dossier-preview");
        var trigger = dossier.Locator("[data-slot='dropdown-menu-trigger']");
        await dossier.WaitForAsync();
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        var triggerBeforeOpen = await trigger.BoundingBoxAsync();

        await trigger.ClickAsync();
        var content = page.Locator("[data-slot='dropdown-menu-content']");
        await Assertions.Expect(content).ToBeVisibleAsync();
        await Assertions.Expect(content).ToHaveAttributeAsync("data-positioned", "true");
        var triggerAfterOpen = await trigger.BoundingBoxAsync();
        Assert.NotNull(triggerBeforeOpen);
        Assert.NotNull(triggerAfterOpen);
        Assert.InRange(Math.Abs(triggerAfterOpen!.X - triggerBeforeOpen!.X), 0, 1);
        Assert.InRange(Math.Abs(triggerAfterOpen.Y - triggerBeforeOpen.Y), 0, 1);
        await Assertions.Expect(content.Locator("[role='menuitem']")).ToHaveCountAsync(5);
        await Assertions.Expect(content.GetByText("Request approval", new() { Exact = true })).ToHaveAttributeAsync("aria-disabled", "true");

        var checkbox = content.Locator("[data-slot='dropdown-menu-checkbox-item']");
        await Assertions.Expect(checkbox).ToHaveAttributeAsync("aria-checked", "true");
        await checkbox.ClickAsync();
        await Assertions.Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");
        await Assertions.Expect(content).ToBeVisibleAsync();

        var compact = content.Locator("[data-slot='dropdown-menu-radio-item']").Filter(new() { HasText = "Compact" });
        await compact.ClickAsync();
        await Assertions.Expect(compact).ToHaveAttributeAsync("aria-checked", "true");

        var export = content.Locator("[data-slot='dropdown-menu-sub-trigger']");
        await export.ClickAsync();
        var submenu = page.Locator("[data-slot='dropdown-menu-sub-content']");
        await Assertions.Expect(submenu).ToBeVisibleAsync();
        await Assertions.Expect(submenu).ToHaveAttributeAsync("data-positioned", "true");
        await Assertions.Expect(submenu).ToHaveAttributeAsync("aria-labelledby", await export.GetAttributeAsync("id") ?? string.Empty);

        var axe = await dossier.RunAxe();
        Assert.Empty(axe.Violations ?? []);

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(submenu).ToHaveCountAsync(0);
        await Assertions.Expect(export).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(content).ToHaveCountAsync(0);
        await Assertions.Expect(trigger).ToBeFocusedAsync();

        await page.GetByTestId("control-dropdown-menu-loop").UncheckAsync();
        await page.GetByTestId("control-dropdown-menu-details").UncheckAsync();
        var source = page.Locator("#preview [data-slot='code-block']").First;
        await Assertions.Expect(source).ToContainTextAsync("Loop=\"false\"");
        await Assertions.Expect(source).ToContainTextAsync("Checked=\"false\"");
    }

    [Fact]
    public async Task DropdownMenuRemainsInsideANarrowRtlForcedColorsViewport()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 360, Height = 780 },
            ReducedMotion = ReducedMotion.Reduce,
            ForcedColors = ForcedColors.Active
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/dropdown-menu").ToString());
        await page.GetByTestId("documentation-direction-toggle").ClickAsync();

        var dossier = page.GetByTestId("dropdown-menu-dossier-preview");
        await dossier.Locator("[data-slot='dropdown-menu-trigger']").ClickAsync();
        var content = page.Locator("[data-slot='dropdown-menu-content']");
        await Assertions.Expect(content).ToBeVisibleAsync();
        var box = await content.BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.InRange(box!.X, 0, 360 - box.Width);
        Assert.InRange(box.X + box.Width, box.Width, 360);
        await Assertions.Expect(page.Locator(".documentation-root")).ToHaveAttributeAsync("dir", "rtl");
        await Assertions.Expect(content).ToHaveCSSAsync("border-top-style", "solid");
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }
}
