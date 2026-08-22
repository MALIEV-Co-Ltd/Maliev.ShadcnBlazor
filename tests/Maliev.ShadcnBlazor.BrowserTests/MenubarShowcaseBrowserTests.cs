using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class MenubarShowcaseBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task MenubarSupportsStablePointerSwitchingAndDirectSelection()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/menubar").ToString());

        var dossier = page.GetByTestId("menubar-dossier-preview");
        await dossier.WaitForAsync();
        var triggers = dossier.Locator("[data-slot='menubar-trigger']");
        await Assertions.Expect(triggers).ToHaveCountAsync(4);

        var file = triggers.Filter(new() { HasText = "File" });
        var edit = triggers.Filter(new() { HasText = "Edit" });
        var menubarBeforeOpen = await dossier.Locator("[data-slot='menubar']").BoundingBoxAsync();
        await file.ClickAsync();
        await Assertions.Expect(file).ToHaveAttributeAsync("data-state", "open");
        var menubarAfterOpen = await dossier.Locator("[data-slot='menubar']").BoundingBoxAsync();
        Assert.NotNull(menubarBeforeOpen);
        Assert.NotNull(menubarAfterOpen);
        Assert.InRange(Math.Abs(menubarAfterOpen!.X - menubarBeforeOpen!.X), 0, 1);
        Assert.InRange(Math.Abs(menubarAfterOpen.Y - menubarBeforeOpen.Y), 0, 1);

        await edit.HoverAsync();
        await Assertions.Expect(edit).ToHaveAttributeAsync("data-state", "open");
        var box = await edit.BoundingBoxAsync();
        Assert.NotNull(box);
        await page.Mouse.MoveAsync(box.X + 3, box.Y + box.Height / 2);
        await page.Mouse.MoveAsync(box.X + box.Width - 3, box.Y + box.Height / 2);
        await Assertions.Expect(edit).ToHaveAttributeAsync("data-state", "open");
        await Assertions.Expect(page.Locator("[data-slot='menubar-content'][data-state='open']")).ToHaveCountAsync(1);

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(edit).ToHaveAttributeAsync("data-state", "closed");
        var view = triggers.Filter(new() { HasText = "View" });
        await view.ClickAsync();
        await Assertions.Expect(view).ToHaveAttributeAsync("data-state", "open");
        var statusItem = page.Locator("[data-slot='menubar-checkbox-item']").Filter(new() { HasText = "Show status bar" });
        await Assertions.Expect(statusItem).ToHaveAttributeAsync("aria-checked", "true");
        await statusItem.ClickAsync();
        await Assertions.Expect(statusItem).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Fact]
    public async Task MenubarHonorsNonLoopingKeyboardNavigationAndResponsiveLayout()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ReducedMotion = ReducedMotion.Reduce,
            ForcedColors = ForcedColors.Active
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/menubar").ToString());

        var dossier = page.GetByTestId("menubar-dossier-preview");
        await dossier.WaitForAsync();
        await page.GetByTestId("control-menubar-loop").UncheckAsync();

        var help = dossier.Locator("[data-slot='menubar-trigger']").Filter(new() { HasText = "Help" });
        await help.FocusAsync();
        await page.Keyboard.PressAsync("ArrowRight");
        await Assertions.Expect(help).ToBeFocusedAsync();

        var overflow = await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > document.documentElement.clientWidth");
        Assert.False(overflow);
        await Assertions.Expect(dossier.Locator(".showcase-menubar-workspace")).ToHaveCSSAsync("border-top-style", "solid");

        var source = page.Locator("#preview [data-slot='code-block']").First;
        await Assertions.Expect(source).ToContainTextAsync("Loop=\"false\"");
    }
}
